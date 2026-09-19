# Changelog

Notable changes to **OpenVaM**, the open rebuild of Virt-a-Mate. The project is in alpha, so the version's
last number counts patches inside the `0.1` line while the first two stand still. Each section is short by
design - the version history is meant to be readable - and the detail behind it, with the measurements, is
in [`docs/release-notes.md`](docs/release-notes.md).

## 0.1.11-alpha

The engine is **Unity 2021.3 LTS** now (`2021.3.45f2`), hop four, and the first one that was not mostly
cheap: the editor opened the project, compiled it and exited 0 without a single error, but the API Updater
came with it and **rewrote six files of our own code** - the first hop where it had anything to rewrite.
Four renames, all of them mechanical: `Texture2D.Resize(a, b)` became `tex.Reinitialize(a, b)` at three
sites, `TextureFormat.ASTC_RGB_6x6` became `ASTC_6x6` at two, and the particle collision and trigger modules
lost `maxPlaneCount`/`maxColliderCount` in favour of `planeCount`/`colliderCount` at seven sites each. The
updater cannot see behaviour, and it did not see the one real regression of the hop: `DAZImport` reads
`HKCU\Software\DAZ\Studio4\NumContentDirs` at start-up, that key does not exist on most machines, and the
2021.3 Mono runtime returns `null` for a missing registry value where the older ones returned the default -
so the cast threw `NullReferenceException` twelve times per character init. One null guard fixes it, and the
Play gate then returns exactly what the 2020.3 baseline returns: **5 errors, 0 exceptions**. Two more things
moved without being asked: `Assembly-CSharp.dll` shrank from 6 079 488 to 5 510 656 bytes with 0 errors,
because 2021.3 compiles against Roslyn reference assemblies, and `ZFBrowser.dll` stopped being discarded as
a broken assembly - it now loads and reports the 16 fields it cannot resolve, because the `UnityEngine.XR.XRNode`
enum it was compiled against left `UnityEngine.VRModule` for `UnityEngine.XRModule` back in 2020.1. Those are
warnings, not errors, and the plugin was already unusable. The item-by-item audit against Unity's 2021 LTS
guide is in [`docs/unity-upgrade-audit.md`](docs/unity-upgrade-audit.md).

The hand run on that hop then found a defect in **this project's own plugin compiler**, which no gate could
have shown: five loads of the boot scene each logged `[CS584] Internal compiler error ... at [687, 6]` and
`Compile of MacGruber.Life ... failed.` with an **empty error list**, so a community plugin stopped loading
without naming a reason. `[687, 6]` is an ordinary `b.Append(message).Append("\n")`; `StringBuilder.Append`
carries a `ReadOnlySpan<char>` overload from 2020.3 on, overload resolution reaches `ReadOnlySpan<T>`'s
`ref readonly T this[int]` indexer, and `TypeParameterInflator.Inflate` threw on the by-ref type - a branch
upstream mono has (`mcs/mcs/generic.cs:1534`) and the 2.x compiler `src\mcs` was decompiled from never needed.
It is fixed, and it was proven before deployment with the game's own `mcs.dll` as the control arm: on the real
`MacGruber` file the control returns `(687,6)`, the in-game address exactly, and this build returns 0 errors
against the same 63 references. Building the compiler also stopped being possible under 2021.3 - `Stack<T>`
left `System.dll` for `System.Collections.dll`, whose profile copy is only a type-forwarding facade - so
`Build-McsCompiler.ps1` now probes the project's editor and falls back to the newest editor that can build it
(`2020.3.49f1`), which is how this 2021.3 project ships a working plugin compiler. The harness is
`artifacts\mcs-ab`, the chain is in *The plugin compiler* in
[`docs/rebuild-project.md`](docs/rebuild-project.md), and the check is section 9 of
[`docs/verification.md`](docs/verification.md).

