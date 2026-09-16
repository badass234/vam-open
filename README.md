# VAMOpen 0.2.0-alpha

Virt-a-Mate never opened its source code and has been stuck on Unity 2018.1.9f2 forever, while the
developers just keep taking money and releasing tiny fixes for years.

VAMOpen is an attempt to bring Virt-a-Mate back to life.

It's reverse engineering, reassembled back into a Unity project, with shaders recovered from the
bytecode that ships in the release build. I don't redistribute the game itself. The repository only
contains the recovered code and the pipeline, and you point it at your own installation.

## What already works

- Compiles and runs.
- Scenes, character, skin, hair load.
- Clothing renders correctly, layers are fine.
- Physics
- UI
- Some shaders
- Post-processing

Also, the shading model of the drawn body has been compared with the released build's own shader
bytecode, instruction by instruction, and matches it: the tangent frame, the normal map, the albedo
override, the gloss and bump offsets, the highlight exponent and Fresnel curves, the SH ambient, the
reflection, the direct light and the final composite. That is what 0.2.0 adds over 0.1.1 - not a new
feature, but the ground the next rounds stand on.

## What's still in progress

- Minor character material defects: characters still shine a little more than the original does.
  The shipped bytecode rules the shader out, so the next round looks at what is bound to it - the
  specular IBL cube's import colour space, the specular/fresnel values the material carries, and the
  bloom threshold. The list is in `CHANGELOG.md` under *Round 4*.
- Many shaders not ported yet.
- Some shaders
- Code is partially readable but needs refactoring.

## Plan

First, get the project to a state where 95% of basic functions work stably.
Then gradually move everything to the latest Unity version.
And after that, add modern stuff: proper Anti-Aliasing, new shaders (subsurface scattering, HDAO,
etc.), then DX12 and Vulkan, add at least basic multithreading so physics doesn't lag when you add
just a couple of characters to the scene, then FSR 4, DLSS, Ray Tracing, and so on.

The hardest part here is the engine migration. Everything else after that is much easier.

## How to try it

You need Windows, Unity 2018.1.9f2, and your own Virt-a-Mate installation. Keep Unity Hub closed,
otherwise it will reissue the license file and the 2018.1 editor will reject the reissued one.

```powershell
scripts\Setup-RebuildProject.ps1
scripts\Invoke-CompileGate.ps1
python scripts\Extract-VaMShaders.py --out artifacts\shader-blobs
python scripts\New-VaMShaders.py
python tools\check_shaders.py
scripts\Invoke-ManualPlay.ps1
```

If the editor comes up showing `Failed to load window layout`, play mode never starts: an editor that
was killed rather than closed leaves an empty or stale `LastLayout.dwlt` in
`%APPDATA%\Unity\Editor-5.x\Preferences\Layouts`, and Unity needs it gone before it will write a
default one.

Stage details are in `docs\`, the source of truth for status is in `CHANGELOG.md`.

## Layout

| Path | What it is |
|---|---|
| `src\Assembly-CSharp` | the game's code as ilspycmd decompiled it, ~2800 `.cs` files |
| `src\Assembly-UnityScript` | the game's second assembly (13 files) |
| `src\RTTypeModel` | the type model the decompiler needs |
| `shader-src\VamGpuSkinning.cginc` | the reconstructed GPU-skinning shading library (see [`docs/shader-reconstruction.md`](docs/shader-reconstruction.md)) |
| `VaM_Rebuild\Assets\Editor\RebuildGate.cs` | the only code written by hand: a batch gate that boots the game and prints a verdict |
| `VaM_Rebuild\ProjectSettings`, `VaM_Rebuild\Packages` | Unity project settings, including the .NET 4.x scripting runtime the decompiled code needs |
| `scripts\` | the pipeline: project setup, shader extraction and generation, compilation gate, smoke runs, log comparison |
| `tools\` | standalone analysers (asset GUIDs, API surface, IL tokens, Unity logs, frame comparison, shader pre-flight) |
| `docs\` | per-stage reports: asset export, project rebuild, editor, verification, parity, shader reconstruction |
| `CHANGELOG.md` | what each release contains: what works, what does not, what is known broken |

## Paths

The repository holds no machine-specific path. Everything the scripts need from outside is a
parameter with a portable default:

- **The game installation** (`-InstallRoot`) is resolved as: the `-InstallRoot` argument, else
  `$env:VAM_INSTALL`, else the parent directory of this repository - used only if it really looks
  like a Virt-a-Mate install (it must contain `VaM_Data\Managed\Assembly-CSharp.dll`). That last
  fallback matches the usual layout, where the repository sits inside the installation directory.
  If none of the three applies, the script stops with a message instead of guessing.
- **The Unity editor** (`-UnityExe`) defaults to
  `%ProgramFiles%\Unity\Hub\Editor\2018.1.9f2\Editor\Unity.exe`. Pass `-UnityExe` for a different
  installation, including one outside Unity Hub.
- **Everything inside the repository** is resolved relative to the script's own location, so the
  scripts work from any working directory. Beware that PowerShell's `Set-Location` does not move the
  process's current directory, which matters for the Unity runs; the runners handle it themselves.

## Diagnostics

Before trusting any diagnostic run, call `-Method Report`. It prints which assemblies and scenes are
loaded and, most importantly, which of the temporary `[DIAG ...]` probes are actually present in the
loaded `Assembly-CSharp`: `RebuildGate.ReportScriptMarkers` reads method bodies through reflection
and looks for each probe's string literal. Unity is free to reuse an already built script assembly,
so instrumented code can sit on disk, compile, and still never run - and a silent probe looks
exactly like a negative result. This check tells the two apart.

The probes themselves are marked `TEMP DIAGNOSTIC` in the sources and are removed once the defect
they investigate is fixed.

## Where help is needed

The two character material defects are the smallest and most visible bugs left. After that, porting
the remaining shaders is mechanical work with real payoff: it's what will let the build run without
the original installation. If you have experience with D3D11 shaders, Marmoset IBL, and
post-processing are the two places where it matters most.
