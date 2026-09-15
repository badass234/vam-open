# Verifying the rebuild

Stage 5 asks one question over and over: *does the rebuild do what the original does?* This document
lists the checks that answer it, what each one actually proves, how to run it, and what the current
answers are. Every check writes to `artifacts\` and exits non-zero when it fails, so they can be run
in sequence without reading the log by eye.

`docs\rebuild-project.md` explains how the project is put together; this file is only about evidence.

## The checks

### 1. Compile gate - `scripts\Invoke-CompileGate.ps1`

Runs `Unity.exe -batchmode -nographics -quit -executeMethod RebuildGate.Report`. Unity has to compile
every script assembly before it can look the method up, so a project that does not compile fails the
run - the method does not exist, and the script reports that instead of the marker.

**Read the marker, not the exit code.** One batch run compiles twice: once before the imported plugin
DLLs exist, and once after, so real `error CS` lines appear in the middle of a log that ends with a
clean build. The verdict is the literal marker `----- RebuildGate OK -----`, which cannot be reached
unless `Assembly-CSharp-Editor` compiled. `tools\parse_unity_log.py` is run over the log afterwards to
group any errors by category.

The runner repairs the one recoverable failure, and repeats the run once; see *When a gate lies* below.

*Current state*: `----- RebuildGate OK -----`, 0 errors, `Assembly-CSharp.dll` 6 159 360 B,
`VaMUnityScript.dll` 16 896 B, `Assembly-CSharp-Editor.dll` present.

### 2. Scene integrity - `scripts\Invoke-SmokeTest.ps1 -Method InspectScene`

Opens the boot scene and counts serialized `MonoBehaviour` references that no longer resolve. A
decompiled-and-recompiled assembly keeps class and field names, so every reference should still bind;
each one that does not shows up as a null component. Runs headless.

*Current state*: `NewStart` - 2 game objects, 3 components (`Transform` x2, `GlobalSceneOptions`),
**0 unresolved components**. The known-dangling GUIDs from the asset export are `null` in the original
export as well (see `tools\known_dangling_guids.txt`), and none of them are in this scene.

Note how empty that scene is: three components is all `NewStart` has, which is why the atom count in
the play test is four and not more.

### 3. Play smoke test - `scripts\Invoke-SmokeTest.ps1 -Method Play`

Starts the game in the editor, lets it run for `-Seconds`, then prints a report and exits. The report
ends with the state of `SuperController`, the singleton the whole of VaM hangs off.

The run is guaranteed to end in a verdict: an editor that outlives `-TimeoutSec` is stopped and its log
read anyway, and the one recoverable failure is repaired and retried once - see *When a gate lies*.

Batch mode creates no window at all - the game renders offscreen, so a plain run proves the boot
works but shows you nothing. `-Visible` drops `-batchmode`, the editor opens normally and the game
renders in its Game view, which `RebuildGate.Play` brings to the front before entering play mode. The
rest of the run is identical: same report, same elapsed time, same self-exit, no need to close the
editor by hand.

```
scripts\Invoke-SmokeTest.ps1 -Method Play -Seconds 150 -Visible
```

*Current state* (45 s / 120 s windowed-batch runs and a 150 s visible run all report this):

| Signal | Value |
|---|---|
| Errors and exceptions | 0 |
| `SuperController.singleton` | `CoreControl` |
| Atoms | 4 - `[CameraRig]`, `CoreControl`, `PlayerNavigationPanel`, `WindowCamera` |
| Loaded scene | `NewStart` |
| MonoBehaviours in the loaded scenes | varies: 210 (batch, 30 s), 260 (batch, 120 s), 154 (visible, 150 s) |

The component count is a diagnostic, not a verdict - it drifts with run mode and length because the
boot both creates and retires transient objects, and the original never prints the number, so there is
nothing to compare it against. What has to hold is that it is in the hundreds rather than collapsed
toward zero, which would mean the scene never instantiated anything.

**The loading flags matter more than the atom count.** "Is it alive" and "has it finished loading"
are different questions, and a boot stuck mid-load still has a live `SuperController`. The report
therefore prints `SuperController.isLoading`, `MeshVR.GlobalSceneOptions.IsLoading` and
`IsSimulationResetting()` explicitly; all three are `False`, so the boot genuinely completes rather
than merely starting.

Matching the original's four atoms is expected, not a sign of a thin scene: the original also starts
in an almost empty `NewStart` and pulls the world in from asset bundles through
`GlobalSceneOptions.LoadAssets`.

### 4. Boot-log parity - `scripts\Compare-BootLogs.ps1`

The original leaves a ground-truth log at

```
%USERPROFILE%\AppData\LocalLow\MeshedVR\VaM\output_log.txt
```

Boot the original once and the rebuild's log can be lined up against it. The script reduces both logs
to their messages - dropping stack traces, `(Filename: ...)` footers, Unity's editor start-up chatter
and the gate's whole report block - normalises durations (`took 0.6 ms` -> `took <ms>`, since two runs
never take the same number of milliseconds), then aligns the two greedily in order. A line the rebuild
added prints as `+`, a line the original printed that the rebuild did not prints as `-`. `-ShowAligned`
prints the matching set, which is the actual parity evidence.

*Current state*: **all 20 messages align in order and neither side prints a line the other does not.**
The script says so in as many words - `the two boots are identical` - and it says it for both the
visible run (the default candidate, `artifacts\smoke-play.log`) and
`-Candidate artifacts\smoke-play-batch.log`, a 30 s windowed-batch run kept as a second reference.

Keep the `+` and `-` lists empty. Anything new is a real behavioural difference.

Two things had to be normalised before the comparison meant anything, both of them properties of the
numbers rather than the behaviour. The gate skips its own report as one block (report header to the
OK/FAILED marker) rather than listing the lines inside it, so a new diagnostic added to the report
cannot show up as a fake difference - two earlier comparisons were polluted by exactly that, with the
report's newer `atoms:` / `loading:` / `scene:` lines reported as "printed by the rebuild, not by the
original". And `Benchmark complete` is entirely averages, none of which repeat between two runs, so
its numbers are replaced with `<n>`; without that the original's `Avg. FPS: 313.36` and the rebuild's
`Avg. FPS: 181.05` look like two different events. A visible run also prints editor-only chatter that
batch mode does not (`EditorUpdateCheck`, `TrimDiskCacheJob`, `IsTimeToCheckForNewEditor`, ...), so
those patterns are filtered as well.

## When a gate lies

Both failure modes below have already happened once, and each of them produced a red verdict that had
nothing to do with the rebuild. A check that can fail for the wrong reason is worse than no check, so
the runners now defend against both. The first failure mode is *repaired*; the second is *contained*.

**Failure mode A - the built assemblies lose `Assembly-CSharp.dll`.** Unity decides what to recompile
from state it keeps in `Library`: the assemblies in `Library\ScriptAssemblies`, the asset database and
the recorded source timestamps. Those can disagree with the sources on disk, and then the next batch run
compiles *only* `Assembly-CSharp-Editor.dll`. Its compiler command line has no
`-r:Library/ScriptAssemblies/Assembly-CSharp.dll`, so `Assets\Editor\RebuildGate.cs` - which uses
`SuperController`, `MeshVR` and `Battlehub` - dies with a wall of cascading

```
Assets/Editor/RebuildGate.cs(300,13): error CS0246: The type or namespace name 'SuperController' could not be found
```

**Nothing in those sources is wrong.** The assembly that declares the types is simply not in the
reference list. Note also that a *genuine* error in `src\` produces the same cascade, since the editor
assembly cannot reference an assembly that never built - which is exactly what the repair must not
mistake for this state.

`scripts\Repair-ScriptAssemblies.ps1` performs the recovery that used to be done by hand: it deletes the
stale assemblies in `Library\ScriptAssemblies` and moves the timestamp of every source under
`Assets\Scripts` forward, so a full recompilation is scheduled. **It changes a timestamp and nothing
else** - no content, no project setting, no asset database. The decision is the exit code:

| Exit code | Meaning | The caller's move |
|---|---|---|
| `0` | the state was present and is repaired | run the same gate once more |
| `3` | not that state, or the project is healthy | trust the run's own verdict |
| `1` | the project could not be read | fix the invocation |

With `-LogFile` it demands the evidence before touching anything: at least three `CS0246` lines naming
`SuperController`, `MeshVR` or `Battlehub`, and **not one error anywhere but
`Assets\Editor\RebuildGate.cs`**. A log that reports an error in `src\` is left alone, because there
`Assembly-CSharp.dll` is missing for a real reason and repeating the run would only cost time.

Both runners call it exactly once, no matter which method failed: the old log is moved aside to
`<log>.attempt1.log` (so the failed attempt stays readable) and the run is repeated. A second failure is
real and stands.

**Failure mode B - the editor does not exit.** The last `-Method Report` never terminated on its own:
the script hit its 1800 s timeout and the editor was stopped by hand, although
`----- RebuildGate OK -----` had already been written to the log. Throwing a timed-out run away is
therefore a way to report a failure that the run itself did not. A timeout is now a *note*, not a
verdict:

- the editor is stopped, and **its log is judged anyway** - the report on disk is what decides;
- before every run the runner deletes both the log and the sidecar `*.report.txt`, so a report that is
  read always belongs to the run that just happened and never to the one before it;
- `-Method Report` and `-Method InspectScene` also pass `-quit`, which guarantees termination even in a
  state where the gate method cannot exit the editor itself. `-Method Play` deliberately has no `-quit`:
  play mode needs the editor alive, and the gate exits it when the run is over.

The consequence is the rule to work by: **a gate always ends in a verdict, never in an exception.** If a
run reports `FAILED`, that verdict came from a report the gate wrote, not from a timeout or a crash.

## What the harness cannot show

**The `Benchmark complete` line needs a rendering device but not a window.** `MeshVR\PerfMon.cs` prints
it at `_totFrames == avgCalcStartFrame + avgCalcNumFrames`, and `_totFrames` only advances inside
`DoUpdate()`, which runs from a `while (true) { yield return new WaitForEndOfFrame(); ... }` coroutine.
With a device the coroutine resumes normally - 3 433 frames in a 30 s batch run, 26 413 in a 150 s
visible run - and the line fires with the original's wording. Under `-nographics` no frame ever ends, a
play run reports `PerfMon frames=0`, and the line can never fire. That is a property of the test stand,
not a defect - do not "fix" it by restarting the average calculation, which the original never calls
either.

**Do not run `-Method Play` with `-nographics`.** VaM's cloth, hair and collider systems are
compute-shader systems; with no graphics device every variant fails to load, `ComputeShader.FindKernel`
returns `-1`, and `GPUCollidersManager.FixedUpdate()` throws every physics frame. The log fills with
`FindKernel failed` and it reads exactly like a broken rebuild. The compute shader assets are fine
(`Assets\Resources\compute\*.asset` are proper serialised `ComputeShader` objects with real DXBC
blobs). `Invoke-SmokeTest.ps1` is windowed for `Play` by default and offers `-Headless` only for the
gates that do not need a device: `Report` and `InspectScene`.

## Named non-issues

Both of these looked like defects and are not. They are recorded so they are not re-investigated.

- **`Unloading broken assembly Assets/Plugins/RTTypeModel.dll`.** The editor prints the identical
  line about its own `UnityEditor.UI.dll`, so it is not specific to the rebuild. The assembly loads:
  `Assembly.Load("RTTypeModel")` returns it with 1 type, and `ProtobufSerializer`'s static
  constructor - the only thing that consumes it - runs without throwing. `ReflectionOnlyLoadFrom`
  reports `v2.0.50727`, so the pre-4.0 runtime version in the metadata provokes the message rather
  than a real failure. It also references `Assembly-CSharp`, which is circular in the editor, so
  rebuilding it would make things worse. Leave the shipped DLL alone.
- **`Scanned 0 packages`.** `FileManager.Refresh()` uses `Directory.GetFiles("AddonPackages", "*.var",
  SearchOption.AllDirectories)` - a path relative to the process working directory. In the player that
  is the install root; in the editor it is the project root, so `FileManager` had quietly created an
  empty `AddonPackages` folder there. `scripts\New-RuntimeDataLinks.ps1` junctions the real folder in
  and copies the user-prefs folder (the editor writes to it), and the rebuild now prints
  `Scanned 9 packages` like the original.

## Still open

- **Visual comparison.** Nothing has been compared against a running `VaM.exe` yet: models, lighting,
  materials, shaders. Shaders from the AssetRipper export are the most likely place for a difference.
- **A loaded scene.** Four atoms is a successful boot, not proof that a scene with content loads and
  animates. Loaded meshes, animation and physics on an actual character are untested.
- **Runtime plugin discovery.** `ConvexDecompositionDll.dll` is not referenced by any managed code, and
  ZFBrowser locates the CEF payload by scanning directories rather than by `DllImport`, so neither has
  been exercised.
