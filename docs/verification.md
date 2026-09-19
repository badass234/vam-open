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
5 511 680 B, `VaMUnityScript.dll` 16 384 B, `Assembly-CSharp-Editor.dll` present. Size is not a health
reading on this line: 2021.3 compiles against Roslyn reference assemblies, so the assembly is **9.4% smaller**
than it was on 2020.3 (6 079 488 B) with 0 errors on both sides.

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
list. See *The plugin compiler* in `rebuild-project.md` for the chain and for the A/B that measured it. The
compiler *binary* is checked the same way and separately - section 9 below.

### 8. IDE project files - `scripts\Invoke-SyncSolution.ps1`

There is no `.sln` in the repository and there is not meant to be one: a solution states which
assemblies Unity compiles out of which files with which references, and only the editor knows that.
So the editor is asked for it - `RebuildGate.SyncSolution` calls the code editor integration behind
*Preferences > External Tools*, and when that writes nothing, the built-in Visual Studio generator
behind it.

```
scripts\Invoke-SyncSolution.ps1              # refuses while an editor holds the project
scripts\Invoke-SyncSolution.ps1 -Force       # stops the running editors first
```

**The failure worth guarding against is an empty success**: a run that reports `OK` and leaves the IDE
nothing to open. The method therefore counts the files it left in the project root, logs each one with
its size, and fails the run at zero, naming the package to install. That zero is not hypothetical - it
is exactly what a project with no `com.unity.ide.*` package gets from `SyncAll()`, because Unity then
registers its own `DefaultExternalCodeEditor`, whose `SyncAll()` is an empty method.

*Current state*: the project has no `com.unity.ide.*` package, so the run logs
`code editor: UnityEditor.DefaultExternalCodeEditor, no external editor set in Preferences`, falls back
to `UnityEditor.SyncVS.SyncSolution()`, and that one generates. It leaves four files in the project
root - `VaM_Rebuild.sln` (1,897 B) and the three project files, `Assembly-CSharp.csproj` (2811 compile
items), `Assembly-CSharp-Editor.csproj` (3) and `VaMUnityScript.csproj` (13) - and the generated
solution checks out structurally: every project file the solution names exists, every `ProjectGuid`
matches the file it names, and **all 620 `HintPath` entries across the three projects resolve** (the
Unity assemblies from the editor installation, `RTTypeModel.dll` from `Assets\Plugins`, the plugin
DLLs staged by `Setup-RebuildProject.ps1`), which is what an IDE needs to open the solution without
hand-editing. The files are gitignored (`VaM_Rebuild/*.sln`, `VaM_Rebuild/*.csproj`): they are build
output that changes with the editor and the package set, not source. See *The IDE solution* in
`rebuild-project.md` for why the decompiled sources keep their own projects under `src\` as well.

**Staleness is the second thing this check is about, and it is measured rather than assumed.** The
fallback only runs when there is no `.sln` yet, so a solution that already exists is refreshed by
Unity's own `SyncVS` step rather than by this method - and that is enough, which was verified the only
way it can be: a probe source file was dropped into `Assets\Scripts\`, the script was run, and
`Assembly-CSharp.csproj` came back at 18:20:13 with `<Compile Include="Assets\Scripts\ZzSolProbe.cs" />`
in it and 57 bytes larger. The probe was then deleted and the script run again: the entry is gone and
the file is back to its previous size, byte for byte, with nothing else rewritten. So the rule in
`rebuild-project.md` - regenerate after adding a source file or a plugin - describes what actually
happens: the editor rewrites only the project whose file list changed, and leaves the rest alone.

### 9. The plugin compiler itself - `artifacts\mcs-ab`

Section 7 checks the list of names the compiler is handed. This section checks the compiler, because it is a
binary this project builds and ships, and a defect in it is invisible from every gate: the compiler aborts,
`Report.Error` throws `FatalException` as soon as `ErrorsCount` reaches `settings.FatalCounter`, the plugin's
own error list comes back **empty**, and the only symptom is a plugin that does not load.

`artifacts\mcs-ab\driver.cs` drives `Mono.CSharp.CompilerContext` directly - the editor's `mcs.exe` cannot
drive a compiler whose identity is `mcs, Version=0.0.0.0` - and the two arms sit side by side as `orig\mcs.dll`
(the game's) and `ours\mcs.dll` (this project's build). The variant under test must be copied beside
`driver.exe` as `mcs.dll`: `-r:` satisfies the reference only, and the runtime binds by simple name.

```
$env:MCS_REFS = 'C:\Games\VaM_Updater\VaM_Data\Managed'   # files and directories; dirs are also search paths
$env:MCS_BREAK_ON_ICE = '1'                               # rethrow instead of reporting CS0584
mono.exe ...\lib\mono\4.5\mcs.exe -target:exe -sdk:4.5 -noconfig -nowarn:0169 -out:driver.exe `
    -r:ours\mcs.dll driver.cs
mono.exe driver.exe
```

Read `ERRORS=` and `UNCAUGHT=`, never the exit code: `ConsoleReportPrinter` exits 0 with errors on the
report. `MCS_REFS` reports what will not load as `REF-FAIL=`, so a silent compile is never taken at face
value, and `MCS_BREAK_ON_ICE=1` is what turns an internal error into a stack: the by-ref defect's runs
`MethodSpec.InflateMember` → `TypeParameterInflator.Inflate` → `MemberCache.InflateMembers` →
`MemberCache.GetUserOperator` → `Convert.UserDefinedConversion` → `OverloadResolver.IsArgumentCompatible` →
`MethodGroupExpr.OverloadResolve`.

*Current state*: three `Span` probes go from `CS0584` under the game's compiler to 0 errors under this build,
and `Life_Internal_MacGruber_Utils.cs` compiled against the original game's own `VaM_Data\Managed` - **63**
assemblies, `Assembly-CSharp.dll` plus every `UnityEngine*.dll` - gives the control

```
(687,6): error CS0584: Internal compiler error: The method or operation is not implemented.
```

against **0 errors and 0 reference failures** for this build. The reference set is load-bearing and it has to
be that one: `using AssetBundles;` at `:19` resolves to a namespace inside `Assembly-CSharp.dll` and in no
UnityEngine module, so the 2021 editor's `Managed\UnityEngine\` cannot stand in for it. On a partial set both
arms report `CS0584=0` and ~56 unrelated `CS0246`, which reads as "the fix changed nothing" - the trap is that
the control failing to fail looks like a pass.

*A second defect, in the repair the hop-3 crash left behind.* `RepairMethodOverrideDeclarations`
(`McsDriver.cs:292`) rewrites the override table mcs leaves on builder methods, and it looked each
declaration's token up by `declaration.MetadataToken` (`:383`). `MethodBuilder` does not override
`MemberInfo.MetadataToken` - the base implementation only throws - so that read raised
`InvalidOperationException` on **every** declaration, the loop's own `catch` swallowed it,
`CollectCreatedMethods` matched nothing, and the repair returned having changed nothing while reporting
nothing. The assembly then died in the runtime with `requested token for MethodBuilder`, which is the hop-3
crash. The other three token reads in the file (`:265` of the created methods, `:474`/`:494` of the resolved
candidates) are on runtime methods and never threw - measured rather than assumed, because the control's
stack names `:383` and nothing else. `ReadMethodToken` (declared at `:429`, `MetadataToken` at `:433`,
`catch (InvalidOperationException)` at `:435`) tries `MetadataToken` first, so a runtime that implements it is
preferred, and falls back to `GetToken().Token`, the builder's metadata table index, which is the number the
created method reports.

*Acceptance, as a control pair.* `artifacts\decal-repro\decal-repro-B38.log` is the pre-fix control and
`artifacts\compiler-fix-ladyclown.log` the post-fix run, one editor session each, read by grep rather than by
line number because the helper shifted every line below `:380`:

| reading | control | post-fix |
|---|---|---|
| `MemberInfo.get_MetadataToken` frames | 2 | 0 |
| `[CS]: System.InvalidOperationException` | 2 | 0 |
| the repair's own marker, `resolved N method override declaration(s)` | 0 | 1, reading 32 |
| `[CS]: System.NotSupportedException` | 0 | 2 |
| plugins that failed to compile | `everlaster.TittyMagic.70`, `AcidBubbles.Timeline.283` | the same two |

Every row of that table is the **token fix's own** pair, and the last two rows are read there for the first
time: the `NotSupportedException` is the second call this section reaches on the way, and section 11 is where it
was resolved and where the pair is read again, one step further on.

The marker reports itself from `McsDriver.cs:405`, which is the `UnityEngine.Debug.Log` call at the end of
`RepairMethodOverrideDeclarations` (`:292`) - reached from `Compile :220` and `CompileFromSettings :128` in the
same stack - and not a further token read. The token reads in that file are `:265`, `:474` and `:494`, all on
runtime methods, plus `ReadMethodToken`'s own `:433`, which is the one that has to tolerate the throw, and the
repaired call site at `:383`.

The exception moves rather than disappears, and that is the fix working. The control's stack is
`MemberInfo.get_MetadataToken()` <- `RepairMethodOverrideDeclarations [0x00252] :383` <- `Compile [0x0027d]
:220` <- `CompileFromSettings :128` - the repair giving up on its first declaration. The post-fix stack is
`TypeBuilderInstantiation.GetMethods` <- `ResolveOnTypeBuilderInst [0x00086] :483` <-
`RepairMethodOverrideDeclarations [0x001d5] :361` - the repair now reaching its own resolution step on the
same plugin and meeting a limit of `System.Reflection.Emit` there. **Nothing was added to catch that, and at
that point it needed nothing added:** it escapes `McsDriver.Compile`, and `CompileFromSettings`'s `catch (Exception)`
(`McsCompiler.cs:130`) turns it into a `CompilerError` through `ErrorText = ex.ToString()`, which is exactly
what the `[CS]:` line and the frames under it are - so `Save()` is never reached **for that plugin** and the
editor survives. The control shows the same containment, so containment is not the difference; the repair
running is.

