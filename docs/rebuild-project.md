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
  ProjectSettings\                   23 .asset + ProjectVersion.txt (2021.3.45f2)
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
there. `com.unity.timeline` was considered and rejected on
evidence: `UnityEngine.Timeline` is referenced by 0 files in `src\` (only the builtin
`UnityEngine.TimelineModule`, covered by `com.unity.modules.director`, is used).

**The 2018.4 editor added six entries by itself, on first open**: `com.unity.ads` 2.0.8,
`com.unity.analytics` 3.2.3, `com.unity.collab-proxy` 1.2.15, `com.unity.package-manager-ui` 2.0.13,
`com.unity.purchasing` 2.2.1 and `com.unity.textmeshpro` 1.4.1 - plus the trailing newline the file
never had. That is 2018.4's own default set for a project that declares no hold on it, so the hop
wrote it into a tracked file. `com.unity.textmeshpro` is genuinely load-bearing - it is the package
that compiles `Library\ScriptAssemblies\Unity.TextMeshPro.dll`, and no TMP copy exists under
`Assets\Plugins`. The same first open also created `ProjectSettings\VFXManager.asset`, a settings
file 2018.1 had no counterpart for.

**The 2019.4 editor then moved the file in two ways: one entry had to go, and uGUI had to be pinned.**
`com.unity.package-manager-ui` 2.0.13 was written against 2018.4's UI Elements
(`UnityEngine.Experimental.UIElements`, a namespace that stopped being experimental in 2019), so it no
longer compiles: **548 of the 612 `error CS` lines of the 2019.4 first open are its own**, all of them
`Experimental.UIElements` lookups. It is removed rather than patched - it is the *editor's* package
manager window, not part of the runtime this project builds. In its place `com.unity.ugui` 1.0.0 is now
an explicit entry: from 2019.2 Unity UI is a Package Manager package, and `Editor\Data\UnityExtensions\`
in 2019.4 no longer carries a `Unity\GUISystem\` folder at all. Before the pin the 309 files under
`src\` that use `UnityEngine.UI` resolved only transitively, through `com.unity.purchasing` - a
dependency nothing in this project controls; `Packages\packages-lock.json` now records
`com.unity.ugui` at `depth: 0` where the transitive route had it at `1`. The rest of the 2018.4
default set is kept exactly as the editor left it, and the compile gate plus a hand run are green in
that state.

**The 2020.3 editor moved the same file again, and this time the manifest owns the one log line the hop
costs.** Six pins were rewritten - `com.unity.ads` 2.0.8 → 4.4.2, `com.unity.analytics` 3.2.3 → 3.6.12,
`com.unity.collab-proxy` 1.2.15 → 2.0.4, `com.unity.purchasing` 2.2.1 → 4.8.0, `com.unity.textmeshpro`
1.4.1 → 3.0.6, `com.unity.timeline` 1.2.18 → 1.4.8 - and `com.unity.ide.visualstudio` 2.0.18 was added,
the one entry nothing in this project asks for; the lock file gained `com.unity.ext.nunit`,
`com.unity.nuget.newtonsoft-json`, `com.unity.services.core`, `com.unity.test-framework`,
`com.unity.modules.androidjni` and `com.unity.modules.uielementsnative`. `com.unity.ugui` stays pinned at
`1.0.0`. The Purchasing bump is the one that shows: the timed Play gate's error list went from **5 lines
to 6**, and the sixth is `UnityEditor.Purchasing.ProductCatalogEditor`'s type initializer failing to load
`UnityEngine.UnityWebRequestModule` in a `-batchmode` run - an editor package this project never calls,
arriving before the module it wants, and absent from the windowed editor. It also writes
`Assets\StreamingAssets\UnityServicesProjectConfiguration.json` on every editor start, which is what the
player's link has to tolerate (see the player's runtime links below).

## The Unity editor that has to open this project

`ProjectSettings\ProjectVersion.txt` names the editor, and the scripts read *that file* instead of
hardcoding a path (`scripts\UnityEditor.ps1`), so a version hop needs no script edit. The project is
now pinned to **2021.3.45f2**, the last non-extended-LTS patch of the 2021 LTS - hop four of the ladder so
far, with 2022.3 and the 6000 line still ahead. The patch after it, `2021.3.58f1`, is extended-LTS and is
refused on a Personal licence, so the newest-looking patch was not the usable one; what the hops cost is in
[`release-notes.md`](release-notes.md), and the item-by-item audit of each hop against
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
**All three later editors accepted that same file without being re-activated** - 2018.4's log opens with
`Initiating legacy licensing module` and reports only `Next license update check is after ...`, and
2019.4 and 2020.3 import, compile and run `-executeMethod` on it unchanged. Keep Unity Hub closed -
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

The build picks its editor, and not always the one the project is on. The sources want `Stack<T>` out of
`System.dll`; 2021 moved that type to `System.Collections.dll`, and the profiles have it only under
`4.5\Facades\` as a type-forwarding facade - a build against it still fails with `CS0246: 'Stack' could not
be found` at ten sites, while adding `System.Collections` to the reference set fails outright with `CS0006`.
The reference *set* is therefore not the thing to change; the profile is. `Build-McsCompiler.ps1` probes the
project's editor with the build's own `-r:System.dll -r:System.Core.dll` and falls back to the newest
installed editor that passes, which lands on `2020.3.49f1` and produces a compiler for a 2021.3 or 2022.3
game. What the emitted assembly references is unchanged and is the part that matters: `mscorlib` 4.0.0.0,
`System` 4.0.0.0, `System.Core` 4.0.0.0 and `System.Xml` 4.0.0.0 - exactly four, re-read per assembly after
the build.

Two patches are applied to the sources. The first, in `src\mcs\Mono\CSharp\Parameter.cs`, is the one that
gets plugins compiling at all: the helper that writes a constant through `ParameterBuilder.SetConstant`
catches `ArgumentException` and `NotSupportedException` and drops the attribute instead of propagating. That
is Mono 2.0's own behaviour with the same input - the emitted parameter simply carries no default - and it is
what makes both the probe and real plugins compile. The engine still reads the default from the source,
because `DynamicCSharp` compiles the plugin for the runtime and does not depend on the reflected attribute.

The second, in `src\mcs\Mono\CSharp\TypeParameterInflator.cs`, is what a hand run on 2021.3 found. Inflating a
generic type handled a type parameter and an array through their container types and then threw
`NotImplementedException`; a **by-ref** type fell through to that throw, and the one the BCL ships is
`ReadOnlySpan<T>`'s `ref readonly T this[int]` indexer. The 2.x compiler this file was decompiled from could
never meet one - mono's `mscorlib` had no by-ref members then - so the branch was simply never written;
upstream mono has it (`mcs/mcs/generic.cs:1534`) and this copy now does too. Why it fires on plugins from
2020.3 on, and never before, is the same fact from the other side: 2020.3 seeds the language with
`ReadOnlySpan<char>`, so the overload sets of `StringBuilder.Append` and `int.Parse` carry a `Span` overload
each, and resolving an ordinary `b.Append(message).Append("\n")` walks into a by-ref member and throws. What
comes back is `[CS584] Internal compiler error: The method or operation is not implemented. in <Unknown> at
[687, 6]` - the address of that very `Append` in `MacGruber.Utils.LogTransform` - with an empty error list
under it, because `Report.Error` throws `FatalException` the moment `ErrorsCount` reaches
`settings.FatalCounter`, so the plugin's own errors are never written. That is the same shape as the
`SetConstant` defect above: the compiler dies, and the only symptom is a plugin that does not load.

The pair is separated by an A/B in `artifacts\mcs-ab\`, with the game's own `mcs.dll` as the control arm and
the reference set taken from `VaM_Data\Managed` - the one place the plugin's first `using` resolves, since
`AssetBundles` is a namespace inside `Assembly-CSharp.dll` and in no UnityEngine module. The control drives
`Life_Internal_MacGruber_Utils.cs` to `(687,6): error CS0584: Internal compiler error: The method or
operation is not implemented.`, the in-game address exactly, and this build returns 0 errors and 0 unresolved
references. `verification.md` section 9 is the harness.

`Setup-RebuildProject.ps1` runs the build, stages the result at `Assets\Plugins\mcs.dll`, and
`Assets\Resources\DynamicCSharp_Settings.asset` keeps `mcs.dll` in its single `referenceRestrictions` entry
- that list is a blocklist, and it exists so the plugin compiler cannot be handed to itself as a reference.

The same asset carries the compiler's *reference* list, `assemblyReferences`, and it is a trap on an engine
hop. The compiler does not scan `Managed\`; it compiles against the bare file names in that list, and Mono
treats one name it cannot resolve as fatal for the whole compilation:

```
[CS6]: Metadata file `UnityEngine.Timeline.dll' could not be found
Compile of MacGruber.Life.12:/Custom/Scripts/MacGruber/Life/MacGruber_Life.cslist failed.
```

