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
