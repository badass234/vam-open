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

**Before every run: `scripts\Sync-Sources.ps1`.** The gate compiles `VaM_Rebuild\Assets\Scripts`, which
is a copy of `src\`, so a gate run without the sync answers about the tree that was copied last - see
failure mode E below.

**The log also names the asset import pipeline** - line 30 of the last run reads
`Using Asset Import Pipeline V2.` The project pins that pipeline in `ProjectSettings\EditorSettings.asset`
(`m_AssetPipelineMode: 1`), so a log that says V1 means the pin is gone and the gates are working on a
database the editor's window does not share.

*Current state*: `----- RebuildGate OK -----`, 0 errors, 0 unique errors, `Assembly-CSharp.dll`
6 079 488 B, `VaMUnityScript.dll` 17 408 B, `Assembly-CSharp-Editor.dll` present.

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

**Loading a scene on top of the boot.** `-Scene` asks the game to load a scene once the boot has had
`-WarmupSeconds`, through the same call the in-game file browser makes, so one report covers the boot
and the scene:

```
scripts\Invoke-SmokeTest.ps1 -Method Play -Seconds 150 -WarmupSeconds 15 `
    -Scene 'MeshedVR.DemoScenes.2:/Saves/scene/MeshedVR/DemoScenes/Cyber/CyberDemoAlt.json'
```

The name is the game's addressing, not a filesystem path: `<packageUid>:/<path inside the package>`
for content that lives in a `.var`, or a bare `Saves/scene/...` path for content that sits in the
installation. A name that is not addressing **does not load, and does not complain**: no error is
raised and the scene already on screen simply stays there. The report therefore carries the flags that
separate "asked" from "arrived", plus the loaded scene's own atom list, and a requested scene that did
not arrive fails the run:

| Signal | A settled `CyberDemoAlt` run |
|---|---|
| `scene load` | `requested=True, taken=True, finished=True after 28.7 s, refused=False` |
| `scene atoms` | `17 declared, 17 present, 0 missing - the scene on screen is the scene that was asked for` |
| `atoms:` | 17 - `Person`, `Start Head Position`, `CoreControl`, `[CameraRig]`, five music buttons, ... |
| `dynamic content` | `1 item(s), 0 cannot load, 1 active` |
| `errors and exceptions` | 0 |
| `loading:` | `SuperController=False, GlobalSceneOptions=False, simulation resetting=False` |

That is not a hypothetical: `-Scene CyberDemoAlt` was accepted, silently loaded nothing, and left the
boot scene in place - the run failed on the missing atoms, which is how the wrong invocation was
found instead of being believed.

**The loading flags matter more than the atom count.** "Is it alive" and "has it finished loading"
are different questions, and a boot stuck mid-load still has a live `SuperController`. The report
therefore prints `SuperController.isLoading`, `MeshVR.GlobalSceneOptions.IsLoading` and
`IsSimulationResetting()` explicitly; all three are `False`, so the boot genuinely completes rather
than merely starting.

Matching the original's four atoms is expected, not a sign of a thin scene: the original also starts
in an almost empty `NewStart` and pulls the world in from asset bundles through
`GlobalSceneOptions.LoadAssets`.

**`errors and exceptions` counts a session, and plugins are what is in it.** The boot loads the
installation's own `Saves\scene\MeshedVR\default.json`, which asks for `MacGruber.Breathing`, and that
plugin cannot run under the dynamic assembly this project builds: its constructor throws
`FieldAccessException` on a private field of its own nested generic queue, `AddComponent` leaves the
half-built component on the object, `Init` throws on top of it, and Unity keeps calling `Update` - so
a log for one scenario holds **8 595** `NullReferenceException` lines from `MacGruber.Breathing.Update`
and **8 595** from `MacGruber.DriverBreathing.Update` (`artifacts\player-run-macgruber.log`: 94 736
lines, 17 194 exception lines) where the same boot after the fix holds **0** from either
(`artifacts\smoke-plugin-fix.log`: 3 945 lines, 12 exception lines, verdict `errors and exceptions: 6`).
The six that remain are one-shots: the constructor, two `Atom` restores reaching the object that
failed `Init`, and one second plugin whose `Init` throws. `MVRPluginManager` wraps creation, switches
off a plugin that throws out of `Init`, and shows the first failure of each plugin in a dialog, so a
broken plugin costs one dialog and a handful of lines instead of a line per frame.

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

### 5. Visual comparison - `tools\compare_view.py`

A rebuild has to be compared against the original game rather than against memory. A reference frame
can come from two places: the preview image VaM stores inside a scene package (a `.var` is a zip, so
`Saves/scene/.../<name>.jpg` can simply be extracted), or a screenshot of a running `VaM.exe`. Only the
second one is a fair comparison for a given camera - a shipped preview is rendered by a dedicated
screenshot camera (`SuperController.screenshotCamera`, `loResScreenShotCameraFOV`, default 40) from a
vantage of its own, not through the camera the player sees.

```
python tools\compare_view.py <reference> <capture> [--crop x,y,w,h] [--out DIR] [--min-zncc X]
```

The reference is scaled to the capture's height and both are then centre-cropped to the smaller width,
so nothing in the comparison is invented and the report states the geometry it had to reconcile. What
it measures: mean RGB, mean level, the share of near-black and of bright pixels, the mean RGB of the
nine zones, the dominant colours at 16 levels per channel, and the alignment - the coarse 32x32
correlation at zero offset, the offset that maximises it within +-4 cells, that offset in pixels, and
the mean difference once aligned. `compare.png` is written next to the capture: the reference, the
aligned reference, the capture, the difference after alignment and both luminance maps.

The correlation is the number that carries weight: the same content from the same camera scores above
roughly `+0.8` even when the tonemapping differs, while two different vantages of one scene sit near
zero. A spatial match is a claim that can be checked, unlike "it looks the same".

*Current state*: **part one done.** Against the shipped preview of `CyberDemoAlt`
(`artifacts\reference\CyberDemoAlt.jpg`, 512x512, a different vantage and aspect) the best correlation
is `+0.010`, so the preview cannot settle the palette question - it is a different view. What it does
show objectively is that the saturated violet mass in our game view has a counterpart of the same
relative size in the preview (`33.2%` of the frame height against `33.2%`), which makes it a scene
object rather than a broken material. The capture of the original from the same camera is what is
still missing.

### 6. Shadow attribution - `tools\measure_shadow.py`

`compare_view.py` answers "does this capture match that render" and averages any difference over the
whole frame. When the question is instead "did this light term darken the surface", the statistic has to
be taken where there was light to lose, which is a different question with a different tool:

```
python tools\measure_shadow.py <reference> <test> [<test> ...] [--threshold 32] [--grid 8] [--out PREFIX]
```

The mask is every pixel of the reference above `--threshold` luma, the delta is `reference - test` on
that mask only (positive means the test is darker), and the report gives the mean, median, p90, p99 and
extremes, the share of the mask past 2/5/10/40 levels, and the mean delta per cell of an 8x8 grid, plus
a composite per test (reference | test | amplified delta over the reference | signed delta with the mask
outlined).

Both halves matter. The **summary** answers "how much", and the **cell breakdown** answers "what kind":
a shadow filter leaves cells that differ by tens of levels with the deep ones where the surface meets
itself, while a colour space or exposure change shifts every cell by the same amount. The measurement
that missed this - averaging over the whole frame, background included - reported a real contact shadow
as 0.6/255; see *Found by measuring* below, where the same pair reads +8.26/255 on the mask.

*Current state*: VaM's transcribed point-light filter is **+8.26/255** mean on the body's lit pixels and
2 196 px deeper than 40 levels against shadows off, where Unity's own filter under the same lights is
+0.98/255 and 240 px, with the control switch (`_VamShadowUnity`) set and restored inside one session.
Re-running the pair on regenerated builds reproduces the mean to a few hundredths (+8.26, +8.26, +8.23)
and the cell range to a few tenths, but not the counts: the reference frame is not bit-identical run to
run, which the drifting mask size says (73 494, 73 435, 73 413 px) before any count is taken. So across
runs compare the mean, and read the counts as the run in front of you. The shadow probe prints this
command with the frames it just wrote, so a play report names its own measurement instead of leaving it
to be reconstructed after the fact.

### 7. Plugin compiler references - `scripts\Update-PluginCompilerReferences.ps1 -Verify`

