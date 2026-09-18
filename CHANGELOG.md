# Changelog

Notable changes to **OpenVaM**, the open rebuild of Virt-a-Mate. The project is in alpha, so the version's
last number counts patches inside the `0.1` line while the first two stand still. Each section is short by
design - the version history is meant to be readable - and the detail behind it, with the measurements, is
in [`docs/release-notes.md`](docs/release-notes.md).

## 0.1.8-alpha

The engine is **Unity 2018.4 LTS** now (`2018.4.36f1`), hopped from the `2018.1.9f2` the game ships with by
the editor's own API Updater, with 2019.4 to come. The code needed two decompiler artifacts repaired and one
enum that went obsolete, and the compile gate is `verdict: OK` at 0 unique errors. The hop rewrote two
tracked project files, and it cost `ZFBrowser.dll`, which the runtime now refuses - the embedded browser is
the open question and is being measured on a built player. `MacGruber.Breathing` fails one step earlier,
still caught. What the hop settled: the **white iris on the `Male 1` skin is gone**.

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
