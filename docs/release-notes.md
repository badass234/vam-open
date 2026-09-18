# Release notes, long form

[`CHANGELOG.md`](../CHANGELOG.md) keeps one section per version and each one is short on purpose, so the
version history stays readable at a glance. This file is where the detail behind those sections lives. It
keeps the numbers they quote, including versions and items that have since been fixed, because a release
note describes the release and not the present.

The analysis behind the measurements is in the other files in this directory - `verification.md` for the
look, `shader-reconstruction.md` for the families, `rebuild-project.md` for the project and its harness,
`asset-export.md` for the extraction, `parity-report.md` for the assembly.

## 0.1.10-alpha

- **The engine moved to Unity 2020.3 LTS (2020.3.49f1).** Hop three of the engine migration, made the same
  way as the two before it (`Unity.exe -batchmode -nographics -quit -accept-apiupdate -projectPath
  VaM_Rebuild`, then the editor opened and left to run its own passes), and by a wide margin the cheapest
  of the three: the editor answered with **34 `error CS` lines**, then **1**, then **0** - against 612 in
  the hop before it. Those 34 are two families and no more: **six `CS0121` ambiguities** in one
  hair-rendering file and the `XRDevice` removals, and the single line of the second report is one removed
  editor property in the gate script. `ProjectVersion.txt` names `2020.3.49f1` with revision
  `18249dd5551b`; the asset import pipeline stays on **V2**, which is what hop 2 pinned, and both the batch
  gates and the editor window still report `Using Asset Import Pipeline V2.`.
