# VaM (Virt-a-Mate) - reverse engineering and rebuild

This is the Virt-a-Mate reverse-engineering project: the point is to give the community something to
work with, so that the old, legacy core can be upgraded onto the newer Unity versions of the game.

So here it all is - the decompiled C# sources, the Unity project they get put back together into, and
the pipeline that compiles, boots and checks the result against the original. The game installation
is only ever read from: nothing here writes into it, and no part of the game ships with the
repository.

Where the work stands, how it got there, and what is still broken: [`docs/verification.md`](docs/verification.md) and
the rest of [`docs/`](docs/).

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

# 5. compare our boot log with the original game's
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

- Compilation: from 1421 errors down to 0; type parity with the original assembly is 2753/2753
  ([`docs/parity-report.md`](docs/parity-report.md)).
- Booting: the game loads, `SuperController` is alive, `isLoading` falls back to false, and the boot
  log matches the original's line for line ([`docs/verification.md`](docs/verification.md)).
- Scenes and animation: `CyberDemoAlt` loads with all 17 atoms present and 1883 MonoBehaviours, and
  the character's 84-bone animation drives the skeleton.
- Shaders: the 51 `*ComputeBuff` shaders that GPU-skinning needs are rebuilt from the shipped DXBC
  ([`docs/shader-reconstruction.md`](docs/shader-reconstruction.md)); Unity compiles all 148 passes
  with 0 shader errors.
- Next: visual comparison and system checks, then the refactoring pass towards readable code.