The names are looked up in `Directory.GetCurrentDirectory()`, and then in `Settings.ReferencesLookupPaths`
(`src\mcs\Mono\CSharp\AssemblyReferencesLoader.cs:10-16`) - which is why this defect looks different in the
editor and in the player. In the editor the working directory is the installation root, whose
`VaM_Data\Managed` still carries the game's own assemblies, so the names resolve; the player runs from its
own folder, resolves against *our* `Managed`, and fails. The game was built with Unity 2018.1, where Timeline
was an engine module shipped as `UnityEngine.Timeline.dll` (93,184 bytes) plus a
5,632-byte `UnityEngine.TimelineModule.dll`. On 2019.4 Timeline is the package `com.unity.timeline` and the
same API arrives in `Unity.Timeline.dll`, so two names in the shipped list could no longer be found, and
every plugin naming them failed to compile - reported only as a plugin that does not load.

The list is data, not code, so the adaptation is two lines - but `Assets\Resources` is generated from the
export and not tracked, so a rebuild would undo an edit made by hand. `scripts\Update-PluginCompilerReferences.ps1`
therefore holds the two-name table (`UnityEngine.Timeline.dll` -> `Unity.Timeline.dll`;
`UnityEngine.TimelineModule.dll` dropped, this engine has no such assembly), refuses to run unless
`Packages\manifest.json` declares `com.unity.timeline`, preserves the file's byte-order mark and line
endings, and is idempotent. It is step 7 of `Setup-RebuildProject.ps1`, and `Invoke-PlayerBuild.ps1` runs it
with `-Verify` against the player it has just built - the only Managed folder where the answer means
anything, since the installation's copy still holds the old names.