- **One guide row needed code, and it is the UI one.** 2020.1 stopped declaring
  `[RequireComponent(typeof(CanvasRenderer))]` on `UnityEngine.UI.Graphic`, so every user-written subclass of
  it here declares the attribute itself - `NonDrawingGraphic.cs`, `ShineEffect.cs`, `TextPic.cs`,
  `UIPrimitiveBase.cs`, each next to its `[AddComponentMenu]`, which is the fix the guide's own sample gives.
  The other 2020 rows were checked and closed: no mesh import here generates secondary UVs
  (`generateSecondaryUV: 1` in **0** `.meta` files), the project builds no AssetBundles of its own and loads
  the ones 2018.1 built, no HLAPI package is added and no `NetworkIdentity`/`NetworkManager`/`NetworkServer`
  is named anywhere in `src\`, nothing bakes lightmaps (`LightingData.asset` comes from the export and the
  lightmaps from the game's bundles), and `AdaptivePerformance`, `ParticleSystemForceField` and the Code
  Coverage package have 0 occurrences. The XR row's `renderScale` → `eyeTextureResolutionScale` was already
  done - the 7 `XRSettings.eyeTextureResolutionScale` uses are the migrated form, and the 39 remaining
  `renderScale` hits are local fields.
- **An addition, not a removal, produced the six ambiguities.** `Material.SetBuffer(string, GraphicsBuffer)`
  arrives in 2020.1, so a bare `null` at the six hair-rendering call sites in
  `GPUTools\Hair\Scripts\Runtime\Render\HairRender.cs` (lines 57, 66, 74, 121, 130, 138) now matches two
  overloads. Each is cast `(ComputeBuffer)null` where the decompiled call passed `null`.
- **What `XRDevice` became.** `XRDevice.isPresent` (obsolete, `CS0619`) is `XRSettings.isDeviceActive` on
  **5** sites - `OVRDebugHeadController.cs:63`, `SuperController.cs:18666` and `:19584` (the
  `"XR device present is "` line), `UnityStandardAssets\WaterVR\Water.cs:235`, `UserPreferences.cs:3910`;
  `XRDevice.model` (`CS0117`) is `InputDevices.GetDeviceAtXRNode(XRNode.Head).name` in
  `SuperController.cs:19586`; and `Leap\Unity\XRSupportUtil.cs` moves `XRDevice.userPresence` and
  `UserPresenceState` to the input-device feature
  (`device.TryGetFeatureValue(CommonUsages.userPresence, out present)`), keying its warning on `!supported`.
- **Two scripts and the manifest had to follow the editor.** 2020.2 ships no Boo/UnityScript under
  `MonoBleedingEdge\lib\mono\unityscript\`, while the game's `CharacterMotor.cs` (the one file left in
  `Assembly-UnityScript`) references `Boo.Lang`: `scripts\Setup-RebuildProject.ps1` now stages the game's own
  `Boo.Lang.dll` (2.0.9.5, `PublicKeyToken=32c39770e9a21a67`) as a plugin when the editor's copy is gone,
  where under 2019.4 the same run must not - **the branch has to flip back if the project ever returns to
  2019.4**. `scripts\New-PlayerRuntimeLinks.ps1` stops linking `StreamingAssets` wholesale, because the
  bumped `com.unity.purchasing` makes Unity Services write `UnityServicesProjectConfiguration.json` into it
  on every editor start, which a junction refuses: it is a real directory now with that one file filtered
  through. The plugin compiler's fixed reference list grew by one name - `UnityEngine.InputLegacyModule.dll`,
  six entries to seven in `DynamicCSharp.cs` - because a plugin naming `Input` needs the module the engine
  split it into. The gate script lost `CodeEditor.CurrentEditorPath` (removed in 2020.3) for
  `CurrentEditorInstallation`, which carries the same `EditorPrefs "kScriptsDefaultApp"` value.
- **The plugin compiler killed the editor on this hop, and that is the headline.** The first plugin compile
  under 2020.3 died natively - not the plugin, the whole process - with `requested token for MethodBuilder`,
  raised inside Mono's `mono_image_create_token` (`sre.c:1237`) as a non-continuable `g_error` and reached
  from `ModuleBuilder.Save()` at `McsDriver.cs:221` under `ves_icall_ModuleBuilder_build_metadata`. The table
  the token creator walks is the module's **overrides table**, and it has no case for an entry whose owner is
  a `MethodOnTypeBuilderInst` - a method of a generic type that is still being built - so the branch ends in
  `g_error("requested token for %s")` and there is no per-plugin failure to read. Exactly one plugin triggers
  it: `everlaster.TittyMagic`, whose `EnumerableExtensions+<ForEach>c__Iterator0<T>` implements
  `IEnumerator<T>.get_Current` and `IEnumerable<T>.GetEnumerator` and contributes **2** such entries into a
  run that resolves **32**. It was named before it was fixed, and named without the editor: a 27-reference
  harness driven by the machine's own `mcs.exe` over every plugin `.cs` reproduced the abort and printed the
  two entries.
- **The fix is in this project's own copy of the compiler** (`src\Assembly-CSharp\DynamicCSharp\Compiler\
  McsDriver.cs`, +359 lines). Before the module is saved, `RepairMethodOverrideDeclarations` resolves each
  `MethodOnTypeBuilderInst` entry onto the real `MethodInfo` of the created generic instantiation - matching
  by `MetadataToken` when the entry carries one, else by name and parameter count - through
  `ResolveOnTypeBuilderInst` / `ResolveBuilderType` / `FindField`, so the table holds `MethodBuilder` and
  `MethodInfo` entries only. The repair is skipped when the module stays in memory (`generateInMemory`),
  which is the path that never needed it. Two limits are worth stating: it repairs the *shape* rather than
  one plugin's name, so the next plugin written like this compiles instead of aborting; and generic
  *methods* on a builder type (`method_arguments.Length > 0`) are unhandled, because no harness run and no
  real batch ever produced one.
- **What the hop was verified by.** The scene that aborted the compile now compiles and renders: the Lady
  Clown smoke logs `resolved 32 method override declaration(s)` (`McsDriver.cs:405`), leaves no crash report
  and reaches `Benchmark complete. … Avg. FPS: 186.37` with the scene's **10 declared / 10 present / 0
  missing** atoms (`artifacts\logs\hop3-methodimpl-ladyclown-smoke.log`). The boot-scene regression returns
  hop 2's own reading - **18/18 atoms**, `Avg. FPS: 218.06` (`hop3-baseline-default-smoke.log`) - which is
  what says the engine move changed no picture. The timed Play gate prints `errors and exceptions: 6` and
  `----- RebuildGate OK -----` (`hop3-play-gate-120s-fixed.log`), the sixth error being the Purchasing one
  below. The standalone player **builds and boots on 2020.3**: `----- RebuildPlayer OK -----`, 10 scenes with
  `NewStart.unity` as the boot scene, `browser: 85 runtime files staged`,
  `Version is '2020.3.49f1 (18249dd5551b) revision 1582237'` (`artifacts\player-build.log`), and the built
  player boots clean at `Avg. FPS: 300.21` (`artifacts\logs\hop3-player-2020.3.log`). The plugin compiler
  rebuilt from source under the new editor is **byte-identical** to the 2018.4 and 2019.4 builds
  (1 967 104 B, SHA-256 `FC5C08BC…`).
- **The package set moved with the LTS, and one move costs a log line.** The manifest rewrote six pins -
  `com.unity.ads` 2.0.8 → 4.4.2, `com.unity.analytics` 3.2.3 → 3.6.12, `com.unity.collab-proxy` 1.2.15 →
  2.0.4, `com.unity.purchasing` 2.2.1 → 4.8.0, `com.unity.textmeshpro` 1.4.1 → 3.0.6, `com.unity.timeline`
  1.2.18 → 1.4.8 - and added `com.unity.ide.visualstudio` 2.0.18. The Purchasing bump is the visible one:
  the timed gate's error list goes from **5 lines to 6**, and the sixth is
  `UnityEditor.Purchasing.ProductCatalogEditor`'s type initializer failing to load
  `UnityEngine.UnityWebRequestModule` in a `-batchmode` run - an editor-side package this project does not
  call, arriving before the module it wants, and absent from the windowed editor the hand run uses.
- **What this hop does not close.** The abort inside `mono-2.0-bdwgc.dll` is still open (plan item 2): it
  has only ever been reproduced on 2019.4, and the four runs of this hop peak at **73 196** loaded objects
  against the **272 217** of the run that faulted, so they are too small to count as the engine having
  cleared it. A long batch play of that size is what would answer it. The hop itself was closed by the hand
  run on the editor, taken 2026-09-18: `scripts\Invoke-ManualPlay.ps1` on the boot scene, `Avg. FPS: 300.68`,
  everything the eye checks intact, and only the pre-existing `MacGruber` plugin pair failing
  (`artifacts\manual-play.log`).

## 0.1.9-alpha

- **The engine moved to Unity 2019.4 LTS (2019.4.41f2).** Hop two of two, again made by the editor's own
  API Updater (`Unity.exe -batchmode -nographics -quit -accept-apiupdate -projectPath VaM_Rebuild`), and
  again only a handful of sites had to change. The first open answered with **612 `error CS` lines**;
  splitting them by origin says where the work was: **548 of them were
  `Library\PackageCache\com.unity.package-manager-ui@2.0.13`** (428 `CS0246`, 68 `CS0234`, 44 `CS0308`,
  8 `CS0115`) - that version of the editor's *own* package manager was pinned in `manifest.json` by the
  2018.4 editor, and its UI files are written against 2018.4's UI Elements, so under 2019.4 they cannot
  compile at all. 62 more lines were four distinct `CS0619` messages on **8 sites** of this project's own
  code, and 2 were one ambiguity. Removing the pin lets the editor use the copy of the package manager it
  ships with, and `com.unity.package-manager-ui` is no longer in the manifest.
- **What 2019.1 removed, and what replaced it.** `MovieTexture` is gone from the engine, so
  `PersistentData` no longer registers it and `PersistentMovieTexture` no longer names the type in
  `WriteTo`/`ReadFrom` - the class and its `loop` field stay, because they are the shape of what a saved
  file carries, not a use of the engine type; the same reasoning keeps `PersistentGUIElement` while
  `typeof(GUIElement)` leaves the registry. The engine's enum-less
  `Graphics.DrawProceduralIndirect(MeshTopology, ComputeBuffer, int)` is obsolete with the migration
  `(UnityUpgradable) -> DrawProceduralIndirectNow(*)`; 2019 split drawing into a deferred and an immediate
  form, and the decompiled call is the immediate one (`material.SetPass` right before it), so the three
  sites in `CinematicEffects\DepthOfField.cs` and `ImageEffects\DepthOfField.cs` become
  `DrawProceduralIndirectNow`. Last, `LeapEyeDislocator` had to qualify its attribute:
  `[InspectorName("Baseline")]` is ambiguous under 2019, because the engine added
  `UnityEngine.InspectorNameAttribute` next to Leap's own property-drawer attribute of the same name
  (`CS0104`); it is now `[Leap.Unity.Attributes.InspectorName("Baseline")]`.
- **Unity UI is a package from 2019.2, and that had to be declared.** Under 2019.4 the editor no longer
  carries `Editor\Data\UnityExtensions\Unity\GUISystem\` (only Tango and UnityVR are left there), and
  `com.unity.ugui` sits in `Editor\Data\Resources\PackageManager\BuiltInPackages\` beside
  `com.unity.2d.sprite`, `com.unity.2d.tilemap` and `com.unity.package-manager-ui`. `UnityEngine.UI` is
  referenced by **309** files in `src\`, and before the pin the 36-entry manifest resolved it only
  transitively - through `com.unity.purchasing` - so a future `purchasing` change would have taken the
  whole UI with it. `"com.unity.ugui": "1.0.0"` is now an explicit dependency, which the lock file records
  as `depth: 0` where it used to be `1`.
- **The plugin compiler is built from source under the new editor too, and the result is the same file.**
  The recipe is editor `mono.exe` + `MonoBleedingEdge\lib\mono\4.5\mcs.exe` over the 797 sources in
  `src\mcs`; both binaries exist in the 2019.4 editor, `scripts\Build-McsCompiler.ps1` runs to exit 0, and
  the output is **1 967 104 B with SHA-256 `FC5C08BC…`, identical to the 2018.4 build and to the
  `mcs.dll` in `artifacts\player\VAMOpen_Data\Managed\`**. So the compiler this project ships does not
  depend on which editor built it, and a hop needs no second look at it.
- **The runtime plugin compiler needed one more thing after the hop, and it was data, not code.** Timeline
  left the engine between 2018.1 and 2019.4 and became the package `com.unity.timeline`, which brings
  `Unity.Timeline.dll` where the engine used to ship `UnityEngine.Timeline.dll` (93 184 B) plus a
  5 632-byte `UnityEngine.TimelineModule.dll`. `DynamicCSharp` does not scan `Managed\`: it compiles a
  plugin against the **bare file names listed in `Assets\Resources\DynamicCSharp_Settings.asset`**, and
  Mono treats one name it cannot resolve as fatal for that whole compilation -

  ```
  [CS6]: Metadata file `UnityEngine.Timeline.dll' could not be found
  Compile of MacGruber.Life.12:/Custom/Scripts/MacGruber/Life/MacGruber_Life.cslist failed.
  ```

  - so every community plugin naming Timeline stopped loading, reported as nothing more than a plugin that
  never appears. The names are resolved in `Directory.GetCurrentDirectory()`, which is why this looked like
  a player-only defect: in the editor the working directory is the installation root, whose `Managed` still
  carries the game's own assemblies and both old names, while the player runs from its own folder. The
  shipped list is generated with `Assets`, so the adaptation lives in
  `scripts\Update-PluginCompilerReferences.ps1` (two names: `UnityEngine.Timeline.dll` →
  `Unity.Timeline.dll`, `UnityEngine.TimelineModule.dll` dropped) and is step 7 of the project setup; a
  build re-checks it against the player it has just made. **Measured both ways on the boot scene in the
  editor: 5 log lines naming the missing file and 2 failed plugin compiles with the shipped list, 0 of each
  with the adaptation** (`artifacts\pluginrefs-stale.log` against `artifacts\pluginrefs-fixed.log`), where
  the earlier standing count to beat was **16 failed compiles**. What is left of that plugin is its own
  runtime fault rather than the compiler's: `MacGruber.Breathing` now loads far enough to throw
  `FieldAccessException` on a private field of its own nested generic type during `Init`. The A/B is the
  regression test for the next engine hop, and the chain is written up under *The plugin compiler* in
  `docs\rebuild-project.md` with the check itself as section 7 of `docs\verification.md`.
