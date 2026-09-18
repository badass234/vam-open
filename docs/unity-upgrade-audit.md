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

## What stays open after this hop

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

## 2019.4 hop

Filled in when the hop is made. What is already measured about the items Unity lists for
[2019 LTS](https://docs.unity3d.com/2019.4/Documentation/Manual/UpgradeGuide2019LTS.html):

- **`com.unity.ugui` becomes a package.** `Packages\manifest.json` holds 36 entries and
  `com.unity.modules.ui` is one of them; `com.unity.ugui` is not there yet, so the editor adds it on
  first open. `UnityEngine.UI.dll` moves out of the built-in `UnityExtensions\Unity\GUISystem\` copy
  and into the package, which is where this project's references to `UnityEngine.UI` have to keep
  resolving from.
- **Assembly definition files and the `unsafe` setting** move: `allowUnsafeCode` becomes a per-assembly
  option. The project has exactly one `.asmdef`,
  `Assets\Scripts\Assembly-UnityScript\VaMUnityScript.asmdef`, and it already carries
  `"allowUnsafeCode": true`, so this is a setting to keep rather than to add; the project-wide
  `allowUnsafeCode: 1` in `ProjectSettings.asset` stays the switch for the assemblies that have no
  asmdef.
- **`mcs`'s Roslyn path**: the editor's own compiler and the runtime compiler it loads are independent, and
  the rebuilt `mcs.dll` is the second one - it is unaffected by the editor moving to a newer Roslyn, but the
  build recipe (editor `mono.exe` + `4.5\mcs.exe`) has to be re-verified against the new editor's Mono.
- **Legacy `ScriptingRuntimeVersion` / `ApiCompatibilityLevel` UI**: 2019 offers only .NET 4.x, and this
  project is already there - `scriptingRuntimeVersion: 1` and `apiCompatibilityLevel: 3` in
  `ProjectSettings.asset` - so what the hop does is make the two fields disappear from the settings
  file while the values they name stay what they were.
