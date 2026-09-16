# OpenVaM - an open rebuild of Virt-a-Mate

Virt-a-Mate's original authors have moved on, and the game is frozen on Unity 2018.1.9f2 with no
source ever published. OpenVaM is the reverse engineering effort that hands the community the thing
the original release never did: the game's code and shaders, and a project that builds from them, so
the legacy core can be maintained and moved onto newer Unity versions by anyone who wants to.

So here it all is - the decompiled C# sources, the Unity project they get put back together into, and
the pipeline that compiles, boots and checks the result against the original. The game installation
is only ever read from: nothing here writes into it, and no part of the game ships with the
repository.

## Release 0.1.1-alpha

The current release is **0.1.1-alpha**: the game compiles, boots, loads a scene and renders an
animated, lit character. Hair, body skin and cloth read close to the original. Part of the
character's materials, the post-processing stage and the refactoring pass are still open.

[`CHANGELOG.md`](CHANGELOG.md) says exactly what works and what does not; how each of those
statements was measured is in [`docs/`](docs/), starting with
[`docs/verification.md`](docs/verification.md).

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

## Requirements

- Windows.
- A Virt-a-Mate installation. Nothing in the repository points at it; see *Paths* below.
- Unity **2018.1.9f2**, activated, installed through Unity Hub. Keep Hub closed while working: it
  reissues `%ProgramData%\Unity\Unity_lic.ulf`, and the 2018.1 editor rejects the reissued file -
  the details are in [`docs/unity-editor.md`](docs/unity-editor.md).
- .NET SDK, ilspycmd and AssetRipper: `scripts\Setup-RebuildProject.ps1` fetches them into `.tools\`.

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

## Reproducing

```powershell
# 1. toolchain, asset extraction, project assembly (creates .tools, work\ripped-core, Assets\<asset type>)
scripts\Setup-RebuildProject.ps1

# 2. the decompiled sources must compile through the editor: 0 errors
scripts\Invoke-CompileGate.ps1

# 3. the ComputeBuff shaders: extract the shipped bytecode, generate the shaders,
#    pre-flight them with fxc (docs\shader-reconstruction.md)
python scripts\Extract-VaMShaders.py --out artifacts\shader-blobs
python scripts\New-VaMShaders.py
python tools\check_shaders.py

# 4. boot the game and read the gate's verdict
scripts\Invoke-SmokeTest.ps1 -Method Report                    # cheap: assemblies, scenes, diagnostic probes
scripts\Invoke-SmokeTest.ps1 -Method Play -Seconds 150         # load a scene, report, exit by itself
scripts\Invoke-SmokeTest.ps1 -Method Play -Seconds 150 -Visible # the same, with a visible editor window

# 5. or skip the gates and test by hand: opens the editor in play mode on the boot scene
#    and leaves it there, with no deadline and no report
scripts\Invoke-ManualPlay.ps1

# 6. compare our boot log with the original game's
scripts\Compare-BootLogs.ps1
```

Runs are headless by default: `-batchmode` means the game renders offscreen and no window appears.
`-Visible` drops `-batchmode`, so the editor opens and the game renders in the Game view - the only
way to watch the rebuild with your own eyes.

The reference for step 4 is the log the original game writes itself, at
`%USERPROFILE%\AppData\LocalLow\MeshedVR\VaM\output_log.txt`.

## Diagnostics

Before trusting any diagnostic run, call `-Method Report`. It prints which assemblies and scenes are
loaded and, most importantly, which of the temporary `[DIAG ...]` probes are actually present in the
loaded `Assembly-CSharp`: `RebuildGate.ReportScriptMarkers` reads method bodies through reflection
and looks for each probe's string literal. Unity is free to reuse an already built script assembly,
so instrumented code can sit on disk, compile, and still never run - and a silent probe looks
exactly like a negative result. This check tells the two apart.

The probes themselves are marked `TEMP DIAGNOSTIC` in the sources and are removed once the defect
they investigate is fixed.

## Status

[`CHANGELOG.md`](CHANGELOG.md) is the source of truth for what works and what does not. In short:

- Compilation: from 1421 errors down to 0; type parity with the original assembly is 2753/2753
  ([`docs/parity-report.md`](docs/parity-report.md)).
- Booting: the game loads, `SuperController` is alive, `isLoading` falls back to false, and the boot
  log matches the original's line for line ([`docs/verification.md`](docs/verification.md)).
- Scenes and animation: `CyberDemoAlt` loads with all 17 atoms present and 1883 MonoBehaviours, and
  the character's 84-bone animation drives the skeleton.
- Shaders: the 43 shaders GPU-skinning and the shared `Custom/Subsurface` materials need are
  rebuilt from the shipped DXBC - 31 `*ComputeBuff` plus their 12 plain siblings
  ([`docs/shader-reconstruction.md`](docs/shader-reconstruction.md)); Unity compiles all 113 passes
  with 0 shader errors, and the two halves of each pair are checked program by program - 580 pixel
  passes compared, 0 with different operands or opcodes. The 92 families the project does not
  transcribe - hair, the
  Marmoset IBL set, the geometry-shader family - are read back by name from the shipped `z_sha`
  bundle at runtime (`MeshVR.VamShaderProvider`), because `Shader.Find` never sees a bundle.
- Manual testing: `scripts\Invoke-ManualPlay.ps1` opens the editor in play mode on the boot scene,
  `Saves/scene/MeshedVR/default.json`, and leaves it there. Other scenes are for the gates that
  audit them: opening several scenes in one session crashes the player.
- Next: the rest of the character's materials (a gloss and bump seam across the shoulder, the lashes
  and the eye), then the shader stubs that are still missing and the post-processing stage, then the
  refactoring pass towards readable code.