- **The hop rewrote two more tracked project files, and touched nothing else.**
  `ProjectSettings\ProjectVersion.txt` now names `2019.4.41f2` (with the revision line the editor adds),
  and `ProjectSettings\GraphicsSettings.asset` came back with `serializedVersion: 12 → 13`, one more
  built-in shader in the always-included list (`fileID: 16001`), `m_LogWhenShaderIsCompiled: 0` and
  `m_AllowEnlightenSupportForUpgradedProject: 1`. Both are kept as the editor wrote them. The fields the
  hop *could* have moved did not move: `scriptingRuntimeVersion: 1`, `apiCompatibilityLevel: 3` and
  `allowUnsafeCode: 1` are as they were under 2018.4, and `EditorSettings.asset` still reports
  `serializedVersion: 7`, with the asset import pipeline left to the entry below.
- **The item-by-item audit of the 2019 LTS guide came out with one item that needed a manifest change and
  no others.**
  `docs\unity-upgrade-audit.md` carries the full table with the evidence per line; in short: `Addressables`,
  animation C# jobs, `UIElements` and LWRP/URP are not used by this project at all, `ShaderUtil`'s renamed
  clearing call is never called (the only `ShaderUtil.` in the sources is this project's own
  `RuntimeShaderUtil`), tilemap and sprite tooling are not used, `.NET 3.5` was already gone, and the
  `UNet` high-level API question ends where `tools\audit_plugin_apis.py` says it does: pointed at the 98
  binary candidates it walks 13 managed assemblies, 22 native DLLs and 61 files with no CLR metadata, and
  the only hits of the whole guide's API list are `NetworkPlayerSurrogate` / `NetworkViewIDSurrogate` in
  `RTTypeModel.dll` - the two surrogate types this repository carries on purpose, which touch no Unity
  networking API (0 hits for `NetworkView`, `NetworkIdentity`, `NetworkManager`, `Network`, `MasterServer`,
  `RPCMode`). The `NetworkMatch` component that does exist compiles against 2019.4, where it is still part
  of `UnityEngine`.
