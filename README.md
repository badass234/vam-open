# VAMOpen 0.1.9-alpha

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
- Shaders: 88 reconstructed from the shipped DXBC bytecode - 256 passes, 6 805 compiled programs.
  Every drawn material of the character now draws with one of them instead of the bundle's copy.
- Post-processing
- Self-shadowing: the point lights' shadow filter is VaM's own, decoded from the released bytecode -
  the 25-tap Poisson disk, the doubled depth bias, the averaging that makes a soft edge as dark as a
  hard one. Against Unity's built-in filter, in one session under the same lights, it darkens the lit
  body by 8.26/255 of mean luminance where the built-in path darkens it by 0.98/255.
- A standalone `VAMOpen.exe` builds in batch mode and runs outside the editor: 10 scenes, D3D11, all
  18 `.var` packages registered, 312 FPS in its own benchmark scene.
- The runtime plugin compiler is rebuilt from the installation's own sources (`src\mcs`), so plugins,
  scenes and presets that the shipped `mcs.dll` could no longer compile on the new engine do compile.
- An in-game screen resolution setting, in the preferences panel next to the physics and vsync rows:
  the modes the display reports, a confirmation popup, and a ten-second revert if the answer never
  comes.

Also, the shading model of the drawn body has been compared with the released build's own shader
bytecode, instruction by instruction, and matches it: the tangent frame, the normal map, the albedo
override, the gloss and bump offsets, the highlight exponent and Fresnel curves, the SH ambient, the
reflection, the direct light and the final composite. That is not a feature but the ground everything
else stands on, and the point light's shadow filter is on the same list, with the one substitution it
needed named where it is made.

## What's still in progress

- Minor character material defects: characters still shine a little more than the original does. The
  quality preset used to be in the way of judging that, because the project ran `Max` while the
  installation runs `High`; the two are now matched and every play report states the preset it ran
  at, and with them equal the preset turns out to account for about half a percent of the skin's
  brightness rather than for the look. The shipped bytecode rules the shader out, so what is left is
  what is bound to it: the specular IBL cube's import colour space, the specular/fresnel values the
  material carries, and the bloom threshold. The list is the *The base pass, read back out of the
  shipped bytecode* bullet in [`docs/release-notes.md`](docs/release-notes.md); the measurement is the
  *The editor and the installation had never rendered under the same settings* one.
- The `Marmoset/` set is not transcribed yet, and it is what the last 21 materials still draw: the live
  skin's `EyeReflection-1` (left with nothing to redirect to by
  `Marmoset/Transparent/Simple Glass/Specular IBLComputeBuff`), the overlay helpers and
  `Unlit/UnlitOverlayShader`. `Marmoset/` is a different shading model - its own SH, exposure and
  sky-range uniforms - so it needs a second model beside the reconstructed one, not an extension of it.
- 25 of the project's shader files are still AssetRipper placeholders that declare no pass, and the
  families that fall back to a Unity built-in, to `GPUTools/Painter` or to `GPUTools/MeshedVR/HairOpt`
  cannot be moved onto one: a placeholder compiles and draws *less* than the shipped shader it stands
  in for, and its takeover would trade a wrong picture for a missing one.
- Code is partially readable but needs refactoring.

## Plan

First, get the project to a state where 95% of basic functions work stably.
Then gradually move everything to the latest Unity version.
And after that, add modern stuff: proper Anti-Aliasing, new shaders (subsurface scattering, HDAO,
etc.), then DX12 and Vulkan, add at least basic multithreading so physics doesn't lag when you add
just a couple of characters to the scene, then FSR 4, DLSS, Ray Tracing, and so on.

The hardest part here is the engine migration. Everything else after that is much easier.

## How to try it

You need Windows, Unity **2021.3 LTS (2021.3.45f2)**, and your own Virt-a-Mate installation. Take that exact
patch: the newer `2021.3.58f1` is an **extended-LTS** build, which needs Unity Industry or Unity Enterprise and
exits before it opens anything on a Personal licence. `scripts\Test-UnityEditorUsable.ps1` reads that
entitlement off an installed editor so you find out before spending a run on it.

```powershell
scripts\Setup-RebuildProject.ps1
scripts\Sync-Sources.ps1               # after any edit in src\, and before a gate
scripts\New-RuntimeDataLinks.ps1       # links the installation's data and matches its graphics preset
scripts\Invoke-CompileGate.ps1
python scripts\Extract-VaMShaders.py --out artifacts\shader-blobs
python scripts\New-VaMShaders.py
python tools\check_shaders.py
scripts\Invoke-ManualPlay.ps1
scripts\Invoke-SyncSolution.ps1         # writes VaM_Rebuild.sln, to open the rebuild in Rider
```

