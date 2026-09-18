# VaM_Rebuild - how the Unity project is assembled

`scripts\Setup-RebuildProject.ps1` builds `VAMOpen\VaM_Rebuild` from two inputs: the AssetRipper
export in `work\ripped-core\ExportedProject` (see `asset-export.md`) and our own verified sources in
`src\`. It deletes the target directory first, so it is safe to re-run at any time.

```
VaM_Rebuild\                        6 518 files, 247.8 MB
  Assets\                           6 500 files
    Scripts\
      Assembly-CSharp\              2 807 .cs  (no .asmdef -> Unity's predefined Assembly-CSharp)
      Assembly-UnityScript\            13 .cs  + Assembly-UnityScript.asmdef
    Plugins\                            13 managed DLLs
    Plugins\x86_64\                 85 native files, 182.3 MB
    VaMAssets\Scenes\                   10 scenes (NewStart + 9 Benchmark*)
    GameObject AnimeClip AnimatorController Avatar Cubemap Flare Font
    Material Mesh PhysicMaterial Resources Shader Sprite TextAsset Texture2D
  Packages\manifest.json
  ProjectSettings\                   17 .asset + ProjectVersion.txt (2018.4.36f1)
```

## What is replaced, and why

**Assembly-CSharp / Assembly-UnityScript come from `src\`, not from the export.** AssetRipper
decompiled them independently and produced exactly the same 2 807 + 13 file set - the two
decompilers agree file for file - but ours has been through the artifact fixers
(`tools\fix_ref_locals.py`, `tools\fix_member_tokens.py`), builds clean, and is proven 2 753/2 753
type-identical to the shipped assembly (`parity-report.md`).

**The other 21 decompiled assemblies are deleted and replaced by the original DLLs.** AssetRipper
exports *every* managed assembly as source, which for this build means 4 500 files of NAudio, mcs,
ICSharpCode.SharpZipLib, ZFBrowser, protobuf-net - and, worse, of `System.Windows.Forms`,
`System.Drawing`, `System.Configuration`, `Mono.Posix`, `Mono.WebBrowser`, `Accessibility`. Those
last ones are the Mono class library that Unity itself supplies; compiling the decompiled copies as
project source collides with the editor's own BCL and cannot work. Third-party assemblies are also
better off as the pristine binaries the game shipped than as machine-decompiled source nobody wants
to maintain. The 8 BCL folders are simply dropped (Unity's Mono profile provides those types) and
the other 13 become `Assets\Plugins\*.dll`.

Excluded from `Assets\Plugins` by `Update-UnityReferences.ps1`'s BCL policy: `mscorlib`,
`netstandard`, `Accessibility`, `System`, `System.Core`, `System.Configuration`, `System.Data`,
`System.Drawing`, `System.EnterpriseServices`, `System.Runtime.Serialization`, `System.Security`,
`System.Transactions`, `System.Windows.Forms`, `System.Xml`, `Mono.Data.Tds`, `Mono.Posix`,
`Mono.Security`, `Mono.WebBrowser`, plus anything named `UnityEngine*` / `UnityEditor*`. The
`-IncludeBclShims` switch narrows that to just `mscorlib` + `netstandard` as a fallback if the
editor reports missing types.

## Native plugins

AssetRipper writes no `Assets\Plugins` at all - the native side of the build is not in its export,
and nothing in `Assets\Scripts` would have worked without it. `VaM_Data\Plugins` is copied to
`Assets\Plugins\x86_64` (85 files, 182.3 MB). The folder is not a guess: `VaM_Data\Plugins` is
precisely the plugin folder of the shipped build, which is why it holds CEF data files like
`icudtl.dat`, `locales\*.pak` and `natives_blob.bin` alongside the DLLs.

**Unity's copy rule is narrower than the folder suggests.** Of the 85 files the editor puts **only
the 21 `.dll`** into `<player>_Data\Plugins`; the other 64 are dropped without a word - the `.pak`
files including the 53 under `locales\`, `icudtl.dat`, the `*_blob.bin` files, `ZFGameBrowser.exe`
and one `.txt`. The build puts them back itself: `RebuildPlayer.cs` copies every non-`.meta` file of
`Assets\Plugins\x86_64` into the player's `Plugins` folder after a successful build
(`StageRuntimePlugins`, 64 files a run, each one length-checked), and writes the 14-byte
`<player>_Data\Resources\browser_assets` index - `zfbRes_v1` followed by `0` - that
`StandaloneWebResources.LoadIndex` reads from `Application.dataPath + "/Resources/browser_assets"`.
A build without them still looks correct: it launches, reaches the main menu, and dies with
`0xc0000409` inside `mono-2.0-bdwgc.dll` the first time a scene opens the browser, because what is
missing surfaces as a `DllNotFoundException` raised inside a native callback.

Only one native library is bound directly from `Assembly-CSharp`, but the rest are reached through
wrappers or through project settings:

| native library | who binds it | how |
|---|---|---|
| `LeapC.dll` | `Assembly-CSharp` | 38 `DllImport`s |
| `libOni.dll` | `Assembly-CSharp` | `DllImport` (OpenNI) |
| `libmpg123-0.dll` | `Assembly-CSharp` | `DllImport` (MP3 via bass) |
| `OVRPlugin` / `OVRGamepad` / `OVRLipSync` / `OVRVoiceMod` / `MemoryReporter` | `Assembly-CSharp` | `DllImport` |
| `AudioPluginOculusSpatializer.dll` | `Assembly-CSharp` | `DllImport` |
| `bass.dll` | `Bass.Net.dll` | `DllImport` inside the wrapper |
| `openvr_api.dll` | `SteamVR.dll` (and `ZFBrowser.dll`) | `DllImport` |
| `AudioPluginMsHRTF.dll` | `ProjectSettings\AudioManager.asset` | `m_SpatializerPlugin: MS HRTF Spatializer` - no managed reference at all |
| `zf_cef.dll` + `chrome_elf.dll`, `libEGL.dll`, `libGLESv2.dll`, `d3dcompiler_4{3,7}.dll`, `widevinecdmadapter.dll`, `ZFGameBrowser.exe`, `.pak` / `.bin` files, `icudtl.dat`, `locales\` | `ZFBrowser.dll` | not `DllImport` - CEF is located at runtime by a directory scan (`cefPath`, `CEFDirs`, `GetCEFDirs`), which is why the build-output layout has to be mirrored exactly |
| `ConvexDecompositionDll.dll` | nothing in the managed build | shipped by the game, no managed reference found by string search; kept for fidelity |

182 MB of it is the CEF payload, which is dead weight unless the in-game browser is exercised - but
removing it would break CEF discovery, and the cost of keeping it is disk only. Use
`scripts\Setup-RebuildProject.ps1 -SkipNativePlugins` for fast re-runs where runtime is not involved.
Unity will generate `.meta` files for all of these on first import; they define no GUIDs anything
references, so the integrity gate is unaffected.

## The .asmdef for Assembly-UnityScript

AssetRipper emitted an `.asmdef` for every assembly it exported except `Assembly-UnityScript`.
Without one the 13 UnityScript files would be merged into the default `Assembly-CSharp`, and the
UnityScript types shadow C# helpers with the same names (`CharacterMotor`, `FPSInputController`,
`ChainMouseOrbit`, ...). The original game has them in a separate `Assembly-UnityScript.dll`, and
`Assembly-CSharp` references that assembly, so the script writes the same split back. Predefined
assemblies automatically reference `.asmdef` assemblies, which is what lets `Assembly-CSharp` keep
using those types.

The `.asmdef` inside that folder is deliberately **not** called `Assembly-UnityScript.asmdef`:
Unity 2018.1 still reserves the name as its own predefined UnityScript assembly and refuses to
compile a project that defines it. The folder keeps the historical name, the assembly is
`VaMUnityScript`, and nothing else changes - UnityScript types are reached by type name, so the
rename cannot break a reference.

## .meta files must follow the sources

Unity resolves a `MonoBehaviour` in a scene or prefab to its class through the 32-hex GUID of the
script's `.meta` file. `Assets\Scripts\Assembly-CSharp` and `Assembly-UnityScript` are deleted and
refilled from `src\`, which does not carry metas - so before the swap the script stashes every
`*.cs.meta` from the export, and afterwards drops each one back next to the `.cs` of the same
relative path. Metas whose `.cs` is not in our tree are reported as orphans (currently 0).

AssetRipper writes metas lazily: only 2 165 of the 2 820 scripts in those two folders have one,
because only referenced GUIDs get a meta. The other 655 scripts are referenced by nothing, so Unity
is free to assign them fresh GUIDs.

**The copy is a copy, not a view.** `Setup-RebuildProject.ps1` fills
`VaM_Rebuild\Assets\Scripts\Assembly-CSharp` out of `src\Assembly-CSharp`, and there is no hardlink
and no symlink between the two: an edit in `src\` reaches the compiler only after something copies it
there. Measured once the divergence was noticed: `UserPreferences.cs` at 129 969 B in `src` against
118 911 B in the project, two days apart, and a green compile gate that was green about the older
tree. `scripts\Sync-Sources.ps1` is that missing step and is meant to be run after every edit in
`src\` and before any gate: it copies the `*.cs` files of both assemblies into the project, leaves
every `.cs.meta` untouched on the project side because a scene reaches its class through that GUID,
re-reads both trees and refuses to report `OK` unless each file is byte-for-byte equal, and compares
the newest source's timestamp with `Assets\Scripts\Assembly-CSharp.dll`, printing
`STALE - the assembly predates the sources` when the build no longer reflects the tree. Run without
it, a gate answers about whatever was copied last.

## Reference integrity

`tools\check_asset_guids.py --project VaM_Rebuild` walks `Assets`, collects the GUIDs defined by
`.meta` files, collects the GUIDs referenced from scenes, prefabs and `.asset` files, and reports
the difference. `tools\known_dangling_guids.txt` lists the references that are expected to stay
undefined, so the checker exits non-zero only on a *new* break. The setup script runs it as its
last step.

| tree | defined | referenced | dangling | expected |
|---|---|---|---|---|
| raw export | 3 078 | 445 | 3 | 3 |
| `VaM_Rebuild` | 2 872 | 445 | 6 | 6 |

Of the 445 references, 442 resolve. The 6 that do not:

| GUID | effect |
|---|---|
| `0000000000000000e000000000000000` | Unity's unknown-script sentinel; ~28 UI prefabs |
| `0000000000000000f000000000000000` | same, ~59 UI prefabs |
| `f5f67c52d1564df4a8936ccd202a3bd8` | same, 22 prefabs (309 component references) |
| `1f8b21067ddc5d3e82ada8871809e11a` | `Resources\SteamVR_Settings.asset` |
| `59472c286047efb8b486f27051c691c7` | `Resources\SteamVR_ExternalCamera.prefab` |
| `a45dbaba982dfcd8b0bc9cfcd1bdd614` | `Resources\ReferencePose_{BindPose,Fist,OpenHand}.asset` |

The first three are in the raw export too and are artifacts of AssetRipper's export, not of the
assembly swap - they correspond to components that were already unresolved in the shipped build.
The three SteamVR ones are ours: see below.

### Why SteamVR necessarily breaks three references

AssetRipper decompiled `SteamVR.dll` into `Assets\Scripts\SteamVR` (450 files) and pointed those
four assets at the generated `.cs` files; we delete that folder and ship `SteamVR.dll` as a plugin
instead.

This cannot be papered over by giving the DLL the same GUID, because the GUID is only half the
reference. Every script reference in the export is `{fileID: 11500000, guid: ..., type: 3}` -
`11500000` is the fixed file ID Unity uses for the `MonoScript` sub-asset of a `.cs` file. A script
inside a DLL is identified differently: one GUID for the whole assembly plus a hash-derived file ID
per class. The 309 component references that use hash file IDs all point at the single undefined
GUID `f5f67c52...`, which is exactly Unity's "could not resolve this script" encoding. The original
project's own identifiers are unrecoverable - AssetRipper invents the entire GUID space, since a
shipped build does not contain the project's `.meta` files.

Because three *different* classes are involved, they cannot all live in one DLL behind one file ID,
so the only way to make those four assets resolve is to keep SteamVR as decompiled source. That is
deliberately not done: 450 files of decompiled Valve code would have to compile before *anything*
else in the project does, which turns a 4-asset cosmetic gap into a project-wide blocker. SteamVR is
used only in VR mode; in desktop mode the missing scripts degrade to default SteamVR settings.

If the trade-off is ever revisited: restore `Assets\Scripts\SteamVR` from the export (with a
`SteamVR.asmdef`), delete `Assets\Plugins\SteamVR.dll`, run the artifact fixers over it, and remove
the three GUIDs from `known_dangling_guids.txt`.

## manifest.json, and what changed it

`Packages\manifest.json` started as the 31 `com.unity.modules.*` entries AssetRipper generated for
2018.1.9f2, and that was correct for the version: in Unity 2018.1 uGUI is not yet a Package
Manager package - `UnityEngine.UI.dll` is supplied by the built-in Unity extension at
`Editor\Data\UnityExtensions\Unity\GUISystem\`, so no `com.unity.ugui` entry exists or is needed
(the package only appears from 2019.2). `com.unity.timeline` was considered and rejected on
evidence: `UnityEngine.Timeline` is referenced by 0 files in `src\` (only the builtin
`UnityEngine.TimelineModule`, covered by `com.unity.modules.director`, is used).

**The 2018.4 editor added six entries by itself, on first open**: `com.unity.ads` 2.0.8,
`com.unity.analytics` 3.2.3, `com.unity.collab-proxy` 1.2.15, `com.unity.package-manager-ui` 2.0.13,
`com.unity.purchasing` 2.2.1 and `com.unity.textmeshpro` 1.4.1 - plus the trailing newline the file
never had. That is 2018.4's own default set for a project that declares no hold on it, so the hop
wrote it into a tracked file. It is kept exactly as the editor left it: the compile gate and a hand
run are both green in that state, and `com.unity.textmeshpro` is genuinely load-bearing - it is the
package that compiles `Library\ScriptAssemblies\Unity.TextMeshPro.dll`, and no TMP copy exists under
`Assets\Plugins`. The same first open also created `ProjectSettings\VFXManager.asset`, a settings
file 2018.1 had no counterpart for.

## The Unity editor that has to open this project

`ProjectSettings\ProjectVersion.txt` names the editor, and the scripts read *that file* instead of
hardcoding a path (`scripts\UnityEditor.ps1`), so a version hop needs no script edit. The project is
now pinned to **2018.4.36f1**, the last 2018 LTS - hop one of two, with 2019.4 to come; what the hop
cost is in [`release-notes.md`](release-notes.md), and the item-by-item audit of the hop against
Unity's own upgrade guide is in [`unity-upgrade-audit.md`](unity-upgrade-audit.md).

That file is also a trap, and it caught the hop once: `Setup-RebuildProject.ps1` lays the export down
on every run, and it used to do that to `ProjectSettings` and `Packages` too - so the gate scripts,
reading the editor to launch from a file the setup had just restored from a 2018.1 snapshot, ran
`2018.1.9f2` while reporting green. Both directories are now stashed across the wipe and copied back
over the export's copy, the same way `Assets\Editor` already was.

The project started on `2018.1.9f2` because that is the engine the game shipped with
(`FileVersion 2018.1.9.10931241`, identical to the game's `UnityPlayer.dll`). A newer editor is not
free - different API surface and different serialisation - which is why the migration is staged
rather than a jump to whatever is installed here (Unity 6000.6.0f1 also is).

The licence blocked everything at first: the legacy validator in 2018.1.9f2 rejects the
`Unity_lic.ulf` that Unity Hub keeps re-issuing. Activating once through the editor's own GUI fixed
it (`C:\ProgramData\Unity\Unity_lic.ulf`), and the project imports, compiles and plays in batch mode.
**2018.4 accepted that same file without being re-activated** - its log opens with `Initiating legacy
licensing module` and reports only `Next license update check is after ...`. Keep Unity Hub closed -
its `updateLicenses` pass re-breaks the 2018 editors. The history is in `unity-editor.md`.

## Project settings that are not defaults

Three settings are what stand between the decompiled code and a clean build:

| setting | value | effect |
|---|---|---|
| `scriptingRuntimeVersion` | `1` (.NET 4.x) | 1421 compile errors -> 20 |
| `apiCompatibilityLevel` | `3` (.NET 4.x) | and 20 -> 0 |
| the UnityScript `.asmdef` name | `VaMUnityScript` | the project refuses to build at all under the original name |

The game shipped on the Mono 2.0 profile, so its references do not map one-to-one onto a 2018.1
editor. `scripts\Setup-RebuildProject.ps1` decides each managed assembly's fate:

- **Supplied by Unity, never copied**: the runtime closure in
  `MonoBleedingEdge\lib\mono\4.7.1-api\` - `mscorlib`, `netstandard`, `System`, `System.Core`,
  `System.Xml`, `System.Xml.Linq`, `System.Runtime.Serialization`, `System.Numerics`,
  `System.Numerics.Vectors` - plus the `Facades\` folder.
- **`Boo.Lang`**: Unity supplies its own copy from `lib\mono\unityscript\` and auto-references it
  unconditionally, so VaM's copy can only ever duplicate it
  (`error CS0433: The imported type 'Boo.Lang.GenericGenerator<T>' is defined multiple times`,
  from `CharacterMotor.cs`). Marking the meta Standalone-only does not help, because the
  auto-reference ignores platforms - the copy has to be deleted.
- **Shipped as ordinary editor plugins**: `Mono.Cecil` (used by `DynamicCSharp\Security`) and
  `System.Drawing` (used by `ImageLoaderThreaded` / `MaterialOptions`). Neither is in the profile
  Unity compiles against. `Mono.Cecil` comes from VaM unchanged. `System.Drawing` does not: VaM's
  copy is Mono's .NET 2.0 build, and on the .NET 4.x runtime its
  `ComIStreamMarshaler+ManagedToNativeWrapper` static constructor throws
  (`TypeInitializationException` -> `NullReferenceException`), so every `new Bitmap(Stream)` fails -
  no scene or character thumbnail ever decodes. The player's `Managed\` folder is filled from
  `MonoBleedingEdge\lib\mono\unityjit\`, whose `System.dll`, `System.Core.dll` and `mscorlib.dll` are
  byte-identical to the built player's, so `System.Drawing` is taken from that same folder and lands
  at `System.Drawing, Version=4.0.0.0`. `Setup-RebuildProject.ps1` checks the staged identity and
  throws if it is still 2.x. Assemblies still compiled against `2.0.0.0` (`Bass.Net`, `NAudio`) are
  unified onto the 4.x assembly by Mono's binder.
- **Left out by default**: the rest of the BCL VaM shipped - `mcs.dll`'s runtime closure
  (`Accessibility`, `System.Configuration`, `System.Data`, `System.EnterpriseServices`,
  `System.Security`, `System.Transactions`, `System.Windows.Forms`, `Mono.Data.Tds`, `Mono.Posix`,
  `Mono.Security`, `Mono.WebBrowser`). Each one can collide with Unity's facades.
  `-IncludeVaMBclExtras` copies them back for a player build that turns out to need them.
- **Rebuilt from source**: `mcs.dll`, the C# compiler VaM drives at runtime through `DynamicCSharp`. VaM
  ships it built against Mono 2.0's `mscorlib`, and on the 4.x profile the installed copy aborts the whole
  compilation on any defaulted nullable value type - see *The plugin compiler* below.
- **Everything else** becomes `Assets\Plugins\*.dll` unchanged - the pristine binaries the game
  shipped, not machine-decompiled source.

### The plugin compiler

`mcs.dll` is not a game library that happens to be in `Managed\`: it is Mono's C# compiler, and VaM's
`DynamicCSharp` layer feeds it the user's plugins - scenes, presets, custom scripts - at runtime. The
installation's copy works on the Mono 2.0 profile it was built for. On this project's 4.x profile it dies
on the first defaulted nullable value type:

```
Mono.CSharp.InternalErrorException: variants.cs(4,24): Defaults.A(int?)
 ---> System.ArgumentException: System.Nullable`1[System.Int32] is not a supported constant type.
   at System.Reflection.Emit.ParameterBuilder.SetConstant(...)
   at Mono.CSharp.Parameter.ApplyAttributes(...)
   at Mono.CSharp.Method.Emit(...)
```

`SetConstant` refuses to write that value into the parameter's metadata, `Parameter.ApplyAttributes` runs
inside attribute emit, and the exception takes the whole compilation down with it. The failure mode is what
makes it expensive: the report comes back empty and *no* plugin loads, so from the outside the installation
simply has no custom content, with no error naming a plugin.

Three prebuilt replacements were measured and rejected before building one:

| candidate | loads in Unity? | compiles a plugin? |
|---|---|---|
| `VaM_Data\Managed\mcs.dll` (shipped) | yes | no - `not a supported constant type` |
| `MonoBleedingEdge\lib\mono\unityjit\Mono.CSharp.dll` | yes | no - different assembly name, `internal` API, 136 x `CS0122` |
| `MonoBleedingEdge\lib\mono\4.5\mcs.exe` | no - an exe-flavoured assembly has no `IMAGE_FILE_DLL` | - |
| the same exe with its PE header relabelled | yes | no - same internal API, 168 errors |

So the compiler is rebuilt from the sources the installation ships. The game's `Managed\` folder holds
`mcs.dll` only as a binary, so the sources come from the export - AssetRipper writes *every* assembly out
as source, and that is where `src\mcs\` (797 files, `Mono\` + `IKVM\`) comes from. `scripts\Build-McsCompiler.ps1`
compiles it with the editor's own `MonoBleedingEdge\bin\mono.exe` + `lib\mono\4.5\mcs.exe` as
`-target:library -sdk:4.5 -noconfig -optimize+ -debug- -langversion:latest`, and validates the output before
it is accepted: at least 700 sources, the `IMAGE_FILE_DLL` characteristic, and `AssemblyName.Name == 'mcs'`
(Unity loads by assembly name, so a renamed build would not be found). `mscorlib.dll` is deliberately not
passed on the command line - with it the compile fails with `CS1685`, `System.Object` defined twice.

One patch is applied to the sources, in `src\mcs\Mono\CSharp\Parameter.cs`: the helper that writes a
constant through `ParameterBuilder.SetConstant` catches `ArgumentException` and `NotSupportedException` and
drops the attribute instead of propagating. That is Mono 2.0's own behaviour with the same input - the
emitted parameter simply carries no default - and it is what makes both the probe and real plugins compile.
The engine still reads the default from the source, because `DynamicCSharp` compiles the plugin for the
runtime and does not depend on the reflected attribute.

`Setup-RebuildProject.ps1` runs the build, stages the result at `Assets\Plugins\mcs.dll`, and
`Assets\Resources\DynamicCSharp_Settings.asset` keeps `mcs.dll` in its single `referenceRestrictions` entry
- that list is a blocklist, and it exists so the plugin compiler cannot be handed to itself as a reference.

`Assets\Editor\RebuildGate.cs` is the batch entry point that proves all of this end to end; see
`verification.md` for what each method checks and `scripts\Invoke-CompileGate.ps1` /
`scripts\Invoke-SmokeTest.ps1` for how to run them.

The `System.Drawing` pairing was measured, not assumed. A 30-line probe compiled with the editor's own
compiler and run under `MonoBleedingEdge\bin\mono.exe` reproduces the player's exact
`TypeInitializationException` -> `NullReferenceException` chain against VaM's 2.0 copy, and decodes the
shipped `Saves\scene\MeshedVR\default.jpg` to `512x512 Format24bppRgb` against the `unityjit` copy. The
same probe, compiled against `2.0.0.0` and resolved by the 4.x assembly, still decodes - which is why
the swap cannot strand `Bass.Net` or `NAudio`.


## Deviations from the decompiled original

Everything in `Assets\Scripts` is the decompiler output, with two deliberate exceptions, both about
where asset bundles come from. In the shipped game `AssetBundleManager` resolves bundles through
`Application.streamingAssetsPath`, which in the editor points *inside* `Assets`; that would drag
16.47 GB of bundles into the AssetDatabase, and the manifest bundle name came from
`Utility.GetPlatformName()`, which returns `null` on any editor platform - so bundling was build-only
in the original and cannot work in the editor as written.

| File | Change | Why |
|---|---|---|
| `AssetBundles\Utility.cs` | `case RuntimePlatform.WindowsEditor:` added to the `StandaloneWindows64` branch | `GetPlatformName()` returned `null` in the editor, and the manifest bundle really is named `StandaloneWindows64` |
| `AssetBundles\AssetBundleManager.cs` | dead `GetStreamingAssetsPath()` replaced by `GetStreamingAssetsDirectory()`; in the editor it returns `<project>\StreamingAssets`, in a player `Application.streamingAssetsPath` | keeps bundles out of `Assets`, and keeps player behaviour byte-identical |
| `SuperController.cs` (`InitAssetManager`) | uses `AssetBundleManager.GetStreamingAssetsDirectory()` | same |
| `MeshVR\GlobalSceneOptions.cs` (`LoadAssets`) | uses `AssetBundleManager.GetStreamingAssetsDirectory()` | same |

The editor path is a directory junction (`scripts\New-StreamingAssetsLink.ps1`, `-Remove` to take it
down) pointing at `VaM_Data\StreamingAssets`. It sits next to `Assets`, not inside it, so Unity never
imports it - the 261 bundles stay where the game put them and the project stays 247.8 MB.

The player's copy of that link lives *inside* the player's data folder, which a build rewrites, so
every player build takes it down. `scripts\Invoke-PlayerBuild.ps1` runs `New-PlayerRuntimeLinks.ps1`
again once the build succeeds, because without the link the player boots to the splash and then stops
on a grey screen: the bundles and the scene list are all behind it.

This junction is what makes the wiping in `scripts\Setup-RebuildProject.ps1` dangerous: PowerShell 5.1's
`Remove-Item -Recurse` deletes the files *behind* a junction rather than the link, so the script unlinks
`StreamingAssets` before it clears `VaM_Rebuild\` and relinks it afterwards. Never clear that folder by
hand while the junction is up - `scripts\New-StreamingAssetsLink.ps1 -Remove` first.

`Launcher.cs` sets `SettingsManager.APP_PATH = Directory.GetParent(Application.dataPath).FullName`, so
`saves`, `Custom`, `AddonPackages` and friends resolve to the *install* root in a player and to the
project root in the editor. `FileManager` does not even use that: it filters its package list with
`Directory.GetFiles("AddonPackages", "*.var", AllDirectories)`, a path relative to the *process*
working directory. In the editor that is `VaM_Rebuild\`, so `FileManager.Refresh()` quietly created an
empty `AddonPackages` there and reported `Scanned 0 packages` - against the player's `Scanned 9
packages`, which is what the package-changed branch and the extra `vamX` probe in the boot log hang
off.

`scripts\New-RuntimeDataLinks.ps1` closes that gap: `AddonPackages` becomes a junction to the
install's (0.84 GB, read-only, so a copy would only waste disk), while `AddonPackagesUserPrefs` -
which the editor writes - is *copied* instead, so a session cannot modify the original install. With
both in place the rebuild prints `Scanned 18 packages` and `Package changes detected`, like the
player. `-Remove` takes the junction back down.

The same script also settles the graphics preset, because it is the one setting that decides what a
comparison against the installation is even comparing. The game reads `prefs.json` relative to the
process working directory (`UserPreferences.RestorePreferences`), so the editor applies the project's
copy and a player applies the installation's, and the two copies were not the same preset: the
installation's five graphics keys are the `High` preset of `UserPreferences.QualityLevels` to the
digit, and the project's were `Max` with `msaaLevel` raised by hand from the preset's 2 to 8. Two of
the differences flatter the skin in a way that looks like a shading defect - `pixelLightCount` is
Unity's per-pixel light budget, and `smoothPasses` is the number of Laplacian smoothing passes
`DAZSkinV2` runs over the body before it rebuilds the normals - so the script now compares the nine
graphics keys and rewrites the ones that differ, printing each one rather than doing it silently.
Pass `-MatchGraphicsPrefs:$false` to only report. The play report carries the same information from
the other side: `RebuildGate.PresetDump` prints the resolved `QualitySettings`, the applied
`UserPreferences` values and the graphics keys of the `prefs.json` it found, so a report always says
which preset produced it.

User content created during editor Play mode otherwise lands in `VaM_Rebuild\`, not in the game
folder; junction any of the other directories the same way if the original content has to be visible
to an editor session.

## Known gaps

- The three SteamVR references above.
- `ConvexDecompositionDll.dll` ships in the build but no managed assembly references it by name. If
  collider or rig generation fails at runtime, that file is the first suspect - it may be loaded by
  a plugin inside an asset bundle rather than by the game assemblies.
- CEF discovery was untested, and the first scene with a browser settled it: the player crashed with
  `0xc0000409` in `mono-2.0-bdwgc.dll` until the build started staging the CEF payload and writing
  `browser_assets` (see "Native plugins" above). That fix is built but has not been through a hand
  load yet. In the **editor** the same gap is still open, and it is structural rather than a missing
  copy: `ZFBrowser` looks for both the payload and the index in the immediate children of
  `Application.dataPath` - `Assets\Plugins\` and `Assets\Resources\browser_assets` - while this
  project keeps the runtime one level deeper, in `Assets\Plugins\x86_64`, so an editor session logs
  `FileNotFoundException: ...\VaM_Rebuild\Assets\Resources\browser_assets` (`artifacts\smoke-look.log`)
  and finds no CEF either. Closing it means a second copy of 182 MB under `Assets\Plugins`, or a
  junction for it.
- `Assets\Scripts\Assembly-CSharp` compiles as the predefined assembly, so it cannot be unit-tested
  in isolation. That is intentional: the original build has the same shape.
- Asset bundles are not part of the project; the editor reads them from a `<project>\StreamingAssets`
  junction (see "Deviations from the decompiled original" above). Without that junction Play mode
  starts but every level, character and prop fails to load.
- 2 433 of 2 872 `.meta` files are unreferenced, i.e. the export contains assets no scene mentions.
  Harmless, and pruning them is not worth the risk.
- The editor logs `Unloading broken assembly` for `Assets/Plugins/RTTypeModel.dll`. **This is
  noise.** The editor prints the identical line about its own
  `UnityEditor.UI.dll`, and the assembly does load:
  `Assembly.Load("RTTypeModel")` succeeds and `ProtobufSerializer`'s static constructor - the only
  thing that consumes it - runs without throwing. `ReflectionOnlyLoadFrom` says `v2.0.50727`, so it
  is the pre-4.0 runtime version in the metadata that provokes the message, not a real failure.
  It also references `Assembly-CSharp`, which is circular in the editor and would be if the
  assembly were ever rebuilt; leave the shipped DLL alone.

## Behaviour under the batch harness

The rebuild reproduces the original's boot log line for line - all 20 messages, in order, with nothing
extra on either side. Two details of the comparison are worth knowing before trusting that number:
`PerfMon`'s `Benchmark complete. …` line only appears when a frame actually ends (see the
`-nographics` note below), and the numbers inside it differ every run, so
`scripts\Compare-BootLogs.ps1` normalises them.

Anything that touches `GPUTools` (cloth, hair, colliders) must **not** be run with Unity's
`-nographics` flag. Those systems are compute-shader based, and with no graphics device every
variant fails to load, `ComputeShader.FindKernel` returns `-1`, and
`GPUCollidersManager.FixedUpdate()` throws every physics frame - which reads exactly like a broken
rebuild. The compute shader assets themselves are fine (`Assets\Resources\compute\*.asset` are
proper serialised `ComputeShader` objects with real DXBC blobs). `Invoke-SmokeTest.ps1` is windowed
for `Play` by default and offers `-Headless` only for the gates that do not need a device. No frame
ends without a device, so `-nographics` also silences the benchmark line.

`-batchmode` creates no window at all, so a batch play run is invisible however it is configured.
`scripts\Invoke-SmokeTest.ps1 -Method Play -Visible` drops `-batchmode`, opens the editor normally and
lets `RebuildGate.Play` bring the Game view to the front before entering play mode; the run, the
report and the self-exit are otherwise the same.

## Shaders are placeholders

The one part of the game that did not come across in a usable form. All 128 `.shader` files under
`Assets` are AssetRipper placeholders carrying `//DummyShaderTextExporter`: no `StructuredBuffer`, no
lighting, `_Color` only, and positions taken from the mesh's own `POSITION` attribute. The export
produced them despite `ShaderExportMode = Decompile` (`docs\asset-export.md:48`), because VaM ships
shader bytecode rather than source and the export holds no shader blobs to decompile.

Consequences worth knowing before reading any visual result: the compute shaders *are* real
(`Assets\Resources\compute\*.asset`, 140 of them, with real DXBC blobs) so the GPU skinning, cloth and
collider systems run; but anything drawn through one of the placeholders renders flat, and the
character's body also renders in its bind pose - it deforms correctly on the GPU and then has that
result bound into a shader that has no vertex buffer to receive it. See *The body defect* in
`docs\verification.md` for the evidence and the routes out.

What the placeholders *do* preserve is worth having: every stub keeps its shader's real property list
(with names, types, ranges and defaults) and its `Fallback` chain, because AssetRipper rebuilt them
from the compiled reflection. That is what makes a replacement shader a tractable job rather than a
guess - the property names the materials expect are known.

