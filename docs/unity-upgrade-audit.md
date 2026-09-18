# Auditing an engine hop against Unity's upgrade guide

Unity's own upgrade guide per version is the checklist for a hop, but it is written for a whole project
and says nothing about which items *this* project is exposed to. This file records the audit of a hop:
item by item, what the evidence was, and which items no amount of reading can settle.

The migration is staged - `2018.1.9f2` (the engine the game ships with) to `2018.4.36f1` to `2019.4.41f2` -
rather than a jump to the newest editor, so there is one guide per hop:
[2018 LTS](https://docs.unity3d.com/2019.4/Documentation/Manual/UpgradeGuide2018LTS.html) and
[2019 LTS](https://docs.unity3d.com/2019.4/Documentation/Manual/UpgradeGuide2019LTS.html).

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
| The new asset import pipeline (V2) | closed, **and it is opt-in** | `EditorSettings.asset` still reports `serializedVersion: 7` and carries no `m_AssetPipelineVersion`, so the project is on **V1** and the V1 → V2 migration question does not arise; declining it deliberately keeps the imported asset database comparable with the one every 2018.4 measurement was taken on |
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
- **The asset import pipeline stays V1 by choice.** Unity recommends V2 from 2019.3; this project declines
  it so that the imported asset database, and every measurement taken on it under 2018.4, stay
  comparable. Moving to V2 is a deliberate item for a later hop, not an oversight.