Both directions were measured in the editor on the boot scene. With the shipped list restored, the run
printed 5 `Metadata file` lines, 2 `Compile of ... failed.` and no trace of the plugin beyond its failure;
with the adaptation in place, both counts are **0**, and `MacGruber` appears 26 times as running code - so
the defect was never the player's alone, and the same A/B is the regression test for the next engine hop
(`artifacts\pluginrefs-stale.log` against `artifacts\pluginrefs-fixed.log`). What is left of that plugin on
**that** engine is its own runtime fault, not the compiler's: `FieldAccessException: Field 'MiniQueue'1:position'
is inaccessible from method 'MacGruber.Breathing/MiniQueue'1<T_REF>:.ctor ()'` - a reading of the 2018/2019/2020
Mono, absent from all 25 logs naming `2021.3.45f2`. A second compiler defect - the by-ref one above - was
standing in front of that fault, and hop four's hand run is the answer to how far the plugin then gets: it
compiles, its `Breathing` MonoBehaviour is instantiated and torn down cleanly, and neither fault appears
(`artifacts\manual-play.log` against the control `artifacts\manual-play-cs584-before.log`).

#### The override repair, and the two reads inside it

The compiler's other patched surface is not in `src\mcs` at all but in the game's own `DynamicCSharp` layer.
Hop three's first plugin compile under 2020.3 **killed the whole editor** - `requested token for MethodBuilder`
out of Mono's `mono_image_create_token`, reached from `ModuleBuilder.Save()` (`McsDriver.cs:221`) - and the
cause was exactly one plugin: `everlaster.TittyMagic`, whose iterator type contributes `MethodImpl` entries
naming a method of a generic type that is still being built. `RepairMethodOverrideDeclarations`
(`McsDriver.cs:292`), called from `Compile` (`:220`) on the writing path only (`if (!generateInMemory)`,
`:218-221`), now resolves every override declaration to a real created method through the token index mcs fills
as it emits (`CollectCreatedMethods`), so `Save()` is never handed a builder at all. The shape is repaired, not
the plugin's name.

Two reads on that path were themselves unguarded, and each cost a plugin its load before being caught.

| read | why it throws | what the repair does now |
|---|---|---|
| `MemberInfo.MetadataToken` | `MethodBuilder` never overrides the property, and the base implementation only throws `InvalidOperationException` | `ReadMethodToken` (declared `:429`) prefers the property (`:433`) and falls back to `GetToken().Token` (`:437`), the index the created method reports |
| `inflated.GetMethods(...)` (`:489`) | the instantiation is a `TypeBuilderInstantiation`, which implements that query as an unconditional `throw new NotSupportedException()`, and `ResolveBuilderType` cannot get around it because `AssemblyBuilder.MakeGenericType` answers with another one | the declaration is resolved from `base_method` through the same token index (`ResolveCreatedMethod`, `:552`), with `throw;` (`:501`) when the index has nothing - today's outcome, and `Save()` stays unreached - and a throw rather than returning a builder no created type accounts for (`:536`) |

**Catching and continuing at either site is the wrong fix**, and that is why both end in a throw rather than a
`null`. A `null` routes the entry to the `unresolved` list (`:316`, filled at `:369-373` and again at
`:390-394`, warned about at `:407-409`), leaves the builder in the overrides
array on its way there, and lets `Save()` reach the very writer state that killed the editor at hop 3. The
throw is protective.

