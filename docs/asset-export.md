# Stage 2 - Asset extraction and reconstruction

Tool: **AssetRipper 2.0.0** (`AssetRipper_win_x64.zip`, `AssetRipper.GUI.Free.exe`),
installed to `VAMOpen\.tools\AssetRipper\`.

AssetRipper 2.x has **no command-line interface** - the console app was removed after 1.3.14.
The GUI binary still runs headless and exposes a local HTTP API, which is what these
scripts drive:

```powershell
VAMOpen\.tools\AssetRipper\AssetRipper.GUI.Free.exe --headless --port 8765 --log-path VAMOpen\artifacts\logs\assetripper.log
```

Endpoint map (from `http://localhost:8765/openapi.json`):

| Purpose              | Call                                                        |
| -------------------- | ----------------------------------------------------------- |
| Load a game folder   | `POST /LoadFolder`      form `path=<game root>`              |
| Read settings form   | `GET  /Settings/Edit`                                        |
| Write settings       | `POST /Settings/Update` form with the settings fields        |
| Export Unity project | `POST /Export/UnityProject` form `path=<output dir>`         |
| Export raw files     | `POST /Export/PrimaryContent`                                |
| Reset                | `POST /Reset`                                                |

All POSTs are `application/x-www-form-urlencoded` and answer `302`.

## Engine version confirmed

`VaM_Data\ProjectSettings` does not exist in a shipped build, but the export produced
`ExportedProject\ProjectSettings\ProjectVersion.txt`:

```
m_EditorVersion: 2018.1.9f2
```

This matches the earlier identification from `UnityPlayer.dll` (2018.1.9.10931241),
`globalgamemanagers` and from the AssetRipper UI, and it confirms **C# 6** as the correct
language level for the decompiled sources.

## Settings used

| Setting                      | Value            | Rationale                                                        |
| ---------------------------- | ---------------- | ---------------------------------------------------------------- |
| `IgnoreStreamingAssets`      | `true`           | Milestone A: core game data only, see "Export scope" below         |
| `ScriptExportMode`           | `Decompiled`     | Emit C# rather than opaque DLLs                                   |
| `ScriptLanguageVersion`      | `CSharp6`        | Matches Unity 2018.1's Roslyn                                    |
| `ScriptContentLevel`         | `Level3`         | Full method bodies                                                |
| `ShaderExportMode`           | `Decompile`      | `Dummy` (the default) would emit no usable shader source          |
| `BundledAssetsExportMode`    | `GroupByAssetType` | Mirrors the `Assets\Material`, `Assets\Mesh`, ... layout        |
| `ImageExportFormat`          | `Png`            |                                                                   |
| `LightmapTextureExportFormat`| `Yaml`           |                                                                   |
| `SpriteExportMode`           | `Yaml`           |                                                                   |
| `TextExportMode`             | `Parse`          |                                                                   |
| `PreferOriginalTextureExtension` | `true`       |                                                                   |

`EnableStaticMeshSeparation` and `EnableAssetDeduplication` are premium-locked in the free
build and were left off.

## Export scope

`VaM_Data\StreamingAssets` is **16.47 GB** across 261 files and holds the game's real
content as flat AssetBundles (`c_neo_mat`, `f_7_mat`, `z_ui1`, `s_mus`, ... - no platform
subdirectory; `StandaloneWindows64` is a zero-byte marker file). Importing that would take
hours and produce tens of gigabytes of meshes and textures, so the export was split:

* **Milestone A (done)** - `IgnoreStreamingAssets = true`: `globalgamemanagers`,
  `level0-9`, `sharedassets0-9`, `resources.assets`. 90 MB of input, 105 MB of output.
* **Milestone B (pending)** - re-load with `IgnoreStreamingAssets = false` to pull the
  bundles in as well. Only needed if we want the bundle content as editable Unity assets.

Full input inventory for planning:

| Path                        |  Size    | Files |
| --------------------------- | -------- | ----- |
| `VaM_Data\StreamingAssets`  | 16.47 GB | 261   |
| `VaM_Data\Plugins`          | 0.18 GB  |       |
| `VaM_Data\Managed`          | 28.2 MB  | 93 DLLs |
| `VaM_Data\Resources`        | 0.03 GB  |       |
| `Custom`                    | 1.09 GB  |       |
| `AddonPackages`             | 0.84 GB  |       |

## Milestone A output