`Sync-Sources.ps1` is easy to miss and expensive to skip: Unity compiles `VaM_Rebuild\Assets\Scripts\`,
not `src\`, and `Setup-RebuildProject.ps1` performs that copy once. The setup also rebuilds the runtime
plugin compiler (`scripts\Build-McsCompiler.ps1`) with the editor's own mono, which is why it wants the
editor installed and not just the project folder. The sync mirrors `*.cs` from `src\`
into the project, never touches a `.cs.meta` (a scene resolves its class through that GUID), verifies
every file byte for byte, and reports whether `Assembly-CSharp.dll` is older than the newest source. A
green compile gate without it is green about whatever was copied last.

`New-RuntimeDataLinks.ps1` is the step that makes an editor session read the same data the game does:
`AddonPackages` and `Custom` as junctions, `Saves` and `AddonPackagesUserPrefs` as copies, and the
graphics keys of `prefs.json` matched to the installation's preset. Skip the last part with
`-MatchGraphicsPrefs:$false` if you want the editor on a higher preset; the play report prints the
preset it ran at either way.

The same project also builds as a standalone player, which needs neither Unity nor an open editor
afterwards:

```powershell
scripts\Invoke-PlayerBuild.ps1        # Unity batch mode, output in artifacts\player
scripts\Invoke-Player.ps1             # links the installation's data, then starts the exe
scripts\Invoke-Player.ps1 -Seconds 150
```

Start the player through the launcher rather than by hand: the game resolves everything - the packages,
`Custom`, `Saves`, the cache, the key file - relative to the process working directory, and a shell that
sits somewhere else (an elevated one starts in `system32`) strips the player of all of it in silence.
The launcher sets the working directory, checks the runtime links, refuses to run two players at once and,
with `-Seconds`, watches the run and reads its log back. Double-clicking `artifacts\player\VAMOpen.exe`
in Explorer is the one manual equivalent, because Explorer starts a process in the folder it lives in.

The log is the player's own, `%USERPROFILE%\AppData\LocalLow\MeshedVR\VaM\Player.log` (`-LogFile`
overrides it). The failure to watch for there is the package count: `Scanned 79 packages` means the
installation's data was found, `0` - or single digits - means the links are missing.

If the editor comes up showing `Failed to load window layout`, play mode never starts: an editor that
was killed rather than closed leaves an empty or stale `LastLayout.dwlt` in
`%APPDATA%\Unity\Editor-5.x\Preferences\Layouts`, and Unity needs it gone before it will write a
default one.

Stage details are in `docs\`, the source of truth for status is in `CHANGELOG.md`, and the detail behind
its short sections is in [`docs/release-notes.md`](docs/release-notes.md).

`Invoke-SyncSolution.ps1` writes `VaM_Rebuild.sln` and the project files beside it using the editor's
own generator, because Unity is the only thing that knows which file belongs to which assembly. The
solution and the project files are gitignored build output, regenerated on demand after a source file or
a plugin is added; `src\` keeps its own
tracked projects, which are what to open when there is no editor around. Both sets and what they are
for are described in [`docs/rebuild-project.md`](docs/rebuild-project.md).

## Layout

| Path | What it is |
|---|---|
| `src\Assembly-CSharp` | the game's code as ilspycmd decompiled it, ~2800 `.cs` files |
| `src\Assembly-UnityScript` | the game's second assembly (13 files) |
| `src\RTTypeModel` | the type model the decompiler needs |
| `src\mcs` | Mono's C# compiler, which the game drives at runtime, rebuilt from the export's sources with one patch (see [`docs/rebuild-project.md`](docs/rebuild-project.md)) |
| `shader-src\VamGpuSkinning.cginc` | the reconstructed GPU-skinning shading library (see [`docs/shader-reconstruction.md`](docs/shader-reconstruction.md)) |
| `VaM_Rebuild\Assets\Editor\` | the only code written by hand: `RebuildGate.cs` boots the game in batch mode and prints a verdict, `RebuildPlayer.cs` builds the standalone player |
| `VaM_Rebuild\ProjectSettings`, `VaM_Rebuild\Packages` | Unity project settings, including the .NET 4.x scripting runtime the decompiled code needs |
| `scripts\` | the pipeline: source sync into the Unity project, project setup, shader extraction and generation, compilation gate, smoke runs, log comparison |
| `tools\` | standalone analysers (asset GUIDs, API surface, IL tokens, Unity logs, frame comparison, shader pre-flight, UI bundle probes) |
| `docs\` | per-stage reports: asset export, project rebuild, editor, verification, parity, shader reconstruction, engine upgrade audit, release notes |
| `CHANGELOG.md` | one short section per release: what works, what does not, what is known broken |
| `docs\release-notes.md` | the long form of those sections, with the measurements |

## Paths

The repository holds no machine-specific path. Everything the scripts need from outside is a
parameter with a portable default:

- **The game installation** (`-InstallRoot`) is resolved as: the `-InstallRoot` argument, else
  `$env:VAM_INSTALL`, else the parent directory of this repository - used only if it really looks
  like a Virt-a-Mate install (it must contain `VaM_Data\Managed\Assembly-CSharp.dll`). That last
  fallback matches the usual layout, where the repository sits inside the installation directory.
  If none of the three applies, the script stops with a message instead of guessing.
- **The Unity editor** (`-UnityExe`) is read from the project instead of being hardcoded: the scripts
  take the version out of `VaM_Rebuild\ProjectSettings\ProjectVersion.txt` through
  `scripts\UnityEditor.ps1` and look for `%ProgramFiles%\Unity\Hub\Editor\<version>\Editor\Unity.exe`.
  That file is the one place an engine version lives, so a version hop needs no script edit - the
  project now names **2021.3.45f2**, the last patch of the 2021 LTS line that is not extended-LTS
  (`2021.3.58f1` is, and refuses to run on a Personal licence). Pass `-UnityExe` for an
  installation elsewhere, including one outside Unity Hub.
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
