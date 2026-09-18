# Changelog

Notable changes to **OpenVaM**, the open rebuild of Virt-a-Mate. The project is in alpha, so the version's
last number counts patches inside the `0.1` line while the first two stand still. Each section is short by
design - the version history is meant to be readable - and the detail behind it, with the measurements, is
in [`docs/release-notes.md`](docs/release-notes.md).

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
depend on the editor. Compile gate `verdict: OK`, 0 unique errors, the standalone player builds and boots on
2019.4, and the item-by-item audit against Unity's 2019 LTS guide is in
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