- **The standalone player builds and boots on 2019.4.** `scripts\Invoke-PlayerBuild.ps1` ends with
  `----- RebuildPlayer OK -----`, the log banner reads
  `Built from 'HEAD' branch; Version is '2019.4.41f2 (6b23d448b533) revision 7021524' Using compiler
  version '191627012'`, 10 scenes are enabled with `NewStart.unity` as the boot scene, and the output is
  `artifacts\player\` - `VAMOpen.exe` 650 752 B, 1 420 665 723 B in total.
- **And the timed Play gate passes on 2019.4.** The ordinary boot run
  (`scripts\Invoke-SmokeTest.ps1 -Method Play -Seconds 150 -WarmupSeconds 15`) reports
  `errors and exceptions: 0`, `loading: SuperController=False, GlobalSceneOptions=False, simulation
  resetting=False`, `atoms: 4 ([CameraRig], CoreControl, PlayerNavigationPanel, WindowCamera)`,
  `probes: no TEMP DIAGNOSTIC probe is compiled in`, `0` shader errors and warnings, and ends with
  `----- RebuildGate OK -----` - the same reading the same gate gave under 2018.4, which is the point:
  the hop did not change what the boot does. Log: `artifacts\smoke-play.log`.
- **The editor window and the batch gates were on different asset import pipelines, and the project now
  names one.** 2018.1's `EditorSettings.asset` carries no pipeline key at all, and with the key absent the
  entry point decides: every batch run of this hop printed `Using Asset Import Pipeline V1.`
  (`artifacts\compile-gate-timeline.log:30`), while the editor's own window printed
  `Using Asset Import Pipeline V2.` and `Rebuilding Library because the asset database could not be found!`
  (`artifacts\editor-open.log:25-26`) - so one `Library` held both databases (`assetDatabase3` for V1,
  `ArtifactDB`/`SourceAssetDB` for V2) and each switch between the two paths paid a full reimport of the
  166,124 imported objects. The key the engine actually serialises is **`m_AssetPipelineMode`**: it sits in
  the native editor's `EditorSettings` table rather than in `UnityEditor.dll`, which is why the name these
  notes had carried until now (`m_AssetPipelineVersion`) appears in no build, and it takes the numbers of
  `UnityEditor.AssetPipelineMode` (`0` = V1, `1` = V2). That mapping was measured rather than assumed - set
  in a throwaway 2019.4 project, where writing the mode through the API left `m_AssetPipelineMode: 0`
  behind, because setting the same property inside *this* project changes nothing (the session already
  reports `Version2`, and the editor saves a settings asset only when a value changes). `EditorSettings.asset`
  now carries `m_AssetPipelineMode: 1`, and the same runs say it took: the compile gate
  (`scripts\artifacts\compile-gate-v2.log:30`) and the scene-integrity run both print
  `Using Asset Import Pipeline V2.` with no `Rebuilding Library` line - one pipeline and one database, for
  the editor and for the gates alike.

## 0.1.8-alpha

- **The engine moved to Unity 2018.4 LTS (2018.4.36f1).** The project was pinned to the editor the game
  ships with (2018.1.9f2) and is now on the last release of the 2018 line, moved by the editor's own API
  Updater rather than by hand, with a second hop to 2019.4 LTS to follow. Only two pieces of code had to
  change, and both were decompiler artifacts the older compiler had accepted: a `SetVector` call with
  three folded arguments in `BloomComponent`, and two `ComputeBufferType.DrawIndirect` uses, which is
  `IndirectArguments`. The engine enum `MeshColliderCookingOptions` lost `InflateConvexMesh`, so the
  skin's mesh colliders cook with `CookForFasterSimulation | EnableMeshCleaning | WeldColocatedVertices`.
  Compile gate **`verdict: OK`**, 0 unique errors, and the boot scene loads and draws under the new
  editor.
- **What the move cost, and what it settled.** `Assets/Plugins/ZFBrowser.dll` is now refused by the
  runtime - `Unloading broken assembly` - where 2018.1 loaded it, and that is the embedded browser's own
  assembly; its native `zf_cef.dll` and `ZFProxyWeb.dll` are still in place, so whether the browser
  still comes up is the open question and is being measured on a built player. `MacGruber.Breathing`
  fails one step earlier and with another exception type, still caught, reported once and shown as a
  dialog. On the other side of the ledger, the **white iris on the `Male 1` skin is gone** in a hand run.
- **The hop rewrote two tracked project files.** `ProjectSettings\ProjectVersion.txt` now names
  `2018.4.36f1`, and `Packages\manifest.json` gained the editor's own 2018.4 defaults - `com.unity.ads`,
  `analytics`, `collab-proxy`, `package-manager-ui`, `purchasing`, `textmeshpro`; the editor also
  created `ProjectSettings\VFXManager.asset`, which 2018.1 had no counterpart for. All of it is kept
  exactly as the editor wrote it, and the 31 `com.unity.modules.*` entries did not move.
- **The plugin compiler the game ships cannot run on this project's profile, so it is now built from
  source.** VaM compiles its plugins at runtime with `mcs.dll`, the Mono C# compiler, driven by
  `DynamicCSharp`; the shipped binary was built against Mono 2.0's `mscorlib`, and the shipping game runs it
  as it always has because that is the profile it has. This project runs the .NET 4.x profile, and there the
  compiler aborts on the first
  plugin that declares a default for a nullable value type - `Defaults.A(int? x = null)` in the probe -
  with `System.ArgumentException: System.Nullable`1[System.Int32] is not a supported constant type`, thrown
  by `System.Reflection.Emit.ParameterBuilder.SetConstant` from `Mono.CSharp.Parameter.ApplyAttributes`
  while the method's attributes are emitted. The abort takes the whole compilation with it: the report comes
  back empty and no plugin loads, so every custom scene, preset and plugin in the installation disappears
  without a single error of its own. That is not the hop's doing - it is in this project's log before the
  engine moved, with the `mcs.dll` byte-identical to the installation's. Three prebuilt swaps were measured
  and rejected: `unityjit`'s
  `Mono.CSharp.dll` declares a different assembly name and its `Mono.CSharp` types are `internal` (136 x
  `CS0122`), `lib\mono\4.5\mcs.exe` is refused by the editor because an exe-flavoured assembly has no
  `IMAGE_FILE_DLL` characteristic, and the same file with its PE header relabelled loads but hits the same
  internal API (168 errors). What works is recompiling the compiler from the sources the installation ships:
  `src\mcs` (797 files) is built by the editor's own `mono.exe` + `mcs.exe` into a library, with one patch in
  `Mono\CSharp\Parameter.cs` that makes `SetConstant` swallow `ArgumentException` and `NotSupportedException`
  for values the emit backend cannot take - which is what Mono 2.0 did with the same input, so the attribute
  is dropped rather than the compilation. `scripts\Build-McsCompiler.ps1` does the build and validates the
  result (>= 700 sources, `IMAGE_FILE_DLL`, `AssemblyName.Name == 'mcs'`); `Setup-RebuildProject.ps1` calls
  it, and the settings asset keeps `mcs.dll` in its reference blocklist. Verified two ways: the probe that
  declares the defaulted nullable value type is rejected by the installed compiler and accepted by the
  rebuilt one, and a play run under the new editor compiles plugin scripts again -
  `artifacts\play-2018.4.log` shows `MacGruber.Life` and `MacGruber.Breathing` running out of a
  runtime-compiled assembly (`<c420d7da11d94f6d85d69ae23a5fd09f>`, no file path, which is what
  `DynamicCSharp` produces), and both fail only inside their own `Init`, for the reason they failed before
  the hop.