The same hand run then found a second defect, this time in the repair the previous hop had left behind, and it
is the one that had been failing plugins silently. `RepairMethodOverrideDeclarations` read each declaration's
token through `MemberInfo.MetadataToken`, which `MethodBuilder` never overrides - the base implementation only
throws - so the read raised `InvalidOperationException` on every declaration, a `catch` inside the loop
swallowed it, and the repair returned having changed nothing while the plugin failed with an empty error list.
`ReadMethodToken` now prefers the property and falls back to `GetToken().Token`, the builder's metadata index,
and the case is measured as a pair against the run before it: `get_MetadataToken` frames 2 → 0,
`[CS]: System.InvalidOperationException` 2 → 0, the repair's marker once reading 32. The second unguarded read
in the same repair was resolved with it, and by the same kind of measurement: the instantiation it enumerates is
a `TypeBuilderInstantiation`, whose `GetMethods` throws by construction and which no `MakeGenericType` call can
be routed around, so the override declaration is now resolved from the created methods by token instead - with
a rethrow when the index has nothing, because the catch-and-continue alternative would leave the builder in the
overrides array and hand `Save()` the writer state that killed the editor at hop 3. On the same pair the
defect's blocks go 1 → 0 and the repair's marker from once to twice (32 and 75 declarations), which is one
plugin finishing where it used to abort. The ordinary gate does repeat the measurement rather than needing hand
runs: `RebuildGate` carries no plugin flag at all - `Play()` and `ManualPlay()` differ only in `ArmPlay(bool
manual)` (`:655`), and `LoadPlayScene()` (`:1106`) takes none - so the marker *pair* in the gate's own
`artifacts\emotion-repro\A4-ladyclown4.log` and `…B5-ladyclown5.log` (`resolved 32` then `resolved 75`, inside
one scene load) is the repeat. Section 10 of [`docs/verification.md`](docs/verification.md) is the script that
reads it and section 11 the record.

The built-in browser works in the editor now, and it is the one open question `0.1.8-alpha` left behind.
Three independent defects kept it dead, and all three are **editor-only** - a player build never
had them, because `RebuildPlayer.cs` already stages the CEF payload and writes the 14-byte `browser_assets`
index, and a player's plugin folder is the one the shipped `FileLocations` looks in. `FileLocations` joins
`Application.dataPath + "/Plugins"` for the payload, the locales and the subprocess, while this project keeps
the 65-file runtime one level deeper, in `Assets\Plugins\x86_64`, because that is where the game ships it -
which is `DllNotFoundException: ZFBrowser failed to load …\Assets\Plugins\ZFProxyWeb.dll`.
`StandaloneWebResources.LoadIndex()` reads `Application.dataPath + "/Resources/browser_assets"` unconditionally
and this project has no such file - which is `FileNotFoundException … browser_assets`. And `UserPreferences`
declares both whitelist files with no directory, so they resolve against the process working directory, an
empty set refuses every host, and the browser reports `… which is not on whitelist`. Two files close all three.
`src\Assembly-CSharp\BrowserNativePaths.cs` is an editor-only `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`
hook that rewrites the cached `FileLocations.Dirs` at the staged folder, points the CEF log at
`Library\browser.log` and writes the index, rather than duplicating 182 MB of CEF into `Assets\Plugins` - a
deliberate deviation from the decompiled original, the same shape as the `StreamingAssets` junction. And
`scripts\New-RuntimeDataLinks.ps1` seeds `whitelist_domains.json` from the installation, its seventh and
narrowest copy of that kind, because the list belongs to the user rather than to this repository. The
acceptance is a same-engine, same-scene pair against the pre-fix control, every cause with its own predicate
reading 1 → 0: `ZFProxyWeb`, `DllNotFoundException`, `browser_assets`, `Could not find file` and
`which is not on whitelist` all go 1 → 0, and `VRWebBrowser` 4 → 0 is its four *failure* frames going away
rather than the component being absent, because its only two log sites print on an exception and on a refusal.
The zeros are kept honest from the other side: the scene's own browser panel, `WebPanelEmissive`, goes 0 → 1
and so does `RebuildGate OK`. Three artifacts of the candidate are not logs at all - its 14 written bytes are a
length-prefixed `zfbRes_v1` and an `int32 0`, exactly the format ZFBrowser's own `LoadIndex()` reads back;
CEF's own `Library\browser.log` runs 2 min 1 s from `zfb_init` to `zfb_shutdown` at a path only this hook
produces; and it shuts down one second before the Unity log closes. Section 13 of
[`docs/verification.md`](docs/verification.md) is the record.

