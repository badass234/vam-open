# Auditing an engine hop against Unity's upgrade guide

Unity's own upgrade guide per version is the checklist for a hop, but it is written for a whole project
and says nothing about which items *this* project is exposed to. This file records the audit of a hop:
item by item, what the evidence was, and which items no amount of reading can settle.

The migration is staged - `2018.1.9f2` (the engine the game ships with) to `2018.4.36f1` to `2019.4.41f2` to
`2020.3.49f1` - rather than a jump to the newest editor, so there is one guide per hop:
[2018 LTS](https://docs.unity3d.com/2019.4/Documentation/Manual/UpgradeGuide2018LTS.html),
[2019 LTS](https://docs.unity3d.com/2019.4/Documentation/Manual/UpgradeGuide2019LTS.html) and
[2020 LTS](https://docs.unity3d.com/2020.3/Documentation/Manual/UpgradeGuide2020LTS.html).

## How the audit is done

Three kinds of evidence, in increasing order of trust:

1. **The project's own files.** Most items are decided by looking for the API in the sources, in the
   serialised assets, or in the project settings: an item whose API appears nowhere cannot bite, and one
   that does appears with a line number.
2. **The compiled assemblies.** Some removed APIs are reached only from binary plugins, which have no
   source to grep. `dnfile` walks every assembly's `TypeRef` and `MemberRef` tables, so a call to an API
   that no longer exists is found even when the caller is a shipped `.dll`. `tools\audit_plugin_apis.py`
   does this: pointed at `Assets\Plugins` it walks the 98 candidate files there and reports 14 managed
   assemblies (12 plugin DLLs plus this project's own `Assembly-CSharp.dll` and `RTTypeModel.dll`), 22
   native DLLs and 61 files with no CLR metadata at all (58 `.pak`, 3 `.bin`), writing
   `artifacts\plugin-api-audit.txt`.
3. **A live run.** Anything about behaviour - the shape of a physics contact, the phase of a root-motion
   curve, the value a shader writes - is settled only by running the thing. A static audit says where to
   look; it does not close the item.

The API Updater inside the editor runs first and rewrites what it can (obsolete members, renamed
callbacks). The audit is what is left over: things the Updater does not touch, and things it silently
changed the meaning of.

## 2018.1.9f2 → 2018.4.36f1

Verdict key: **closed** - evidence is in the project or the binaries and needs no run; **by hand** - only a
play run or a look at the screen can decide it.

| Guide item | Verdict | Evidence |
|---|---|---|
| `UnityPackageManager` folder renamed to `Packages` | closed | no such folder exists; `Packages\manifest.json` is tracked |
| Improved prefabs (2018.3) | closed | 65 of 65 `.prefab` files carry `m_Modifications:`, i.e. they are in the 2018.3+ format already |
| `PrefabUtility.LoadPrefabContents` / `SaveAsPrefabAsset` / `UnloadPrefabContents` / `AddObjectToAsset`, `ObjectFactory.CreateGameObject` | closed | 0 occurrences in `src\`; the one `ReplacePrefab` match is the project's own `src\Assembly-CSharp\ReplacePrefab.cs`, not `PrefabUtility.ReplacePrefab` |
| USS property changes | closed | 0 `.uss` and 0 `.uxml` files in the project |
| Exceptions now logged from managed threads (2018.3) | closed | behaviour, not an API: it is why the boot log carries 12 `NullReferenceException` lines that 2018.1 swallowed |
| `VFACE` inverted on DirectX (2018.3) | closed | 0 occurrences of `VFACE` across the 160 shaders of `Assets\Shader` and `Assets\VaMShaders` |
| PhysX 3.3.1 → 3.4.2 | closed (settings), **by hand** (behaviour) | `DynamicsManager.asset` present: `m_ContactsGeneration: 1`, `m_ContactPairsMode: 0`, `m_AutoSyncTransforms: 0`, default gravity; `m_PivotOffset` is 0 in every prefab. The engine itself changed what a contact patch is - see *What stays open* |
| `AssetBundle.mainAsset` obsolete | closed | obsolete only; no `get_mainAsset` in any `MemberRef` |
| NavMesh components moved out of `Assets/StandardAssets` (2018.3) | closed | the project uses the NavMesh VaM shipped, not Unity's GitHub drop |
| `TerrainData.splatPrototypes` obsolete (2018.3) | closed (warning-only), **by hand** (behaviour) | the API is obsolete, not removed: `PersistentTerrainData.cs:58/80/100` reads and writes it, and the build answers with four `CS0618` warnings a pass rather than an error. The surrogate is untouched, because it serialises a saved format. The behavioural half - contact generation with terrain, and `TerrainData.thickness` being gone - is in *What stays open* |
| Particle system fixes (2018.3) | closed | the affected setting is the mesh-pivot offset, and `m_PivotOffset` does not appear in any prefab or settings file we own; runtime content lives in the installation's `.var` files |
| C# compiler changed to Roslyn (2018.3) | closed | affects how the *editor* compiles its own scripts; no `csc.rsp` and no `mcs.rsp` exists in the project. The runtime compiler this project ships is a separate story - `rebuild-project.md`, *The plugin compiler* |
| UnityScript and Boo compilers removed (2018.3) | closed | 0 `.js` and 0 `.boo` files; the second assembly still builds - the compile gate produces `VaMUnityScript.dll` |
| Animator root motion playback (2018.3) | closed (code shape), **by hand** (behaviour) | `CubemanCharacter.cs:191/222/228` and `ThirdPersonCharacter.cs:172/200/206` assign `applyRootMotion` **and** override `OnAnimatorMove()` (`:202`, `:183`) - the pattern the guide asks for; `PersistentAnimator.cs` and `RTTypeModel.cs` only serialise the field. What is left is behavioural, see below |
| `UIElements.ContextualMenu` API changes | closed | editor-only API, not referenced from runtime code |
| Legacy networking removed (2018.2) | closed | binary audit: `NetworkView`, `Network`, `MasterServer`, `RPCMode` and the rest appear in **0** of the 14 managed assemblies scanned. `NetworkPlayerSurrogate` / `NetworkViewIDSurrogate` occur only in `RTTypeModel.dll`, and this project's copies of those two classes are empty `[ProtoContract]` shells that touch no Unity networking API |
| PSD transparency option removed (2018.2) | closed | 0 `.psd` files |
| Coroutine behaviour when an object is disabled or destroyed (2018.1) | closed | this is 2018.1's own change, and the project started there |
| `BuildPipeline` callbacks replaced by `BuildReport` | closed | editor-only API; `BuildPlayer` and `BuildAssetBundles` appear in **0** `MemberRef`s of the shipped plugins |
| `Application.CancelQuit` deprecated | closed | 0 `CancelQuit` references |
| `AvatarBuilder.BuildHumanAvatar` deprecated (WSAPlayer) | closed | 0 `BuildHumanAvatar` references; UnityEngine's avatar builder is not used |
| `TouchScreenKeyboard.wasCanceled` / `.done` obsolete | closed | 0 `wasCanceled` references |
| MonoDevelop 5.9.6 removed | closed | not used by the project or the machine |
| Assets inside plugin folders (2018.2) | closed | 0 directories named `*.bundle`, `*.plugin` or `*.folder` |
| GPU instancing and GI on custom shaders (2018.1) | **by hand** | the option only affects shaders written for it; every one of the 135 rebuilt families would have to be looked at on screen |
| Complex handle defaults | closed | editor-only API |
| `unsafe` C# now requires `allowUnsafeCode` | closed | `allowUnsafeCode: 1` in `ProjectSettings.asset`, with 73 `unsafe` occurrences in `src\` |

Nothing in the guide required a code change that the API Updater had not already made; the three repairs
the hop needed (`SetVector` folding, `ComputeBufferType.DrawIndirect`, `MeshColliderCookingOptions`) came
from the compiler and the editor, not from the guide.

## What stays open after the 2018.4 hop

The guide names these as behaviour changes, and no static audit closes them. They are on the manual test
list for the hop, and the manual test is the only gate that can pass them:

- **Physics contacts.** PhysX 3.4.2 generates up to six contacts per pair where 3.3.1 stopped lower, and
  it changed the algorithm that generates contacts with terrain. Two consequences are documented by Unity:
  objects can tunnel through terrain (`TerrainData.thickness` no longer exists to soften it - continuous
  collision detection is the fix if it shows), and negatively scaled meshes invert their normals when
  `scale.x * scale.y * scale.z < 0`. Ragdoll stability and collider response are the things to watch.
- **Root motion.** Where a script sets `applyRootMotion = false`, 2018.2 stopped position and rotation
  curves from being applied at all; 2018.4 applies them. A model whose curves used to be ignored will now
  move. The two characters that script root motion explicitly are the ones to watch.
- **Particle systems.** Non-uniformly scaled billboards and meshes, and spherical wind zones, were fixed -
  which means particles that used to be deformed now are not. Any effect that was tuned around the old
  behaviour will look different.
- **GI with custom shaders.** GPU instancing and lightmapping only reach a shader that opts in; a family
  that is not updated simply loses GI rather than failing. This is a picture, not an error.

## 2018.4.36f1 → 2019.4.41f2

The hop is **made**. The editor's API Updater ran on the first open
(`Unity.exe -batchmode -nographics -quit -accept-apiupdate -projectPath VaM_Rebuild`), the compile gate is
green, the standalone player builds and boots under 2019.4, the timed Play gate reports
`errors and exceptions: 0` with `----- RebuildGate OK -----` on the boot scene under both editors, and the
numbers behind every line below are in [`release-notes.md`](release-notes.md), `0.1.9-alpha`.

The first open answered with **612 `error CS` lines**. Peeling them apart by origin is the audit's real
content, because it shows how little of the guide's list the hop actually touched: **548 of the 612 came
from `Library\PackageCache\com.unity.package-manager-ui@2.0.13`** - the editor's own package manager,
pinned into `manifest.json` by the 2018.4 editor and written against 2018.4's UI Elements, so every one of
its `UnityEngine.Experimental.UIElements` / `UnityEditor.Experimental.UIElements` references became
`CS0234`/`CS0246` (428 + 68 + 44 + 8 lines). Removing that pin removes all 548. The remaining 62 are four
distinct `CS0619` removals on 8 sites of this project's own code, plus one `CS0104` ambiguity - that is the
whole of what the guide's list cost here.

| Guide item | Verdict | Evidence |
|---|---|---|
| Unity UI leaves the built-in editor extensions and becomes a package (2019.2) | **change needed - the package is pinned** | under 2019.4 `Editor\Data\UnityExtensions\Unity\` holds `Tango` and `UnityVR` and **no `GUISystem`**, while `com.unity.ugui` sits in `Editor\Data\Resources\PackageManager\BuiltInPackages\`. `UnityEngine.UI` is referenced by **309** files under `src\`, and before the pin the 36-entry manifest resolved it only *transitively*, through `com.unity.purchasing` - a dependency nothing in this project controls. `manifest.json` now carries `"com.unity.ugui": "1.0.0"` (line 8) beside `com.unity.purchasing` (line 6), and `Packages\packages-lock.json` records it at `depth: 0` where the transitive route had it at `1` |
| Tilemap Editor and Sprite Editor move to packages | closed | `GridBrush`, `GridSelection`, `GridPalette`, `UnityEditor.Tilemaps`, `UnityEditor.U2D.Sprites` - **0** occurrences in `src\`; the one asmdef references none of them |
| UI Elements is no longer `Experimental` | closed | 0 occurrences of `UnityEditor.Experimental.UIElements` in `src\`; the only `UnityEngine.Experimental.UIElements` lines in the whole hop log belong to the package manager above |
| `Addressables`' `AsyncLoad` correction | closed | Addressables is not used: 0 `Addressables` and 0 `AsyncOperationHandle` in `src\` |
| Animation C# jobs (`UnityEngine.Experimental.Animations`) | closed | 0 occurrences; no `AnimationStream`, no `AnimationScriptPlayable`, no `IAnimationJob` |
| LWRP → URP, and the SRP API changes | closed | no `com.unity.render-pipelines.*` in the manifest and no `ScriptableRenderPipeline` / `RenderPipelineManager` / `beginCameraRendering` in `src\`; this project's look comes from its own Marmoset-family shaders, and the only SRP type it names is `RenderPipelineAsset`, serialised by a surrogate |
| `RenderPipelineAsset` changes namespace | closed, **no edit was needed** | measured against the engine rather than read: under 2019.4 `UnityEngine.CoreModule.dll` declares **`UnityEngine.Rendering.RenderPipelineAsset` only** (reflection over the type list; no `UnityEngine.Experimental.Rendering` copy). `PersistentData.cs` already imports `UnityEngine.Rendering` (line 23) as well as `UnityEngine.Experimental.Rendering` (line 20, still a live namespace for other engine types), so `typeof(RenderPipelineAsset)` at line 228 binds to the `UnityEngine.Rendering` one on both sides of the hop |
| The high-level UNet API leaves the engine | closed, **and the prediction was wrong in a useful way** | the components really are gone - `NetworkIdentity`, `NetworkManager`, `NetworkServer`, `NetworkClient`, `NetworkBehaviour`, `NetworkTransform` have **0** occurrences in `src\`, and the HLAPI is the `com.unity.multiplayer-hlapi` package. But `UnityEngine.Networking.Match.NetworkMatch` - the mapping at `PersistentData.cs:224`, with `using UnityEngine.Networking.Match;` at line 21 - **still exists in 2019.4**: the API Updater did not touch the line and the compile gate is green, so no package had to be added and no mapping dropped. The plan expected this entry to be deleted; the measurement says otherwise |
| `ShaderUtil.ClearShaderErrors` → `ClearShaderMessages` | closed | 0 uses of `ShaderUtil.ClearShaderErrors`; the only `ShaderUtil.` in the sources is this repository's own `Battlehub\RTEditor\RuntimeShaderUtil.cs` |
| `UNITY_ADS` is no longer defined | closed | 0 occurrences of `UNITY_ADS` and 0 of `UnityEngine.Advertisements` in `src\` |
| Legacy .NET 3.5 scripting runtime removed | closed | the hop left `ProjectSettings.asset` **byte-identical** - it is not in the hop's diff at all - and it still reads `scriptingRuntimeVersion: 1` (line 534) with `apiCompatibilityLevel: 3` (line 611): .NET 4.x, where this project has sat since the 2018.4 hop |
| `allowUnsafeCode` becomes a per-assembly option | closed | the project-wide `allowUnsafeCode: 1` (`ProjectSettings.asset:532`) is still there and is what the `unsafe` code in the asmdef-less `Assembly-CSharp` compiles under; the single asmdef, `VaMUnityScript.asmdef`, already carries `"allowUnsafeCode": true` |
| The new asset import pipeline (V2) | **decided - the project pins V2** | the engine's key is `m_AssetPipelineMode` (`0` = V1, `1` = V2, the numbers of `UnityEditor.AssetPipelineMode`); it is in the native editor's `EditorSettings` table and in no managed assembly, and the name this row used to carry (`m_AssetPipelineVersion`) is in neither build. 2018.1's `EditorSettings.asset` has no such key, and with the key absent the entry point decides, so this hop's own runs were split - batch on **V1**, the editor window on **V2** with `Rebuilding Library because the asset database could not be found!` - and one `Library` held both databases. The project now writes `m_AssetPipelineMode: 1`, and both paths report `Using Asset Import Pipeline V2.` |
| `UnityAPICompatibilityVersionAttribute` constructor change | closed | 0 occurrences |
| `AvatarBuilder.BuildHumanAvatar` (WSAPlayer) | closed | 0 occurrences |
| `TouchScreenKeyboard.wasCanceled` | closed | 0 occurrences |

**The removals the hop did hit are not guide rows - the compiler named them** (`CS0619`), and each one is
decided:

| Removed API | Sites | What was done |
|---|---|---|
| `MovieTexture` (removed in 2019.1) | `PersistentData.cs` (the registry entry) and `PersistentMovieTexture.WriteTo`/`ReadFrom` | the type mapping leaves `m_objToData` and the surrogate's two bodies stop naming the engine type; the classes stay, because they are the shape of a saved file, not a use of the engine |
| `GUIElement` (removed in 2019.1) | `PersistentData.cs` (the registry entry) | same: `Add(typeof(GUIElement), …)` goes, `PersistentGUIElement` stays |
| `Graphics.DrawProceduralIndirect(MeshTopology, ComputeBuffer, int)` → `DrawProceduralIndirectNow` | 3 calls - `UnityStandardAssets\CinematicEffects\DepthOfField.cs:464`, `…\ImageEffects\DepthOfField.cs:248` and `:302` | Unity marked the old form `UnityUpgradable` and split drawing into a deferred and an immediate form in 2019.1; the decompiled call sites are immediate (`material.SetPass` immediately before them), so they were renamed |
| `UnityEngine.InspectorNameAttribute` appears beside Leap's `InspectorName` | `Leap\Unity\LeapEyeDislocator.cs:14` | `CS0104`: the attribute is qualified as `[Leap.Unity.Attributes.InspectorName("Baseline")]` |

**Two more things a hop has to be checked for, and both were.** `scripts\Build-McsCompiler.ps1` re-run
under the new editor exits 0 and produces an `mcs.dll` **byte-for-byte identical** to the 2018.4 build
(1 967 104 B, SHA-256 `FC5C08BC…`): both halves of the recipe, `MonoBleedingEdge\bin\mono.exe` and
`…\lib\mono\4.5\mcs.exe`, exist in 2019.4, so the runtime compiler this project ships does not depend on
the editor that built it. And the plugins compiled against 2018.1 were watched through the editor's own
log, because a hop breaks those silently: during the *broken* first open three assemblies were reported
unloadable - `ZFBrowser.dll`, `SteamVR.dll` and `RTTypeModel.dll` - while with the hop finished and the gate
green only **`ZFBrowser.dll`** is, exactly as under 2018.4. `SteamVR.dll` and `RTTypeModel.dll` reference
`Assembly-CSharp`, so they were unloadable only while the project did not compile; what survives the hop is
what was already there before it.

## What stays open after the 2019.4 hop

As with the hop before it, what the guide has left is behaviour, and behaviour is settled by a run:

- **`ZFBrowser.dll` reports as a broken assembly under both 2018.4 and 2019.4.** The hop neither caused nor
  changed it (under 2018.1 the same message named `RTTypeModel.dll` instead), and it is in the same class
  of noise as the `RTTypeModel` note: the assembly is loadable for compilation - `Assembly-CSharp`
  references its types and compiles - while the editor's runtime domain refuses it. What it could break is
  the embedded-browser UI, so it belongs to the **UI milestone**, not to the hop.
- **PhysX on 2019.4 is not the PhysX of the 2018.4 hop.** The engine moves between LTS releases
  independently of the API surface, so the physics items above - contact patches, terrain contacts,
  negatively scaled meshes - are re-opened by this hop and stay on the manual test list, root motion
  included.
- **The renderer is a newer one, and this project's look is a reconstruction.** The frame comparison that
  measured `+0.986` against the original was taken under 2018.4; whether the reconstructed shaders and
  their keyword sets survived the newer built-in shader library is something `tools\compare_view.py` has to
  be re-run to answer.
- **The shipped AssetBundles and the shader library inside them (`z_sha`) were built by 2018.1.** The
  editor-side runs load them (the scene, its materials and the bundle-resolved families all work in the
  gate runs), but reading them from a **built player** is the check that settles it, and it is the next
  manual step (`artifacts\player\`).

## 2019.4.41f2 → 2020.3.49f1

The hop is **made**. The editor's API Updater ran on the first open (the same
`Unity.exe -batchmode -nographics -quit -accept-apiupdate -projectPath VaM_Rebuild`), the compile gate and
the timed Play gate are green on the boot scene, the `SoftEros777.Lady_Clown` scene - the one that aborts
this hop's plugin compile - loads with all ten of its atoms and then renders, and the standalone player
**builds and boots under 2020.3**: `Initialize engine version: 2020.3.49f1 (18249dd5551b)`,
`Scanned 79 packages in 315.0 ms`, `Benchmark complete. … Avg. FPS: 300.21`. The numbers behind every line
below are in [`release-notes.md`](release-notes.md), `0.1.10-alpha`.

The editor's own report answered this hop **34 `error CS` lines, then 1, then 0** - against 612 in the hop
before it - and the 34 are two families and no more: **six `CS0121` ambiguities** in one hair-rendering
file, and the `XRDevice` removals. The single line of the second report is one removed editor property in
the gate script. That is the whole of what the guide's list cost here.

| Guide item | Verdict | Evidence |
|---|---|---|
| All Mesh vertices are transformed before the lightmap UVs are generated (2020.1) | closed | the change reaches only UVs the importer *generates*, and none does: `generateSecondaryUV: 1` has **0** occurrences across the project's `.meta` files, so no mesh import in this project is exposed to it |
| All AssetBundle hashes differ, so every bundle is rebuilt (2020.1) | closed | the project builds no bundles of its own - 0 `BuildAssetBundles` and 0 `BuildPipeline` anywhere under `src\` - and the bundles it loads are the ones the game shipped, built by 2018.1, so no hash of theirs is recomputed by a newer engine |
| The multiplayer HLAPI package is no longer installed automatically (2020.1) | closed, **no package added** | no `com.unity.multiplayer-hlapi` in the manifest, and `NetworkIdentity`, `NetworkManager`, `NetworkServer`, `NetworkClient`, `NetworkBehaviour` still have **0** occurrences in `src\`; the one UNet type this project names, `UnityEngine.Networking.Match.NetworkMatch` (`PersistentData.cs:224`), is engine-side and survived 2019.4 too |
| Improved LOD baking with the progressive lightmapper (2020.1) | closed | nothing here bakes: the ten scenes carry the export's `LightingData.asset` container (one per scene, ~12 kB) and their lightmap textures come from the game's bundles, so there is no baked data to clear and regenerate |
| Adaptive Performance 1.0 → 2.0 | closed | 0 occurrences of `AdaptivePerformance` in `src\` and no `com.unity.adaptiveperformance` in the manifest |
| Xcode project generation | closed | macOS only; the player here is `StandaloneWindows64` |
| Particle System Force Field now simulates against a 30 fps reference | closed | no Force Field is used: `ParticleSystemForceField` has 0 occurrences in `src\` and 0 in the ten `.unity` scenes |
| `UnityEngine.UI.Graphic` no longer declares `RequireComponent(typeof(CanvasRenderer))` | **change needed - four classes** | every user-written `Graphic` subclass in this project declares it now: `NonDrawingGraphic.cs`, `ShineEffect.cs`, `TextPic.cs`, `UIPrimitiveBase.cs` each carry `[RequireComponent(typeof(CanvasRenderer))]` next to their `[AddComponentMenu]`, which is exactly the fix the guide's sample gives |
| Code Optimization changes how Code Coverage works | closed | the Code Coverage package is not used: no `com.unity.testtools.codecoverage` in the manifest and 0 occurrences of `codecoverage` in `src\` (the lock file did gain `com.unity.test-framework` and `com.unity.ext.nunit` on its own) |
| XR Plug-in Management replaces the legacy integration, "Single pass" is gone, `renderScale` → `eyeTextureResolutionScale` | **change needed - the `XRDevice` sites, listed below** | the project installs no XR provider - its Oculus and OpenVR support are the game's own plugin DLLs under `Assets\Plugins\`, not a package - and the desktop player runs flat, so the first two parts are notes for the VR question rather than hop tasks. The third was already done: the 7 `XRSettings.eyeTextureResolutionScale` uses (`OVRManager.cs:869-879`, `UserPreferences.cs:2916-2918`) are the migrated form, and the 39 remaining `renderScale` occurrences in `src\` are local fields (`maxRenderScale`, `minRenderScale`, `_renderScale`), not the removed API. What did need the hop's attention is `XRDevice` itself |

**The removals the hop hit are decided like this** - five of them are not guide rows at all, they are what
the compiler named:

| Removed / changed API | Sites | What was done |
|---|---|---|
| `XRDevice.isPresent` (obsolete, `CS0619`) → `XRSettings.isDeviceActive` | 5 - `OVRDebugHeadController.cs:63`, `SuperController.cs:18666` and `:19584` (the `"XR device present is "` line), `UnityStandardAssets\WaterVR\Water.cs:235`, `UserPreferences.cs:3910` | the Updater rewrote all five; `XRSettings.isDeviceActive` is the surviving spelling of the same question |
| `XRDevice.model` (removed, `CS0117`) | `SuperController.cs:19586` | rewritten as `InputDevices.GetDeviceAtXRNode(XRNode.Head).name` |
| `XRDevice.userPresence` and `UnityEngine.XR.UserPresenceState` (removed) | `Leap\Unity\XRSupportUtil.cs` | rewritten to the input-device feature: `InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.Head); bool supported = device.isValid && device.TryGetFeatureValue(CommonUsages.userPresence, out present);`, and the warning it prints is now keyed on `!supported` |
| `Material.SetBuffer(string, GraphicsBuffer)` **added** in 2020.1 | 6 sites - `GPUTools\Hair\Scripts\Runtime\Render\HairRender.cs` lines 57, 66, 74, 121, 130, 138 | an addition rather than a removal, and it is what produced the six `CS0121`: a bare `null` now matches both the `ComputeBuffer` and the `GraphicsBuffer` overload, so each site is cast - `(ComputeBuffer)null` - where the decompiled call passed `null` |
| `UnityEditor.CodeEditor.CurrentEditorPath` (removed in 2020.3, `CS0117`) | `RebuildGate.cs:385` | this is hop 2's gate script and not the game: the property is gone, and `CurrentEditorInstallation` carries the same `EditorPrefs "kScriptsDefaultApp"` value the check wanted |

**Four more things the hop settled that no guide item covers.**

- **The plugin compiler crashed on this hop, and it is the hop's headline.** `McsDriver.cs` compiles a
  plugin by emitting a `ModuleBuilder` and then either keeping it in memory or writing it out; the
  out-of-memory path (`Save()` → `ves_icall_ModuleBuilder_build_metadata` → `mono_image_create_token`)
  walks the module's **overrides table**, and Mono's token creator has no case for an entry whose owner is
  a `MethodOnTypeBuilderInst` - a method of a generic type that is itself still being built. There is no
  fallback: the branch ends in `g_error("requested token for %s")`, which is a non-continuable
  `RaiseException`, so the process dies rather than reporting a compile failure. It takes exactly one
  plugin to trigger it: `everlaster.TittyMagic`, whose `EnumerableExtensions+<ForEach>c__Iterator0<T>`
  implements `IEnumerator<T>.get_Current` and `IEnumerable<T>.GetEnumerator`, contributes **2** such
  entries into a run that resolves **32**. The fix is in this project's own copy of the compiler
  (`src\Assembly-CSharp\DynamicCSharp\Compiler\McsDriver.cs`, +359 lines): before the module is saved,
  `RepairMethodOverrideDeclarations` resolves each `MethodOnTypeBuilderInst` entry onto the real
  `MethodInfo` of the generic type it belongs to (matching the base by `MetadataToken` when the entry
  carries one, else by name and parameter count), so the table holds `MethodBuilder` and `MethodInfo`
  entries only. The repair is skipped when the module stays in memory, which is the path that never
  needed it. Measured on the scene that used to abort the run: `resolved 32 method override
  declaration(s)`, no crash report, no `requested token`, the scene with 10 declared and 10 present atoms,
  and a live render after the compile - `Benchmark complete. … Avg. FPS: 186.37`.
- **And the compiler this project ships is still the same file under a third editor.** `scripts\Build-McsCompiler.ps1`
  re-run under 2020.3 exits 0 and produces an `mcs.dll` **byte-for-byte identical** to the 2018.4 and
  2019.4 builds - 1 967 104 B, SHA-256 `FC5C08BC…` - so both halves of the recipe
  (`MonoBleedingEdge\bin\mono.exe` and `…\lib\mono\4.5\mcs.exe`) exist in every editor this project has
  lived on and the compiler does not depend on which one built it.
- **The manifest and the lock file moved, and one of the moves costs a log line.** 2020.3 rewrote six
  pins - `com.unity.ads` 2.0.8 → 4.4.2, `com.unity.analytics` 3.2.3 → 3.6.12, `com.unity.collab-proxy`
  1.2.15 → 2.0.4, `com.unity.purchasing` 2.2.1 → 4.8.0, `com.unity.textmeshpro` 1.4.1 → 3.0.6,
  `com.unity.timeline` 1.2.18 → 1.4.8 - and **added `com.unity.ide.visualstudio` 2.0.18**, which the
  editor wants for script editing and which is the one entry here that nothing in this project asks for;
  the lock file gained `com.unity.ext.nunit`, `com.unity.nuget.newtonsoft-json`, `com.unity.services.core`,
  `com.unity.test-framework`, `com.unity.modules.androidjni` and `com.unity.modules.uielementsnative`. The
  Purchasing bump is the visible one in the gate: the timed Play gate's error list goes from **5 lines to
  6** across this hop, and the sixth is
  `UnityEditor.Purchasing.ProductCatalogEditor`'s type initializer failing to load
  `UnityEngine.UnityWebRequestModule` inside a `-batchmode` run - an editor-side package this project does
  not call, arriving earlier than the module it wants. The game itself never touches it.
- **Two scripts and one settings file had to follow.** 2020.2 ships no Boo/UnityScript under
  `MonoBleedingEdge\lib\mono\unityscript\`, while the game's `CharacterMotor.cs` (the one file left in
  `Assembly-UnityScript`) references `Boo.Lang`: `scripts\Setup-RebuildProject.ps1` now tests for the
  editor's copy and, when it is gone, stages the game's own `Boo.Lang.dll` (2.0.9.5,
  `PublicKeyToken=32c39770e9a21a67`) as a plugin instead of excluding the name - and it has to branch,
  because under 2019.4 the same run must *not* stage it.
  `scripts\New-PlayerRuntimeLinks.ps1` had to stop linking `StreamingAssets` wholesale: the bumped
  `com.unity.purchasing` makes Unity Services write `UnityServicesProjectConfiguration.json` into it on
  every editor start, which fails inside a junction, so the junction is replaced by a real directory and
  that one file is the only name the filter lets through. The editor also created
  `ProjectSettings\VersionControlSettings.asset` (visible meta files) where the export had none, and added
  `m_DashboardUrl` and `m_PackageRequiringCoreStatsPresent: 0` to `UnityConnectSettings.asset`; the asset
  import pipeline stayed on **V2**, which is what hop 2 pinned, and both the batch gates and the editor
  window report `Using Asset Import Pipeline V2.`. The plugin compiler's reference list grew by one name -
  `UnityEngine.InputLegacyModule.dll`, six entries to seven in `DynamicCSharp.cs` - because a plugin naming
  `Input` needs the module the engine split it into, and one name Mono cannot resolve fails that whole
  compilation. The hop also reimported every shader once
  (`Shader import version has changed; will reimport all shaders … 0.046365 seconds`), which is the built-in
  library's own format change and not this project's shaders.

## What stays open after the 2020.3 hop

The guide's leftover items are, again, behaviour, and behaviour is settled by a run:

- **`ZFBrowser.dll` is broken in a new way, and the new way names a namespace this hop moved.**
  Under 2018.4 and 2019.4 the assembly was refused with a missing type; under 2020.3 the message is
  `Could not load signature of ZenFulcrum.EmbeddedBrowser.VR.OpenVRInput:GetNodeName due to: Could not
  resolve type with token 01000022 (…)` - a signature over `UnityEngine.XR.XRNodeState`, which lives in
  `UnityEngine.VRModule` on one side of the hop and does not arrive with the assembly the other side. The
  embedded browser is a **UI-milestone** item either way, and the desktop player runs without it.
- **PhysX is a third PhysX.** Each LTS hop replaces the engine's physics, independently of the API surface,
  so the items hop 1 opened - contact patches, terrain contacts, negatively scaled meshes - and root
  motion, are re-opened by this hop as well and stay on the manual test list.
- **The renderer is newer again, and the look is a reconstruction.** The `+0.986` frame comparison was
  measured under 2018.4; `tools\compare_view.py` has to be re-run against 2020.3 to say whether the
  reconstructed shaders and their keyword sets survived this built-in shader library too.
- **The shipped AssetBundles and the `z_sha` shader library were built by 2018.1.** They load: the boot
  scene, the Lady Clown scene and the player's own start-up all use bundle-resolved materials in the runs
  above. What the hop could still move is inside a shader program the newest engine compiles differently,
  and only the frame comparison answers that.
- **And the hop is verified by hand, not by these gates.** The three gates and the player run above say the
  process comes up; whether what is on screen is the game is the user's check, on the boot scene and on a
  loaded character.