The failing set is identical before and after, and one plugin in it changed shape. `everlaster.TittyMagic.70`
no longer dies in the repair at all - it now reaches the point where the compiled assembly is loaded, and
mono refuses the plugin's own generic-iterator vtable at `:921`, twin at `:6636`. Quoted from `:921` whole -
it is one line of the log, wrapped nowhere here - with only the manager's own `Exception during compile of
<plugin>: ` prefix removed:

```
System.TypeLoadException: Could not set up parent class, due to: Generic Type Definition failed to init, due to: Parent class vtable failed to initialize, due to: Method overrides a class or interface that is not extended or implemented by this type assembly:data-00000271549756A0 type:ColliderModel member:(null) assembly:data-00000271549756A0 type:ColliderModel`1 member:(null) assembly:data-00000271549756A0 type:ColliderModel`1 member:(null)
```

`AcidBubbles.Timeline.283` still failed at this point, through the residual `NotSupportedException` above -
that second call was resolved afterwards, and section 11 is its record; the reading above is left as it
stood. Both are recorded as they are: the same two plugins failed before the fix, one of them now fails later
and for a reason of its own, and that reason is a different class of defect.

When counting failures in these logs, mind three traps. `Select-String` is **case-insensitive by default**, so
a search for `Compile of ` also matches `Exception during compile of ...` and reports four hits where two
plugins failed; use `-CaseSensitive` for exact phrases. And the gate logs each failure a second time with an
`[Error]` prefix, so every count is half again what it looks like unless the twins are excluded.

The third trap is the one that looks like evidence of a lost error, so it is worth stating as a mechanism
rather than as a caution: **`... failed. Errors:` is always followed by nothing, and nothing is missing.**
Every string this compiler puts into `ScriptCompiler.errors` is built by `AddError` as
`$"[CS{code}]: {message}"` (`ScriptCompiler.cs:180-186`, with `AddWarning` at `:167-176` doing the same for
warnings), and the print loop at `MVRPluginManager.cs:800-806` skips every entry whose `text8.StartsWith("[CS]")`
(`:802`) - which, on this codebase, is every entry there is. The error itself is not suppressed: it is printed
once by `ScriptCompiler.PrintErrors` (`:64-71`, a bare `Debug.LogError` per entry) from
`ScriptDomain.CompileAndLoadScriptSources` (`ScriptDomain.cs:175`, immediately after the compile at `:173`), so
the `[CS]` block appears **above** the header, not under it. Measured in both logs rather than reasoned: control
`:928` + `:953` and `:1074` + `:1099`, post-fix `:1049` + `:1075`, with the header's next line in every case an
unrelated material warning. The sibling branch - `catch (Exception ex)` at `:811-829`, which prints
`... failed. Exception: ` at `:817` and then, when `Errors.Length > 0`, an **unfiltered** list at `:820-825` -
never fired in either run, and neither did the residual `Compile of ... failed. Exception:` shape: **0** in both.
The catch-all that does fire, `Exception during compile of ` at `:1022`, prints no list at all, which is why the
TittyMagic `TypeLoadException` above has no `[CS]` twin.

### 10. Plugin-compiler repair acceptance - `scripts\Test-PluginCompilerRepair.ps1`

Check 9 proves the compiler by building it and compiling a known file through it. This check is about the
*repair* inside the compiler's driver, and it exists because that acceptance had been done by hand four times
and mis-read three of those times. The repair itself is committed as `9674a1f` (`McsDriver.ReadMethodToken`
prefers `MemberInfo.MetadataToken` and falls back to `GetToken().Token` under `catch
(InvalidOperationException)`), and the ordinary gate exercises it: `RebuildGate.cs` carries no plugin flag at
all - `Play()` calls `ArmPlay(false)` (`:635`), `ManualPlay()` calls `ArmPlay(true)` (`:651`), the only parameter
is `bool manual` (`:655`), and `LoadPlayScene()` (`:1106`) has none - so a `Play` run is a plugin-enabled run.
The standing pair is therefore the gate's own output, and this script is the reading of it.

```powershell
# the standing pair: pre-fix control against the post-fix run
scripts\Test-PluginCompilerRepair.ps1 -Control artifacts\manual-play.log `
                                      -Candidate artifacts\compiler-fix-ladyclown.log
# judge one freshly produced log on its own
scripts\Test-PluginCompilerRepair.ps1 -Log artifacts\your-run.log
```

Exit codes: **0** the expectation holds, **1** it does not (for `-Log` on a control, that is the *correct*
verdict and the wording says so), **2** bad input - a missing file, an unreadable file, the same path given
twice, `-Log` combined with `-Control`, a directory. The expectation is *post-fix*: token blocks **0**,
`get_MetadataToken` frames **0**, and the repair's marker **present**.

**Count blocks, never lines**, and the script does the grouping itself: hits are assigned to the block they
belong to (the exception line and its duplicated `[Error]` twin merge into one block; the IL frames under it
are a second family; the marker, the `failed. Compile of` header and the quiet `Exception during compile of`
shape are separate families again). The raw-hit column is printed beside the block column only to make the
difference visible, and it is deliberately **not** part of the verdict - in the standing pair `[4]` reads 2
blocks against 2 raw hits in the control and 1 block against 2 raw hits in the candidate, so a rising total is
not a regression.

*Current state*: `PASS`, exit `0`. Control `artifacts\manual-play.log` (3 605 lines, 09-19 16:42:33) - token
blocks **2** (`:1249`, `:1395`), `get_MetadataToken` frames **2**, marker **absent**, and two
`Compile of … failed. Errors:` headers (`everlaster.TittyMagic.70` at `:1274`, `AcidBubbles.Timeline.283` at
`:1420`). Candidate `artifacts\compiler-fix-ladyclown.log` (7 605 lines, 17:02:46) - token blocks **0**, frames
**0**, marker **present** at `:894` reading `resolved 32`, one remaining `Compile of` header, and the three
`[Error]`-prefixed twins merged into their originals. The by-ref pair (`artifacts\manual-play-cs584-before.log`
against the same candidate) is reported too but judged only on the families its control exhibits - `[CS584]`
**10 → 0** and the `MacGruber.Life.12:` headers **5 → 0** - because that control has no token defect in it and
is therefore not a token control. The second older pair, `artifacts\decal-repro\decal-repro-A37.log` against
`…B38.log`, exits **1**: it is a pre-fix pair on both sides (`10 → 2`, no marker anywhere), and that is the
honest reading rather than a failure of the fix.

Two things the script states in the output rather than leaving to the reader, both of them traps this project
has fallen into. The list under `… failed. Errors:` is **always empty and nothing is missing** (`:798` writes
the header, `:802` skips every entry beginning `[CS]`, `:804` logs the survivors), so it reports the mechanism
instead of reporting "no errors". And defect B - `TypeBuilderInstantiation.GetMethods` →
`NotSupportedException` from `ResolveOnTypeBuilderInst :483` - is printed as its **own named block** and is
never folded into the token verdict: in the candidate it is one block at `:1049` (raw 2 / 2 / 2 for
`NotSupportedException`, `TypeBuilderInstantiation`, `ResolveOnTypeBuilderInst`). At that point it was a
separate, still open defect; it has since been resolved, and section 11 is its record - the numbers above are
the pre-fix side of section 11's pair, and sections 10 and 11 read the same script.

The long form of the script's own validation, its trap list and its measured runs is
`artifacts\plugin-compiler\acceptance\README.md`, written by the workstream that built it.

### 11. The repair's second call, resolved - the same script, read as a pair

Check 10's candidate carried one defect away with it, and check 9's record named it: the repair
`RepairMethodOverrideDeclarations` resolves a `MethodOnTypeBuilderInst` by enumerating
`inflated.GetMethods(...)` (`McsDriver.cs:489`), and for the shape mcs produces when the interface itself is
still a builder that enumeration cannot be reached at all. Read out of the corelib the editor actually loads:
`TypeBuilderInstantiation.GetMethods(BindingFlags)` is `throw new NotSupportedException()` with no condition.
The resolution that looked like a way around it is closed too - `ResolveBuilderType`'s
`TypeBuilderInstantiation` branch ends in `definition.MakeGenericType(args)` (`:637`), which for a builder
`definition` is `AssemblyBuilder.MakeGenericType`, whose whole body, read out of the emit implementation rather
than inferred, is `return new TypeBuilderInstantiation(gtd, typeArguments);` - so "resolve `inflated` through `ResolveBuilderType`
first" is **refuted by the source** rather than by experiment: it can only answer with another type that
refuses every method query.

The other shape mcs produces (`MethodOnTypeBuilderInst` out of `MethodBuilder.MakeGenericMethod`) does
enumerate, because its instantiation is an ordinary `TypeBuilder` - but `TypeBuilder.GetMethods(DeclaredOnly)`
answers with the builder's own method array, i.e. with `MethodBuilder`s, and that is the same unwritable member
the hop-3 abort was about: `Save()` asks it for a token and `mono_image_create_token` kills the process. One
shape threw and the other returned a builder, so the resolver had **no** path to a member the writer could name.

**The resolution is a token lookup into the index the repair already builds, and never a catch.** `createdMethods`
(`:303`, filled by `CollectCreatedMethods :261`) is what already resolves the non-generic `MethodBuilder`
declarations, and it is the writer-usable route: the member it answers with is a method of this module, and the
token the writer reads off it is the one the method reports (`ReadMethodToken`, check 9). `ResolveOnTypeBuilderInst`
(`:454`) now takes that dictionary, wraps the enumeration and, on `NotSupportedException` (`:491`), resolves
`base_method` through a new `ResolveCreatedMethod` (`:552`); a `bySignature` candidate is put through the same
helper before it leaves the method (`:531`). Two guard rails keep the outcomes honest: with nothing in the index
for `base_method` the exception is **rethrown** (`:501`), which is today's outcome and leaves `Save()` unreached,
and a builder candidate no created type accounts for now **throws** (`:536`) rather than being returned. Catching
and returning `null` was rejected rather than merely not chosen: it routes the entry to the `unresolved` list
(`:316`, filled at `:369-373` and again at `:390-394`, warned about at `:407-409`), leaves the `MethodOnTypeBuilderInst` in the overrides array, and lets `Save()` reach the writer
state that aborted the editor at hop 3 - so the throw is protective, not merely a failure.

*Acceptance, as a pair* - the fixed run `artifacts\defect-b\ladyclown-tokenindex.log` against check 10's
candidate `artifacts\compiler-fix-ladyclown.log`, one hand-run plugin-enabled editor session each on
`SoftEros777.Lady_Clown.1:/Saves/scene/ladyclown.json`, read per block by the check-10 script:

| reading | before | after |
|---|---|---|
| defect-B blocks (`[6]`: `NotSupportedException`, `TypeBuilderInstantiation`, `ResolveOnTypeBuilderInst`) | 1 block at `:1049`, raw 2/2/2 | **0** |
| `Compile of … failed. Errors:` headers (`[4]`) | 1 - `AcidBubbles.Timeline.283`, `:1075` | **0** |
| the repair's marker (`[3]`) | 1 block, `:894`, `resolved 32` | **2 blocks** - `:825` `resolved 32`, `:980` `resolved 75` |
| quiet `Exception during compile of …` (`[7]`) | 1 - `everlaster.TittyMagic.70`, `:921` | 2 - `…70` at `:852`, `AcidBubbles.Timeline.283` at `:1007` |
| token blocks (`[1]`), `get_MetadataToken` frames (`[2]`) | 0, 0 | 0, 0 |
| `System.NotSupportedException`, `requested token for MethodBuilder`, `mono_image_create_token` | 2, 0, 0 | 0, 0, 0 |
| `could not be resolved` (the `unresolved` warning) | 0 | 0 |