- **The setup script was reverting the hop, silently.** `Setup-RebuildProject.ps1` wipes the project and lays
  the AssetRipper export back down on every run, and it did that to `Packages` and `ProjectSettings` too - the
  export is a 2018.1 snapshot, so `ProjectVersion.txt` came back as `2018.1.9f2`, `VFXManager.asset` was
  deleted and six tracked settings files lost their edits. Nothing failed loudly, because the gate scripts
  read the editor to run *from* that very file: after any setup run the compile gate and the player build
  banner `Built from '2018.1/release' branch; Version is '2018.1.9f2'`, and the hop looked like it had never
  happened. Both directories are now stashed across the wipe and copied back over the export's copy, exactly
  like `Assets\Editor`; the export's own `ProjectVersion.txt` survives only as the bootstrap for a project
  that has never had one. The runtime-version patch no longer rewrites the profile only when it recognises a
  specific old value (`apiCompatibilityLevel: 0` -> `1`, `2` -> `3`) - that regex was written for the 2018.1
  export and would have thrown on this one, where the value is already `1` - it now asserts the final
  `scriptingRuntimeVersion: 1` / `apiCompatibilityLevel: 3`. Verified: after a full setup run
  `ProjectVersion.txt` reads `2018.4.36f1`, `VFXManager.asset` is present, `git status` is clean, and both
  the compile gate and the play log banner the 2018.4 editor.
