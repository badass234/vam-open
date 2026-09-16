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
character: the body skin sits on **our** reconstructions while the hair, cloth and eye-reflection
items sit on **bundle copies the project never defined**.

| Population | Families (slots) | Origin |
|---|---|---|
| body skin | `GlossNMTessMappedFixedComputeBuff` (14), `GlossNMCullComputeBuff` (26), `CullComputeBuff` (8), `GlossCullComputeBuff` (7), `AlphaMaskComputeBuff` (2), `Transparent{Gloss,}ComputeBuff` (3) | project |
| hair item | `Custom/Hair/MainSeparateAlphaLayer1` (7), `..Layer2` (6), `..Layer3` (6) | bundle only |
| cloth | `Custom/Subsurface/TransparentGlossNMDetailNoCullSeparateAlpha` (10) | bundle only |
| eye reflection | `Marmoset/Transparent/Simple Glass/Specular IBLComputeBuff` (2, 6 passes) | bundle only |

This supersedes the earlier reading in this file that the body is unlit and that the whole scene
renders flat: the body's slots resolve to project shaders with their passes intact, and the reported
symptoms - gloss and bump *reacting* to light, only disagreeably - are not symptoms of an unlit
surface.

Evidence is `artifacts\census3-play.report.txt`, produced by `ShaderCensus()` in
`VaM_Rebuild\Assets\Editor\RebuildGate.cs`, which resolves every material a draw path reaches to its
shader, pass count and origin. See section 3 for the command.

### Defect 1 - gloss and bump seams where Torso meets Head, Neck and Forearms

Verdict: **the shader side is not the cause.** The cause is data-side (material values or the
skinned buffers), or the original has the same seam.

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
* **The one remaining shader-side deviation is the additive pass**, which is still a plain Lambert
  diffuse in ours (`VamGpuSkinning.cginc`, header). It is identical for both families, so it too
  cannot open a seam, but it does make the per-light response wrong and stays on the list.
* **Material assignment cannot mix neighbours up either.** `MeshGPU.InitMaterials` sizes
  `GPUmaterials` from `dazMesh.materials.Length`, and both draw loops
  (`MeshGPU.DrawMesh`, `DAZSkinV2.DrawMeshGPU`) pass the same index as both the material slot and the
  submesh index to `Graphics.DrawMesh`, gated by `materialsEnabled[i]`. Submesh `i` therefore always
  gets `GPUmaterials[i]`, so two adjacent submeshes can never swap materials.

What was not measured, and is what the seam now has to be: the per-material values the active
character's 30 `GPUmaterials` entries actually carry, and the handedness (`float4.w`) of the skinned
tangent buffer that `DAZSkinV2` writes, since `VamSkin()` Gram-Schmidt-orthogonalises the binormal
around it.

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

Verdict: **mixed.** Two of the three parts are ours; the third is the original shader.

- `Eyelashes-1` is on the project `Custom/Subsurface/TransparentGlossNoCullSeparateAlphaComputeBuff`
  (2 passes) with `_AlphaTex=Olympia6EyelashesTr`. Wrongly applied lashes are a transcription defect.
- `Cornea` is on the project `Custom/Subsurface/AlphaMaskComputeBuff` (2 passes), which is the likely
  source of an over-bright eye.
- `EyeReflection-1` is on the bundle-only `Marmoset/Transparent/Simple Glass/Specular IBLComputeBuff`
  with 6 passes, i.e. the original game shader, so its intensity is not a porting gap; it depends on
  the environment the scene sets (`_SpecCubeIBL=SkyCyber2SPEC`, `_ExposureIBL=(0.1, 0.02, 0.002, 1)`)
  and has to be compared against the original before anything is changed.

### Defect 4 - cloth with an inverted colour and no alpha

Verdict: **`_AlphaTex` is never assigned, and the family is absent from the project.**

`Pantie_Cloth-1`, `Pantie_Sides-1`, `Stocking-1`, `Shoe-1` and `Sole-1` are all on the bundle-only
`Custom/Subsurface/TransparentGlossNMDetailNoCullSeparateAlpha` (2 passes), with
`_AlphaTex=NONE`, `_MainTex=AyaneLeg_Diffuse`, `_SpecTex=_Specular`, `_GlossTex=_Glossy`,
`_BumpMap=_Displacement` and `_DetailMap=_Bump`. A family named *SeparateAlpha* whose `_AlphaTex` is
empty samples an alpha that was never assigned, which is a transparency defect by construction; the
inverted colour is the `_DetailMap` and `_BumpMap` slots pointing at the diffuse and displacement
maps, which is how a material looks before the clothing loader has remapped its slots.

### What the census cleared

Two things that looked like suspects are not: `Marmoset/Transparent/Cutout/{Diffuse,Bumped
Diffuse,Specular,Bumped Specular} IBL` and `Marmoset/Beta/Skin IBL Soft` fail to compile in
`artifacts\manual-play.log` (`All passes removed`), but no material of this scene uses them, so they
cannot be responsible for anything on screen. Separately, the `fallback shader ... not found`
warnings are real but belong to the transcription backlog: our generated `*ComputeBuff` shaders
declare `Fallback "Marmoset/Specular IBL SoftComputeBuff"` and
`"Marmoset/Specular IBL Soft NoCullComputeBuff"`, names faithful to the original that
`transcribe-hair-marmoset` has not built yet.

## Still open

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