The ladder also refuses two third-party plugins, and both are the plugin's own defect rather than ours, so both are
delivered as new revisions and neither touches `src\`. Decal Maker's `GetResource` opens with
`new Texture2D(1, 1, TextureFormat.DXT5, linear)` - a 1x1 **compressed** texture, and 1 is not a multiple of 4.
2020.3 tolerated the call and let the GPU upload fail instead; 2021.2 moved the check into the constructor, so the
plugin threw on its first cache miss and its skin-image path never ran. `scripts\New-DecalMakerPatch.ps1` copies all
46 entries byte-for-byte and rewrites that one line to `4, 4` - one compressed block, and the size the shipped game
uses for the same idiom - writing `Chokaphi.DecalMaker.38.var` beside the shipped `.37`, whose bytes are untouched.
The acceptance reads per block rather than per line, because the benign `GetCurrentGPUTexture` blocks are matches
too: `Failed to create texture` 2 → 0, the rule's own text 1 → 0 and `GetResource` frames 6 → 0, while
`UpdateSkinImage` and `GetCurrentGPUTexture` go 1 → 8 and 0 → 8.

The second delivery is E-Motion, whose `EmotionEngine.cs:763` holds
`private static float breathRate = Random.Range(0.2f, 0.3f);` - the only `Random.` call in the 179-entry package
outside an ordinary method body, and a static field initializer runs inside `GameObject.AddComponent`, where the
engine refuses it. The `.cctor` throws, every later touch of the type re-raises the failure, and `EmotionEngine` is
`partial` across 34 of the package's 36 script entries, so one dead field stopped a whole plugin.
`scripts\New-E-MotionPatch.ps1` rewrites that one line to `0.25f` - the midpoint of the range it was drawn from,
and lossless, because the field has no reader anywhere in the package - writing `VRAdultFun.E-Motion.5.var` under
exactly the same rule: 179 entries byte-for-byte, one rewritten, net -19 bytes. Its pair is one scene run twice
with the version token as the only difference: `Range is not allowed` 5 → 0, `TypeInitializationException` 6 → 0,
and the plugin's own `RegisterUIElements` and `Init` 0 → 4 each. Neither delivery reaches a scene that named the
old revision, because `FileManager.GetPackage` resolves a version-qualified uid exactly - so the shipped revisions
stay untouched and repointing a scene's own version token is the consumer's edit.

## 0.1.10-alpha

The engine is **Unity 2020.3 LTS** now (`2020.3.49f1`), the third hop from the `2018.1.9f2` the game ships
with, and the first one that needed a code change rather than a rewrite: the editor's API Updater answered
this hop in **34 errors, then 1, then 0**, against 612 the hop before. `UnityEngine.UI.Graphic` stopped
declaring `[RequireComponent(typeof(CanvasRenderer))]` in 2020.1, so the four user-written `Graphic`
subclasses here declare it; `Material.SetBuffer` gained a `GraphicsBuffer` overload, which turned six
hair-rendering calls into genuine ambiguities until each `null` was cast; the `XRDevice` sites the 2020 guide
names moved to `XRSettings` and `InputDevices`; and the setup script now stages the game's own `Boo.Lang.dll`,
because 2020.2 ships no Boo/UnityScript compiler to fall back on. One real find came with the hop: **the first
plugin compile under 2020.3 killed the whole editor**, `requested token for MethodBuilder` out of Mono's
`mono_image_create_token`. The cause was a single plugin, `everlaster.TittyMagic`, whose iterator type
contributes 2 `MethodImpl` entries that name a method of a generic type still being built - a shape Mono's
token creator has no case for, so it raises a non-continuable error instead of failing the one plugin. It was
diagnosed without the editor, with `mcs.exe` and a harness across every plugin source, and fixed in this
project's copy of the compiler: the module's overrides table is resolved onto the real `MethodInfo` of the
created generic instantiation before `Save()`. The scene that aborted the compile now loads and renders
(`resolved 32`, Lady Clown at ~186 FPS), the boot scene returns hop 2's own reading (`18/18 atoms`), the
standalone player builds and boots on 2020.3, and the plugin compiler rebuilt from source is still
**byte-identical** to the 2018.4 and 2019.4 builds. The item-by-item audit against Unity's 2020 LTS guide is
in [`docs/unity-upgrade-audit.md`](docs/unity-upgrade-audit.md).

## 0.1.9-alpha

The engine is **Unity 2019.4 LTS** now (`2019.4.41f2`), the second and last hop from the `2018.1.9f2` the game
ships with, and again the editor's own API Updater made it. Two engine types the game still used were
removed in 2019.1 - `MovieTexture` and `GUIElement` - so their entries leave the save/load registry while
their surrogate classes stay behind as carriers of the serialisation they own, and the three
`Graphics.DrawProceduralIndirect` calls in the two depth-of-field effects become the immediate form Unity
renamed them to. One pinned package had to go: `com.unity.package-manager-ui` 2.0.13 is 2018.4's copy of the
editor's own package manager, and under 2019.4 it is what produced **548 of the 612 error lines** of the
first open. `com.unity.ugui` had to be pinned explicitly, because 2019.2 moved Unity UI out of the built-in
editor extensions and into a package. The **plugin compiler is rebuilt from source under the new editor and
comes out byte-for-byte identical** to the 2018.4 build (same size, same SHA-256), so that recipe does not
depend on the editor. The hop also moved Timeline out of the engine and into a package, which the runtime
compiler does not see on its own: it compiles against a fixed list of assembly names in
`Assets\Resources\DynamicCSharp_Settings.asset`, and one name it cannot resolve is fatal for that whole
compilation, so every plugin naming Timeline silently did not load. Two names are adapted for the package
form (`scripts\Update-PluginCompilerReferences.ps1`, step 7 of the project setup); measured on the boot
scene, **5 log lines naming the missing file and 2 failed plugin compiles without the adaptation, 0 of
each with it**, and the plugin whose compilation failed now runs. The editor window and the batch gates were
also on different asset import pipelines, because 2018.1's `EditorSettings.asset` carries no key for one and
the entry point decides: the window ran **V2**, every batch run **V1**, and one `Library` held both databases
(`assetDatabase3` and `ArtifactDB`/`SourceAssetDB`). The project now writes the key the engine really
serialises - `m_AssetPipelineMode: 1`, the numbers of `UnityEditor.AssetPipelineMode`, not the
`m_AssetPipelineVersion` these notes had carried - and both paths report `Using Asset Import Pipeline V2.`
Compile gate `verdict: OK`, 0 unique errors, the standalone player builds and boots on 2019.4, and the
item-by-item audit against Unity's 2019 LTS guide is in
[`docs/unity-upgrade-audit.md`](docs/unity-upgrade-audit.md).

## 0.1.8-alpha

The engine is **Unity 2018.4 LTS** now (`2018.4.36f1`), hopped from the `2018.1.9f2` the game ships with by
the editor's own API Updater, with 2019.4 to come. The code needed two decompiler artifacts repaired and one
enum that went obsolete, the compile gate is `verdict: OK` at 0 unique errors, and the standalone player
builds and boots. The **plugin compiler is rebuilt from source** - the `mcs.dll` VaM ships aborts on any
plugin with a defaulted nullable value type under this project's .NET 4.x profile, taking the whole
compilation with it, as it already did before the hop. Three more consequences: `ZFBrowser.dll` is refused by
the runtime, so the embedded browser is the open question; `MacGruber.Breathing` fails one step earlier,
still caught; and the **white iris on the `Male 1` skin is gone**. The setup script no longer reverts the
hop: it used to put `ProjectSettings` and `Packages` back from the 2018.1 export, so every gate after a
setup ran the old editor.

## 0.1.7-alpha

Everything since the first alpha. **Clothing renders in its own colour** - the generator read the render
state's colour-write mask backwards, so the transparent families never wrote red. **Hair, lashes and the
eye are transcribed** from the shipped programs, and character materials draw with this project's shaders
rather than the bundle copies. **Self-shadowing** is VaM's own point-light filter, decoded from the released
bytecode. In the built player, **scene previews decode again**, a plugin the installation cannot run no
longer floods the log, and there is an **in-game screen resolution setting**. The harness was repaired too:
`Sync-Sources.ps1` byte-verifies what Unity actually compiles.

## 0.1.0-alpha

First public alpha: the code compiles, boots, loads a scene and renders an animated, lit character; skin and
hair read close to the original, the cloth does not. **Works**: `Assembly-CSharp` at **1421 errors down to
0**, parity **2753/2753**, the boot log matching the game's own, and 43 of the 135 shader families rebuilt
(**3023/3023 programs**). **Does not work yet**: cloth, the shoulder/back seam, the lashes and the eye, 92
of the 135 families, post-processing stubs; physics, UI and other scenes untested. **Requirements**:
Windows, Unity **2018.1.9f2**, your own installation.