- **The white iris was the additive pass's alpha slot.** On one male skin the iris and pupil read white where
  every other one renders: the eye probe measured **32470 px** of the frame around the pupil at
  `(0.573, 0.477, 0.141)`. Eight of the additive families blend `SrcAlpha One`, so their alpha is the weight
  of the colour being added, and the shipped programs write the surface's own alpha there -
  `saturate(texelA * _Color.a + _AlphaAdjust)`, or `saturate(alphaTexelA + _AlphaAdjust)` in the
  separate-alpha ones - and the literal `1.0` only where the blend is `One One`. Ours wrote `1.0` in every
  one of them, so a cornea with `_Color.a = 0` added its full colour over the iris.
  `New-VaMShaders.py` now emits `VAM_ADD_ALPHA_SURFACE` from the pass's own `srcBlend` (**5** or **9**), and
  the same probe measures **0 px** - the cornea paints exactly as much as the original's does, which is
  nothing. Shader gate **7479/7479** programs.
- **A chosen preset loaded nothing, and the path separator was the bug.** In *Person → Appearance Presets →
  Select Existing* picking a preset did nothing: `SyncPresetBrowsePath` loads only when
  `PresetManager.CheckPresetExistance()` answers yes, and a `false` there is silent. The composed name takes
  its store-relative half from `Path.GetDirectoryName`, which returns **Windows** separators whatever it is
  handed, so a `\` landed inside a path the file manager spells in `/`, and `FileManager.FileExists` - a
  dictionary hit with no separator normalisation - missed. A probe driving 7 stores: before, every store
  whose presets sit in a subfolder answered `false` while the same path in `/` answered `true`; after
  `.Replace('\\', '/')` on the `Path.GetDirectoryName` results, all 7 answer `true`.

## 0.1.7-alpha

Everything since the first public alpha: the boot scene loads, and the character, its skin, hair, clothing and
eyes draw from this project's own code.

- **Clothing renders with its own colour, and transparent where the original is transparent.** The cause was
  the generator, not the data: it decoded `rtBlend*.colMask` with the bit order `(1, R) (2, G) (4, B) (8, A)`
  while Unity's `ColorWriteMask` is `Alpha = 1, Blue = 2, Green = 4, Red = 8`, so the transparent families'
  `14` became `ColorMask GBA` and the pass never wrote red. 44 pass states moved from `GBA` to `RGB`, gate
  3023/3023 programs.
- **Hair, lashes and the eye.** `Custom/Hair/*ComputeBuff` is transcribed (+14 shaders, +55 passes); pass 0 of
  `Main*` **writes black and keeps the alpha**, the lashes are `saturate(_AlphaTex.a + _AlphaAdjust)`, and the
  clipping and the gate come from the shipped programs: 4414/4414. The plain twins needed a second path -
  `DAZHairMesh` draws through `Graphics.DrawMesh` and never reaches the `ComputeBuff` lookup - and their
  contracts come from `z_sha` (317 names, none in both): 88 shaders, 256 passes, 6805/6805.
- **Materials draw with this project's shaders.** `VamShaderProvider.UseProjectShader` moved the character's
  materials off the bundle copies - those on a bundle copy fell from 84 to 76 - a shader looked up by name no
  longer answers with an AssetRipper placeholder where there is no transcription, and `VamSkin()` no longer
  orthogonalises the tangent, which the shipped programs never rotate.
- **The base pass, read back out of the shipped bytecode.** `DIRECTIONAL-MARMO_LINEAR` disassembled with `fxc`
  and walked against our compile: same arithmetic, same order, one divergence equal only at exposure 1.000, one
  dead instruction. Left outside: the specular IBL cube's import colour space, the `_SpecInt`/`_Shininess`/
  `_Fresnel` the game pushes against what the material holds, and the bloom threshold.
- **A standalone `VAMOpen.exe` builds in batch mode** (`RebuildPlayer.Build`), its verdict read from the
  `----- RebuildPlayer` markers rather than the exit code: ten scenes, 312 FPS. The missing packages were a
  missing key file - `Keys/1.21/key.json` - and with it the same build logs `Scanned 18 packages` instead of 9.
- **The editor and the installation had never rendered under the same settings.** `RebuildGate.PresetDump`
  prints the resolved quality level and the working directory's `prefs.json` graphics keys, and
  `scripts\New-RuntimeDataLinks.ps1` matched **5 of 9**: the installation runs the **High** preset of
  `UserPreferences.QualityLevels` to the digit while the editor ran **Max** with `msaaLevel` raised to 8. With
  them equal the two pictures measure 0.7144 against 0.7106 of mean lit-skin luminance (**+0.54 %**), 0.61 % of
  the frame differing by more than 24/255.
- **Self-shadowing: VaM's own point-light shadow filter, decoded from the released bytecode.**
  `VAM_LIGHT_ATTENUATION` now comes from the release build's `MARMO_LINEAR + POINT + SHADOWS_CUBE` - cube uv
  projection, the 25-tap Poisson disk, its taps averaged rather than lerped toward 1. At `shadowStrength` 0.10
  it darkens the lit body by **+8.26/255** of mean luminance, against the built-in path's +0.98.
- **Scene previews in the built player.** The lost previews were one wrong file: Mono 2.0's `System.Drawing.dll`
  (448 512 B) staged into `Managed\`, where its marshaller cannot start, so 469 previews threw; the `unityjit`
  build (483 840 B) is staged instead and the gate decodes a preview through it. In a fresh player 157 scenes
  each resolve to a decoded 512x512 `.jpg` (`uFileBrowser.ThumbnailDiagnostics`, dormant without
  `-vamopen-diag`).
- **A plugin the installation cannot run flooded the log**: 17 194 `NullReferenceException` lines, 8 595 each
  from `MacGruber.Breathing.Update` and `DriverBreathing.Update`. `MVRPluginManager` now destroys the
  half-built component, switches the plugin off and shows its first failure as a dialog, so that boot logs 12
  exception lines, none from `Update`.
- **The harness was attacking the workspace.** Setup deleted the gate apparatus with `VaM_Rebuild\`, recursed
  through the `StreamingAssets` junction into 16.47 GB of bundles (PowerShell 5.1 deletes files *behind* a
  junction) and made a stderr warning fatal; `RebuildPlayer` deleted its output through the same link, and setup
  wiped the `AddonPackages`/`Custom` junctions so the editor scanned 0 packages in silence, and a gate ran
  green on untracked generated shaders. Junctions unlink with `RemoveDirectory` now.
- **An in-game screen resolution setting.** `InitScreenResolutionUI` clones the `Physics Update Cap Popup` row
  at runtime, because the panels live in bundles and not here: `Screen.resolutions` above a 640x480 floor,
  within 0.02 of the display's aspect, six entries plus the current mode, hidden in VR. The value is
  `prefs.json`'s `screenResolution`, applied at boot only when the display still reports that mode; the popup
  is `AlertUI` with Keep/Revert and a ten-second countdown that replays the previous mode through
  `ApplyScreenResolution(..., prompt: false)`.
- **The project had been compiling a copy of the sources, and no gate could see it.** `src\` is where this
  project is edited (129 969 B `UserPreferences.cs`), `VaM_Rebuild\Assets\Scripts\` is what Unity compiles
  (118 911 B, **0** hits for `screenResolution`), and the compile gate answered `verdict: OK` about the unedited
  tree. `scripts\Sync-Sources.ps1` now mirrors the `*.cs` files, byte-verified, before every gate, and reports a
  stale assembly instead of trusting it.
- **The panel names the build**: it reads `VaMOpen 0.1.7-alpha (content version: 1.22.0.13)`, the first half
  from `VaMOpenBuild.Version` and the second from the installation's own `version` file. The compile gate is
  `verdict: OK` with 0 unique errors, and the player `verdict: OK` over 10 scenes.

## 0.1.0-alpha

First public alpha: the code compiles, boots, loads a scene and renders an animated, lit character; skin and
hair read close to the original, the cloth does not.

- **Works**: `Assembly-CSharp` (2809 files) and `Assembly-UnityScript` at **1421 errors down to 0**, parity
  **2753/2753**; the boot log matches the game's `output_log.txt`; the body follows an 84-bone animation; 43 of
  the 135 families from shipped DXBC (**3023/3023 programs**).
- **Does not work yet**: cloth (fixed in 0.1.7-alpha), the shoulder/back gloss-bump seam, the lashes and the
  eye; 92 of the 135 families, so the installation stays a runtime dependency; post-processing stubs; physics,
  UI and other scenes untested.
- **Known issues**: one scene per session crashed the player; 25 of the 68 `.shader` files are placeholders;
  the `Marmoset/Specular IBL*` fallbacks are missing; tessellation is D3D11-only.
- **Requirements**: Windows, Unity **2018.1.9f2**, your own installation.