Both were accepted on runs that carry a full plugin boot, and **the ordinary gate is one of them**: `RebuildGate`
takes no plugin flag anywhere - `Play()` calls `ArmPlay(false)` (`:635`), `ManualPlay()` calls `ArmPlay(true)`
(`:651`), the only parameter is `bool manual` (`:655`), and `LoadPlayScene()` (`:1106`, called from `:999`) has
none - so a gate run *is* a run with plugins enabled. The pair behind sections 9 to 11 is the gate's own output,
`scripts\Invoke-SmokeTest.ps1 -Method Play -Scene Saves/scene/emotion-ab/ladyclown{4,5}.json -Seconds 60
-WarmupSeconds 25`, and each log carries the census of a plugin boot (`MVRPluginManager` 84 and 80 mentions,
`DynamicCSharp` 74 and 64) plus the repair's own marker **twice** inside one scene load: `resolved 32` at `:894`,
then `resolved 75` at `:1049` (B: `:1041`). Every reachable `.prefs` root reads `"pluginsAlwaysEnabled" : "true"`
- 5 files in the install root, 18 under `VaM_Rebuild`, 17 under `artifacts\player`, none `false` - and the only
switch that could disable a package is `MVRPlugin.cs:56`'s predicate, which needs `pluginsAlwaysDisabled:"true"`.
`docs\verification.md` sections 9 to 11 are the record, and section 10's script
(`scripts\Test-PluginCompilerRepair.ps1`) is the census that replaced the hand greps. What is **not** settled
either way is whether a builder declaration answered with the created method of its generic *definition* is the
semantically right row - it is writable, and the loader refuses it (`TypeLoadException`, `Method overrides a
class or interface that is not extended or implemented by this type`), on `everlaster.TittyMagic.70` first and
on `AcidBubbles.Timeline.283` after the second read was fixed. That is one open defect in the plugins' own
shape, not two.

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
on a grey screen: the bundles and the scene list are all behind it. Since the 2020.3 hop there is one
file in the way: the bumped `com.unity.purchasing` makes Unity Services write
`UnityServicesProjectConfiguration.json` into `StreamingAssets` during the build, which means the build
replaces the junction with a real directory to do it. The script therefore forgives that one name -
anything else left in the folder still stops it - and relinks. That file is a build artifact the game
never reads.

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

## The IDE solution

Opening the rebuild in an IDE is the editor's job, not the repository's: a solution states which
assemblies Unity compiles out of which files with which references, and only the editor knows that.

```
scripts\Invoke-SyncSolution.ps1              # writes the solution at the project root
scripts\Invoke-SyncSolution.ps1 -Force       # stops a running editor first
```

`RebuildGate.SyncSolution` calls the code editor integration behind *Preferences > External Tools*
(`com.unity.ide.rider`, `com.unity.ide.visualstudio`, `com.unity.ide.vscode`) and, when that writes
nothing, the built-in Visual Studio generator the editor has carried since before those packages
existed. Nothing is added to `Packages\manifest.json` for it, so there is no package to download and
the project stays as bare as it was. Which of the two paths actually ran, and what came out of it, is
recorded as check 8 of `docs\verification.md`.

What comes out, in the project root next to `Assets`, is `VaM_Rebuild.sln` plus three project files:
`Assembly-CSharp.csproj` (all 2811 scripts), `Assembly-CSharp-Editor.csproj` and
`VaMUnityScript.csproj`. Their references are Unity's own - 620 `HintPath` entries taken from the
editor installation, from the plugins `Setup-RebuildProject.ps1` stages and from
`Assets\Plugins\RTTypeModel.dll` - so the IDE's completion, navigation and refactoring work on the
same compile-time surface the game is built from. Rider opens the file as it is.

Two project sets now exist in this repository, and they are not substitutes for each other:

- `VaM_Rebuild\*.sln` and `VaM_Rebuild\*.csproj` are **generated** and gitignored. Deleting them costs
  one command, and committing them would only make them stale, because they name the editor's own
  install path. Add a source file or a plugin and run the script again: the editor rewrites the one
  project whose file list changed and leaves the others alone, which is measured in check 8 of
  `verification.md` rather than assumed.
- `src\` holds the **tracked** projects - `Assembly-CSharp.csproj`, `Assembly-UnityScript.csproj`,
  `RTTypeModel.csproj` and `Directory.Build.props`. They are SDK-style, target `net472`, are pinned to
  C# 6 and compile against the references `scripts\Update-UnityReferences.ps1` writes into the
  gitignored `src\UnityReferences.props`. They are what the sources can be read, diffed and built with
  when no editor is running - with `.tools\dotnet\dotnet.exe`, because the machine's own `dotnet` is a
  runtime without an SDK.

So: the editor's solution is the one to open for playing and debugging, and `src\` is the one to open
for reading the code.

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