The first two rows together with the marker are the reading that matters: the pass that used to abort inside the
repair now completes, and it completes **twice** (32 and 75 declarations), because a repair that finishes on one
plugin lets the manager go on to the next. The fourth row is the honest boundary - `Timeline` ends in the *same*
shape the control already showed for `TittyMagic`: it compiles and mono refuses its generic vtable at load,

```
System.TypeLoadException: Could not set up parent class, due to: Generic Type Definition failed to init, due to: …
```

so what the fix does is **move that plugin's failure from the repair to the loader**, which is where the other
one already was. The two plugins are the same two before and after; the difference is that both now fail outside
the code this project wrote, and `ColliderModel\`1` is present in the control as well, so it is pre-existing
rather than introduced.

*Run it as check 10 does, but read the `[6]` line rather than the verdict.* The pair whose control exhibits
defect B cannot produce a `PASS`, because the script's verdict is about the token family and that control has no
token defect in it:

```powershell
# note the roles: the post-token-fix run is the control here, the defect-B run is the candidate
scripts\Test-PluginCompilerRepair.ps1 -Control artifacts\compiler-fix-ladyclown.log `
                                      -Candidate artifacts\defect-b\ladyclown-tokenindex.log
# -> RESULT FAIL - INCONCLUSIVE CONTROL, exit 1, printing: defect B control 1 block(s), candidate 0 block(s)
```

`INCONCLUSIVE CONTROL` is the script refusing to claim more than its control supports, and the `defect B` line
under it is the measurement itself - the same arrangement check 10 describes, where that family is printed
separately and never folded into the token verdict. Judged on its own instead
(`-Log artifacts\defect-b\ladyclown-tokenindex.log`) the fixed run is a **`PASS`**, exit **0**.

*What the fix leaves unsettled, stated with it.* The repair now hands `Save()` a writable member, not necessarily
the *right* one: answering with the created method of the generic **definition** is what the code's own comment
(`:482-484`) intends, and it is the loader that rejects that row. Whether a `MethodImpl` row could instead name
the closed instantiation's own method is **not** settled here - it is the same question the `TypeLoadException`
poses for `TittyMagic`, so it is one open defect and not two. The three `return null` paths the method already
had (`:461` for a reflection field that is not there, `:467` for an instantiation or base method that will not
resolve, `:472` for an instantiation `ResolveBuilderType` refuses, plus a `bySignature` left null at `:485`) were
deliberately left alone - all four still route the entry to the `unresolved` list and let the compile fail there
rather than write a row the writer cannot name. The new
`NotSupportedException` guard rail at `:536` was **not exercised** by either run - `System.NotSupportedException`
is 0 in the post-fix log - so it is written and unproven by observation. And one scene with one plugin set was
run, which is the standing policy rather than a limit of this check.

### 12. The E-Motion plugin, accepted by a one-byte scene A/B

`VRAdultFun.E-Motion.4` is a third-party plugin that fails on every engine of the ladder, and the failure is its
own: `VRAdultFun.EmotionEngine..cctor` calls `UnityEngine.Random.Range` from a **static initializer**, which Unity
refuses - `Range is not allowed to be called from a MonoBehaviour constructor (or instance field initializer)`.
That initializer runs inside `GameObject.AddComponent` while `DynamicCSharp.ScriptType.CreateBehaviourInstance`
builds the component (`ScriptType.cs:105`, reached from `MVRPluginManager.CreateScriptController`), so it arrives
as a `TypeInitializationException`; the object survives with a poisoned type and rethrows when `Init()` is called.
None of that is this project's code, and the fix is delivered as a **new revision** (`VRAdultFun.E-Motion.5.var`,
`scripts\New-E-MotionPatch.ps1`) rather than as a patch to `src\`.

The question this check answers is the part that is easy to get wrong: a plugin that loads **successfully prints
nothing naming itself**, so "the exception is gone" can equally mean the plugin never ran. What separates the two
runs below is a set of stack frames, not the absence of a message.

*The pair, and how it is rebuilt.* Two copies of one scene, one byte apart:

| | copy | bytes | sha256 | `VRAdultFun.E-Motion.4:` | `.5:` |
|---|---|---|---|---|---|
| control | `Saves\scene\emotion-ab\ladyclown4.json` | 341 667 | `45f040d8…c83956fe` | 1 | 0 |
| candidate | `Saves\scene\emotion-ab\ladyclown5.json` | 341 667 | `523ad903…f28f3039` | 0 | 1 |

Both derive from `Saves\scene\decal-ab\ladyclown38.json` (`artifacts\emotion-repro\make-ab-scene.py`), the copy the
Decal Maker acceptance already built, because it names `Chokaphi.DecalMaker.38` and so keeps the other known
third-party crash out of the pair. The token substitution is size-neutral and touches **exactly one byte**, offset
27942; the control copy is byte-identical to the base. The scene's own content is unchanged - 10 declared atoms,
71 `DAZImport` and 154 `DAZHairGroup` components on both sides. The delivered `.5` needs its own
`AddonPackagesUserPrefs\VRAdultFun.E-Motion.5.prefs` (125 B, written by the delivery script), because
`VarPackage.LoadUserPrefs` hands an unconfirmed package to a `UserConfirm` a headless run cannot answer.

*Run it as check 10 does*, one run per copy, with a warmup long enough for the scene to finish loading inside the
window:

```powershell
scripts\Invoke-SmokeTest.ps1 -Method Play -Seconds 60 -WarmupSeconds 25 `
    -Scene 'Saves/scene/emotion-ab/ladyclown4.json' -LogFile 'artifacts\emotion-repro\A4-ladyclown4.log'
scripts\Invoke-SmokeTest.ps1 -Method Play -Seconds 60 -WarmupSeconds 25 `
    -Scene 'Saves/scene/emotion-ab/ladyclown5.json' -LogFile 'artifacts\emotion-repro\B5-ladyclown5.log'
```

Both runs agree on everything that is not the plugin: `played: 60.0 s`, the scene asked for after 25 s,
`requested=True, taken=True, finished=True after 52.7 s / 52.5 s, refused=False`, `errors before the load: 5`,
`scene atoms: 10 declared, 10 present, 0 missing`, 66 compute shader assets, and `----- RebuildGate OK -----`. The
logs are 900 814 B against 896 661 B, and the per-family census is `artifacts\emotion-repro\census-{A4,B5}.txt`.
**Mind the line counter**: by CRLF terminators alone they are 7 599 and 7 587 lines, and `Get-Content` reads
7 620 and 7 604 because a progress line writes a lone carriage return, which it also counts as a break. Nothing
in the verdict below depends on which of the two numbers is quoted - every reading in it is a frame, counted per
block - but two documents quoting "the line count" of this pair will disagree unless they say which one they mean.

*The verdict is not the count, and that is this check's first lesson.* `PlayVerdict()`
(`RebuildGate.cs:1152-1202`) fails only on a missing `SuperController`, a refused load, an unaudited or unfinished
scene, declared-but-absent atoms, and `PlayErrors` entries containing `" is missing"` or `"Not ready for load"`.
The E-Motion crash is none of those, so **both runs pass** - and the printed integer is not an error count for
this plugin either: it is `PlayErrors.Count`, distinct `[Type] first-line` strings with the stack stripped
(`:808`), deduped through `PlaySeen` (`:812-815`) and capped at `playErrorLimit` (`:56`, `:667`). It reads **18**
in the control against **17** in the candidate - fewer errors where the plugin works, but a number built out of
four missing addon packages, a screen-resolution line, two other plugins' `TypeLoadException`s and eight material
warnings. Read the frames.

*The frames that discriminate, counted per block:*

| frame | control | candidate |
|---|---|---|
| `MVRPluginManager.cs:518 ` (bare - the caught exception's own trace) | 1 - line 1004 | 0 |
| `MVRPluginManager.cs:518)` (with paren - a `Debug.Log` stack) | 0 | 4 - 957, 980, 1003, 1026 |
| `MVRPluginManager.cs:522)` - the `Init` rethrow's reporter | 1 - 1011 | 0 |
| `MVRPluginManager.cs:529)` - the plugin-failure reporter | 1 - 1034 | 0 |
| `MVRPluginManager.cs:640)` - `ReportPluginFailure` | 1 - 1033 | 0 |
| `MVRPluginManager.cs:440)` - the `AddComponent` call site | 2 | 0 |
| `VRAdultFun.EmotionEngine:Init ()` | 0 | 8 - 4 blocks x 2 frames |
| `ScriptType.cs:105` | 2 | 0 |
| `UnityException: Range is not allowed` | 5 | 0 |
| `TypeInitializationException` | 6 | 0 |
| `plugin#1temp` | 5 | 0 |
| `Tried registering param` | 0 | 6 - 4 live, 2 in the tagged list |
| `MVRPluginManager.cs:884)` - the success path, reached on both sides | 4 | 4 |
| `[DynamicCSharp] resolved …` - the repaired compiler's marker | 2 - `32` at 894, `75` at 1049 | 2 - `32` at 894, `75` at 1041 |

*The `:518` spelling trap, and it is the one that matters.* The same line number is spelled two ways in these
logs, because they are two kinds of frame: an original exception trace writes
`at MVRPluginManager.CreateScriptController (…) [0x00251] in <path>\MVRPluginManager.cs:518 ` - no closing paren,
no `Assets\` prefix, an IL offset - while a Unity `Debug.Log` stack writes
`MVRPluginManager:CreateScriptController (…) (at Assets/Scripts/Assembly-CSharp/MVRPluginManager.cs:518)`. A grep
for `:518)` therefore reports **0 in the control and 4 in the candidate** and hides the control's own `:518`; a
grep for `:518 ` reports the reverse. Both readings are true and neither decides anything on its own: search the
bare line number, then read the frame's format.

*The control's failure, as the log states it.* The throw happens inside `AddComponent`, so `CreateInstance` fails
and the poisoned type rethrows at `Init()`; it is reported twice, and each report carries the caught exception's
own trace ending on the bare `:518` form:

```text
Exception during plugin script Init: System.TypeInitializationException: The type initializer for
'VRAdultFun.EmotionEngine' threw an exception. ---> UnityEngine.UnityException: Range is not allowed to be
called from a MonoBehaviour constructor (or instance field initializer) … 'EmotionEngine' on game object
'plugin#1temp'.
  at (wrapper managed-to-native) UnityEngine.Random.Range(single,single)
  at VRAdultFun.EmotionEngine..cctor () [0x00e7d] in <6cbc8299d4434f17a197020d506056ff>:0
   --- End of inner exception stack trace ---
  at MVRPluginManager.CreateScriptController (…) [0x00251] in <path>\MVRPluginManager.cs:518