VaM compiles the user's plugins at runtime, and `DynamicCSharp` hands Mono a fixed list of bare
assembly names - `assemblyReferences` in `Assets\Resources\DynamicCSharp_Settings.asset` - rather than
scanning `Managed\`. One name in that list that the engine does not ship is fatal for the whole
compilation, and nothing in the log calls it that: the plugin simply never loads. Names are resolved in
`Directory.GetCurrentDirectory()`, so the same list can pass in the editor (working directory = the
installation root, whose `Managed` holds the game's own assemblies) and fail in the built player.

```
scripts\Update-PluginCompilerReferences.ps1 -Verify            # report only, exit 0/1
scripts\Update-PluginCompilerReferences.ps1                    # apply the two-name adaptation
scripts\Update-PluginCompilerReferences.ps1 -Verify -PlayerPath artifacts\player
```

The check is a file-existence test over the list, taken against `<player>_Data\Managed`, and it is the
player it has to be taken against: the installation's `Managed` still carries the 2018.1 Timeline
assemblies, so verifying there passes whether or not this project is fixed. `Invoke-PlayerBuild.ps1`
runs it automatically after a build, and the fix script itself is step 7 of
`Setup-RebuildProject.ps1`.

*Current state*: **27 references, 99 managed assemblies in the player, every reference resolves** -
exit 0. The engine-name adaptation in effect is `UnityEngine.Timeline.dll` -> `Unity.Timeline.dll`
with `UnityEngine.TimelineModule.dll` dropped, which is the whole of what 2019.4 changed under this
list. See *The plugin compiler* in `rebuild-project.md` for the chain and for the A/B that measured it.

## When a gate lies

Both failure modes named first below have already happened once, and each of them produced a red verdict that had
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
The one end that carries no verdict is a run where the editor itself dies in flight, and that state is
reported as what it is (below) rather than as a compile failure.

**Failure mode B, second half - the editor dies in flight.** A run can also end with *nothing at all*:
the 2019.4 abort (`plan.md`, item 2) kills the process inside `mono-2.0-bdwgc.dll` after
`Resources.UnloadUnusedAssets` and the log simply stops - no verdict, no report, no exception, no crash
report. The runner read that as `the gate method never ran - the project did not compile or start`,
which sent the reader after `CS` errors that did not exist. It now tells the two apart by the gate's own
phase lines and prints the state it actually read -
`the editor died mid-run - the gate never wrote its report`, with the last phase and the last log line
underneath. A death in flight still exits non-zero: the run did not pass. What changed is only that the
sentence is now true of the run.

**Failure mode C - the gate measures a buffer nothing draws.** A green line that is true of the wrong
object is the most expensive kind, because it sends the search in a direction the rebuild cannot
satisfy. The body defect below is the worked example: `SkinReport()` read `skin.rawVertsBuffer` and
printed the posed bounds, which is a *correct* statement about that buffer and a false one about the
screen - `DAZSkinV2.DrawMeshGPU()` picks the vertices it actually draws from `_useSmoothing`, so a
smoothed skin's material reads `smoothedVertsBuffer` (`DAZSkinV2.cs:4751`), and the skin in the scene
smooths. The report now prints the buffer the material consumes, names the shader that receives it,
and states whether that shader could be found at all, so the sentence on screen is about the pixels.
When a report disagrees with your eyes, this is the class of bug to suspect first: the numbers are
probably right about the wrong buffer.

**Failure mode D - the gate measures a generated copy of what you edited.** `VaM_Rebuild\Assets\Shader\*.shader`
and `VaM_Rebuild\Assets\VaMShaders\VamGpuSkinning.cginc` are generated by `scripts\New-VaMShaders.py`
and are not tracked; `shader-src\VamGpuSkinning.cginc` is the source of truth and the generated copy is
whatever the last run of the generator wrote. Edit the include, skip the generator, and the compile
gate and the play gate both pass **green on the previous round's shaders** - there is nothing for
either to fail on, because both are measuring a build that is internally consistent, just not the one
you think you built. The gate sequence the pipeline scripts run puts the generator first for that
reason, and the
tell is worth knowing: in Round 7 a control frame that returned a *different* code path came out
bit-identical to the session frame. That reads like "the switch does nothing" and it means "the switch
is not in the build".

A gating change cannot catch this, because a gate that re-ran the generator would report its own
success as the evidence. The defence is the runbook's order, and the check to reach for when a shader
edit appears to have no effect:

```
python scripts\New-VaMShaders.py
git --no-pager diff --numstat -- shader-src/VamGpuSkinning.cginc   # what has not shipped yet
fc.exe /b shader-src\VamGpuSkinning.cginc VaM_Rebuild\Assets\VaMShaders\VamGpuSkinning.cginc
```

**Failure mode E - the gate compiles a copy of the sources.** The same shape as D, one level up and
without a generator to blame: `src\Assembly-CSharp` is the tree under version control and the tree a
person edits, while Unity compiles `VaM_Rebuild\Assets\Scripts\Assembly-CSharp`, which
`Setup-RebuildProject.ps1` fills from `src\` when the project is first set up. The two are separate
copies - no hardlink, no symlink - so an edit in `src\` is invisible to the compiler until something
copies it across, and both gates then answer green about the older tree, with every number they print
being true about the wrong files. Measured when a user-visible setting failed to appear in a build:

| | `src\Assembly-CSharp\UserPreferences.cs` | `VaM_Rebuild\Assets\Scripts\Assembly-CSharp\UserPreferences.cs` |
|---|---|---|
| size | 129 969 B | 118 911 B |
| timestamp | the round's edits | two days earlier |
| `screenResolution` occurrences | 130 | 0 |
| builds on the new code | 0 | 1 (`Assembly-CSharp.dll`, 6 180 864 B, predating the edits) |

`scripts\Sync-Sources.ps1` closes it and belongs in every loop that touches code: it copies the `*.cs`
files of `Assembly-CSharp` and `Assembly-UnityScript` from `src\` into the project, never writes or
deletes a `.cs.meta` (a scene resolves its class through that GUID, `docs\rebuild-project.md`),
re-reads both trees and prints `verdict: OK - N source(s)` only when every file matches byte for byte,
and compares the newest source against `Assets\Scripts\Assembly-CSharp.dll`, reporting
`Assembly-CSharp.dll: STALE - the assembly predates the sources` instead of letting the next gate
report on a stale build. Its first run: `copied 5, deleted 0, already identical 2820`. After it,
`Assembly-CSharp.dll` was **6 188 544 B** at 19:37:19, newer than every source, and the gate's
`verdict: OK` with 0 unique errors was about the tree that had been edited.

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

All three of these looked like defects and are not. They are recorded so they are not re-investigated.

- **A hair slot the scene has switched off.** What the skeleton report printed as a broken skeleton -
  `10 of 20 bones share a name with the body, 10 of those are rotated differently`, the hair `hip`
  sitting `4.18 m` from the body's - is the built-in `VictoriaElitePonytailHair` slot. `CyberDemoAlt`
  selects exactly one hair (`RenVR:Simone (REN).vam`, `enabled: true`) and every other hair item in the
  scene is inactive; a slot the scene does not use stays in the scene with its own copy of the joints,
  switched off and left in the rest pose it was built with, which is why a whole skeleton reads as
  rotated and metres away. The report now says `<- inactive in the hierarchy` and skips the comparison,
  and the underlying accessor is the reason it has to: `SceneObjects<T>()` uses
  `Resources.FindObjectsOfTypeAll` and deliberately includes objects that are switched off. The same
  mistake hid in `DAZBone components in the scene: 108, rotated away from identity: 101`, which counted
  dead slots and dumped five of their bones; it now reads `108, in active objects: 84, of those rotated
  away from identity: 82` and dumps the live body's bones.

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

## The body defect: a stub shader where the skinned vertices go

**What the eye sees.** The character is on screen in the right place, but the body is a static,
unlit, fullbright shell standing in the *bind pose* while the hair moves with the skeleton and is
lit. It reads as a body cut out of the scene, or as a skin hanging detached from an invisible model -
the two descriptions of one picture.

**What is not the cause.** Not the skeleton, and not the binding. Every live skin reports
`root=<character>/PhysicsModel/Genesis2Female`, `skin=True`, `draw=True`, `method=GPU`, against the
same skeleton the report measures - 86 bones under the root, from 108 `DAZBone` components of which
84 are in active objects - and no bone name fails to resolve. The scene does
carry a second `Genesis2Female` copy, and `DAZSkinV2.InitBones` is fed by
`DAZCharacter.rootBonesForSkinning`, which comes from `DAZCharacterSelector.rootBones` - a serialized
prefab field no code assigns. All of that is true, and all of it is a dead end: the skin follows the
skeleton it was given, correctly. The `Genesis2Female.Shape` `SkinnedMeshRenderer` (24 119 verts, 80
bones) sits in the hierarchy switched off and is not drawn at all, so nothing there can be the
detached skin either.

**The cause.** `DAZSkinV2` deforms the body on the GPU and hands the result to a material -
`GPUmaterials[i].SetBuffer("verts", rawVertsBuffer)` (`DAZSkinV2.cs:4751`), then
`Graphics.DrawMesh(mesh, ..., GPUmaterials[i], ...)` (`:4778`) - with the ordinary `MeshRenderer`
switched off so nothing else draws the body. `SkinMeshGPUMaterialInit()` (`:1901`) prepares that
material by swapping its shader for `Shader.Find(shader.name + "ComputeBuff")` (`:1929`). Those
materials exist and are named in the report:

```
GPUmaterials=30: Custom/Subsurface/GlossNMTessMappedFixedComputeBuff, ...
```

but **all 128 `.shader` files in the project are AssetRipper placeholders**. Every one carries
`//DummyShaderTextExporter`, not one declares a `StructuredBuffer`, and not one has a `verts`
property - so `SetBuffer` binds to nothing, silently, because a `Material` accepts any property name.
The stub's vertex shader transforms the mesh's *own* `POSITION` attribute and returns `_Color` with no
lighting code. The body on screen is therefore the bind-pose mesh, flat. The hair escapes the problem
because it is drawn from meshes that already sit in their root's frame, so it moves rigidly with the
skeleton rather than being deformed by a shader that ignores its input.

**Why it cannot simply be fixed by editing something.** VaM ships shader *bytecode*, not source.
`ShaderExportMode = Decompile` (`docs\asset-export.md:48`) was chosen over the default and still
produced these stubs: AssetRipper reconstructed each shader's property list and its stage inputs from
the compiled reflection, which is enough to name the material properties and see one `POSITION` input
and one `TEXCOORD0`, and not enough to see a vertex id or a structured buffer. The export itself
carries no shader blobs - a text search for `DXBC`, `m_SubPrograms` and `m_CompileInfo` across
`ExportedProject` returns nothing. The bytecode has not vanished from the world, though: the installed
game's `VaM_Data\globalgamemanagers.assets` contains 64 `DXBC` headers alongside the `ComputeBuff`
shader names, so the original compiled shaders are still on disk if anyone wants them back. The
compute shaders that *do* work survive for a different reason - they are different objects, and
`Assets\Resources\compute\*.asset` holds 140 real serialised `ComputeShader` assets with real DXBC
blobs.

**The three routes out**, with what is actually known about their cost:

