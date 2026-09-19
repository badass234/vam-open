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



## 2020.3.49f1 → 2021.3 LTS

This hop is audited **before** it runs, because the 2020 LTS → 2021 LTS guide is the longest of the three so far and
more than half of what it lists is behaviour rather than a compile error. The source is
[`UpgradeGuide2021LTS.html`](https://docs.unity3d.com/2021.3/Documentation/Manual/UpgradeGuide2021LTS.html) - **21**
items, one row each below, in guide order. The `/2021.3/` copy of `UpgradeGuide2020LTS.html` is *not* a source for
this hop: it is the 2020 page again - its title is "Upgrading to Unity 2020 LTS" and it names neither the
Device Simulator nor the Windows XR plugin that 2021.2 removed, so citing it would cite the wrong hop. Where the guide
statement alone was too thin to test against source - the RenderTexture depth row and the BuildReport row - the 2021.1,
2021.2 and 2021.3 release notes were consulted, and those rows say so. Two of those rows are about assets rather
than code (Environment Lighting, and the subtractive mixed-light change), and one is about the editor's own module
set rather than the project at all.

Every "what this project meets" cell is a count over `src\` at commit `a2243b9`, or - where the item is a settings
or an asset matter - over the tracked `VaM_Rebuild\ProjectSettings\`, the **10** `.unity` scenes and the **75**
`.unity`+`.prefab` assets under `VaM_Rebuild\Assets\`. The ignored mirror `VaM_Rebuild\Assets\Scripts\` is
excluded everywhere, counts are lines the way `git grep` reports them, and the pattern is given wherever a count
is **0**.

| Guide item | Verdict | Evidence |
|---|---|---|
| Device Simulator - `UnityEngine.Device.Screen`/`Application`/`SystemInfo` (2021.2) | closed | the namespace is opt-in and nothing opts in: `UnityEngine\.Device` is **0** in `src\`, while `Screen.` is **71** lines in **28** files that still bind `UnityEngine.Screen`. The simulator changes behaviour only for a script that aliases a type to it (`using Screen = UnityEngine.Device.Screen;`), and none does |
| Environment Lighting - the Editor now bakes the default skybox and an ambient probe automatically | closed, **does not apply** | the trigger is a scene with no Lighting Data Asset and a non-black environment colour. All **10** scenes carry the export's baked lighting - `m_LightingDataAsset` **10**, `m_LightingDataAsset: {fileID: 0}` **0** - with `m_AmbientSkyColor: {r: 0, g: 0, b: 0, a: 1}` **10**, `m_AmbientMode: 3` **10**, `m_SkyboxMaterial: {fileID: 0}` **10**, `m_AmbientIntensity` **10**, `m_UseRadianceAmbientProbe` **10** and `m_GIWorkflowMode` **10**. There is no un-baked environment here for a new rule to fill |
| Enable Code Coverage preference | closed | the package is not installed - no `com.unity.testtools.codecoverage` among the manifest's **38** dependencies - and `\bCoverage\b\|codecoverage` is **0** in `src\`; no gate passes `-enableCodeCoverage` either |
| Particle System Force Fields now simulate against a 30 fps reference | closed | no Force Field is used *and* none is enabled: across the **75** `.unity`+`.prefab` assets, `!u!198` ParticleSystem **39** blocks carry `ForceModule:` **39** and `ExternalForcesModule:` **39**, and every one of those **78** module blocks reads `enabled: 0` (the regex `enabled: 1` matches in either module **0** times). `forcefield` is **0** in `src\` |
| Particle System Start Delay and Rate over Distance emission | closed | there is no distance emission for a start delay to shift: **39** `startDelay:` blocks and **39** `rateOverDistance:` blocks, every one of the 78 reading `serializedVersion: 2` / `minMaxState: 0` / `scalar: 0` / `minScalar: 0`. The **17** `rateOverDistance` lines in `src\` are serialization code rather than curves, and only two files hold them: `Battlehub\RTSaveLoad\PersistentObjects\PersistentEmissionModule.cs` and `RTTypeModel.cs` |
| BuildReport - `PackedAssets.file` is obsolete, use `shortPath` (from the 2021.1 release notes) | closed | `PackedAssets` **0** and `BuildReport` **0** in `src\`; this project builds nothing through `BuildPipeline`. When the Updater does meet the field, 2021.x has it obsolete with no direct replacement, so the audit leaves it to the compile |
| Terrain APIs out of experimental (WIP) | closed | `TerrainPaintTool`, `Experimental\.TerrainAPI` and `Experimental\.Terrain` are all **0** in `src\`; the bare word `Terrain` is **5** lines, and no scene or prefab carries a Terrain component (`!u!218` **0** across the 75 assets) |
| `Texture2D.Resize` renamed to `Reinitialize` (2021.1) | **change needed - three call sites** | `Reinitialize` is **0** in `src\`, and `Resize` is called on a `Texture2D` at exactly **3** sites: `TextureScale.cs:67` (`tex.Resize(newWidth, newHeight)`), `mset\CubeBuffer.cs:815` (`tex.Resize(faceSize, 6 * faceSize)`, inside `toColTexture(ref Texture2D tex, …)`) and `Battlehub\RTSaveLoad\PersistentObjects\PersistentTexture2D.cs:22` (`texture2D.Resize(width, height)`). All three take the two-argument form, so all three are the obsolete overload. Checked against the editors themselves: 2020.3.49f1's `UnityEngine.CoreModule` declares three `Texture2D.Resize` overloads and no `Reinitialize`, and marks none of them `[Obsolete]`, while 2022.3.76f1 declares `Reinitialize` three times and marks every `Resize` overload `[Obsolete(… "deprecated because it actually reinitializes the texture. Use Texture2D.Reinitialize(…) instead (UnityUpgradable) -> Reinitialize(…)")]`. The Updater can do the rename; the semantics are the part to watch, because `Resize` never resized |
| Android changes - Gradle project assets are no longer copied, and files under the `GENERATED BY UNITY` marker are no longer ignored | closed | there is no Android build here: the only subdirectory of `VaM_Rebuild\Assets\Plugins\` is `x86_64`, so there is no `Android\res` or `Android\assets` to stop copying, and `VaM_Rebuild\Assets\` holds **0** `.gradle`, **0** `.pro` and **0** `.aar` files. `Gradle\|gradle` is **0** in `src\` as well. `RuntimePlatform` is **20** lines in `src\` and the only non-desktop case is `RuntimePlatform.WebGLPlayer` (`AssetBundles\Utility.cs:22`), in a switch that is still compiled for a build that is never made |
| UI Toolkit - the default `Image.scaleMode` changed from `ScaleAndCrop` to `ScaleToFit` | closed | no UI Toolkit document exists: `.uxml` and `.uss` are **0** under `VaM_Rebuild\Assets\` and **0** in `src\`. The **14** `scaleMode`/`ScaleMode` lines in `src\` belong to other APIs - `CanvasScaler.ScaleMode`, the `ScaleMode` of `GUI.DrawTexture`, and `RTTypeModel`'s serialization of the same enum |
| Mono upgrade behaviour changes - `Directory.GetFiles` unsorted, `Object.GetHashCode` different, new exception messages | **note, no edit expected** | `Directory.GetFiles` is called **15** times in **9** files and no call site sorts: `FileManager.cs:422,430,1712,1954`, `PackageBuilder.cs:2302,2627`, `SuperController.cs:7107,7191`, `VarPackage.cs:426,849`, `SystemDirectoryEntry.cs:39,72`, `MeshVR\PresetManager.cs:318`, `Battlehub\RTSaveLoad\FileSystemStorage.cs:159` and `src\mcs\Mono\CSharp\CommandLineParser.cs:185`. Each result is consumed order-independently - `FileManager.cs:422`/`:430` and `PackageBuilder.cs:2302` insert into a `HashSet`, the rest compare `Length` or feed a filter - so the newly-unsorted order has nothing to break. `GetHashCode` is **196** lines; the overrides are value hashers on runtime-only structs (`Leap\LeapQuaternion.cs:89`, `Leap\Vector.cs:214`, `Battlehub\RTSaveLoad\ID.cs:22`, `Leap\Unity\Pose.cs:107`) that are never persisted, so the guide's "do not rely on it between processes" warning has no site. The exception-message wording is **unverified** - only a run that throws would settle it |
| Adaptive Performance 3.0 | closed | `AdaptivePerformance` is **0** in `src\` and there is no `com.unity.adaptiveperformance` in the manifest, so there is no 2.x call site to move |
| RenderTexture `DepthStencilFormat` - a request for depth 32 now selects D32_S8 and doubles the depth buffer (2021.1 release notes) | closed | no site asks for 32. All **29** `new RenderTexture(…)` calls in `src\` were read for their depth argument: **8** pass 16, **8** pass 0, **4** pass 24 (`Floor_ReflectionScriptCamera.cs:96`, `MirrorReflection.cs:632`, `:641`, `OVRCubemapCapture.cs:79`), **2** pass a computed local (`OVRSandwichComposition.cs:281`, `:287`, where `num` is 24 unless `mainCamera.targetTexture` supplies its own depth) and **1** passes 8 (`Water_DistortionAndBloom.cs:79`). `depthStencilFormat` is **0** in `src\`. The doubled-memory note still applies to a user's own 32-bit render texture, never to this code |
| Graphics formats `DepthAuto`, `ShadowAuto` and `VideoAuto` are deprecated | closed | `GraphicsFormat` **0**, `ShadowSamplingMode` **0** and `DefaultFormat` **0** in `src\`; the one depth render texture here names the surviving enum - `RenderTextureFormat.Depth` at `Water_DistortionAndBloom.cs:79` - and the **8** `RenderTextureFormat` lines are all pre-`GraphicsFormat` spellings that 2021.3 still ships |
| WebGL: Emscripten 2.0.19, every native plugin must be recompiled | closed | the player is Windows only - `ProjectSettings.asset` has no `WebGL` key - and the **35** `.dll` files under `Assets\Plugins\` are **14** managed and **21** native, every native one a Windows PE binary - **19** x86-64 and **2** 32-bit x86 (`bass.dll`, `OVRPlugin.dll`, `openvr_api.dll`, `LeapC.dll`, `d3dcompiler_47.dll`, …). None is a WebGL object file, and the **58** `.pak` and **3** `.bin` files beside them are data, not native plugins |
| Progressive GPU Lightmapper drops CPU OpenCL devices | inert | nothing here bakes: the **10** scenes carry the export's `LightingData.asset` and their lightmaps come out of the game's bundles, so there is no bake for the fallback message to change. The **9** `!u!108` light components in the 75 assets are the only lights the project owns |
| `OnPostprocessAllAssets` behaviour changes - `didDomainReload`, and the callback moved out of the import loop | closed | `AssetPostprocessor`, `OnPostprocessAllAssets` and `didDomainReload` are all **0** in `src\`; on the editor side `[InitializeOnLoad]` appears at `RebuildGate.cs:29` and `VaMInspector.cs:981` and neither class is an `AssetPostprocessor`, so no callback here can run inside the import loop |
| Mixed point and spot lights with no shadows now bake direct light in subtractive mode | **change needed - by hand** | this is the hop's one asset-level change, and every scene uses it: `m_MixedBakeMode: 2` (Subtractive) is in all **10** scenes, `m_Lightmapping: 4` **9** times and `!u!108` **9**. The guide's own remedy, when a scene's lightmap goes missing, is to make the affected Mixed lights Realtime or to move the scene to Baked Indirect or Shadowmask - a lighting decision rather than a code edit, so the row is settled by comparing each scene after the first open, and the boot scene first |
| Shader keyword system improvements | closed for the declarations, **by hand** for the look | the tracked shader library is one file - `git ls-files` finds **1** `.cginc` and **0** `.shader` - and the real one is untracked under `VaM_Rebuild\Assets\`: **159** `.shader`, **1** `.cginc`, **0** `.compute`. There, `multi_compile` sits on **257** lines in **89** files, `shader_feature` is **0**, and no declaration carries a `_local` or `_global` suffix (the 9 case-insensitive `_global` matches are comments naming a `MASK_ONLY_GLOBALS` define). `multi_compile` is the global form on both sides of the hop, so the guide's locality change has no declaration to act on. On the C# side keywords are touched **147** times, **68** of them through `Shader.EnableKeyword`/`Shader.DisableKeyword` in **10** files, and none names a local or global form either. What is left is whether the look survives, and only the frame comparison answers that |
| Unity supports .NET Standard 2.1 APIs | closed, **no edit was needed** | this project compiles against .NET 4.x: `ProjectSettings.asset:534` `scriptingRuntimeVersion: 1` and `:611` `apiCompatibilityLevel: 3`, with `apiCompatibilityLevelPerPlatform: {}` at `:535`. 2021.3 keeps that enumeration and only adds names to it, so the API surface handed to the compiler does not move. The names the guide warns about are absent - `System.Memory`, `System.Span`, `System.Range` and `Span<` are **0** in `src\` - and neither `ApiCompatibilityLevelAttribute` nor `UnityAPICompatibilityVersionAttribute` appears, so no explicit compatibility request can go stale. The **14** managed DLLs under `Assets\Plugins\` are precompiled against 4.x and none is `System.Memory.dll`. What the row does decide is the surface those plugins were built for: a plugin that wants a 2.1 type needs the project moved to .NET Standard, and that is a later hop's call |
| Windows XR plugin removed (2021.2) | closed | there is no provider package for an update process to remove - the manifest has `com.unity.modules.xr` and no `com.unity.xr.*` entry - and the VR support this project carries is the game's own plugin DLLs (`OVRPlugin.dll`, `openvr_api.dll`, `SteamVR.dll`, …). `ProjectSettings\XRSettings.asset` holds `["VR Device Disabled","VR Device User Alert"]`, not a provider list |

**The removals and renames the hop has to decide are handed to the compiler like this.** The guide names one of
them - `Texture2D.Resize` - and that row is in the table above. The rest were searched for by name; a hit only
counts when the site is this project's own use of the engine type, never when it is a `Battlehub` surrogate class,
a type token in a serializer, or a comment.

| Removed / changed API | Sites | What this hop does |
|---|---|---|
| `UnityEngine.WWW` | **0** for the qualified name. The unqualified `\bWWW\b` is **12** lines in **6** files: `AssetBundles\AssetBundleManager.cs:33,256,260,275,383,385,435`, `ImageControl.cs:886`, `OVRLipSyncTestAudio.cs:19`, `OldMoatGames\AnimatedGifPlayer.cs:241`, `SkyshopLightController.cs:373`, `URLAudioClipManager.cs:262` | `WWW` is obsolete-but-present on both sides of **this** hop, so there is nothing to port yet; the move to `UnityWebRequest` belongs to whichever hop drops it |
| `Application.LoadLevel` / `LoadLevelAsync` | **1** - `Assembly-UnityScript\ChainMouseOrbit.cs:77` (`Application.LoadLevel(0)`); `LoadLevelAsync` **0** | the method is obsolete but still shipped in 2021.3, so this compiles and warns. The replacement is `SceneManager.LoadScene`, and this is the only site in `src\` that would need it |
| `MovieTexture` | **59** lines in **4** files, and **0** of them a live use: `Battlehub\RTSaveLoad\PersistentObjects\PersistentMovieTexture.cs`, `PersistentData.cs`, `PersistentTexture.cs`, `RTTypeModel.cs` | the engine type is gone; what is left is a surrogate class, two type tokens and one serializer, none of which needs the engine type to compile |
| `GUIElement`, `GUIText`, `GUITexture` | `GUIElement` **56** lines in **4** files (`PersistentGUIElement.cs`, `PersistentBehaviour.cs`, `PersistentData.cs`, `RTTypeModel.cs`); `GUIText` **0**; `GUITexture` **0** | the same shape as `MovieTexture`: never instantiated, only named |
| UNet - `NetworkView`, `NetworkIdentity`, `NetworkManager`, `UnityEngine.Networking` | `NetworkView` is **104** lines in **4** files but the engine type itself is **0** - nothing matches `(^\|[^A-Za-z])NetworkView([^I]\|$)` - so every hit is `PersistentNetworkView` or a comment; `NetworkIdentity` **0**; `NetworkManager` **0**; `UnityEngine.Networking` **10** lines in **8** files, and the only engine type among them is the matchmaker `UnityEngine.Networking.Match` (**2** lines) | hop 3's row stands: the HLAPI package is not installed and no code here calls it, so this hop has nothing to remove |
| `Object.FindObjectOfType` / `FindObjectsOfType` | **29** and **33** lines | neither is `[Obsolete]` in 2021.3, so there is nothing for the Updater to rewrite; the `FindAnyObjectByType`/`FindObjectsByType` pair arrives at a later hop |
| `Physics.autoSimulation` | **3** - `SuperController.cs:2350`, `:5970`, `:6131`; `simulationMode` **0** | still the supported spelling in 2021.3. The replacement is `Physics.simulationMode`, which no version this hop visits asks for |
| `Graphics.DrawMeshInstanced` | **0** | nothing to rewrite, and the obsolete overloads still exist anyway |
| `Lightmapping.giWorkflowMode` | `Lightmapping\.` **0**; `Lightmapping` **1** line, and that one is this project's own class in `Assembly-CSharp\LightmappingManager.cs` | the engine property is not used here |
| `EditorApplication.playmodeStateChanged` | **0** in `src\` | the only occurrence in the repository is the editor side's correctly-cased `playModeStateChanged` (`RebuildGate.cs:44-54`), which 2021.3 still declares |
| `AssetDatabase.CreateFolder` | **0**; `AssetDatabase` **1** line in `src\` | the editor-side uses are the eight in `RebuildGate.cs` (`:383`, `:1765`, `:6638`, `:6992`, `:7858`, `:7894`, `:7929`, `:7956`) and none of them is `CreateFolder` |
| `[RequireComponent(typeof(CanvasRenderer))]` on `Graphic` subclasses | **4** - `UnityEngine\UI\Extensions\NonDrawingGraphic.cs:6`, `ShineEffect.cs:5`, `TextPic.cs:14`, `UIPrimitiveBase.cs:8` | hop 3's row already fixed this: the 2020 removal makes the attribute the caller's job, and all four sites carry it |
| `PlayerSettings.apiCompatibilityLevel` / `scriptingRuntimeVersion` | **0** in `src\` | both are settings rather than API: `ProjectSettings.asset:534` and `:611` keep their 2020.3 values through this hop, and 2021.3 still accepts them |
| Input - legacy `Input` against `m_ActiveInputHandler` | `m_ActiveInputHandler` **0** in `ProjectSettings.asset`; `src\` drives the legacy `Input` class throughout | the Input System package is not in the manifest, so there is no active-handler value to choose this hop, and no `InputSystem` call site to miss |
| `Texture2D` / `TextureFormat` removals | `TextureFormat` **244** lines; `Reinitialize` **0** | the three `Resize` sites in the guide row are the whole of the rename, and the guide names no `TextureFormat` member removal. The enum argument of the two obsolete `Resize` overloads is unrelated to it |
| `AnimationClip` / legacy `Animation` | **75** and **51** lines | `UnityEngine.AnimationModule.dll` exists in both editors' module sets and the guide lists no change to either type, so there is nothing to do |
| `NavMesh` | **356** lines; `UnityEngine.AI` **10** | `UnityEngine.AIModule.dll` is in both module sets and the guide is silent on navigation |
| ParticleSystem modules | **39** ParticleSystem components; **39** `startDelay`, **39** `rateOverDistance`, **78** force-field modules, every one zeroed or disabled | the two particle rows above are the whole of what changes, and both are closed |
| `Terrain` | **5** lines in `src\`; `!u!218` **0** | no terrain in the project, and no experimental terrain API |
| `Cloth` | **92** lines in **50** files | `UnityEngine.ClothModule.dll` is present in both editors and the guide names no Cloth change; the hits are the game's cloth-driven avatar pieces, which keep their serialization |
| `ComputeShader` | **107** lines in **19** files | `UnityEngine.CoreModule` still declares it, and the only compute-shader item in the guide is the local-keyword count, which the shader row above closes |
| `Shader.Find` / shader compile targets | `Shader.Find` **92** lines in **47** files, **22** of which are this project's `MeshVR.VamShaderProvider` path (`MeshVR.` **32** lines in total) | `Shader.Find` is not obsolete in 2021.3 and every shader is still built by the built-in pipeline, so the hop's only shader work is the keyword-locality look question |
| Gradle / Android | `Gradle\|gradle` **0** in `src\`; **0** `.gradle`, **0** `.pro`, **0** `.aar` under `VaM_Rebuild\Assets\`; no `Assets\Plugins\Android` | no Android target is set, so the incremental Gradle path never runs |
| The module assemblies themselves - `UnityEngine.TextCoreModule.dll` gone, `TextCoreFontEngineModule`, `TextCoreTextEngineModule` and `NVIDIAModule` new | **78** module assemblies in `Data\Managed\UnityEngine\` for 2020.3.49f1 against **90** for 2022.3.76f1 (**86** in `2021.3.45f2`, the build this hop uses, re-measured) | an editor fact rather than a project task, but it is the ground the plugin compiler's reference list stands on: the one module the set loses is named nowhere in `src\` (`TextCore` **0**) |

**Five more things the hop has to decide that no guide item covers.**

- **The plugin compiler's reference list survives 2021.3 as it is.** `DynamicCSharp.cs:32` starts the plugin
  compiler with `assemblyReferences = {"Assembly-CSharp.dll"}` and `:34` adds seven module names to it -
  `UnityEngine.AudioModule`, `CoreModule`, `InputLegacyModule`, `JSONSerializeModule`, `ParticleSystemModule`,
  `PhysicsModule` and `UIModule` - which `:111-116` appends to the first list with `Array.Resize`. All seven are
  still shipped by 2021.3, so nothing is added and nothing is removed. The module set does change around the list:
  `Data\Managed\UnityEngine\` holds **78** assemblies in 2020.3.49f1 and **86** in `2021.3.45f2`, gaining the two
  TextCore text-engine assemblies and `NVIDIAModule` and losing `UnityEngine.TextCoreModule.dll`. Nothing in
  `src\` names any of them (`TextCore` **0**), and the list is
  names rather than paths, so the compiler keeps resolving. What *does* change is the compiler binary itself:
  `Assets\Plugins\mcs.dll` is a managed assembly built for the old module graph, so a hop that raises the editor
  version has to re-run `scripts\Build-McsCompiler.ps1` - `McsCompiler.cs:156-165` walks
  `CompilerSettings.AssemblyReferences` and resolves each name against the new editor, which is exactly the step
  that fails loudly if a module were ever dropped. What the 2021.3 editor cannot do is build that compiler at
  all: `Stack<T>` left `System.dll` for `System.Collections.dll` in 2021, and the profile carries the type only
  under `4.5\Facades\`, as a type-forwarding facade a build cannot use - ten sources come back `CS0246: 'Stack'
  could not be found`, and adding `System.Collections` to the build's reference set fails with `CS0006`. So
  `Build-McsCompiler.ps1` now probes the project's editor first and falls back to the newest installed editor
  whose profile can build it, which is **2020.3.49f1**; the binary it emits still references `mscorlib`,
  `System`, `System.Core` and `System.Xml`, the same four as every hop before it.
- **The Boo/UnityScript branch stays where hop 3 put it.** `scripts\Setup-RebuildProject.ps1:303-325` stages VaM's
  `Boo.Lang.dll` whenever the editor ships none, and its own comment at `:310` records why: 2020.2 removed Boo and
  UnityScript from `MonoBleedingEdge\lib\mono\unityscript\`, so the shipped game's `Assembly-UnityScript` needs the
  assembly put back. That is still true on 2021.3 - `Boo.Lang.dll` is absent from 2020.3, 2021.3 and 2022.3 alike -
  so the branch fires for the same reason and no edit follows. The consumer is unchanged too: the **14** tracked
  files in `src\Assembly-UnityScript`, of which `CharacterMotor.cs:5` is the one that says `using Boo.Lang`.
- **The editor version is data, not code, so no gate needs editing for this hop.** Nothing in `RebuildGate.cs` or
  `VaMInspector.cs` names a version. The version is read out of `VaM_Rebuild\ProjectSettings\ProjectVersion.txt` -
  `scripts\UnityEditor.ps1:31-41` parses `^m_EditorVersion:\s*(\S+)` and `:43-63` turns it into
  `${env:ProgramFiles}\Unity\Hub\Editor\$Version\Editor`, which `scripts\Activate-UnityLicense.ps1:6` also uses.
  Raising the hop is therefore a one-line data change plus an install, and the gate's `[InitializeOnLoad]`
  (`RebuildGate.cs:29`) and `playModeStateChanged` hook (`:44-54`) do not care which version opens the project.
  The one code-level trace of the 2020 hop is still in place: `:385-388` uses
  `Unity.CodeEditor.CodeEditor.CurrentEditorInstallation` because 2020.3 removed `CurrentEditorPath`.
- **This hop does not touch the render pipeline.** `GraphicsSettings.asset:124` still has
  `m_CustomRenderPipeline: {fileID: 0}` and the manifest carries no `com.unity.render-pipelines.*` package, and
  `plan.md` item 17 keeps the built-in-pipeline question open until **6000.3** (Unity 6.3) on the route
  `2020.3 → 2021.3 → 2022.3 → 6000.3`. Nothing in this audit changes a render pipeline asset, and the shader-keyword
  row above is a keyword-locality question inside the built-in pipeline, not a pipeline move.
- **The plugin compiler itself carried a defect that no gate and no guide could name, and the hand run found it.**
  Five loads of the boot scene logged `[CS584] Internal compiler error: The method or operation is not
  implemented. in <Unknown> at [687, 6]` once per load, with
  `Compile of MacGruber.Life.12:/Custom/Scripts/MacGruber/Life/MacGruber_Life.cslist failed.` and an empty error
  list under it. The throw is `TypeParameterInflator.Inflate` meeting a by-ref type -
  `ReadOnlySpan<char>`'s `ref readonly T this[int]` indexer, in the overload sets of `StringBuilder.Append` and
  `int.Parse`, both of which carry a `Span` overload only from 2020.3 on. The missing branch is the one upstream
  mono has at `mcs/mcs/generic.cs:1534`; see *The plugin compiler* in `rebuild-project.md` for the chain and
  `verification.md` section 9 for the A/B. The lesson this hop leaves behind: a guide can only name an API *this
  project* calls, and this defect lived in the compiler the project ships to other people's plugins, so only
  running a plugin could show it.

## What stays open after the 2021.3 hop

- **Three call sites change API, and one of them can fail quietly.** The mechanical part is the rewrite of
  `TextureScale.cs:67`, `mset\CubeBuffer.cs:815` and `PersistentTexture2D.cs:22` from `Resize` to `Reinitialize`,
  which the API Updater will offer as a `(UnityUpgradable)` rename. The part to watch is that the two methods do
  not mean the same thing: `Resize` allocated a new texture and left the contents to `SetPixels`/`Apply` at call
  site `:67`, while `Reinitialize` clears the pixels to the format default. `PersistentTexture2D.cs:22` makes this
  worth a second look, because the call sits inside a `try`/`catch`, so a throw from a texture whose format 2021.3
  no longer allows to be reinitialized would be swallowed rather than reported.
- **The subtractive scenes have to be looked at by eye.** All **10** scenes bake in Subtractive mode
  (`m_MixedBakeMode: 2`), around the **9** `!u!108` Mixed lights the project owns. If the new direct-light
  bake-in shows up, the two remedies the guide offers - make the affected Mixed lights Realtime, or move the scene
  to Baked Indirect or Shadowmask - are both hand edits to a scene the export owns, so they have to be decided
  scene by scene and then carried by whatever writes the shipped assets.
- **`Directory.GetFiles` order is the widest behaviour change with the least to do.** The **15** call sites listed
  in the Mono row all consume their result order-independently, so no edit is planned; if anything downstream does
  turn out to be order-sensitive, it will show as a variable-length list or a hash-set-fed count that differs
  between runs rather than as a compile error, which makes it a good candidate for a log diff against hop 3.
- **Physics moves with the engine.** Whatever the 2021.3 physics upgrade changes is not reachable from this code:
  the project's only setting is `Physics.autoSimulation` in three places (`SuperController.cs:2350`, `:5970`,
  `:6131`); the step behaviour of the avatar and any collider chain is a hand check, not a code check.
- **The look is a reconstruction, and the shader keyword change lands inside it.** Keyword declarations are all
  `multi_compile` in **89** shader files, so the locality change has no declaration to act on, but the **257**
  `multi_compile` lines and the **68** `Shader.EnableKeyword`/`Shader.DisableKeyword` call sites in **10** files are
  the same surface the earlier hops measured, and the only verdict that counts is the frame comparison.
- **What the hop is verified by, in order**: the three `Resize` renames compiled and the reinitialized textures
  still show their art; the boot scene opened and compared against its import frames; the shader reimport run
  against the same camera and the same frame; and the scene-by-scene lighting pass above. Anything the Updater
  reports that the tables here do not name is new evidence and belongs in this section before the next hop.