```

followed by the reporter's own stack - `CreateScriptController (:522)` → `SuperController:Error (:7728)`, through
`SyncPluginUrlInternal (:884)` and `SyncPluginUrl (:583)` from `CreatePluginWithId`'s continuation (`:178`) - and
then by the second, named report, `Plugin VRAdultFun.E-Motion.4:/Custom/Scripts/E-Motion/E-Motion_AddThisONLY.cslist
failed to initialize [TypeInitializationException: …]`, ending `CreateScriptController (:529)` →
`ReportPluginFailure (:640)`.

*The candidate's healthy call, and its positive marker.* The same line is reached and returned from four times,
and the only thing the plugin produces is the side effect of `RegisterUIElements` running: `JSONStorable`
complains when the plugin registers a parameter the scene's instance already owns. The block opens
`(Filename: Assets/Scripts/Assembly-CSharp/SuperController.cs Line: 7728)` and its stack runs `Debug:LogError` →
`JSONStorable:RegisterBool (JSONStorableBool) (…JSONStorable.cs:641)` → `VRAdultFun.EmotionEngine:RegisterUIElements
()` → `VRAdultFun.EmotionEngine:Init ()` → `MVRPluginManager:CreateScriptController (…) (:518)` →
`SyncPluginUrlInternal (:884)` - **no catch frame anywhere in the chain**, which is what makes it a live plugin
rather than a recovered failure. The four live lines are `Tried registering param Adjust Hands that already
exists` at 949 (`RegisterBool`, `JSONStorable.cs:641`) and `Tried registering param Custom Body Weight Mult that
already exists` at 972, 995 and 1018 (`RegisterFloat`, `:732`); the gate's deduped list holds the same two strings
at 6644 and 6645. The control prints this family **zero** times, because its `Init()` never gets that far. It is a
real marker but a weak one - it only appears when a registration collides, so its absence would not prove failure.
The pair of stacks above is what proves success or failure.

*What this check leaves unsettled, stated with it.* The one-byte substitution changes **which revision runs**, not
the plugin's defect: `.4` still throws from its static initializer for anyone who loads it, and the shipped `.4` is
untouched by design. The candidate's own URL appears in neither log - the census reads `plugin url .5 anywhere` as
**0/0** - so its identity rests on `.5` being the only delivered revision, on its prefs file, and on the candidate's
stack reaching `Init()` at all; the scene's token is the only thing that was changed, and the frames are the only
thing that shows the change took effect. The two logs' tagged error lists differ by exactly **one** entry - the
control contributes three E-Motion entries (`Range is not allowed`, the `Init` rethrow, the named failure report)
and the candidate two (`Tried registering param`, deduped to two strings) - so the 18-to-17 difference *is*
E-Motion's replacement, and the rest of both lists is identical. One scene with one plugin set was run, which is
the standing policy and not a limit of this check.

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

#### The skin's materials and textures, read out of the bundle that ships them

Verdict: **the Inspector being asked for does not exist in this project, and cannot.** The character's
17 skin materials and its 23 skin textures are objects *inside the character's own asset bundle*, not
imported project assets, so there are no `.meta` importer settings and no `.mat` file to open. What
would be on those two screens is the serialized data below, and the two specular textures that meet on
the leg are **identical in every one of those fields**. The entire difference between them is in their
pixels: 0.2560 against 0.2990 mean level, 16.8 %, which the shader turns into a factor of 1.42 in
`s.spec`.

- **No project asset for this skin, and so no importer settings for it.** `VaM_Rebuild\Assets\` holds
  72 `.mat` (54 in `Material\`, 18 in `Resources\`) and 167 `png`. The 72 materials are gizmo, UI,
  sky, particle, hair, cursor and projection materials - their `m_Shader` lines are the built-in
  Standard shader, three built-in internal shaders or a Battlehub/skyshop shader guid, and **not one of
  the 72 names a `Custom/Subsurface/*` shader**; the 167 images are editor icons, sky cubemap faces,
  smoke frames, blue-noise tiles and placeholder art, and the only two whose names contain `SPEC` are
  `Cubemap\museum_SPEC.png` and `Cubemap\untitled_SPEC1.png`, neither of which is used by any material.
  Nothing under `VaM_Rebuild\Assets\` has `Lexi` in its name, and no `.mat` in the project has a skin
  map in any slot. The report says the same thing per submesh: `material="Legs-1"
  (Custom/Subsurface/GlossNMTessMappedFixed, bundle only (not in project))`,
  `artifacts\play.report.txt:401`. (The *shaders* do exist in the project - `Irises-1` reads
  `(bundle (project defines one))` - the materials do not.)
- **For contrast, what an importer's settings look like in this project.** Of the 167 imported `png`s,
  99 are `sRGBTexture: 1` and 68 are `sRGBTexture: 0`, and the two closest in role to a body material's
  companion maps - `Material\Chest_DetailMap.png` and `Material\Chest_MaskMap.png`, a detail map and a
  mask map of one material - carry *identical* importer settings to each other:

  | field | `Chest_DetailMap.png` | `Chest_MaskMap.png` | the 23 bundle `Texture2D` objects |
  | --- | --- | --- | --- |
  | `sRGBTexture` / `m_ColorSpace` | 0 | 0 | 1 on D, 0 on S, G and N |
  | `enableMipMap` / `m_MipCount` | 1 | 1 | 13 |
  | `filterMode` / `m_FilterMode` | 1 | 1 | 1 |
  | `aniso` / `m_Aniso` | 1 | 1 | 1 |
  | `mipBias` / `m_MipBias` | 0 | 0 | 0.0 |
  | `wrapU`, `wrapV` / `m_WrapU`, `m_WrapV` | 0, 0 | 0, 0 | 0, 0 |
  | `maxTextureSize` | 2048 | 2048 | 4096, the asset's own size |
  | format | `textureFormat: 1`, platform `-1` = Automatic, `overridden: 0` | same | 10 = DXT1 on D/S/G, 12 = DXT5 on N |
  | `nPOTScale`, `alphaIsTransparency`, `isReadable` | 1, 0, 0 | 1, 0, 0 | fields do not exist |

  Where the two chains *can* be compared they agree on every field, and the project's importers carry
  exactly one `platformSettings` entry each (`DefaultTexturePlatform`, `textureFormat: -1`,
  `textureCompression: 1`, `overridden: 0`), so no texture in this project is imported differently for
  one platform than for another either.
- **Where the skin actually lives.** `VaM_Data\StreamingAssets\f_rlx_mat`, 136.8 MB, contains exactly
  1 `AssetBundle`, 29 `Material` and 23 `Texture2D` objects, and no mesh and no renderer (read with
  `UnityPy` 1.25.3). A texture reference inside it is either `m_FileID=0` (the same bundle) or
  `m_FileID=2` (external) - the latter only for the three `_TessTex` masks. `f_rlx` next to it is the
  behaviour half (12 `MonoBehaviour`, 1 `GameObject`, 1 `Transform`, plus 7 `MonoScript`) - so the names
  *are* on disk after all, as the `m_Name` of those 23 `Texture2D` objects, which refines the earlier
  remark in this document that they are "not on disk or in any `.var`": they are not in the project tree
  and not in any package, but they are in the character's bundle.
- **The upstream tiles, and the resolution they were authored at.** The preset itself carries no paths -
  all 24 of its texture-URL fields are empty strings - so the source package has to be established from
  the game's own import cache instead, and there it is exact. `artifacts\player\Cache\Textures\` holds
  one `<name>_<sourcebytes>_<time>__<space>.vamcache` plus a `.vamcachemeta` for every tile the
  compositor loaded, and the byte size baked into each cache *name* equals the size of the matching
  entry in `AddonPackages\Riddler.Skin_6_4k.8.var` (151.2 MB, 40 entries, 35 of them images):

  | cached tile | bytes in cache name | matching entry in the package | bytes |
  | --- | --- | --- | --- |
  | `TorsoD (Shaved)` | 5,128,805 | `.../Skin 6 4k/Shaved/TorsoD (Shaved).jpg` | 5,128,805 |
  | `TorsoS (Shaved)` | 5,903,643 | `.../Shaved/TorsoS (Shaved).jpg` | 5,903,643 |
  | `TorsoG (Shaved)` | 4,480,183 | `.../Shaved/TorsoG (Shaved).jpg` | 4,480,183 |
  | `TorsoN (Shaved)` | 16,013,554 | `.../Shaved/TorsoN (Shaved).jpg` | 16,013,554 |
  | `LimbsD` / `LimbsS` / `LimbsG` / `LimbsN` | 5,744,950 / 6,499,322 / 4,812,178 / 17,073,505 | `.../Skin 6 4k/Limbs{D,S,G,N}.jpg` | identical |
  | `FaceD` / `FaceS` / `FaceG` / `FaceN` | 5,728,558 / 6,630,261 / 4,838,361 / 21,438,001 | `.../Skin 6 4k/Face{D,S,G,N}.jpg` | identical |
  | `GenitalsD (shaved)` / `S` / `G` / `N` | 596,415 / 618,420 / 479,636 / 1,148,469 | `.../Shaved/Genitals{D,S,G,N} (Shaved).jpg` | identical |

  Sixteen tiles, sixteen exact size matches, so this character's skin *is* that package - and it is the
  package's `/Shaved/` halves for the torso and the genitals side by side with the unsaved face and
  limbs, which is what the package's own layout offers, because `/Shaved/` contains only `Torso*` and
  `Genitals*`. Everything in that folder tree is **4096 x 4096**: all four maps of all four parts, and
  every decal sheet. The four 512 x 512 images in the package are preset thumbnails
  (`Custom/Atom/Person/Skin/Preset_Skin 6*.jpg`), not maps. So there is no lower-resolution map anywhere
  in the tile set and no part whose maps differ from its neighbours' - the "roughness map is a smaller
  atlas than the diffuse" family (claim 2) has nothing here to attach to.
- **The second pipeline's importer settings, and they agree with the first.** Every tile above was
  loaded through `ImageLoaderThreaded`, whose disk-cache entry is named after the flags the loader was
  asked for - `ImageLoaderThreaded.cs:119-150`: `setSize` prepends `width_height`, then `compress`
  appends `_C`, `linear` appends `_L`, `isNormalMap` appends `_N`, `createAlphaFromGrayscale` appends
  `_A`, `createNormalFromBump` appends `_BN<bumpStrength>`, `invert` appends `_I`. Those flags are set
  by map type in one switch, `DAZCharacterTextureControl.cs:881-896`:

  ```
  bool createMipMaps = true;   // :881  for every map type
  bool linear = false;         // :882  the default, i.e. sRGB
  bool isNormalMap = false;    // :883
  bool compress = true;        // :884
  switch (ttype) {
  case TextureType.Specular:
  case TextureType.Gloss:  linear = true; break;                                  // :887-889
  case TextureType.Normal:
  case TextureType.Detail: linear = true; isNormalMap = true; compress = false;
                           break;                                                  // :891-896
  }
  ```

  `Diffuse` and `Decal` therefore match no case and keep the defaults. The cache file names are a
  readback of that rule, and the JSON beside each one repeats its result:

  | map | flags in the entry name | decoded | `.vamcachemeta` | bundle-baked twin |
  | --- | --- | --- | --- | --- |
  | `TorsoD (Shaved)`, `LimbsD`, `FaceD`, `GenitalsD (shaved)` | `__C` | compressed, **not** linear | `"format": "DXT1"`, 4096 x 4096 | `Lexi_*D`: format 10 = DXT1, `m_ColorSpace` 1 = sRGB |
  | `TorsoS/G`, `LimbsS/G`, `FaceS/G`, `GenitalsS/G` | `__C_L` | compressed, linear | `"format": "DXT1"`, 4096 x 4096 | `Lexi_*S/G`: format 10 = DXT1, `m_ColorSpace` 0 = linear |
  | `TorsoN`, `LimbsN`, `FaceN`, `GenitalsN` | `__L_N` | linear, normal map, **not** compressed | `"format": "RGBA32"`, 4096 x 4096 | `Lexi_*N`: format 12 = DXT5, `m_ColorSpace` 0 = linear |
  | the same cache's overlay sheets: `FaceDecal`, `FaceDecal_A`, `Face_D` | `__C`, `__C_A` | compressed, sRGB (the `_A` ones also build alpha from grayscale) | `"format": "DXT5"`, `"DXT1"` for `Face_D`, 4096 x 4096 | - |
  | `Face_S` | `__C_L` | compressed, linear | `"format": "DXT5"`, 4096 x 4096 | - |

  Both pipelines therefore put the diffuse map in sRGB and the specular, gloss and normal maps in
  linear space, and both compress diffuse, specular and gloss - under a rule that is stated per map
  *type* and never per body part, so no part can be treated differently from its neighbour. They differ
  only on the normals, where the runtime obeys an explicit `compress = false` (`:895`) and keeps
  `RGBA32` while the bundle bakes `DXT5`; that is a quality choice on one map type, again uniform
  across all four parts. `createMipMaps = true` (`:881`) is likewise unconditional, for every map type.
  This bullet also settles where the variant suffixes come from: they are the compositor's, matched to
  preset fields rather than inherited from file names, since `skin/Face = 'D'` lands on `Lexi_FaceD (D)`
  (`play.report.txt:786`) and `skin/Nails = 'Purple'` on `Lexi_LimbsD (Purple)` (`:787`) although
  `Purple` is not a file name in any of the 79 `*Skin*.var` packages in `AddonPackages\`.
- **The runtime binds those objects unmodified**, on two independent tells: the bound maps are
  **DXT1/DXT5** (`play.report.txt:807-939`), which a runtime-composed sheet cannot be, because the
  compositor builds `RGBA32` (`DAZCharacterTextureControl.cs:1377`); and the report's own
  averages reproduce the pixels decoded out of the bundle - `Lexi_TorsoD` prints as
  `avg=(0.64, 0.41, 0.33)` (line 828) and decodes to a linear mean of 0.6479, 0.4059, 0.3261. That
  convention is worth stating once, because the numbers below depend on it: **the report prints each
  map's average as the GPU samples it** - linearised for the sRGB-flagged diffuse maps (a stored 0.82
  prints as 0.64) and raw for the linear S/G/N maps (a stored 0.256 prints as 0.26).

**The material side - which slots the character's 17 skin materials carry.** Every material in this
table has all four skin maps from its own tile set; none of them shares a sheet with a material of a
different tile set:

| tile set | shipped `Material` objects carrying it | `_MainTex` | `_SpecTex` | `_GlossTex` | `_BumpMap` | `_TessTex` |
| --- | --- | --- | --- | --- | --- | --- |
| torso | `Head-1`, `Neck-1`, `Ears-1`, `Torso-1`, `Hips-1`, `Nipples-1` | `Lexi_TorsoD` | `Lexi_TorsoS` | `Lexi_TorsoG` | `Lexi_TorsoN` | `torso_tess_mask2` on `Torso-1`/`Hips-1`; none on `Nipples-1` |
| limbs | `Shoulders-1`, `Legs-1`, `Forearms-1`, `Hands-1`, `Feet-1`, `Toenails-1`, `Fingernails-1` | `Lexi_LimbsD` (`Lexi_LimbsD (Purple)` on both nail slots) | `Lexi_LimbsS` | `Lexi_LimbsG` | `Lexi_LimbsN` | `legs_tess_mask` on `Legs-1`, `shoulders_tess_mask` on `Shoulders-1`; none on the rest |
| face | `Face-1`, `Lips-1` | `Lexi_FaceD` (`Lexi_FaceD (D)` at runtime) | `Lexi_FaceS` | `Lexi_FaceG` | `Lexi_FaceN` | none |
| face | `Nostrils-1` | `Lexi_FaceD` | `Lexi_FaceS` | `Lexi_FaceG` | *(no `_BumpMap` slot at all)* | none |
| genitals | `defaultMat-1` | `Lexi_GenitalsD` | `Lexi_GenitalsS` | `Lexi_GenitalsG` | `Lexi_GenitalsN` | none |

Two things about that table matter for a seam:

- **Every texture slot on every one of those materials has `m_Scale=(1,1)`, `m_Offset=(0,0)`.** No
  material carries a `_ST`, a `_tex_ST`, or any per-slot transform: the four maps of a material are
  sampled at the same `uv` through `TRANSFORM_TEX`, and the identity here is what guarantees it. The
  three properties whose names look like texture offsets - `_DiffOffset`, `_SpecOffset`, `_GlossOffset`
  - are **not** UV transforms; they are scalar biases added after the sample
  (`shader-src\VamGpuSkinning.cginc:564`, `:595`, `:596`), so they cannot move one map against
  another either. They are stored data rather than anything computed at draw time: the appearance
  preset's own `skin` storable carries them by name - `Diffuse Texture Offset`, `Specular Texture
  Offset`, `Gloss Texture Offset`, quoted as `-0.003`, `-0.156`, `0.5`, which is exactly the triple
  the bundle materials carry.
- **The tile sets are per body region, and the boundaries fall at the hip and the shoulder.** `Legs-1`
  is the limb tile and its world box is `center (0.00, 0.45, 0.00) size (0.35, 0.60, 0.19)`
  (`play.report.txt:401`), i.e. it ends at `y = 0.75`; `Hips-1` is the torso tile and its box starts at
  `y = 0.745` (`:420`). The only place on the upper leg where two *different* sheets can meet is
  therefore that boundary. The shoulder tile is the same story read the other way round: `Shoulders-1`
  takes the *limb* sheet, between the torso sheet above it and the rest of the arm below, so the arm is
  one sheet from hand to deltoid and the seam is pushed onto the shoulder line. `Hips-1` to `Torso-1`
  is invisible for the same reason - both are the torso sheet.

**The scalar side.** Identical on all 15 body/limb/face/genital slots, in the bundle
(`play.report.txt:940-969`, the 30 `materials[]` lines) and in the runtime dump (the 30
`GPUmaterials[]` lines `:775-803`); only the two nail slots
differ, and they differ in the direction of *less* shading, not more:

| property | body/limb/face/genitals | both nail slots |
| --- | --- | --- |
| `_SpecOffset` | -0.156 | 0 |
| `_GlossOffset` | 0.5 | 0 |
| `_DiffOffset` | -0.003 | 0 |
| `_SpecInt` | 2.272 | 1.0 |
| `_Shininess` | 6.408 | 6.0 |
| `_Fresnel` | 0.8 | 0.0 |
| `_DiffuseBumpiness` | 0.447 | 1.0 |
| `_SpecularBumpiness` | 1.092 | 1.0 |
| `_IBLFilter` | 0.0 | 0.0 |
| `_Tess` / `_TessPhong` | 3.35 / 0.5, tessellated family only | - |
| `_Color` | (1, 1, 1, 1) | (1, 1, 1, 1) |
| `_SpecColor` asset-side | (1, 1, 1, 1) | (1, 1, 1, 1) |
| `_SubdermisColor` | (0.902, 0.804, 0.733, 1) | (0.902, 0.804, 0.733, 1) |

**The texture side - the whole "import settings" equivalent, all 23 objects.** Identity of name for
name; the names in one row share every field in it, and the names in different rows differ *only* in
the column that differs:

| `Texture2D` objects | size | `m_TextureFormat` | `m_ColorSpace` | `m_MipCount` | `m_FilterMode` | `m_Aniso` | `m_MipBias` | `m_WrapU/V/W` | `m_IsReadable` |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `Lexi_TorsoD`, `Lexi_LimbsD`, `Lexi_FaceD`, `Lexi_FaceD (A)`, `Lexi_FaceD (B)`, `Lexi_FaceD (C)`, `Lexi_FaceD (D)`, `Lexi_LimbsD (Pink)`, `Lexi_LimbsD (Purple)`, `Lexi_LimbsD (Red)`, `Lexi_GenitalsD` | 4096 x 4096 | 10 = DXT1 (BC1) | **1 = sRGB** | 13 | 1 = bilinear | 1 | 0.0 | 0, 0, 0 = Repeat | False, except the four `Lexi_Genitals*` = True |
| `Lexi_TorsoS`, `Lexi_LimbsS`, `Lexi_FaceS`, `Lexi_GenitalsS` | 4096 x 4096 | 10 = DXT1 (BC1) | 0 = linear | 13 | 1 = bilinear | 1 | 0.0 | 0, 0, 0 = Repeat | False, except `Lexi_GenitalsS` = True |
| `Lexi_TorsoG`, `Lexi_LimbsG`, `Lexi_FaceG`, `Lexi_GenitalsG` | 4096 x 4096 | 10 = DXT1 (BC1) | 0 = linear | 13 | 1 = bilinear | 1 | 0.0 | 0, 0, 0 = Repeat | False, except `Lexi_GenitalsG` = True |
| `Lexi_TorsoN`, `Lexi_LimbsN`, `Lexi_FaceN`, `Lexi_GenitalsN` | 4096 x 4096 | **12 = DXT5 (BC3)** | 0 = linear | 13 | 1 = bilinear | 1 | 0.0 | 0, 0, 0 = Repeat | False, except `Lexi_GenitalsN` = True |

No object has a `platformSettings` list, an `sRGBTexture`, an `alphaIsTransparency` or an `npotScale`,
because none of them was imported - those fields belong to `TextureImporter`, and there is no importer
in the chain. The only fields that exist are the ones above. And the one asymmetry in them is the
correct one: normals are BC3 while albedo/specular/gloss are BC1, because a BC1 normal map would have
one channel for two.

**The two specular textures, side by side.** `Lexi_TorsoS` is on the pelvis, `Lexi_LimbsS` is on the
leg; they are the two maps that meet at the boundary found above:

| | `Lexi_TorsoS` | `Lexi_LimbsS` |
| --- | --- | --- |
| size / format / colour space | 4096 x 4096, DXT1 (BC1), linear | *identical* |
| mips / filter / aniso / mip bias / wrap | 13 / bilinear / 1 / 0.0 / Repeat | *identical* |
| readable | False | False |
| bundle | `f_rlx_mat` | `f_rlx_mat` |
| material slot scale / offset | (1, 1) / (0, 0) | (1, 1) / (0, 0) |
| mean level as sampled | **0.2560** | **0.2990** |
| after `saturate(sample + _SpecOffset)` | **0.1018** | **0.1447** |

So on the *settings* axis the two specular maps are indistinguishable, and on the *pixel* axis they
differ by 16.8 % of level. That difference is the only one that survives in this pair, and the shader
multiplies it by a highlight that does not clamp it away: `s.spec = saturate(sample + _SpecOffset) *
_SpecColor` is 1.42x larger on the leg side, the Blinn-Phong highlight
(`VamLightHighlight`, `shader-src\VamGpuSkinning.cginc:637-646`) is linear in `s.spec`, and its two
remaining factors move the other way by only 1.3 %: gloss feeds `gg = 2g - g^2` to
`mip = 7 + gg * (1 - _Shininess)` and `exponent = exp2(1 + gg * (_Shininess - 1))` (`VamGlossTerms`,
`:671-676`), which for the shipped `_GlossOffset = 0.5` and `_Shininess = 6.408` gives mip 2.679
against 2.663, exponent 40.03 against 40.56 and `specScale` 6.689 against 6.773. **The highlight is
worth 1.44x across that boundary and the gloss is worth 1.3 %.**

The albedo side of the same boundary is much smaller and the reason is worth keeping: the two diffuse
sheets agree in mean level to 3.5 % (decoded linear means 0.6946 against 0.7109 in channel mean, with
the blue channel further apart, 0.5958 against 0.6332) - the report rounds both to 0.64. What *is*
large between the two sheets is per-pixel artwork: 8.9 % mean absolute difference, with 65 % of pixels
more than 0.05 apart. That number has to be read carefully, because it is a *whole-sheet* statistic
comparing texels that are not the same point on the body - the two sheets are different images with
their own detail density, and their uv islands are disjoint (measured earlier in this section). It
bounds the mismatch; it is not the mismatch at the boundary. The boundary-local version of this number
is the bounded check at the end of this subsection.

**The compositor is present, and it is not engaged.** `src\Assembly-CSharp\DAZCharacterTextureControl.cs`
implements the tile compositing - `Region {Face, Torso, Limbs, Genitals}`, `TextureType {Diffuse,
Specular, Gloss, Normal, Detail, Decal}`, `Init()` at `:2000`, `OnImageLoaded` at `:718`,
`StartSyncImage` at `:868`, `FindTexturesInDirectory` at `:1753` - and its output is a sheet per
body region, always built as `new Texture2D(w, h, TextureFormat.RGBA32, true, linear)` (`:1377`), with
the genital-blend paths refusing to run unless the torso sheet is exactly 4096x4096 (the four guards at
`:1480`, `:1546`, `:1612`, `:1678`, each followed by its own `LogError`). Three things about that
constructor matter for the seam. It takes the region's **own** source size - `new
Texture2D(inTorsoTex.width, inTorsoTex.height, ...)`, `:1377` - so a region's output is as large as its
input and no region is resampled to a common size. It allocates `RGBA32` with a full mip chain (`true`),
so the sheets behind the seam are uncompressed and mipmapped when they are made, whatever they are
baked to afterwards. And its `linear` argument is passed per map by the four call sites, which state
the colour-space rule outright: `BlendGenitalTexture(texture2D, value, false, true)` for the diffuse
sheet (`:1491`, sRGB), `(..., true, false)` for gloss (`:1557`) and specular (`:1623`), and
`(..., true, false, true)` for the normal (`:1689`) - the same split the bundle and the cache both
carry, now a third independent statement of it. The composite is driven entirely by the
appearance preset's texture URLs and by `customTexture_MainTex`, and for this character **every one of
its 24 texture-URL fields is empty** in `Preset_Ren_Lexi.vap` - `{face, torso, limbs, genitals}` times
`{Diffuse, Specular, Gloss, Normal, Detail, Decal}` - every `customTexture_*` field is `""` too, and
the only non-empty texture fields the preset has are `skin/Face = 'D'`, `skin/Nails = 'Purple'` and the
two blend switches `autoBlendGenitalTextures = 'false'`, `autoBlendGenitalSpecGlossNormalTextures =
'true'`. So no sheet is composed at runtime; the
`Lexi_*` names that the materials reference are the compositor's **baked** output, shipped in the
character's bundle. The naming betrays the provenance: the preset carries `"Face":"D"` and
`"Nails":"Purple"` and the bound textures are `Lexi_FaceD (D)` (`play.report.txt:786`) and
`Lexi_LimbsD (Purple)` (`:787`), against a bare-named `Lexi_FaceD` sitting unused in the same bundle -
the `(variant)` suffix is the compositor's variant selector, and the naked name is its default. The
compositor therefore *is* the mechanism that wrote these tiles, and its per-region tile size is the
4096 the shipped sheets still have.

#### The six candidate causes, against that data

| # | claim | verdict | what settles it |
| --- | --- | --- | --- |
| 1 | sRGB mismatch inside one material | **refuted** | every map of every skin material follows one rule with no exception - D `m_ColorSpace=1` (sRGB), S, G and N `=0` (linear) - across all 23 objects in the bundle, and there are no import settings or `platformSettings` that could override a flag for one map. Independently, the only other importer in the chain - the game's own texture cache, written where these tiles were decoded before compositing - made the *same* split, and states it in its own file names: `_C` compressed and not linear on the four diffuse sheets, `_C_L` compressed and linear on the eight specular and gloss sheets, `_L_N` linear and normal map on the four normals, decoded from `ImageLoaderThreaded.cs:119-150` against the flag switch at `DAZCharacterTextureControl.cs:881-896` that sets `linear = true` only for `Specular`, `Gloss`, `Normal` and `Detail`. Two independent importers, same colour space per map kind, no exception - so the mismatch would have to be *inside* one tile, not between the tiles that meet |
| 2 | low-resolution roughness atlas against high-resolution diffuse | **refuted as stated** | all four map kinds of the character are 4096 x 4096 with the full 13 mips; the gloss and specular sheets are exactly as large as the diffuse ones. The substance survives in a different form - see below |
| 3 | hardware / anisotropic filtering | **refuted** | `m_FilterMode=1` (bilinear), `m_Aniso=1` (anisotropy off), `m_MipBias=0.0`, `m_WrapU/V/W=0` (Repeat) on **all 23** objects, so no map is filtered differently from the map beside it; and no filtering rule draws a straight edge |
| 4 | mipmap downgrade | **refuted** | `m_MipCount=13` - the full chain for 4096 - on all 23, and `m_MipBias=0.0`; the runtime report prints the bound maps at the same 4096 in DXT1/DXT5, so no map is a smaller or mipless copy of itself; and the pipeline that builds the sheets asks for mips unconditionally - `bool createMipMaps = true` (`DAZCharacterTextureControl.cs:881`) sits above the per-type switch and is never reassigned |
| 5 | atlas corner bleeding at UV seams | **refuted, and not applicable** | there is no atlas: four sheets per region, one map per slot, one material per region, all slots at scale (1,1) offset (0,0). The nearest real structure is the tile's own outer ring, which is measurably not the interior (on `Lexi_LimbsS` the outer 4 px average 0.376 against an interior 0.288), but with `m_WrapU/V=0` = Repeat an out-of-range uv wraps instead of clamping to that ring, so it is only reachable by an island that reaches the sheet border |
| 6 | material parameters computed per draw call | **refuted** | the parameters are asset data, and every one the report prints agrees between the asset and the GPU: `materials[0]` (`play.report.txt:940`) and `GPUmaterials[0]` (`:775`) both read `_SpecInt=2.272; _Shininess=6.408; _Fresnel=0.800; _DiffuseBumpiness=0.447; _SpecularBumpiness=1.092`, and the tessellated pair carries `_Tess=3.350; _TessPhong=0.500` on both sides. The three offset floats are not printed by the report at all; the bundle's typetree gives `_DiffOffset=-0.003`, `_SpecOffset=-0.156`, `_GlossOffset=0.5` as floats on every body, limb, face and genital material, and 0 on both nail materials - and the same triple is stored, not computed, in the appearance preset's `skin` storable as `Diffuse Texture Offset`, `Specular Texture Offset`, `Gloss Texture Offset`. The one quantity the runtime does change, `_SpecColor`, is `(1.000, 1.000, 1.000)` in the asset (`:940`, `:957`) and `(0.678, 0.725, 0.769)` on the GPU (`:775`, `:792`), and that triple is exactly the preset's `Specular Color` HSV(0.5797102, 0.117347, 0.7686275) converted to RGB (0.6784, 0.7255, 0.7686) - one field of the appearance preset, applied to all 30 slots, so it multiplies both tiles by the same vector and cancels out of any step between them |

**What is left.** Nothing in the settings, the flags, the sampling state or the parameters differs
between the two maps that meet on the upper leg; the maps themselves do, by 16.8 % of specular level
and 1.44x of highlight, and by 3.5 % in whole-sheet albedo mean. So the seam is either (a) that level
difference - two sheets baked separately, from a source set that has no cross-tile continuity to give -
or (b) not a texture-side effect at all. One measured asymmetry looked strong enough to be the answer on
its own and is **not**: the tessellation density masks are the only differently-sized maps in the whole
material set, and the pair that meets on the upper leg is the pair that disagrees - `legs_tess_mask`
2048 x 2048, mean 0.12 (`play.report.txt:811`) against `torso_tess_mask2` 1024 x 1024, mean 0.44
(`:833`) - which through `VamTessDensity` (`shader-src\VamGpuSkinning.cginc:1150-1156`, `sample * _Tess
+ 0.01`, with `VAM_TessScale` defined as `_Tess` at `:231` and defaulted to the asset's own 3.35 at
`:233`) is a **3.6x step in edge factor - 0.41 against 1.48 - on a shared position**. It is not a cause,
and the reason is the same one that makes the hull a hull: `VamTessHull` returns its control points
unchanged (`:1178-1179`), so `VamTessInterpolate` (`:1206`) evaluates the *same* function of bary on
both sides, and a more sub-divided patch is the un-divided one sampled more often. **A density step
moves coverage, not geometry** - which is the same conclusion this document already reached from the
other end, where doubling the density moves the frame by a quarter of what killing the projection moves.
What the density step is good evidence *of* is that these regions were authored as separate regions, at
two resolutions, with a four-fold difference in density. And between two such regions the thing the
shader does not smooth over is the **boundary normals**: each submesh has its own, they were smoothed
apart from each other, and `VamTessInterpolate` feeds them into both the position
(`posOS = lerp(flat, phong, VAM_TessPhong)`, `:1216`, with `_TessPhong = 0.500` on **both** sides,
`play.report.txt:794` and `:795`) and the normal that goes to the pixel
(`nrmOS = p[0].nrm * bary.x + ...`, `:1217`). A step in the border normals is therefore a step in the
normal field exactly on the boundary line - tone and surface detail both change, abruptly, along a
straight edge, with no texture involved. That is a better fit to the symptom than a 1.4x highlight is,
and both `Legs-1` and `Hips-1` are the **same** shader family here (`GlossNMTessMappedFixedComputeBuff`,
`:879` and `:898`), so it is not the family split this document already refuted - it is a *mesh* split
inside one family.

**Three bounded checks, none needing the editor.** They are what separates (a) from (b), and they are
named here rather than run because the mesh is in a bundle on the other agent's side of the fence:

1. **The boundary-local pixel ratio - this decides (a).** From the character's mesh, take the border
   loop shared by the `Legs-1` submesh (`play.report.txt:401`) and the `Hips-1` submesh (`:420`), report
   the (u, v) of those vertices in each sheet, and sample `Lexi_TorsoS` against `Lexi_LimbsS` *there*
   rather than as whole-sheet means. The whole-sheet means say the sheets are 16.8 % apart; only this
   says whether the texels that actually meet are. It is a UV read of a mesh plus two `Texture2D`
   decodes. Prediction if (a) holds: the ratio at the loop stays near 1.42 after `_SpecOffset`.
2. **The border-normal read - this decides (b).** From the same mesh, take both sides of that border
   loop and report the vertex normals the two submeshes carry at coincident positions. If they agree to
   a few degrees, the geometric path is smooth across the seam and (a) stands; if they disagree - which
   is what separately smoothed per-region submeshes do at a border - then the primary cause is geometry
   and no texture, import setting or material value will remove it. This check also has a prediction:
   the seam's magnitude should scale with `_TessPhong`, so an existing frame taken at `_TessPhong = 0`
   should show it *reduced* while one at `_TessPhong = 1` should show it *increased* - a comparison the
   same-size pass behind `seam1..seam5` can make from frames already on disk.
3. **The two-sided frame test.** On a captured frame with the seam visible, sample a line across the
   boundary and report the diffuse-only and the total difference separately: a step that is present in
   the lit result but absent in the albedo locates it in the highlight or in the geometry, not in the
   texture. This is the same shape of measurement as the `seam1..seam5` passes, on frames already
   captured, and it needs no new render.

Remaining unknowns on this axis: which submesh boundary the front-most edge of the visible seam lies on
(this document has the boxes, not the posed screen-space line); whether the islands reach any sheet
border (which is what makes claim 5's ring reachable at all); and the boundary-local level ratio of
check 1.

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
engine we build with (`UnityEngine.CoreModule.dll`; the values were read under 2018.1.9f2 and re-read under
every hop since - 2018.4, 2019.4, 2020.3 and 2021.3 all leave them as they were) as `Alpha = 1, Blue = 2, Green = 4,
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

## A third-party plugin the new engine refuses: Decal Maker

**What a hand run shows.** Play, 2021.3.45f2, and the third-party plugin **Chokaphi's Decal Maker** ends
an unhandled coroutine with `UnityException: Failed to create texture because of invalid parameters.`
The warning printed immediately above it states the rule itself -
`Compressed TextureFormat RGBA Compressed DXT5|BC3 requires a texture size that is a multiple of 4` -
and the throw is the *four-argument* `Texture2D` constructor, which pins the source line exactly: the
package ships **readable C# and no DLL** (one generated `.cs`, 220 502 bytes, compiled at load time by
DynamicCSharp, which is why every plugin frame carries an assembly GUID instead of a path), and it
contains exactly one four-argument construction - `new Texture2D(1, 1, TextureFormat.DXT5, linear)` at
`VAM_Decal_Maker.cs:219`, the cache-miss line of `GetResource`. `1` is not a multiple of 4, Unity
validates a compressed format when the texture is constructed, and `LoadImage` - which would have
reallocated the surface to the decoded 4096x4096 - never runs.

**It is the plugin's defect, it has always been one, and 2021.3 only promoted it.** The plugin assembly
is not compiled by the editor, so no hop can have changed its behaviour, and the *previous* hop's logs
carry the same stack with the device printing the arguments numerically:
`d3d11: failed to create 2D texture id=2645 width=1 height=1 mips=3 dxgifmt=29 [D3D error was 80070057]`
- `E_INVALIDARG`, twice in each of the three 2020.3 baseline logs. Under 2020.3 Unity logged an
assertion and continued with an invalid texture; the `multiple of 4` check is a 2021.2/2021.3 addition
(no 2020.3 log contains that message at all), and with it a silent degradation became fatal, because
`GetResource` and every frame above it is unguarded. The reconstruction contributes nothing: the
dimensions are the plugin's own literals, no game API on the path supplies a size, and the shipped game's
own DXT5 idiom is the legal one - `new Texture2D(4, 4, TextureFormat.DXT5, ...)` at `ImageControl.cs:880`
and `SkyshopLightController.cs:364`, both reproduced in `src\`.

**The fix is a patch to the package, and it is delivered.** `scripts\New-DecalMakerPatch.ps1` copies all
46 entries of `Chokaphi.DecalMaker.37.var` byte-for-byte and rewrites that one line to
`new Texture2D(4, 4, TextureFormat.DXT5, linear)` - the author's format, exactly one DXT5 block, and the
game's own number. (`TextureFormat.RGBA32` is the alternative and has no size constraint at all; it was
not taken because it would change the format the texture reports before `LoadImage` runs, which is wider
than the defect needs.) The result is a sibling package, `AddonPackages\Chokaphi.DecalMaker.38.var`
(1 350 901 bytes); the shipped `.37` is untouched, so the patch is a version bump and the rollback is
`-Remove`. All three `AddonPackages` paths in play - the installation, the editor project and the
standalone player - resolve to one directory through junctions.

**That the higher number wins on its own is not true, and the source says so.** `FileManager.GetPackage`
(`src\Assembly-CSharp\FileManager.cs:1560-1592`) reaches the package *group* for exactly two spellings -
`.latest` and `.minNN` - and resolves everything else by an exact lookup on the uid or the path, where the
uid of a file inside a package is the version-qualified string itself (`VarFileEntry.Uid = vp.Uid + ":/" +
InternalSlashPath`, `VarFileEntry.cs:29`). A scene that says `Chokaphi.DecalMaker.37:/...` therefore loads
revision 37 whether or not `.38` exists, and the first A/B run proved it operationally by loading `.37`
with `.38` sitting next to it. Reaching the scenes that already exist needs the scene's own version token
repointed - an edit on a loose scene file, which is what the A/B did on a copy. `scripts\New-DecalMakerPatch.ps1`
also carries `-InPlace` (rewrite the shipped `.37`, saving the shipped bytes to `<package>.var.original`
first, `-Remove` restoring them), but that is a capability of the tool and not this project's route: the
package belongs to its author, so the delivery is the new revision and the token is the consumer's own
edit.

The second gate is the package's confirmation state. `VarPackage.LoadUserPrefs` (`VarPackage.cs:314-335`)
reads `<userPrefsFolder>/<Uid>.prefs` and defaults `pluginsAlwaysEnabled` to `false` when the file is
absent, after which `MVRPluginManager` hands the package to `UserConfirm` instead of compiling it - an
interactive panel a headless run cannot answer. So a delivered package also needs a 125-byte prefs file,
which the script now writes (and `-Remove` deletes only if it is byte-identical to the one the script
wrote, so a recorded denial is left alone).

What makes the repack
checkable is that it is a byte-level copy rather than a zip-library rewrite: the two script entries have
the same length, share their first and last sixteen bytes, and differ at **exactly two offsets** (11 662
and 11 665, `0x31` -> `0x34`), while every other entry hashes identically.

**Two things are observed and not yet explained, and the acceptance has to respect that.** First, the run
also prints `The referenced script (VAM_Decal_Maker.ButtonExt) on this Behaviour is missing!` twelve times
and `VAM_Decal_Maker.Decal_Maker` once (log `:24999-25014`) - but that block is not decal-specific:
`MacGruber.SuperShot`, `MacGruber.UIDynamicTextInfo` and `VRAdultFun.EmotionEngine` appear in it too, and
the plugin demonstrably *ran*, since the exception is thrown from inside it. Two readings settle the block:
in the 1.6 GB log it sits at `:24999-25014`, some twenty-two thousand lines *after* the crash at
`:2935-2956`, so it belongs to a later moment of one long session rather than to the crash; and in the paired
A/B below it reads **0 in both** runs, the control that still crashes and the candidate that does not, which
makes it unusable as a marker in either direction. The tempting explanation -
that `FileManager`'s incremental package registration (`RegisterPackage`: `uidToVarFileEntry.Add` and
`pathToVarFileEntry.Add` for four keys, with no build-then-swap step) leaves the index half-built when a
read throws inside it - is real as a mechanism but **does not apply here**, because in `GetResource` the
read returns before the constructor throws. Second, that 1.6 GB log names the package nowhere else at all: no
`Chokaphi` line, no `.var` path. So **the absence of the exception is not evidence that the plugin
loaded**, and the acceptance was therefore designed as an A/B (`.37` against `.38`) looking for positive
evidence that the plugin initialises, not for the exception's silence. The qualification that came later: the
*smaller* log that survives - `artifacts\manual-play.log` - does name it, and that is what let the user's own
session be read as the same A/B, in its stronger one-session form (the last paragraph of this section).

**The acceptance has since run, and it passed.** A `.37`-against-`.38` A/B on the same scene, its only
difference a version token, measured per block rather than per raw line count (the raw count *rises*,
28 -> 42, because the eight benign `GetCurrentGPUTexture` blocks the fixed plugin now reaches are matches
too): `Failed to create texture` **2 -> 0**, the plugin's own `multiple of 4` block **1 -> 0**,
`at VAM_Decal_Maker....GetResource` frames **6 -> 0**, `GetResource`/`ConvertNormal` anywhere **2/2 ->
0/0**, `UpdateSkinImage` frames **1 -> 8**, and `GetCurrentGPUTexture` frames **0 -> 8** - which is the
decal-specific positive evidence the paragraph above asked for, since it is the plugin's own skin-image
path being reached instead of throwing on its first cache miss. The run also unloads the plugin's own
bundle by the new revision's name (`Unloading unused asset bundle Chokaphi.DecalMaker.38:...`) and
destroys its component cleanly. Two honest caveats stay attached: that unload line exists only because
the editor was closed gracefully, and the control side of the A/B loaded through the package while the
candidate loaded through a loose copy of the same scene, because a package's internal JSON cannot be
edited in place.

**The mechanism was then isolated on its own, and the pair re-run.** A throwaway edit-mode probe
(`DecalSizeProbe`, editor-side scratch, deleted after the run; its output survives as
`artifacts\decal-repro\step1-texture-probe.txt`) built the constructor the
plugin calls: DXT5 is **refused at 1x1, 2x2 and 3x3, with and without a mip chain**
(`UnityException: Failed to create texture because of invalid parameters.`) and **accepted at 4x4, 8x8,
4x4 DXT1 and 1x1 RGBA32** - so `1, 1` -> `4, 4` is both necessary and sufficient, and the refusal is a size
rule rather than a mip or a format one. `LoadImage` onto a 4x4 DXT5 placeholder then returned `True` on six
real plugin images, every time reallocating the texture to the image's own size (4096x4096, and 8192x8192
for `GenitalMaker/_FemaleGenitals.png`) with a rebuilt thirteen-level mip chain - the "only the format ever
mattered" sentence measured rather than argued. The re-run as a pair (`decal-repro-A37.log` against
`decal-repro-B38.log`, census `step3-ab-pair.txt`) gives the same verdict on 1375 and 2151 lines: the control
prints the rule at `:1132`, the exception at `:1147` and the six-frame stack at `:1151-1157` with
`GetCurrentGPUTexture` at 0, and the candidate prints none of them with `UpdateSkinImage` /
`GetCurrentGPUTexture` at **8 / 8**. One reading from that pair is worth keeping: the control crashed
**three times in one session**, because a stray mouse click (`LookInputModule:ProcessMousePressAlt` ->
`MVRPluginManager:RemoveAllPlugins`) loaded a second scene - so a raw crash count tracks how many scenes were
loaded rather than the defect, and the per-block reading is the one that means something.

Full record: `artifacts\decal-repro\REPORT.md`, machine analysis
`artifacts\decal-repro\step2-acceptance.txt`.

**And the loop was then closed by the user's own hand, which is the same A/B in a stronger form.** The run
behind the fourth paste, `artifacts\manual-play.log` (3 605 lines, 363 186 B, static since 16:42:33), is a
single editor session on the single scene that was failing, `SoftEros777.Lady_Clown.1:/Saves/scene/ladyclown.json`
(`:937`, opened through the scene file browser at `:943`; the boot scene `MeshedVR/default.json` is `:909`, and
no third scene is loaded in the file). It names the revisions, so it reads as a measurement rather than as one
more paste. The `.37` instances throw `Failed to create texture` six times - `:1468`, `:1673`, `:1895` and
`:2119` with the deleted probe's dumps interleaved (`:1637`, `:1859`, `:2081`, `:2305`), then `:2358` and
`:2413` with no probe in sight - and they are torn down as `Unloading unused asset bundle
Chokaphi.DecalMaker.37:...` at `:2316`, `:2371` and `:2426`. The first two teardowns are the URL control's
**Reload** (`JSONStorableUrl:Reload` <- `Button:Press` <- `LookInputModule:ProcessMousePressAlt`), and each
Reload re-creates the same `.37` URL, so the instance that follows throws again (`:2413`, MVID `a036ba03...`);
the third is the plugin's own **Remove** button (`MVRPluginManager:RemovePlugin` <- `CreatePluginWithId`'s
`b__1` <- `Button:Press`, `:2436`). After it the surviving instance runs the skin-image path **eight** times
(`Material:GetTexture` <- `Decal_Maker:GetCurrentGPUTexture` <- `ManagerPanel:UpdateSkinImage`, frames
`:2454`-`:2538`) with **no** `Failed to create texture` anywhere after `:2413`, and its own teardown unloads
`Chokaphi.DecalMaker.38:...` (`:2632`) as the scene closes. A bundle can only be unloaded if it was loaded, so
the surviving instance was the delivered revision - **`.37` throws, `.38` does not, in one session on one
scene, with no scene-side edit at all**.

**The line numbers this document carried for that log before were 9-10 lines early throughout, and the count
of 54 was not a crash count; both are corrected here.** The numbers were taken while the file was still being
written, from a snapshot of about 2 567 lines, so every one of them shifted - `:928` for the scene load,
`:1459`/`:1664`/`:1886`/`:2109`/`:2348`/`:2403` for the throws, `:2306`/`:2361`/`:2416` for the `.37` teardowns
and `:2622` for the `.38` one. The set in this section is the one re-measured against the finished 3 605, and
nothing here should be re-derived from the old set. The **54** occurrences of the engine's own `requires a
texture size that is a multiple of 4` are rule lines, not throws: six belong to the plugin (`:1453`, `:1648`,
`:1870`, `:2094`, `:2343`, `:2398`) and the other 48 are the throwaway probe's deliberate grid - six DXT5 and
six DXT1 per round over four rounds - which is why the probe's dumps sit in the same windows. The 10-line gap
between a rule line and its throw is the log's own frame aliasing: a `MethodName (args)` frame line, then the
IL frame carrying the MVID about ten lines later. Quote the frame line and say which one it is; the frame
lines are the stable reading, the IL lines move with every recompile.

Three limits, in the same spirit as the ones above. The log prints **no plugin URL string**, so the `.38`
attribution rests on the teardown's bundle identity and on the absence of any further `.37` rather than on a
logged value - and the creation of the surviving instance is not logged either, because a plugin whose source
compiles succeeds silently, while the two Reloads are visible only through the teardown each one causes. The
game's own save corroborates the attribution from a second source: `Save Saves\scene\1789828695.json` at
`:2547`, and that file names `Chokaphi.DecalMaker.38` at its own line 894, i.e. the live URL held the delivered
revision by then, with the eight clean blocks at `:2454`-`:2538` just before it and the `.38` teardown at
`:2632` after. And the session **straddles** commit `fe96e9a`, which deleted the probe sources at 16:39 while
this editor was open (the log records the deletion and the following recompilation), so only the throws at
`:2358` and `:2413` are live-plugin evidence. Each reload recompiles the plugin through `DynamicCSharp`, which
is why the crashing MVIDs progress `de7b9d90...` -> `828f997d...` -> `a036ba03...`.

**And the class of defect has exactly one member on this machine.** Every `new Texture2D(...)` reachable from
the installation was enumerated as text - all 80 `*.var` archives unpacked and scanned as ZIPs (12 758
entries, 1 221 text files read) plus the loose `Custom\Scripts\` sources, 24 textual matches over **18
distinct sites**, 16 of them live - and exactly one can construct a compressed texture at a size the engine
refuses: `VAM_Decal_Maker.cs:219`. Fourteen are provably valid (the 4 096-square RGBA32 icons each revision
carries, ARGB32, RGB24, and the delivered `DXT5` at 4x4), two are commented out, and one is indeterminate
rather than invalid: `MacGruber.Essentials.16`'s `MacGruber_SkyMagicLoader.cs:223` builds
`new Texture2D(mipsize, mipsize, cube.format, false)` where the size is a power of two and therefore safe, but
`cube.format` comes from `mySkyProbe.customBakedTexture as Cubemap` at runtime and cannot be resolved from
text. The delivered revision differs from the shipped one in **this line alone** - one changed line, the md5
the compiled plugin assembly name embeds is `6BA98B00...` in the shipped `.37` against `B91C3B6A...` in the
delivered `.38` - which is an independent confirmation of the byte-level minimality argument above. Full
record: `artifacts\plugin-census\REPORT.md` and `census.csv`.

The full record - the method verbatim, the call chain with its IL offsets, the package provenance, the
2020.3 A/B and the evidence file list - is `docs\decal-texture-crash.md`.

## Still open

- **`MacGruber.Life` - closed, and closed as a live plugin rather than as a failure that moved.** Under
  2021.3 the plugin compiles once the by-ref `TypeParameterInflator` gap is repaired (`bb6a177`), and that is
  measured on this very plugin: `[CS584]` **10 -> 0** and `Compile of MacGruber... failed.` **5 -> 0** between
  `artifacts\manual-play-cs584-before.log` and `artifacts\manual-play.log`. The single `failed to initialize`
  left in the post-fix run is another plugin's, `VRAdultFun.E-Motion.4` (`TypeInitializationException` on
  `VRAdultFun.EmotionEngine`, `:1372`).

  **Compiling clean is not the verdict - instantiating is.** The positive markers read **1 / 1 / 2** in
  `artifacts\manual-play.log` (`MacGruber_Breathing.audiobundle` unloaded, `MacGruber.Breathing:OnDestroy`,
  `Unloading unused asset bundle MacGruber`) and **0** in every earlier run, so the plugin's `Breathing`
  MonoBehaviour existed there and did not before. The `FieldAccessException` the two earlier hops recorded is
  **absent from all 25 logs naming `2021.3.45f2`** and present in **47** of the ones before them, each
  carrying it twice - 31 on `2018.1.9f2` (oldest `artifacts\smoke-manual.log`, 09-15 22:35, and still there in
  `artifacts\smoke-plugin-fix.log` 09-17 17:38 and `artifacts\pluginrefs-fixed.log` 09-18 13:30, i.e. after
  the plugin-reference A/B), 2 on `2018.4.36f1`, 8 on `2019.4.41f2` and 6 on `2020.3.49f1` (newest
  `artifacts\logs\hop4-baseline2-default-smoke.log`, 2026-09-19 00:10) - so `MiniQueue'1`'s private field was
  inaccessible to the 2018/2019/2020 Mono and is not to the 2021.3 one. An
  `Unload demand activated morph MacGruber.Life.12:.../Breathing_*.vmi` line is **not**
  evidence either way: the pre-fix control carries 12 of them while the plugin never compiled at all, because
  `default.json` declares `plugin#0_MacGruber.Breathing`'s morphs and the atom loads those `.vmi` files
  itself.

  **And a gate cannot see a plugin that works.** The plugin path prints failures and teardowns and nothing
  else, which is why `MacGruber_Breathing.audiobundle` reads **0** in both `artifacts\smoke-play.log` and
  `artifacts\logs\hop4-baseline2-default-smoke.log` against **1** in the hand run: the 2021.3 gate's plugin
  silence is not a reading about the plugin in either direction. What it does show is that nothing failed
  where 2020.3 failed - 5 errors against 12, the plugin block contributing 7 of them.

  The old bullet also named the wrong scene, and that misreading is worth one line because it recurs: the
  `atoms: 18` reading is the **boot scene**'s (`default.json`, `hop4-baseline2-default-smoke.log:6240`),
  while `CyberDemoAlt` is 17 of 17 and is named **0** times in that log, in `artifacts\smoke-play.log` and in
  `artifacts\manual-play.log`.

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