`VAMOpen\work\ripped-core\ExportedProject` - 11 837 files, 105 MB, 734 exported items,
32.5 s.

```
Assets\
  AnimationClip        18.8 MB
  AnimatorController    0.0 MB
  Avatar                1.5 MB
  Cubemap               1.7 MB
  Flare                 0.0 MB
  Font                  0.2 MB
  GameObject            4.0 MB
  Material              0.1 MB
  Mesh                  4.4 MB
  PhysicMaterial        0.0 MB
  Resources             4.8 MB
  Scripts              31.6 MB
  Shader                0.2 MB
  Sprite                0.0 MB
  TextAsset             0.0 MB
  Texture2D             5.9 MB
  VaMAssets\Scenes      6.7 MB
Packages\manifest.json
ProjectSettings\*.asset  (17 files)
```

All **10 scenes** were reconstructed: `NewStart` (the real entry point) plus the nine
`Benchmark*` performance scenes. `NewStart.unity` is only 6 KB, which is expected - it
does nothing but boot a loader MonoBehaviour that pulls the rest out of the bundles.
`BenchmarkCrypt.unity` is the largest at 6 MB.

AssetRipper also reconstructed the assembly boundary information: every referenced
assembly got its own `Assets\Scripts\<Assembly>` folder plus an `.asmdef`:

| Assembly                        | .cs files |
| ------------------------------- | --------- |
| Assembly-CSharp                 | 2807      |
| System.Windows.Forms            | 1172      |
| mcs                             | 797       |
| NAudio                          | 461      |
| SteamVR                         | 450       |
| Bass.Net                        | 369       |
| Mono.WebBrowser                 | 326      |
| ZFBrowser                       | 257      |
| System.Drawing                  | 225      |
| Valve.Newtonsoft.Json           | 191      |
| Mono.Posix                      | 130      |
| System.Configuration            | 131      |
| System.EnterpriseServices       | 119      |
| ICSharpCode.SharpZipLib         | 108      |
| System.Security                 | 103      |
| protobuf-net                    | 93        |
| MHLab.PATCH                     | 87        |
| Assembly-UnityScript            | 13        |
| Accessibility                   | 4         |
| SteamVR_Actions                 | 3         |
| RTTypeModel                     | 2         |
| System.Runtime.CompilerServices.Unsafe | 1  |
| System.Runtime.InteropServices  | 1         |

## Key finding: the bundles are a runtime dependency only

`Assembly-CSharp\AssetBundles\AssetBundleManager.cs` decides where bundles come from:

```csharp
private static string GetStreamingAssetsPath()
{
    if (Application.isEditor)
    {
        return "file://" + Environment.CurrentDirectory.Replace("\\", "/");
    }
    ...
    return "file://" + Application.streamingAssetsPath;
}
```

and `MeshVR\AssetLoader.cs` then calls `AssetBundle.LoadFromFileAsync(path)`.

Consequences for the rebuild:

1. A **built player** must ship the original `VaM_Data\StreamingAssets` next to its
   executable. The bundles are never touched at edit time, so not importing them into the
   Unity project costs nothing at runtime.
2. In the **editor** the path is `Environment.CurrentDirectory`, not the project - so when
   testing in Play mode the editor's working directory has to be pointed at a folder that
   contains the `*_mat`/`z_ui*` bundle files (a junction to
   `VaM_Data\StreamingAssets` is the least invasive way to do that).

This is why Milestone A is enough to get the code and the core scenes into a Unity
project; Milestone B is an inspection convenience, not a prerequisite.

## Known gaps

* 7 import warnings, all harmless:
  * `UnityEngine.Analytics` / `UnityEngine.Advertisements` /
    `SteamVR_Windows_EditorHelper` are editor-only stubs that a shipped build strips.
  * `Could not read MonoBehaviour structure for UnityEngine.GUISkin` - `GUISkin` is an
    engine type, not a script.
* `Packages\manifest.json` lists only builtin modules. Unity 2018.1 also needs
  `com.unity.ugui` (provides `UnityEngine.UI.dll`) and `com.unity.timeline`
  (provides `UnityEngine.Timeline.dll`); both DLLs exist in `VaM_Data\Managed`. The
  manifest has to be corrected when the project is first opened.
* AssetRipper's own `Assembly-CSharp` output (2807 files) is **not** used. The verified
  sources in `src\Assembly-CSharp` are byte-for-byte parity-checked against the original
  assembly (see `docs\parity-report.md`) and build clean, so they replace it.