| Route | What it means | Cost |
|---|---|---|
| **Replace the `*ComputeBuff` shaders** | The contract is exactly specified: `verts` (stride 12), `normals` (stride 12) and `tangents` (stride 16) `StructuredBuffer`s indexed by vertex id, drawn with an identity object matrix (`DAZSkinV2.cs:4746-4778`), so the buffers already hold **world-space** positions. A replacement reads `verts[SV_VertexID]`, multiplies by `UNITY_MATRIX_VP`, and shades. The materials' own property lists survive in the exported stubs, so `_MainTex`, `_BumpMap`, `_SpecInt`, `_Shininess`, `_SubdermisColor` and the rest keep their meanings. Only the lighting model has to be invented | 8 distinct ComputeBuff shaders carry the character; the same work makes cloth, hair and colliders pose too. Approximate look, correct movement |
| **Recover the original bytecode** | The compiled DXBC blobs *do* survive in the installation - `VaM_Data\globalgamemanagers.assets` holds 64 `DXBC` headers and the `ComputeBuff` shader names. Extract them, decompile to HLSL, rebuild the Unity shaders | Most faithful, and the source of truth is on disk; but needs DXBC tooling and a DXBC-to-HLSL decompiler, then hand-conversion into Cg |
| **Use Unity's own skinning** | The rip already has `Genesis2Female.Shape` as a real `SkinnedMeshRenderer` (24 119 verts, 80 bones, `Standard` materials - a *built-in* shader, so it lights correctly): enable it and stop VaM's GPU draw for the body | Minutes, not days; the body moves and is lit, without the morph/UV-morph pipeline - coarse, but enough to finish stage-5 verification |

Note that the body is not the only victim: the 120 plain stubs are drawn with correct geometry but
**no lighting at all**, so the whole scene renders flat. Anything that is not a Unity built-in shader
is unlit. A second, separable tier of work - giving the plain stubs a real lit surface shader while
keeping their exported property names - is what the scene as a whole needs.

Until one is chosen, the visual comparison below is bounded by this: **movement, lighting and
material response on the body cannot be verified against the original**, because the rebuild does not
currently possess the machinery that produces them.

## The four reported look defects, triaged

The question that opened this triage was whether the defects are simply shaders that have not been
ported yet. The census answers it with two populations, and the split runs straight through the
character: the body skin sits on **our** reconstructions while the hair and eye-reflection items sit
on **bundle copies the project never defined**. The cloth only *names* a bundle family - it is drawn
through our reconstructed `*ComputeBuff` twin, and that is where its defect turned out to live.

The census predates the hair transcription, so its third column records where a family came from at
the time of the run, not the present state; the hair has since moved to the project column, which is
why the fixes below are in our generator rather than in a census entry.

| Population | Families (slots) | Origin |
|---|---|---|
| body skin | `GlossNMTessMappedFixedComputeBuff` (14), `GlossNMCullComputeBuff` (26), `CullComputeBuff` (8), `GlossCullComputeBuff` (7), `AlphaMaskComputeBuff` (2), `Transparent{Gloss,}ComputeBuff` (3) | project |
| hair item | `Custom/Hair/MainSeparateAlphaLayer1` (7), `..Layer2` (6), `..Layer3` (6) | bundle only then, **project** now |
| cloth | `Custom/Subsurface/TransparentGlossNMDetailNoCullSeparateAlpha` (10), swapped to our `…ComputeBuff` twin at draw time | bundle, drawn by project - see defect 4 |
| eye reflection | `Marmoset/Transparent/Simple Glass/Specular IBLComputeBuff` (2, 6 passes) | bundle only |

This supersedes the earlier reading in this file that the body is unlit and that the whole scene
renders flat: the body's slots resolve to project shaders with their passes intact, and the reported
symptoms - gloss and bump *reacting* to light, only disagreeably - are not symptoms of an unlit
surface.

Evidence is `artifacts\census3-play.report.txt`, produced by `ShaderCensus()` in
`VaM_Rebuild\Assets\Editor\RebuildGate.cs`, which resolves every material a draw path reaches to its
shader, pass count and origin. See section 3 for the command.

### Defect 1 - gloss and bump seams where Torso meets Head, Neck and Forearms

