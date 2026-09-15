# The `*ComputeBuff` shaders, rebuilt from the shipped DXBC

Virt-a-Mate does not let Unity skin its characters. `DAZSkinV2` runs the skinning as a compute
shader and draws the result with `Graphics.DrawMesh(mesh, identity, material, ...)`, binding the
compute output as structured buffers:

| Buffer | Type | Stride | Contents |
|---|---|---|---|
| `verts` | `StructuredBuffer<float3>` | 12 | world-space positions |
| `normals` | `StructuredBuffer<float3>` | 12 | world-space normals |
| `tangents` | `StructuredBuffer<float4>` | 16 | world-space tangents (body meshes only) |

The pose of a character lives entirely inside those buffers. A stock Unity shader cannot read them,
so it draws the mesh in its bind pose - the T-pose - no matter what the skeleton does. `DAZSkinV2`
therefore swaps every material for a name-mangled twin:

```csharp
// DAZSkinV2.SkinMeshGPUMaterialInit
Shader.Find(material.shader.name + "ComputeBuff")
```

which is why the game ships two shaders under one name: the ordinary one and the `ComputeBuff` one
that reads the buffers. AssetRipper exports these as placeholders, so the rebuilt project loses the
body: the mesh draws its bind pose, flat, while the (separately skinned) hair animates correctly on
the real skeleton.

The original HLSL is not in the installation. Unity keeps no shader source in a player build, and no
copy of these files survives in `VaM_Data`. What the build does keep is the ShaderLab contract and
the compiled Direct3D 11 bytecode, so the shaders are reconstructed from that.

## Extraction

```powershell
python scripts\Extract-VaMShaders.py --out artifacts\shader-blobs     # artifacts\ is not committed
```

For each of the 135 shaders in the player data this writes a directory holding

- `contract.json` - properties with types and defaults, subshader tags, and per pass the render
  state, the keyword table, per-variant reflection (textures, constant buffers, and which structured
  buffers the variant binds) plus a `programs` list tying every blob index back to its pass, stage
  and keyword set;
- one `.dxbc` per compiled program, named
  `kShaderCompPlatformD3D11_<blobIndex>_<type>_<KEYWORDS>.dxbc`.

The run yields **58 748 programs**; the ones that matter here are the 51 `*ComputeBuff` shaders.
`register_slot` inside a constant buffer is a raw byte offset (`index / 16`, then `/4` for the
component), which is how every uniform below was tied to the register its bytecode reads.

## Generation

```powershell
python scripts\New-VaMShaders.py            # writes VaM_Rebuild\Assets\Shader\*.shader
python scripts\New-VaMShaders.py --list     # the inventory, without writing anything
```

`New-VaMShaders.py` turns each contract into a `.shader`: the properties, the subshader tags, a
`CGPROGRAM` per pass with the render state Unity recorded (`LightMode` from `state.m_Tags`, cull,
z-test, z-write, the blend factors, the per-pass alpha cutoff), and the pragmas that reproduce the
pass's keyword set. It then copies `shader-src\VamGpuSkinning.cginc` next to the shaders so the
`#include "../VaMShaders/VamGpuSkinning.cginc"` resolves.

Result: **51 shaders, 148 passes**, with 32-104 keyword variants each. 14 passes are deliberately
skipped - the `META`, `DEFERRED`, `PREPASSBASE` and `PREPASSFINAL` ones - because the rebuilt project
uses forward rendering only and those entry points would need a deferred/lightmap pipeline that does
not exist yet.

Every uniform is declared by the generated shader, in the family that published it, together with a
`VAM_HAS_<name>` define. Nothing is auto-declared by Unity here: the only two names its own includes
provide are `_LightColor0` and `_SpecColor`, so a shader for a family that lacks, say, `_SubdermisColor`
compiles because the library falls back to that property's documented default.

## The library

`shader-src\VamGpuSkinning.cginc` holds the whole shading model: the two vertex stages (skinned
forward pass, shadow caster), the surface fetch, the ambient/IBL terms, the specular response, the
fragment stages and the shadow-caster fragment. It is a transcription of the body shader's bytecode,
with three cosmetic deviations listed in its header - a linear reflection mip ramp instead of the
original `exp()` curve, sheen/subdermis added in the fragment stage instead of folded into the
per-vertex colour, and the lightmap indirection reduced to a plain ambient scale because the rebuilt
project bakes no lightmaps. Pose, silhouette, normals, diffuse/specular response, shadow coordinates
and alpha cutoff are direct.

Two Unity macro contracts cost real debugging, because they differ per keyword variant and neither
is visible in a single-pass compile:

- **`SHADOW_COORDS`.** `Multi_compile_fwdadd_fullshadows` compiles a bare `SHADOWS_DEPTH` variant that
  `AutoLight.cginc` does not cover - it defines the macro for `SHADOWS_DEPTH && SPOT`, for
  `SHADOWS_CUBE`, for `SHADOWS_SCREEN` and for "no shadows at all", but not for `SHADOWS_DEPTH` on its
  own. `UNITY_SHADOW_COORDS` forwards straight to it, so a struct that declares its shadow coordinate
  through the macro fails that one variant with *unrecognized identifier 'SHADOW_COORDS'*. The library
  supplies the missing definition.
- **Shadow caster.** `UnityClipSpaceShadowCasterPos(vertex, normal)` expects *object* space - it
  applies `unity_ObjectToWorld` itself - while the compute buffers are world space, so the library
  reproduces its normal-offset half instead of calling it. And `V2F_SHADOW_CASTER_NOPOS` only declares
  the `vec` varying where `SHADOWS_CUBE_IN_DEPTH_TEX` is **not** available; on D3D11 it is, and the
  fragment shader writes depth directly, so the light-space vector must not be written at all.

## Verification

```powershell
python tools\check_shaders.py             # all 51 shaders
python tools\check_shaders.py GlossNMCull # one family
```

`tools\check_shaders.py` lifts every `CGPROGRAM` block out of the generated files and hands it to the
Windows SDK's `fxc.exe` with the profile the pragmas ask for, once per entry point *and once per
keyword set*. Compiling with one keyword set hides exactly the bugs above; with ten of them the sweep
runs **3851/3851 programs, 0 failures**.

That is a pre-flight, not a verdict: fxc knows nothing about ShaderLab, and it is not Unity's
compiler. The verdict comes from a real run, which must report no shader errors at all:

```powershell
scripts\Invoke-SmokeTest.ps1 -Method Play -Seconds 60 -WarmupSeconds 20 `
    -Scene "MeshedVR.DemoScenes.2:/Saves/scene/MeshedVR/DemoScenes/Cyber/CyberDemoAlt.json"
Select-String -LiteralPath artifacts\smoke-play.log -Pattern 'Shader error'
```

## Regenerating from scratch

1. `python scripts\Extract-VaMShaders.py --out artifacts\shader-blobs` - needs the install's player data.
2. `python scripts\New-VaMShaders.py` - needs nothing but the blobs.
3. `python tools\check_shaders.py`.
4. `scripts\Invoke-SmokeTest.ps1 -Method Play ...` and check the log for shader errors.