Verdict: **the shading math is not the cause.** Compiling both families and both stages against the
shipped bytecode shows the split computes the same shading on either side of the boundary, so it
cannot open a seam. The one behavioural difference the split genuinely carries - the original
tessellates the `TessMappedFixed` submeshes and our build did not - has since been closed (see
*The tessellation stages* below); the geometry is now subdivided on both sides of the boundary, so if
the seam survives it is data, or it is in the original too. The last shader-side deviation this
section used to name - the additive pass - has since been both re-derived and repaired (see *The
additive pass's alpha slot* below and Done 23), and it was identical on both sides of the boundary
anyway, so it never could open one.

The body is a single mesh of 28 submeshes, and those submeshes do not all land on the same family:

| Submeshes | Shader | Origin |
|---|---|---|
| `Torso`, `Legs`, `Nipples` (and their `-1` copies) | `Custom/Subsurface/GlossNMTessMappedFixedComputeBuff` | project, 3 passes |
| `Head`, `Neck`, `Face`, `Forearms`, `Hands`, `Ears`, `Lips`, `Sclera`, `Irises`, `Teeth`, `Tongue` | `Custom/Subsurface/GlossNMCullComputeBuff` | project, 3 passes |
| `Gums`, `InnerMouth`, `Lacrimals`, `Pupils` | `Custom/Subsurface/CullComputeBuff` | project, 3 passes |

The seams are exactly the boundaries of that first row - the shoulder (Torso against Forearms) and
the back and throat (Torso against Neck and Head), so the family split was the first suspect. It has
since been refuted by compiling both of our generated families with `fxc` and comparing the
programs, and by comparing our program against the shipped one:

* **Our two families compile to the same program.** FORWARDBASE pixel 180 instructions on both
  sides with differences only in `cb0` offsets (the `_Tess*` properties shift our own layout);
  FORWARDADD 106/106 with two offset-only differences; the vertex program 43/43 with zero
  differences. Identical programs cannot shade a shared edge differently, so the split cannot
  produce the seam.
* **Our program is a faithful port of the shipped one.** Against the original
  `PixelSM40_DIRECTIONAL-MARMO_LINEAR` blob (201 instructions) the only material difference is the
  documented omission of the lightmap indirection: the original's `if_nz` probe-volume block with
  its `cb3[0..6]` uniforms and `cb2[46]` (Unity's baked occlusion) is 21 instructions, which is
  exactly the 201-180 delta. Every other register maps one-to-one onto ours - `cb0[71]` subsurface
  to our `cb0[9]`, `cb0[72]` bumpiness to our `cb0[7]`, `cb0[73..74]` IBL to our `cb0[18..19]`,
  `cb0[86..94]` (`_SH0.._SH8`) to our `cb0[21..29]`, `cb2[39..46]` to our `cb2[39..45]`. Both
  programs reference 21-22 `cb0` registers, i.e. the same property set.
* **The tangent frame is fed correctly.** `VAM_NO_TANGENTS` is defined for exactly one pass of the
  Cull family (SHADOWCASTER, whose original binds no tangent buffer); FORWARDBASE and FORWARDADD
  read `t2`, so the normal map is not evaluated in a fallback frame.
* **The additive pass has been re-derived too, and its alpha slot repaired.** Its per-light response
  was compared against the shipped additive blob instruction by instruction (Done 23) and its alpha
  slot turned out to be the one thing in it that was wrong: it writes the surface alpha wherever the
  pass blends `SrcAlpha One`, and a constant only where it blends `One One` - the body's two families
  are both `One One`, so this was never a candidate for the seam either way.
* **Material assignment cannot mix neighbours up either.** `MeshGPU.InitMaterials` sizes
  `GPUmaterials` from `dazMesh.materials.Length`, and both draw loops
  (`MeshGPU.DrawMesh`, `DAZSkinV2.DrawMeshGPU`) pass the same index as both the material slot and the
  submesh index to `Graphics.DrawMesh`, gated by `materialsEnabled[i]`. Submesh `i` therefore always
  gets `GPUmaterials[i]`, so two adjacent submeshes can never swap materials.

The split is not inert, though, and the part of it that is still missing is worth naming here because
it is easy to mistake for the cause:

* **The tessellation stages are reconstructed now.** The shipped `TessMappedFixed` family carries a
  hull (`hs_5_0`, 3 control points, `domain_tri`, `partitioning_fractional_odd`) and a domain
  (`ds_5_0`) program on its passes, and five shaders carry them on all three passes each - the four
  other `Custom/Subsurface/*TessMapped*` shaders are the same pipeline with a different pixel
  program, so 15 passes in total. Both stages are transcribed register by register into
  `shader-src\VamGpuSkinning.cginc` (the *Tessellation* section) and the passes are emitted with
  `#pragma hull` / `#pragma domain` and `#pragma target 5.0`, which is the model the original's hull
  and domain were compiled for. *The tessellation stages* below records what was decoded and how it
  was checked.
* **The pipeline behind the buffers is the original's, not ours.** `DAZGPUSkin` and `MeshGPU` are
  loaded as shipped from the `z_sha` bundle by `MeshVR.VamComputeShaderProvider`, so the Laplacian
  smoothing (`_useSmoothing` is true on the drawn skin - census `smoothing=True`), the
  Laplacian/HC/spring passes, `RecalculateNormals` and `RecalculateTangents` all run the original's
  own kernels and neighbour tables; `MeshSmoothGPU` fails loudly rather than silently if a kernel is
  missing, and the gate reports no such error.

What was not measured, and is what the seam now has to be: the per-material values the active
character's 30 `GPUmaterials` entries actually carry, and the handedness (`float4.w`) of the skinned
tangent buffer that `DAZSkinV2` writes.

`VamSkin()` also used to Gram-Schmidt-orthogonalise the tangent against the normal, and **the shipped
programs do not**: every keyword variant of the non-tessellated vertex stage spends exactly one
`dp3`/`rsq`/`mul` on the tangent and hands it to the bitangent cross as it comes, the pixel stage never
re-orthogonalises, and the hull forwards `tangents[vid]` unreformed (see *The tessellation stages*).
The reconstruction now normalises and stops, so the rotation the seam was charged to is gone. Whether
the seam moved is a hand check, not a measurement.

#### Both of those are now measured, and both are clean

`RebuildGate` reports them for the drawn skin every run, so they no longer have to be guessed at.

* **The 30 `GPUmaterials` slots hold the right maps.** `AppendSubjectTextures` averages `_MainTex`,
  `_DetailMap`, `_GlossTex` and `_BumpMap` on the GPU - a name in the slot table cannot answer
  whether the texture behind it holds pixels, because a map that failed to decode or was generated at
  runtime and never filled in keeps its name and still averages `(1, 1, 1)`. The result is the
  reverse of the expectation the seam had been attributed to:

  | Slot | `_MainTex` | average | reads as |
  |---|---|---|---|
  | `Legs-1` (renders correctly) | `Lexi_LimbsD` | `(0.64, 0.43, 0.38)` | warm skin |
  | `Torso-1`, `Hips-1`, `Neck-1`, `Head-1` | `Lexi_TorsoD` | `(0.64, 0.41, 0.33)` | warm skin |
  | `Face-1`, `Nostrils-1`, `Lips-1` | `Lexi_FaceD` | `(0.68, 0.40, 0.31)` | warm skin |
  | `defaultMat-1` | `Lexi_GenitalsD` | `(0.64, 0.41, 0.36)` | warm skin |

  The gloss maps average `0.04-0.07` (glossy, as skin should be) and the normal maps `(1.00, 0.49,
  0.49)` - intact DXT5nm with `y` and `z` packed in `.g`/`.a`, which is what a correct bump map
  averages. The one slot that *is* near-white is `Cornea-1` at `(0.99, 0.99, 0.99, 0.99)`, and that
  is `S6EyesTr`, a transparency mask, whose family no longer shades at all (Done 18).

  So the suspicion that `Lexi_TorsoD` is generated blank and the submeshes sharing it go neutral is
  **refuted**: the map the broken submeshes sample carries the same warm skin as the map the working
  legs sample. (It also closes the question of where those names come from - they are runtime
  compositor names, not literals in the recovered code, and not on disk or in any `.var`.)

* **The tangent basis is correct where the seam is.** `TangentReport` compares `drawTangents` and
  `startTangents` with the mesh's own tangents, and reads back the `_tangentsBuffer` and
  `delayedTangentsBuffer` that the shaders actually sample.

  ```
  drawTangents[24928]: w>0=0, w<0=24928; not perpendicular to the normal=0/24928; differ from the mesh=156/24928
  _tangentsBuffer (GPUSkin)[25088]: w>0=45, w<0=24883, w=0=160
  _tangentsBuffer: by world height - below the hip 181, hip 19, torso 3, neck and head 2
  ```

  Every CPU tangent is left handed, so nothing is inverted in the staging arrays; the buffer has 205
  slots that are not left handed, and **181 of them are below the hip - the region that renders
  correctly - against 3 in the torso and 2 in the neck and head.** A bad binormal would have to be
  concentrated along the shoulder to explain a seam there; it is concentrated in the legs instead, and
  the legs are the good part of the render. The 160 `w=0` slots sit past the end of the drawn
  vertices and are the buffer's padding. Handedness is therefore **not** the cause either.

With the shaders refuted (above), the materials refuted and the tangent frame refuted, defect 1 has no
remaining suspect on the skin at all. What the same run *does* show is where the brightness the defect
is described in terms of actually lives. Sampling the character's own column of the frame by world
height, the pixels that are pure white (`min(RGB) >= 235`) and the median colour of the pixels around
them:

| Band | white | median of the skin-coloured pixels |
|---|---|---|
| legs `0.20-0.70` | 0.00 % | `(245, 168, 128)` |
| hip `0.80-1.05` | 6.94 % | `(255, 187, 146)` |
| torso `1.10-1.45` | 11.15 % | `(255, 225, 169)` |
| neck and head `1.50-1.75` | 13.81 % | `(255, 198, 153)` |

The legs are the only band with no blown pixels and the only band whose median is not clipped - red
reaches 255 in every band above them. Whatever is over-bright is **additive and grows with height**,
which is not how a wrong texture or a wrong tangent frame behaves; both would be uniform over the
submesh. The two things that grow with height are the garments (a near-white top and panty, averages
`0.91` and `0.89`) and the hair, and the hair is the one that is not in the project:

```
[bundle] Scalp-1 : shader Custom/Hair/MainSeparateAlphaLayer1 | origin=bundle only (not in project) | passes=3
```

`New-VaMShaders.py` carried `UNTRANSCRIBED_FAMILIES = ("Custom/Hair/", "Marmoset/")` at the time of
this run, so the hair drew from the bundle's own shader and had no asset in the project at all - fine
on a machine with the installation, and exactly what a standalone build would lose. **Superseded:** the
hair is transcribed now (`UNTRANSCRIBED_FAMILIES = ("Marmoset/",)`, 14 of its shaders generated), and
it is the one family whose pass 0 turned out to be a mask - see *Found by reading the bytecode*. The
original run's reading therefore stands only against the bundle shader, which is the game's own and
cannot itself be the over-brightness.

#### The seam probe - every candidate but one, measured away

The slot dump answered *what the values are*; the seam probe answers *what they do to the picture*. It
lives in the editor gate (`SeamProbe` in `RebuildGate.cs`), freezes one frame of the Halloween scene,
and for each override writes one scalar across every material whose shader declares it, re-renders,
and reports both the changed pixels and a continuous `mean |rgb| distance` against the frozen
baseline; a second pass moves the camera 0.55 m from the closest drawn **vertex pair between a
tessellated and a plain part of the same skin**, which is the only place a joint is actually visible.
Three corrections were needed before it measured anything: the scene's own lighting left the frame at
a mean of **0.075**, so the probe now adds a directional light on the camera axis; a pixel count with
no magnitude hides a 5 % rewrite of a frame, so the mean distance sits beside it; and the close-up
aimed down the torso's mean normal - the shoulder covered by the arm - until it was moved onto the
horizontal axis `edge - skin.position`.

| override | slots | changed px | mean abs diff/1000 |
| --- | --- | --- | --- |
| `_Tess=0` | 7 | 962 (0.37 %) | 0.29 |
| `_Tess=8` | 7 | 239 (0.09 %) | 0.07 |
| `_TessPhong=0` | 7 | 967 (0.37 %) | 0.29 |
| `_TessPhong=1` | 7 | 397 (0.15 %) | 0.13 |
| `_SpecularBumpiness=0` | 16 | 16 (0.01 %) | 0.01 |
| `_SpecularBumpiness=4` | 16 | 480 (0.18 %) | 0.17 |
| `_DiffuseBumpiness=0` | 22 | 73 (0.03 %) | 0.04 |
| `_DiffuseBumpiness=4` | 22 | 5408 (2.06 %) | 0.80 |
| `_SpecInt=0` | 29 | 22514 (8.59 %) | 2.74 |
| `_SpecInt=20` | 29 | 26895 (10.26 %) | 8.25 |
| control (every property back) | - | **0** | **0.00** |

Close up at the Torso-to-Forearms joint: `_TessPhong=0` **4398 px / 1.53**, `_Tess=0` **4388 px /
1.53**, `_TessPhong=1` 1049 px / 0.63, `_SpecInt=0` 123908 px / 25.82, control 0 px / 0.00. Three
readings, each from a number rather than a reading:

* **The tessellation lever is one lever.** Zeroing the density and zeroing the Pn projection move the
  frame by the same amount (962 against 967 px, 4388 against 4398), and doubling the density moves it
  by a quarter of that. Subdivision on its own paints nothing; only the projection of the position
  does, and `_TessPhong` behaves as the linear blend weight its `lerp` says it is.
* **No scalar differs across the joint.** The slot dump already found the per-part values identical
  (Done 19), and the one family-level knob that differs, the bumpiness pair, is worth 0.01 to 0.80 of
  1000 - a factor of 30 to 100 below the tessellation lever. `_SpecInt` is the only lever with real
  weight and it is not a mechanism.
* **What is left is a difference in the code, not in the data.** The plain path transforms the buffer
  normal and tangent **per vertex** (`UnityObjectToWorldNormal(normals[v.vid])`,
  `UnityObjectToWorldDir(tanIn.xyz)`) and lets the rasteriser interpolate world-space values; the
  tessellated control point binds no object-to-world matrix at all, forwards `verts[]`/`normals[]`/
  `tangents[]` untouched in object space, and the domain interpolates them barycentrically and
  transforms them per fragment. `normalize()` does not commute with interpolation, so the two families
  disagree by a small rotation of the TBN at the same surface point. That is a smoothing function's
  error in diffuse, which is a smooth function of the normal, and a visible step in a highlight, which
  is not - which is exactly the reported shape of the defect.

**An absolute "step" metric was built and abandoned**, and the reason is worth keeping: with the skin
filling the frame, every row contains some large jump - median row maximum **2.953 of 3.0** at a 9-px
span, 307 of 307 rows - because eyelashes, brows and blown highlights share the picture. The close-up
pass therefore reports differences between frames, not steps within one.

**The next measurement is a disassembly**, and it is bounded: dump the shipped **domain** program of a
tessellated family (`Custom/Subsurface/GlossNMTessMappedFixedComputeBuff`) and compare it with
`VamTessVaryings` - whether the shipped domain transforms the interpolated object-space normal and
tangent, as this project reconstructs it, or whether the hull transforms the control points and the
domain interpolates world-space values, which would make it agree with the plain family and make the
reconstruction itself the seam. Evidence: `artifacts\seam1..seam5.seam.txt` with their frames; the
absolute numbers above are `seam5`.

### Defect 2 - the scalp patch floating in the air

Verdict: **the hair-card draw path, plus the three hair families being absent from the project.**

Two facts, and only the second one explains a scalp that is visible at all:

- Every hair submesh - `Scalp-1`, `HairHolder1-1`, `HairHolder2-1`, `Top1-4-1`, `TopInner1/2-1`,
  `Tail1/2-1`, `TailThin1/2-1` - belongs to `VictoriaElitePonytailHairMain`, whose `dazMesh` is
  `geometry` but whose **`morphedUVMappedMesh` is `NULL`**. That node is also inactive. A skin with no
  morphed mesh has no geometry to draw, so these submeshes cannot be the patch on screen.
- What does draw is the hair cards, on the project-defined `GPUTools/MeshedVR/HairOpt` reached as a
  bundle copy, from `Sim2Hair/Sim2Hair/Head/Styles/Long + Straight/HairSettings/Render` and
  `RenVR:Simone (REN)/Render`. `DAZHairMesh` draws them with
  `Graphics.DrawMesh(mesh, Matrix4x4.identity, hairMaterialRuntime, ...)`, and its `init()` takes the
  strand and scalp vertices from the *skin's* local arrays without re-anchoring them to the item's
  transform. Anything whose input positions were lifted without that re-anchoring lands at the world
  origin instead of on the head - which is what a patch hanging in mid-air, cut off from the body,
  looks like.

### Defect 3 - eyelash materials and the shiny eye

Verdict: **mixed.** Three of the parts are ours - all three porting gaps, all three fixed (the lash
mask, the cornea family, and the additive pass's alpha slot, the last of which is what the white iris
on male 1 turned out to be); the fourth is the original shader.

#### The lash alpha mask - **fixed and confirmed by hand**

*Symptom:* the lashes render as **solid black cards** - the whole fringe is an opaque shape instead of
separate strands.

*Cause:* the shared include built the surface alpha out of the **diffuse map** and then **added** the
alpha mask on top of it, instead of letting the mask *replace* it:

```hlsl
alpha += tex2D(_AlphaTex, i.uvMain).r + VAM_AlphaAdjust;   // before
```

`Eyelashes-1` (census line 799) has `_MainTex=None`, which the generator stubs to `float4(1,1,1,1)`, so
that line evaluated `1 * 1 + mask.r + 0` and saturated to a constant `1`: the mask was read but could
never win. Two further errors hid behind it - the mask was read from the **`.r`** channel and sampled
with the **diffuse map's** UV set, while the shipped code reads **`.a`** at the mask's **own** `_ST`.

*What the shipped bytecode does* (obtained by disassembling the lash's `DIRECTIONAL` + `MARMO_LINEAR`
fragment; `_AlphaAdjust=cb0[68].x`, `_Cutoff=cb0[100].x`):

```hlsl
sample _MainTex  -> sample _AlphaTex at its own UV
add_sat r3.w, r3.w, cb0[68].x        ; saturate(_AlphaTex.a + _AlphaAdjust)
mul     r2.xyz, r2.xyzx, r3.wwww     ; premultiply RGB by alpha
add     r1.w, r3.w, -cb0[100].x      ; - _Cutoff
lt / discard_nz
mov     r5.w, l(0)                   ; o0.w = 0
```

So the mask **replaces** the diffuse alpha (no `_Color.a * _MainTex.a` term at all), it is read from
`.a`, from its own UV set, the RGB is **premultiplied** by it, and the written alpha is forced to `0`.
That is not a lash quirk: **all 27 families that declare `_AlphaTex` read `.a`/`.w` and combine it with
`_AlphaAdjust` via `add_sat`**, and every one of them premultiplies in a discarding pass.

*The fix* is four edits to the one git-tracked place a shader fix belongs,
`shader-src\VamGpuSkinning.cginc`, behind the existing per-property macros so no other family changes
behaviour:

- a `VAM_SAMPLE_ALPHA(uv)` / `VAM_UV_ALPHA(uv)` macro pair under `VAM_HAS__AlphaTex`, with `0.0` / `uv`
  stubs in the `#else`;
- `float2 uvAlpha : TEXCOORD9` in `vam_v2f` (with `UNITY_SHADOW_COORDS` bumped to `10`), filled by
  `VamPack`;
- the alpha block in `VamSurface` rewritten to the shipped formula, including the premultiply;
- the no-mask path keeps `VAM_Color.a * diffuseTex.a`, now also `+ VAM_AlphaAdjust` - which is what the
  shipped `Custom/Hair/MainAlternate*` and `Custom/Subsurface/AlphaMask*` families do.

*Verification so far:* `python tools\check_shaders.py` compiles every program it generates -
**4414/4414**, 0 failed - and `python tools\verify_twins.py` still proves all **112** of the twin
families' pixel passes the same instruction stream up to renaming, 0 different. The lash fragment we
emit now disassembles to the shipped instruction sequence - `_AlphaTex` is sampled at its own UV
(`v3.zw`, previously the diffuse `v1`), and the `add_sat` / premultiply / `discard` chain is present.
The sampler register numbers differ from the shipped side and always will, because ours are laid out by
the generator; only `contract.json` names them. **A visible run confirmed it**: `Invoke-ManualPlay.ps1`
on the game's own boot scene draws the cards as strands instead of solid planes.

#### The cornea - **fixed and measured**

*Symptom:* the eye reads as **over-bright** instead of as a dark, mostly transparent cornea.

*Cause:* the family had never been transcribed. `Cornea-1` is on the project's
`Custom/Subsurface/AlphaMaskComputeBuff` (census: `queue=2001`, `_Color=(0.80, 0.80, 0.80, 0.47)`,
`_MainTex=S6EyesTr`), which the generator classified as one of the skin families and therefore shaded
with the shared model. The shipped fragment is not a shading model at all:

```hlsl
sample r0.xyzw, v1.xyxx, t0.xyzw, s0
mad_sat o0.w, r0.w, cb0[69].w, cb0[68].x   ; saturate(_MainTex.a * _Color.a + _AlphaAdjust)
mov o0.xyz, l(0,0,0,0)                     ; RGB is hard black
```

Three instructions - and the additive pass is four: it samples `_MainTex` at a **constant `(1, 0)`**
and writes the same black-plus-alpha. Constant because that pass has no uv of its own, which is also
why the shipped vertex signature of that pass carries no `TEXCOORD 2` while the base pass does.

The family declares exactly three properties (`_Color`, `_MainTex`, `_AlphaAdjust`), so it reads none
of the inputs the shared model needs, and its passes write the whole RGBA target (`colMask 15`) - so
nothing suppressed the shading our version produced: our pass 0 compiled to **74 instructions** with
`mul o0.xyz, r0.xyzx, cb0[11].wwww` and the rest of the lighting stack, and pass 1 *added* a second
shaded colour under `Blend One One`. That is the over-bright eye.

*The fix* is a mask-only fragment in the same include, `VamFragmentMask`, chosen by `mask_only()` in
the generator rather than by a property test or a name table. The render state cannot tell this
family's passes from any other transparent pair, and the property list cannot tell it from the diffuse
IBL families, which are equally short of inputs - but the pass's own fragment program can: a pass that
shades reads many constants, a mask reads at most a colour tint and an alpha adjustment. The rule is
therefore `LightMode in (FORWARDBASE, FORWARDADD)` and `$Globals ⊆ {_Color, _AlphaAdjust}`, measured
over every emitted pass as selecting exactly **5 of 136** - this family's two passes and the hair's
three, with the nearest non-match reading **29** globals. It keeps the shipped formula,
`saturate(_MainTex.a * _Color.a + _AlphaAdjust)`, writes zero to RGB, and samples at the interpolated uv
in the base pass and at `VAM_MASK_CONSTANT_UV` in the additive one. The rule found the hair's
under-layer passes as a side effect; that is written up in *Found by reading the bytecode* below.

*Verification so far:* both of the passes we now emit disassemble to the shipped sequence - `sample` /
`mad_sat o0.w` / `mov o0.xyz, l(0,0,0,0)` - with pass 1 carrying the same literal
`l(1.000000, 0.000000, 0.000000, 0.000000)`. Ours reads `t2` where the shipped program reads `t0`, and
`cb0[0]`/`cb0[2]` where it reads `cb0[69]`/`cb0[68]`; those are the generator's own layout, as with the
lash. **Measured** (below): the layer paints nothing on its own. The Danika cornea
(`_Color.a = 0.47`, `_MainTex = S6EyesTr`) also measures **0 px** for `only Cornea`, and its
`no-Cornea` frame differs from the baseline by **2153 px** of ≤ 0.003 in a colour whose queue is
`2001` - that is the transparent sort seeing one slot fewer, not a draw, and it is the one number in
this section that is an observation rather than a measurement.

#### The additive pass's alpha slot - **fixed and measured**

*Symptom:* male 1's iris and pupil read **white** (the user's report, and the one skin it shows on).
The eye probe measured the frame around the pupil at `(0.573, 0.477, 0.141)` where the original has
to paint nothing.

*Cause:* the include's `VamFragmentAdd` wrote the constant `1.0` in the alpha slot, and **not every
shipped additive pass does**. The additive pass is `Blend SrcAlpha One` in the families that blend,
so its alpha is not padding there, it is the weight of the colour it adds, and the shipped programs
in those families write the **same** expression the base pass writes -
`saturate(texelA * _Color.a + _AlphaAdjust)`:

```hlsl
; Custom/Subsurface/TransparentComputeBuff, add pass (blob 084)
mad_sat o0.w, r2.w, cb0[69].w, cb0[68].x   ; cb0[69].w = _Color.a, cb0[68].x = _AlphaAdjust
; Custom/Hair/MainSeparateAlphaMimicAlphaComputeBuff, pass 1
add_sat r1.w, r3.w, cb0[68].x
mov     o0.w, r1.w
; Custom/Subsurface/TransparentSeparateAlphaComputeBuff, pass 1   (_AlphaTex family)
add_sat o0.w, r1.w, cb0[68].x
```

`Cornea-1` on male 1 is `_Color.a = 0`, `_AlphaAdjust = 0`, `_MainTex = None` (Unity binds white, so
`texelA = 1`), i.e. its alpha is **0 in both passes** and the original's additive pass adds
`colour * 0`. Ours added `colour * 1` under a blend that multiplies by alpha, so the whole lens
painted its own colour on top of the iris.

*How the two groups are told apart:* by the pass's own blend state, not by the family name -
`srcBlend ∈ {5 SrcAlpha, 9 SrcAlphaSaturate}` identifies exactly the **8 families, 16 generated
shader files** that write the surface alpha (the two `Custom/Hair/Main*SeparateAlpha*` pairs, the two
`Custom/Subsurface/Transparent*` pairs); the other additive passes are `One One` and do write the
literal `mov o0.w, l(1.000000)` that our constant stood for. The disassembly cannot be the
classifier: `Custom/Subsurface/Cull` pass 1 is `Zero Zero` and its blob **never writes `o0` at all**,
so "no `mad_sat`" is not the same question as "alpha does not matter".

*The fix* is a define rather than a second fragment: `scripts/New-VaMShaders.py` emits
`#define VAM_ADD_ALPHA_SURFACE` on an additive pass whose `srcBlend` reads alpha, and
`shader-src\VamGpuSkinning.cginc`'s `VamFragmentAdd` returns `s.alpha` under it and `1.0` otherwise.
The 5 non-`SrcAlpha` families that also declare `VamFragmentMask` are untouched by construction: a
mask pass has no additive variant.

*Verified by measurement* - the eye probe run before and after, same scene, same skin, frozen pose
(`artifacts\eyeframes3.eyeframes.txt` and `artifacts\eyeframes4.eyeframes.txt`):

| probe line, male 1 | before | after |
|---|---|---|
| `only Cornea, against the empty skin` | **32470 px** `(0.573, 0.477, 0.141)` | **0 px** |
| `without Cornea` | 10216 px `(0.586, 0.482, 0.004)` -> `(0.135, 0.097, 0.004)` | 0 px |
| `baseline` pupil region | `(0.496, 0.372, 0.008)` | `(0.163, 0.103, 0.007)` |
| `all-layers-back-on` against the baseline (the control) | 0 px | 0 px |

The two frames are not merely close: `only-Cornea.png` hashes **identically** to `nothing.png`, and
`no-Cornea.png` to `baseline.png` - the cornea now draws exactly as much as the original's does,
which is nothing. The control staying at 0 px says the probe itself did not drift between the runs,
and the shader gate reports **7479/7479 programs, 0 failed**.

*One deliberate deviation:* our vertex stage fills `uvMain` through `_MainTex_ST`, while the shipped
program binds no `_MainTex_ST` at all - fxc reports it unused. The two agree wherever a material leaves
its tiling at the default, which the VaM eye material does.

#### The third part

- `EyeReflection-1` is on the bundle-only `Marmoset/Transparent/Simple Glass/Specular IBLComputeBuff`
  with 6 passes, i.e. the original game shader, so its intensity is not a porting gap; it depends on
  the environment the scene sets (`_SpecCubeIBL=SkyCyber2SPEC`, `_ExposureIBL=(0.1, 0.02, 0.002, 1)`)
  and has to be compared against the original before anything is changed.

### Defect 4 - cloth with an inverted colour and no alpha - **fixed** (`e956c78`)

Verdict: **the family was transcribed all along; the generator read the colour-write mask backwards.**
The first reading of this defect was wrong in both halves, and both are worth keeping as refutations:

- *"`_AlphaTex` is never assigned"* - **refuted**. The bundle that ships the clothing,
  `StreamingAssets\c_heu_mat` (66 814 745 B), holds the 13 real `hu_*` materials and every one of them
  has `_AlphaTex = None`; the shipped fragment program of this family samples it regardless
  (`_MainTex, _AlphaTex, _BumpMap, _DetailMap, _SpecTex, _GlossTex`). An unbound sampler returns Unity's
  white default, so alpha is 1 and the garment is **opaque by design** - our shader does exactly the
  same thing for exactly the same reason.
- *"the family is absent from the project"* - true only of the *plain* family
  `Custom/Subsurface/TransparentGlossNMDetailNoCullSeparateAlpha`. The garments are drawn by
  `DAZSkinWrap`, which asks `MeshVR.VamShaderProvider.FindComputeBuff` for the `*ComputeBuff` twin, and
  that twin **is** one of the 51 reconstructions. The cloth was on our own shader the whole time.

The cause is one line of the generator. `scripts\New-VaMShaders.py` decoded the serialised render
state's colour-write mask as `(1,R) (2,G) (4,B) (8,A)`. Unity's `ColorWriteMask` reads out of the
engine we build with (`UnityEngine.CoreModule.dll`; the values were read under 2018.1.9f2 and both hops -
2018.4 and 2019.4 - left them as they were) as `Alpha = 1, Blue = 2, Green = 4,
Red = 8, All = 15`. This family's passes serialise as **14**, i.e. **RGB**, and we emitted
**`ColorMask GBA`** - so the shader never wrote the red channel and the red of whatever lay behind bled
through. That is the whole symptom: an inverted colour that also reads as transparency, because the
channel that should have carried the cloth carried the body instead.

The shipped bytecode supports the reading and not the old one. The base pass writes all four channels
(`mad o0.xyz, ...` / `mov o0.w, ...`) with `SrcAlpha`/`OneMinusSrcAlpha`, so the mask decides only what
is *stored* - alpha still drives the blend - which is what makes "RGB but not A" the coherent state for
a blended pass. The disassembly is kept at
`artifacts\cloth-asm\Custom_Subsurface_TransparentGlossNMDetailNoCullSeparateAlpha\` (34 programs, one
per keyword variant, from `python tools\dump_dxbc.py Custom_Subsurface_TransparentGlossNMDetailNoCullSeparateAlpha --stage fragment --asm --out artifacts\cloth-asm`;
`…_064_…DIRECTIONAL-MARMO_LINEAR.dxbc.asm` is the one read most), and the contract that enumerates the
passes and their texture bindings is
`artifacts\shader-blobs\Custom_Subsurface_TransparentGlossNMDetailNoCullSeparateAlpha\contract.json`.
The bit order fits the whole corpus too: across the 135 extracted contracts the mask
takes three values, `0` (3 passes), `14` (109) and `15` (3016), and **every** `14` pass belongs to the
`RenderType=TransparentDynamicVertices` family - the GPUSkin, alpha-mask and transparent-subsurface
passes that composite with camera colour - while the ordinary `Opaque` and `TransparentCutout` passes
keep `15`. Dropping red from `TransparentDynamicVertices` would have made VaM's hair, lashes and cloth
colour-blind in red; dropping only alpha *storage* from a blended pass is what the blend wants.

Scope of the fix: three lines of generator, and 44 pass states moved from `GBA` to `RGB` - every
`SeparateAlpha`, `Transparent` and `Cutout` family. The `GlossNMCull`-style skin families serialise as
`15`, so they were never affected, which is why only the clothing, the lashes and the transparent skin
layers looked wrong. Shaders are generated, so the fix is a regeneration plus the compile gate: **0
errors**. `python tools\check_shaders.py` re-run after the fix assembles every program of the set as
it stood then (`3023/3023`, 0 failed), and the user's own manual run on
`Saves/scene/MeshedVR/default.json` confirms the garments.

One real gap is left open, and it has no effect on colour: the shipped program also samples
`_DetailMap` (t3, on its own UV set) and unpacks it as a tangent-space normal - `mul x, w, x` /
`mad xy, xy, 2, -1` / `z = sqrt(1 - x^2 - y^2)` - adds it to the `_BumpMap` normal scaled by
`_DetailWeight`, then derives two normals by scaling the deviation from `(0,0,1)` by `cb0[73].x` and
`.y`, i.e. `_DiffuseBumpiness` and `_SpecularBumpiness`. Our cginc keys the detail layer on `_DecalTex`
(`VAM_SAMPLE_DECAL`), a name this family never declares, so the detail layer is dropped and the cloth
draws with the base bump alone. Tracked as `cloth-detail-layer`.

### What the census cleared

Two things that looked like suspects are not: `Marmoset/Transparent/Cutout/{Diffuse,Bumped
Diffuse,Specular,Bumped Specular} IBL` and `Marmoset/Beta/Skin IBL Soft` fail to compile in
`artifacts\manual-play.log` (`All passes removed`), but no material of this scene uses them, so they
cannot be responsible for anything on screen. Separately, the `fallback shader ... not found`
warnings are real but belong to the transcription backlog: our generated `*ComputeBuff` shaders
declare `Fallback "Marmoset/Specular IBL SoftComputeBuff"` and
`"Marmoset/Specular IBL Soft NoCullComputeBuff"`, names faithful to the original that the pending
Marmoset item has not built yet - that set is now the only entry in `UNTRANSCRIBED_FAMILIES`.

### Found by reading the bytecode: the hair's under-layer drew a lit colour

*Symptom:* none reported - this was found while establishing *why* a pass can need a different
fragment than its family's, and it is recorded here because it is a real behavioural divergence, not
because anything on screen pointed at it.

*Cause:* each hair family's **pass 0** is not a lit pass. Its shipped fragment is 11 instructions -
sample `_MainTex`, `mul o0.w, r0.x, cb0[68].w`, `mov o0.xyz, l(0,0,0,0)` - i.e. it lays down the
texel's alpha times `_Color.a` and **black**, and the lit layer is drawn over it by a later pass. The
generator was emitting the shared model for that pass, so the pass wrote a second fully lit layer
where the game writes a dark one.

What made this hard to see is the render state: every pass of every hair family serialises
`colMask = 14`, which under the engine's `ColorWriteMask` (Alpha 1, Blue 2, Green 4, Red 8) means RGB
masked **off**, alpha only. So the shipped pass's `mov o0.xyz, l(0,0,0,0)` is dropped by the colour
mask, and the pass is visible only as coverage - a transparent `ZWrite Off` `SrcAlpha
OneMinusSrcAlpha` pass that a lit colour would land through. Reading the state cannot therefore even
reveal that the pass is a mask: `Custom/Subsurface/AlphaMaskComputeBuff`, the mask we already knew
about, serialises `colMask = 15` (RGB written as zero) and only differs in that respect.

The three hair mask passes and the two alpha-mask ones do not share a formula, which is why the
generator emits the alpha terms per pass rather than the fragment hard-coding them (`VAM_MASK_COLOR`,
`VAM_MASK_ADJUST`, both read from the pass's own `$Globals`):

| pass | shipped alpha | disassembly | globals |
|---|---|---|---|
| `Custom/Hair/MainComputeBuff` p0 | `texel.a * _Color.a` | `mul o0.w, r0.x, cb0[68].w` | `_Color` |
| `Custom/Hair/MainThickenComputeBuff` p0 | `texel.a * _Color.a` | byte-identical to the above | `_Color` |
| `Custom/Hair/MainThickenSeparateAlphaComputeBuff` p0 | `texel.a` untouched | `mov o0.w, r0.w` | none |
| `Custom/Subsurface/AlphaMaskComputeBuff` p0/p1 | `saturate(texel.a * _Color.a + _AlphaAdjust)` | `mad_sat o0.w, r0.w, cb0[69].w, cb0[68].x` | `_Color`, `_AlphaAdjust` |

*The fix:* `mask_only()` replaced the name table the cornea fix had introduced. The old table mapped
`shader name -> pass indices`, which cannot be defended: pass indices are not comparable across
families - the opaque forward base is index 0 in `MainAlternateComputeBuff`, `Scalp` and
`SimpleCutout`, and index 1 in `MainSeparateAlphaLayer*` and in `MainComputeBuff` - and the same
structural shape kept reappearing in newly transcribed families. The replacement is
`LightMode in (FORWARDBASE, FORWARDADD)` **and** `$Globals ⊆ {_Color, _AlphaAdjust}`.

*Verification so far:* over all **136** emitted non-skipped passes of the reconstructed families the
rule selects exactly **5**, all five intended, and the nearest pass it does not select reads **29**
globals. The four affected fragments were disassembled and the three formulas above match the shipped
programs. `python tools\check_shaders.py` reports **4414/4414 programs, 0 failed** (4444 before: a
pass declaring `VamFragment` is also compiled once as `VamFragmentAdd`, so an ordinary pass costs two
programs per keyword set, while a mask pass declares `VamFragmentMask` and costs one - 3 passes x 10
keyword sets = the 30), and `scripts\Invoke-CompileGate.ps1` is green. **A visible run is
outstanding**, as it is for the rest of this change set.

### The tessellation stages

The five `Custom/Subsurface/*TessMapped*ComputeBuff` shaders are the only ones in VaM that ship a hull
and a domain program, and they ship them on *every* pass - forward base, forward additive and
shadow caster alike. The four outside `GlossNMTessMappedFixed` differ only in their pixel program, so
one reconstruction serves all fifteen passes. What the material controls is `_Tess` (how much of the
density map's range reaches the tessellator, default 3.35), `_TessTex` (the density map itself) and
`_TessPhong` (how far the patch bends towards its normals, default 0.75).

Decoded from the shipped assembly, and what the reconstruction does:

- **The control point carries the buffers through in object space.** The shipped vertex program
  (`vs_5_0`, blob `000`) forwards `verts[vid]` as `INTERNALTESSPOS`, and the raw `normals[vid]` and
  `tangents[vid]` unreformed - no normalize, no Gram-Schmidt, no object-to-world. The shipped hull's
  input signature is exactly that, so the transform belongs to the domain. Note it does not read
  `POSITION.w`, i.e. unlike the non-tessellated path there is no per-vertex offset term.
- **The hull turns the density map into factors.** `h(n) = tex2Dlod(_TessTex, cp[n].uv0.xy,
  lod = cp[n].uv0.w).x * _Tess + 0.01`, then `edge0 = (h1+h2)/2`, `edge1 = (h2+h0)/2`,
  `edge2 = (h0+h1)/2`, `inside = (h0+h1+h2)/3` - eleven instructions, reproduced literally,
  including the 0.01 floor that keeps a black sample from collapsing its patch. Each edge factor is
  symmetric in its two corners, so two patches sharing an edge compute the same number for it and the
  subdivision cannot crack. The density-map mip comes from the control point's own `TEXCOORD0.w`, and
  the arithmetic is byte-identical in every keyword variant.
- **The domain is a Pn triangle.** The position is interpolated flat first, then pushed onto each
  control point's tangent plane by `P_n = flat - normal_n * dot(flat - pos_n, normal_n)`, the three
  projections are weighted barycentrically and blended against the flat point by `_TessPhong` (0
  leaves the patch flat, 1 makes it follow the normals). Only the position is projected; the normal
  stays a barycentric interpolation. The result is an object-space position - object because the
  original draws the body with `Matrix4x4.identity` - which the object-to-clip matrix turns into the
  clip position; ours goes through `unity_ObjectToWorld` and `UNITY_MATRIX_VP` by the same route.
- **The rest of the domain is the varyings.** The five `_ST` transforms are applied after the
  barycentric UV interpolation, the tangent and normal are transformed to world space and the
  bitangent is rebuilt as `cross(N,T)` scaled by the interpolated tangent `w` times
  `unity_WorldTransformParams.w`, and the screen-space shadow coordinate is computed from the domain's
  own clip position - the shipped `SHADOWS_SCREEN` variant does this, and the domain cannot call
  `TRANSFER_SHADOW`, which reads `a.pos` from the vertex stage. The domain finishes in the same
  world-space layout `VamPack()` builds for the non-tessellated vertex path, which is what lets the
  fragment stage stay the same program.
- **The shadow pass is tessellated too.** Its depth would otherwise be the mesh's silhouette, not the
  subdivided body's. That domain repeats the same Pn position and then reproduces Unity's own
  shadow-caster placement from `unity_LightShadowBias`: the normal offset scaled by the sine of the
  light angle, then the linear-bias clamp.

Checked with the compile gate, which now compiles a tessellated pass as its four programs -
control point at `vs_5_0`, hull at `hs_5_0`, domain at `ds_5_0`, fragment at `ps_5_0` - with
`SHADER_TARGET 50`, the model the original's hull and domain were built for:

*Current state*: `7479/7479 programs compiled, 0 failed`, over the 88 shaders the generator emits
(`python scripts\New-VaMShaders.py --list`: 256 passes, 15 of them tessellated - the five
`*TessMapped*` families, three passes each). The count was 3023 until the eye's alpha-mask family
stopped being compiled as the shared fragment as well as its own, 4444 until the hair's three mask
passes did the same, and 6805 until the hair families' and the plain materials' twins were
transcribed too - a pass
that declares `VamFragment` is compiled a second time as `VamFragmentAdd`, so a mask pass declaring
`VamFragmentMask` costs one program per keyword set instead of two.

Two things a reader should not expect from this:

- **The Pn normal is not carried.** The original ships its Phong-projected normal to the pixel stage
  in `TEXCOORD6` while also shipping the interpolated one; our fragment stage is a transcription of
  the *non-tessellated* family's program, which reads only the interpolated one, so nothing consumes
  it. Detail-bump shading on a tessellated submesh is therefore what it was before this change; what
  changed is the geometry.
- **The varying packing is ours, not the original's.** The original interleaves tangent, bitangent
  and both normals across `o4`/`o5`/`o6`; we repack into our own layout. That is invisible as long as
  both stages come from the same source, which they do, and it is what keeps the fragment stage shared
  between the two families.

Visual confirmation is still outstanding: the gate proves the programs compile and the arithmetic
matches, not that the body looks right. The manual run in the checklist below is the check for that.

### Found by measuring: which materials still draw the bundle's copy

The census prints an `origin=` for every material it sees, and after the plain twins landed it read
`bundle (project defines one)` for materials whose family the project had just rebuilt. That is not a
bookkeeping problem: a material VaM instantiated from `z_sha` holds the bundle's `Shader` object, and
no amount of compiling our own copy changes which object a material points at. The label was telling
the truth about the draw.

`VamShaderProvider.UseProjectShader(material)` closes the gap by name, and
`docs\shader-reconstruction.md` (*Who answers a name*) documents the gate and the six call sites. What
this section keeps is the measurement, because the measurement is what caught three separate traps:

- **The gate has to ask what was written, not what was found.** `Shader.Find("GPUTools/MeshedVR/HairOpt")`
  answers - AssetRipper wrote a placeholder with that name - so a redirection gated on
  `Find != null` would swap the hair's optimised path onto a one-pass placeholder and draw *less*. The
  generator now emits `VamProjectShaders.cs`, the 88 names the run wrote, and 0 of them collides with
  the 25 placeholders.
- **The census was asking the same wrong question.** `ProjectDefinesShader` was loose enough to accept
  `Standard`, `Unlit/Texture` and every placeholder, so 36 materials were counted as take-overs waiting
  to happen. A separate `ProjectReconstructionDefines` refuses a name only a placeholder carries, and
  those 36 now read `bundle (only a stub or a built-in answers)`.
- **A material slot is not a drawn material.** The `character materials:` line counts a de-duplicated
  draw list, while the per-material list prints one row per `RecordDrawMaterial` call, so the same
  `MaterialUse` can appear twice - once `via renderer` and once `via skin GPUmaterials`. The "16
  character materials on a bundle copy" is 16 *calls*, not 16 distinct drawn materials.

What the sweep actually moved, on the boot scene: `material(s) on a bundle copy` **84 → 76**, the eight
being the hands (`Hands_Mat_01_MVR`, `Hands_Mat_02_MVR`), the equipped cloth simulations
(`hu_pty_body-1`, `hu_skt_body1/2-1`, `hu_skt_metal-1`, `hu_skt_str-1`) and `HairTool` - all off the
character and all now on `Custom/Subsurface/GlossNMCull`, `TransparentSeparateAlpha`,
`TransparentGlossNMNoCullSeparateAlpha` or `TransparentGlossNMDetailNoCullSeparateAlpha`, which are the
project's own. Everything else is unchanged: 118 renderers (14 disabled), 154 material slots, 120
distinct materials, 31 distinct shaders, 0 unsupported, 0 on a stub, 0 `Shader error`, and the same 12
distinct exception lines as the previous three runs.

The bucket that stays is the one worth writing down, because its count is misleading in the flattering
direction and it makes the round look unfinished when it is not:

- **19 of the 20 remaining bundle materials belong to the unequipped `VictoriaElitePonytailHair*`
  template.** `DAZSkinV2.GPUmaterials` is filled in `SkinMeshGPUMaterialInit`, called from `Awake` only
  when `Application.isPlaying`, and a skin outside the enabled hierarchy never runs `Awake` - so it
  keeps the materials serialised with the scene. The new census line `skins still holding a bundle
  shader` prints each skin with `active=` / `inHierarchy=` and its off-bundle ratio, which is what turns
  "16 on the character" into "15/15 and 4/4 off-bundle, not in the hierarchy, not drawn".
- **The twentieth is the live skin's `EyeReflection-1`**, on
  `Marmoset/Transparent/Simple Glass/Specular IBLComputeBuff` - a family nobody has transcribed, so
  there is nothing to redirect it to.

Two sets are therefore left alone **on purpose**, and a future reader should not read either as a bug:

- **The 15 hair-card materials on the inactive clones.** The hair's under-layer pass writes alpha only
  (`mov o0.xyz, l(0,0,0,0)`), so the project's pass - which writes the colour - would paint black cards
  over the hair until that pass is verified against the original.
- **Everything behind a placeholder.** The interaction rig (`GPUTools/Painter`, 34 slots), the hair
  optimiser (`GPUTools/MeshedVR/HairOpt`), the sky (`Marmoset/Skydome`) and the particles
  (`Marmoset/Diffuse IBL`) all answer through a placeholder that keeps more passes than the shader it
  would replace, so the redirection must not touch them until the families themselves are transcribed
  (item 3).

## Found by measuring: VaM's own point-light shadow filter, and who computes the darkening

The report was **"no self-shadowing"**, and the shading model agreed with it: `VAM_LIGHT_ATTENUATION`
did not exist, so every point light went through `AutoLight.cginc`'s built-in path. The filter was
transcribed from the release build's `MARMO_LINEAR + POINT + SHADOWS_CUBE` fragment program
(`artifacts\_tmp\ship_point.asm`, lines 190-246) - see the *Self-shadowing* bullet in
[`release-notes.md`](release-notes.md) for the decode - and what this section keeps is the method, because
the round needed two corrections to reach
a number it could believe.

**The statistic has to be taken on the pixels that had light to lose.** The previous round averaged the
session-against-shadows-off difference over the whole frame and read 0.6/255, which is roughly noise
and was written down as "the filter contributes nothing". A dark background that does not change
dominates that average. On the same pair, masked to the reference's lit pixels (luma > 32, 73 494 px),
the difference is **mean +8.26/255**, median +2.43, p90 +18.89, p99 +121.55, max +206.35, and the 8x8
cell means run +0.00 to **+30.36** with the deep cells where the body meets itself. `tools\measure_shadow.py`
is that statistic, and it prints the cell breakdown and a composite alongside the summary, because the
*structure* is half the answer: a shadow term leaves cells that differ by tens of levels, a colour
space or exposure change leaves every cell shifted by the same amount.

**Authorship needs a control inside one build, not a comparison across two.** `uniform float _VamShadowUnity`
returns Unity's own `UnityComputeForwardShadows` from the same call site, so the two filters run in one
build, one scene, one session and one camera, and the switch is a `Shader.SetGlobalFloat` rather than a
keyword so it costs no variants. The pair:

| measurement, framed on the body, reference = shadows off | VaM's filter | Unity's filter |
| --- | --- | --- |
| mean delta on the lit mask | **+8.26/255** | +0.98/255 |
| p90 / p99 / max | +18.89 / +121.55 / +206.35 | +1.00 / +18.91 / +138.00 |
| pixels darker by more than 40 | **2 196** | 240 |
| 8x8 cell means, range | +0.00 .. +30.36 | +0.00 .. +6.30 |

The frame taken after the switch is restored is identical to the session frame (max |difference| 0), so
the pair isolates the switch. The built-in path is not doing nothing - 0.98/255 with 240 deep pixels is
a real filter - but under the installation's own `shadowStrength` of 0.10 it is nearly invisible, which
is why the defect was reported as absence rather than as weakness. Unity's filter also *differs in
kind*: it lerps toward 1 per tap and uses `_LightShadowData.x` as the light's `shadowStrength`, where
the shipped program averages the taps (`* 0.04`) and uses `_LightShadowData.x` only to size the disk,
`(1 - x) * 0.1`.

**What cannot be claimed.** The shipped hash begins with a sample of a full-screen texture at the
screen-space uv, and that texture could not be identified - no ShaderLab text survives in the bundle
and no VaM script binds it. The port derives the seed from the pixel coordinate instead, keeping the
blob's own `m`, `sincos` and `frc` sequence. So the port reproduces the *amount and structure* of the
darkening and not the original's exact dither pattern, and a per-pixel comparison of the two shadow
maps would be a false test to write. `AutoLight.cginc` is what leaves the pixel coordinate available:
for a cube-shadow point light it declares `unityShadowCoord3` = `worldPos - _LightPositionRange.xyz`
and leaves the slot it uses for screen/depth shadows unused, so `i.pos` carries the seed in no varying.

## Still open

- **`MacGruber.Life` compiles now but throws on load.** This is the one plugin the boot scene names
  (`plugin#0`), and after the reference-list adaptation above it gets as far as running its own code:
  `FieldAccessException: Field 'MiniQueue'1:position' is inaccessible from method
  'MacGruber.Breathing/MiniQueue'1<T_REF>:.ctor ()'`, then `NullReferenceException` in
  `MacGruber.Breathing.Init`/`Cleanup`, then `failed to initialize`. It is not a gate failure - the
  scene loads 18 of 18 atoms with the plugin broken - but a reader comparing logs against the
  installation will see the difference. Tracked as item 16 of the plan; the fields it names are in the
  plugin's own nested generic type, accessed from that type's constructor, which a compiler should not
  emit at all.

- **The shadow filter is proven to be ours, but not tuned.** The transcribed disk darkens the body by
  8.26/255 on the lit pixels under the scene's own three point lights at `shadowStrength` 0.10, where
  the built-in path darkened it by 0.98/255. Whether that amount is the installation's amount is a hand
  run and not a gate, and the deepest pixels - p99 121/255 over 3.0 % of the lit body - are where to
  look first. A hand run taken before the filter landed is not a baseline for one taken after it,
  because the light term moved. That hand run was taken on 2026-09-17, on the boot scene through
  `scripts\Invoke-ManualPlay.ps1`, and read the amount as right: the body darkens itself where it meets
  itself. It is a judgement, not a measurement - there is no reference frame from the installation in
  the repository to subtract - which is why it is recorded here and not counted as a gate.
- **Visual comparison.** Against a running `VaM.exe`: models, lighting, materials, shaders. The tool is
  ready (`tools\compare_view.py`, section 5) and the target is a capture of the original on
  `CyberDemoAlt` through the camera the player actually sees, which is the desktop monitor rig -
  `SceneAtoms/CoreControl/WorldScaleAdjust/NavRig/[CameraRig]/HeightOffset/MonitorRig` at
  `(-1.81, 1.14, -4.13)`, depth 2, framed 16:9. That position is the scene's own `[CameraRig]` atom
  (`(-1.810732, 0, -4.127665)`, yaw `320.24`) with the rig's eye height on top, so the framing can be
  read out of `artifacts\reference\CyberDemoAlt.json` rather than remembered. An earlier note here
  recorded `(-3.96, 1.48, -3.82)` for the same rig; that reading does not reproduce and is superseded -
  give the run enough time past the load for `simulation resetting` to go `False` (`-Seconds 150` with
  `-WarmupSeconds 15`) before capturing anything.
  The shader suspicion recorded here earlier is now confirmed and has its own section above: the
  exported shaders are stubs, so the difference on the body is not subtle and no camera will make it
  go away. The comparison is still worth running for everything else - scene layout, props,
  lighting, the character's placement and scale.
- **Animation, physics and UI.** `CyberDemoAlt` loads and its character is on screen, but the
  animation, physics and UI systems have not been exercised end to end: the reference scenarios are
  the `* Benchmark.bat` files in the installation root. One cheap question belongs here - the scene's
  `animationSelection` names `Dance - Hip Hop 3` while `sequence[0]` names `Idle - Lying 3`, and the
  rebuild plays the sequence, hence a body that lies down. Note that the animation *is* proven to run
  by bone motion (84 of 84 bones moving, clock advancing) even while the body's own mesh does not show
  it - see the body defect above.
- **Runtime plugin discovery.** `ConvexDecompositionDll.dll` is not referenced by any managed code, and
  ZFBrowser locates the CEF payload by scanning directories rather than by `DllImport`, so neither has
  been exercised.
