# VaM's custom shaders, rebuilt from the shipped DXBC

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
MeshVR.VamShaderProvider.FindComputeBuff(material.shader.name)
```

which is why the game ships two shaders under one name: the ordinary one and the `ComputeBuff` one
that reads the buffers. AssetRipper exports these as placeholders, so a naive rebuild loses the
body: the mesh draws its bind pose, flat, while the (separately skinned) hair animates correctly on
the real skeleton. `VamShaderProvider` answers that lookup from this project's reconstructions or,
failing those, from the game's own shader library; the sections below cover both halves.

The original HLSL is not in the installation. Unity keeps no shader source in a player build, and no
copy of these files survives in `VaM_Data`. What the build does keep is the ShaderLab contract and
the compiled Direct3D 11 bytecode, so the shaders are reconstructed from that.

## The shipped shader library in `z_sha`

`VaM_Data\StreamingAssets\z_sha` (23.3 MB) is the game's shader library: an AssetBundle holding
**261 `Shader` objects under 260 unique names** (counted by walking the bundle with UnityPy), and
listed as a dependency of 217 other bundles. Because
VaM ships a Direct3D 11 player, the bundle carries the compiled bytecode of every one of them -
including all 57 names this project reconstructs, together with the plain half of every pair.

That looks like it makes the reconstruction unnecessary. It does not, because `Shader.Find` **does
not see shaders that live inside an AssetBundle**. Measured by moving the whole
`VaM_Rebuild\Assets\Shader\` directory aside and re-running the smoke test (then 88 `.shader` files,
now 82):

| | project shaders present | project shaders removed |
|---|---|---|
| the body's 30 GPU-skinned materials | `Custom/Subsurface/GlossNMCullComputeBuff` ×30 | `Custom/Subsurface/GlossNMCull` (no `ComputeBuff`) |
| the body in the render | drawn, posed, lit | gone |
| atoms, exceptions, skinned-vertex counts | 17/17, 0, 23046/24928 | 17/17, 0, 22973/24928 |

The skinning keeps running either way; only the draw breaks. `DAZSkinV2.SkinMeshGPUMaterialInit`
asks for `shader.name + "ComputeBuff"`, gets `null` when that name is only in `z_sha`, keeps the
plain shader, and the plain shader binds `verts`/`normals`/`tangents` - attributes a compute-skinned
mesh does not supply.

At the same time the bundle's shaders *are* used, by a different mechanism: a material loaded from a
bundle references its shader **by pointer** (`m_Shader`), and that reference is resolved across the
dependency chain. That is why `Custom/Subsurface/TransparentSeparateAlpha`, `EmissiveCutout`,
`AlphaMask` and the rest keep working in the rebuilt project although it never defines them.

The gap between the two paths is closed by `VamShaderProvider`, which makes a name lookup a second
consumer of the same library: a family this project has not transcribed yet renders with the game's
own bytecode instead of a stub. That gives `VaM_Rebuild\Assets\Shader\` two rules. A reconstructed
file still wins, because `Shader.Find` is tried first. And a name that is deliberately not
transcribed must have **no** file here: otherwise `Shader.Find` returns a shader built for the wrong
shading model and the fallback is never reached. That is why the Marmoset IBL set is excluded in
`New-VaMShaders.py` rather than merely left pending.

### Reading a shipped shader back by name

`src\Assembly-CSharp\MeshVR\VamShaderProvider.cs` is that shim. It tries
`Shader.Find` first - a reconstruction of this project wins, because it is the one that has been
checked against the bytecode - and on a miss loads `z_sha` and indexes its shaders by `Shader.name`,
which is the ShaderLab name the name-mangled lookup asks for. The bundle's own asset paths are
unrelated to that name, so the index is built by loading the bundle's shader assets once and keying
them by name. A miss is deliberately not cached, because the scene bundles may still be loading, and
an attempt is made at most once every five seconds.

The two swappers call it instead of `Shader.Find`: `DAZSkinV2.SkinMeshGPUMaterialInit` (including the
`ComputeBuffCopy` variant it prefers, which is never in the bundle) and `DAZSkinWrap.InitMaterials`.
A material therefore reaches its `ComputeBuff` twin whether the project generated it or the game
shipped it:

the family table]:

| family | served by |
|---|---|
| `Custom/Subsurface/*`, `Custom/Hair/*`, `Custom/DebugNormals`, `Custom/Subsurface/Classic`, ... | this project, 57 shaders |
| `Marmoset/*`, `GPUTools/MeshedVR/Hair*` and the rest of the 78 pending contracts | `z_sha`, verbatim |

`New-VaMShaders.py` names the second of those families in `UNTRANSCRIBED_FAMILIES`, so the generator
stops claiming it, and its generated files were removed with that change. Shading the Marmoset IBL
set with `VamGpuSkinning.cginc` was never right - it is a shading model of its own - and the shipped
bytecode is both correct and free. The body and hair families are reconstructed rather than borrowed:
they are the ones whose every detail this project reads, and `VAM_HAS_<name>` switches in the library
depend on knowing which uniforms a family publishes.

The hair was the one family that looked like a second shading model and was not: its contracts bind
the same three buffers, its passes use the same light modes, and the library already carried the
whole model it needs. What was missing was five uniforms in three mechanisms, and a pass's cutoff
selected by evidence rather than by position - see *What the hair needed*. The plain `Custom/Hair/*`
twins are the pair the table does not cover: no contract of theirs was ever extracted, so there is
nothing to reconstruct them from, and nothing asks for them (every lookup appends `ComputeBuff`).

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

The run yields **58 748 programs**; the ones that matter here are the 45 `*ComputeBuff` shaders this
project reconstructs, out of the 51 that exist.
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

Result: **57 shaders, 168 passes** (15 of them tessellated), with 1-26 non-`STEREO` keyword variants
per pass in the contracts. `--list` prints the same inventory without writing anything. The `META`, `DEFERRED`,
`PREPASSBASE` and `PREPASSFINAL` passes are not generated - `SKIP_LIGHTMODES` - because the rebuilt
project uses forward rendering only and those entry points would need a deferred/lightmap pipeline
that does not exist yet.

Every uniform is declared by the generated shader, in the family that published it, together with a
`VAM_HAS_<name>` define. Nothing is auto-declared by Unity here: the only two names its own includes
provide are `_LightColor0` and `_SpecColor`, so a shader for a family that lacks, say, `_SubdermisColor`
compiles because the library falls back to that property's documented default.

### Which pass clips

The per-pass alpha cutoff is the one piece of a pass the contract does not record. A shader property
owns its constant-buffer slot whether or not a program loads it, and the render state has no field
for a discard, so neither the property list nor `state` can say which of a family's several cutoffs a
given pass tests. The program that discards can, and does: `cutoff_for()` in `New-VaMShaders.py`
returns the single name in
`stage_globals(pass, "progFragment") & {"_Cutoff", "_Pass1Cutoff", "_Cutoff1", "_Cutoff2"}`.
`stage_globals` ties each of those names to the register its bytecode reads through the contract's own
reflection (`register_slot`), then asks which of those registers the variant's constant-buffer
reflection lists.

That replaced an inferred rule, and the inference is worth recording because it looked sound: "a pass
clips only if it is opaque (`zWrite == 1`) or additive (`LightMode == FORWARDADD`); the first opaque
pass reads `_Cutoff` and any later one `_Pass1Cutoff`; families that publish `_Cutoff1` / `_Cutoff2`
are matched by name order too; a `SHADOWCASTER` pass inherits whichever name its opaque pass resolved
to", with a hand-written `CUTOFF_IN_TRANSPARENT_PASS` list for the passes that discard while blended.
Measured over the 136 emitted passes of the 45 reconstructed families, against "does the pass's own
fragment contain a `discard`":

- the opaque-or-additive premise asks for a cutoff on **44 passes that never discard**, so the
  transparent side of the rule was carrying most of the decision;
- it misses **6 passes that do discard**, all of them transparent `FORWARDBASE` layers - **4 hair**
  (`MainAlternate` 3, `MainSeparateAlphaLayerScalp` 1, `Scalp` 0, `ScalpSeparateAlpha` 0) and two
  skin (`TransparentCutoutSeparateAlpha` 0, `TransparentGlossNoCull` 0);
- a whitelist could only paper those 6 over by shader *and pass index*, which is a list that dies the
  moment a family is regenerated - and pass indices are not comparable between families anyway.

The reflection rule, measured the same way over every non-`STEREO` variant of every emitted pass:

- **80 passes discard and exactly those 80 are given a cutoff - 0 disagreements**, and no pass reads
  a cutoff it does not test;
- **0 passes read more than one** of the four names, so a per-pass answer exists at all;
- **0 keyword variants of a pass disagree**, so one `VAM_PASS_CUTOFF` per pass is enough and a pass
  needs no keyword-dependent cutoff.

`CUTOFF_IN_TRANSPARENT_PASS`, `is_opaque_pass()` and the name-ordering helper are gone with it.

The utility shaders that clip against hard-coded literals - `Battlehub/RTHandles/VertexColorClip`,
`Custom/Discard`, `Oculus/OVRMRCameraFrame` and `OVRMRCameraFrameLit` - still discard in their shipped
programs, and none of them is reconstructed: `classify()` claims a `*ComputeBuff` name (or a
`Custom/Subsurface/` twin of one), and none of the four carries that suffix. `OVRMRCameraFrameLit`
does bind `verts` and `normals`, so it would be reconstructable the day something looked it up under a
`ComputeBuff` name; nothing does.

### What the hair needed

The hair is the family that looked like a second shading model and is not, and the way that was
established is worth stating, because the usual tool cannot answer it. `tools\verify_twins.py`
compares a plain contract against its `ComputeBuff` twin, and the hair has **no plain contract**: all
14 are the `ComputeBuff` half, while the plain `Custom/Hair/Main` and its relatives produced no blob
and have no entry in `z_sha` either. The evidence is therefore the uniform set, read per pass from the
reflection:

- the fragment globals the hair binds that the body family never binds are the four window uniforms
  `_uvXMin`, `_uvXMax`, `_uvYMin`, `_uvYMax` plus `_Cutoff1`, `_Cutoff2` and `_Pass1Cutoff` - and
  those three are the cutoff mechanism the library already has, not a new one;
- the body-only fragment globals are its four extra scalars - `_ConserveEnergy`, `_DetailWeight`,
  `_DiffuseBumpiness`, `_SpecularBumpiness` - which `VAM_HAS_<name>` already handles;
- the buffer set is the body's minus `tangents`: every hair pass binds `verts` and `normals` only;
- the light modes are the three the body uses: `FORWARDBASE`, `FORWARDADD`, `SHADOWCASTER`.

Disassembled side by side the two fragments are close but not identical, and the difference is the
inputs: the hair's forward-base is ~181 instructions with 4 samplers against the body's 227 with 7,
and the opcodes only the body has - `if_nz`/`else`/`endif`, `eq`, `movc`, `exp`, `log`, `dp4`,
`dp4_sat`, `dp3_sat`, `add_sat`, `min`, `sample_l`, and roughly twice the `dp3`/`rsq` - are the extra
terms those four scalars and the body's extra maps drive. This is not byte identity and must not be
described as such: it is the same model with fewer inputs, which is what the library's `VAM_HAS_*`
switches exist for.

Five uniforms in three mechanisms were all that was missing, plus a cutoff selected by evidence rather
than by position:

- **the uv window.** `_uvXMin`, `_uvYMin`, `_uvXMax`, `_uvYMax` remap one uv: `VAM_UV_WINDOW`
  interpolates the texel between the corners and `VamPack` feeds `VAM_UV_MAIN(uv)` through it, so
  every map samples there. The generator emits `VAM_UV_WINDOWED` only for a shader that publishes all
  four, and validates the substitution at the same time - a windowed family's vertex program
  transforms `_MainTex_ST` and **nothing else**, which is what permits one windowed uv to stand in for
  every other map's `_ST`;
- **`VAM_PASS_VERTEX_OFFSET`.** The two families that thicken a strand before lighting it move each
  vertex along its own normal through `_VertexNormalOffset`, in object space and before the object
  transform, which is why it is a vertex-stage term rather than a scale on the position. It is emitted
  only for the passes whose vertex program reads the uniform: passes 1, 2 and 3 of
  `MainThickenComputeBuff` and of `MainThickenSeparateAlphaComputeBuff` - the forward, add and shadow
  of the layer being thickened - and not for those families' layer-2 passes;
- **`VAM_NO_TANGENTS`** for the missing buffer, which is also what the eye families need;
- **the cutoff**, read out of each pass's own fragment as described above.

With those, `Custom/Hair/` came out of `UNTRANSCRIBED_FAMILIES`, which now names only `Marmoset/`.

## The library

`shader-src\VamGpuSkinning.cginc` holds the whole shading model: the two vertex stages (skinned
forward pass, shadow caster), the surface fetch, the ambient/IBL terms, the specular response, the
fragment stages and the shadow-caster fragment. It is a transcription of the body shader's bytecode,
with the deviations listed in its header and repeated here: lightmap indirection is not transcribed
(the 3-D LUT lookup, the probe-volume path and Unity's baked-occlusion dot are replaced by Unity's own
lightmap / screen-space shadow macros), the per-vertex emissive term is omitted because the vertex
program hard-codes that interpolator to zero, and the additive pass keeps a plain Lambert diffuse
until its own DXBC blob is transcribed. Everything that decides the pose, the silhouette, the surface
response and the base shading - the structured-buffer skinning, the normals, the Fresnel curve, the
gloss-driven highlight and reflection mip, the two-source SH ambient, the shadow coordinates and the
alpha cutoff - is a direct transcription, register for register.

The body pixel shader is the interesting one, and four of its registers are worth writing down
because a "sensible" rewrite of any of them changes the look:

- **Two perturbed normals, two jobs.** The bump map is applied twice, with `_DiffuseBumpiness` and
  `_SpecularBumpiness`, and the split is not diffuse/specular as the names suggest: the *specular*
  normal drives the Fresnel, the reflection and the ambient, while the *diffuse* normal drives the
  direct light only.
- **`_SpecInt` is bounded, not multiplied.** The specular scalar enters through a curve that passes
  `1 -> f0 -> f0^3` on `_Fresnel`, then has a square root taken out of it (`0.922444` is a literal
  from the bytecode) so grazing angles cannot blow the highlight out.
- **Gloss curves score twice, and the curve is not the obvious one.** The material feeds one value into
  both the reflection mip and the highlight's exponent. Writing `s = saturate(glossMap + _GlossOffset)`
  for that input, the listing forms `a = 1 - (1-s)^2 = 2s - s^2`, then `mip = 7 + a * (1 - _Shininess)`
  and `power = 2^(1 + a * (_Shininess - 1))`. The mip lands in `0..7` (7, the last level of the
  reflection cube, at zero gloss) and the power in `2..256` at `_Shininess = 8`, which is what lets
  VaM's 2..8 range produce specular that tight. Two traps sit in those five instructions. The listing
  never writes `a` down: it computes the pair `(1 - g^2, 8 - g^2)` from `g = 1 - s` and then
  `mip = (8 - g^2) - _Shininess * (1 - g^2)`, which is `7 + a * (1 - _Shininess)` only after the
  substitution (`1 - g^2 = a`, `8 - g^2 = 7 + a`); reading it as `8 - _Shininess * a` is off by `1 - 2a`.
  And the exponential is **base 2**: DXBC's `exp` is `2^x` (fxc compiles HLSL `exp` to `mul 1.442695` +
  `exp` and HLSL `exp2` to a bare `exp`), so the natural-exponential reading shifts the curve rather
  than only brightening it. The exponent normalisation `power * 0.159155 + 0.318310` scales the
  **highlight only** - it never reaches the reflection, and letting it (the first port did) multiplies
  the reflection by roughly 3 to 30, which is a gloss that reads as polished plastic.
- **Exposure is one knob over the whole result.** `_ExposureIBL` is a single `float4`: `.x` scales the
  sky SH, `.y` the image-based reflection, `.w` everything. There is no `.z` use in this pass, and the
  two branches do not escape the master scale - the indirect term reaches the output as `ibl * .w`,
  while each direct-light term is pre-multiplied by the same `.w` inside the pass.

The bump map's alpha masks the X slope only - another quirk that is reproduced rather than fixed.

**`_AlphaTex` replaces the diffuse alpha, it does not add to it.** Twenty-seven of the families declare
an alpha mask, and every one of them reads it from the **`.a`** channel with its **own** `_ST`, combines
it through `add_sat(mask.a + _AlphaAdjust)`, premultiplies the RGB by that result in discarding passes,
and writes an output alpha of `0`. So the family macro pair
`VAM_SAMPLE_ALPHA` / `VAM_UV_ALPHA` - `0.0` / `uv` when the family has no mask - **substitutes** for the
`_Color.a * _MainTex.a` product rather than joining it. Families with no mask keep that product, plus
`_AlphaAdjust`. Getting this wrong is what made the eyelashes render as solid cards: see
`verification.md`, defect 3.

**Some passes do not shade at all.** The library's model is not universal: a pass whose shipped
fragment samples `_MainTex`, keeps the texel's alpha and writes `mov o0.xyz, l(0,0,0,0)` has no
lighting in it, and the shared model writes a lit colour into it instead, which the pass's own blend
then lands. Nothing in the *data* selects the other entry point: the render state is an ordinary
transparent one and the properties are the same short list a diffuse IBL family has. What can select
it is what the fragment program reads - a program that shades reads many constants, a mask reads at
most a colour tint and an alpha adjustment. `mask_only()` in `New-VaMShaders.py` is that test: the
pass's `LightMode` is `FORWARDBASE` or `FORWARDADD`, and its fragment's `$Globals` are a subset of
`{_Color, _AlphaAdjust}`. Over the emitted passes it selects exactly five, four of which are the hair's
under-layer:

- `Custom/Hair/MainComputeBuff`, `Custom/Hair/MainThickenComputeBuff` and
  `Custom/Hair/MainThickenSeparateAlphaComputeBuff`, pass 0 - the transparent layer a hair family lays
  down before its lit layer draws over it, 11 instructions each. It is not decoration: the shipped
  pass writes black, so shading it writes a second lit layer where the game darkens.
- `Custom/Subsurface/AlphaMaskComputeBuff` passes 0 and 1.

Which terms a mask keeps is read the same way, into `VAM_MASK_COLOR` and `VAM_MASK_ADJUST`: the hair's
mask multiplies the texel's alpha by `_Color.a` (`mul o0.w, r0.x, cb0[68].w`) and stops there, the
separately-alpha one keeps the texel untouched (`mov o0.w, r0.w`), and the alpha-mask family does
`mad_sat o0.w, r0.w, cb0[69].w, cb0[68].x`, which is that product plus `_AlphaAdjust`. A pass samples
at the interpolated uv unless `VAM_MASK_CONSTANT_UV` names another: the alpha-mask family's additive
pass is the one that samples at `float2(1, 0)` instead, which only its disassembly shows. Shading the
alpha-mask family with the shared model is the over-bright eye in `verification.md`, defect 3.

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

## The plain twins

The `ComputeBuff` name suffix is not decoration and it is not universal. `DAZSkinV2` appends it only
when it swaps a material it is going to skin; every other material keeps the ordinary shader. The
extracted set reflects that: **135 contracts, of which 51 are `*ComputeBuff` and 84 are plain**. Of
the 51, **45 are reconstructed here** - 31 of the body family and 14 of the hair - and the remaining
6 are Marmoset IBL families left to the shipped library, because they are not this library's shading
model. Of the 84 plain contracts, 12 turned
out to be the *same program* as a `ComputeBuff` twin, so only the vertex data source had to change.

### How that was established, and not guessed

`tools\dump_dxbc.py` disassembles the extracted blobs through `fxc /dumpbin`, so the shipped
bytecode can be compared program by program:

```powershell
python tools\dump_dxbc.py GlossNMCull --stage fragment --all-variants
```

- The **pixel** programs of the two are the same instruction stream. `tools\verify_twins.py` runs
  both through `fxc /dumpbin` and compares the disassembly. Two things had to be got right first.
  **Pairing** is by stage, *pass index* and keyword set: `(stage, keywords)` alone is not a key,
  because one keyword set can hold two different passes - `DIRECTIONAL/MARMO_GAMMA/SHADOWS_SCREEN`
  has both an `_088_` and a `_280_` pass - and pairing across those two invented 50 differences
  that are not twin differences at all (`dcl_constantbuffer CB0[74]` against `CB0[95]`, a
  `texture3d` against a `texturecube`). `blob_index` is the pass's index into the shader's pass
  table, and every one of the 51 twin families enumerates the same `blob_index` and keyword layout
  as its sibling, so it is a sound key.
- Reducing the two encodings to a common form is not enough on its own, because the two
  compilations allocate temporary registers differently and allocate them in a different order.
  The comparison therefore rewrites both sides into SSA: every register read is replaced by the
  index of the instruction that produced it, so `r4.x` and `r9.z` become the same token when they
  hold the same value, and destination masks are then rewritten to the lanes that are actually read
  (a lane no read uses is dropped, so a write the other side does not make is not a difference).
  Operands of a commutative opcode are sorted, and a write is renamed only *after* its own sources
  are named, so `add_sat r3.w, r3.w, x` reads the older `r3.w`. Each pass is then classified rather
  than merely passed or failed:
  `identical` (byte-equal after the encoding fixups), `canonical` (same up to how the temporaries
  were allocated), `mask_only` (same up to the write mask of a sample), `lane_assignment` (the same
  components, placed in different lanes, with the operand swizzles and the lane order of a literal
  rotated to match), and finally `operand_diff` / `opcode_diff` / `layout_mismatch`, which are the
  only classes that make the tool fail.
  This is why the check is a comparison of whole streams and not a search for suspicious operands:
  an earlier version grepped a register for reads and reported `dp3 r4.x, r4.xyzx, r4.xyzx` as
  differing when that exact line is present in **both** programs. A heuristic over operands reports
  differences that are not differences.
  Full sweep: 580 passes compared - **474 identical, 32 `canonical`, 60 `mask_only`, 14
  `lane_assignment`, 0 `operand_diff`, 0 `opcode_diff`, 0 `layout_mismatch`**, so 566 of 580 are
  proven the same instruction stream up to renaming and the remaining 14 agree once which lane a
  component sits in is ignored. The 14 were also checked by hand against the raw `fxc` output; they
  are real lane renamings, not an artefact of the comparison.
  ```powershell
  python tools\verify_twins.py                 # a sample of every family
  python tools\verify_twins.py --pairs-per-family 0 --show 5   # all 580 passes
  ```
  It exits non-zero if any pass falls into one of the last three classes.
- The **vertex** programs are *not* the same instructions, and they are not supposed to be: the
  twin's vertex stage adds `ld_structured` calls to pull the skinned position and normal out of a
  buffer, takes `SV_VertexID` and `POSITION.w`, and drops the `TANGENT` and `NORMAL` inputs, while
  the plain one takes `POSITION.xyzw`, `TANGENT` and `NORMAL` directly. Their **output signatures
  are identical**, which is why `vam_v2f` and the whole fragment path are shared unchanged.
- Where the twin samples with a narrower destination than the plain shader, the dropped component
  is not read: the SSA pass drops any destination lane that no read uses, so a write the other side
  simply does not make is not a difference. A plain text search here would have reported those
  lanes as live reads; they are not.

### Why the plain `Custom/Subsurface/*` family is the safe subset

`Custom/Subsurface/Cull` and its siblings are shared assets: one `.mat`, referenced by many
characters. The game tints them per character and per channel (`Genitalia.cs`, the breast physics
code) by writing into the material instance. A per-character tint cannot be skinned through a shared
structured buffer, so these materials were never candidates for the `ComputeBuff` path - which is
exactly what the bytecode says. `MESH_FAMILY_PREFIXES` in `New-VaMShaders.py` encodes that prefix,
but the real gate is `classify()`: the twin must exist in the contract set **and** no rendered pass
of the plain shader may bind `verts`, `normals` or `tangents`. `Custom/Subsurface/EmissiveGlow` has a
contract but **no `ComputeBuff` twin at all** - no contract, no file, and no entry in `z_sha` - so
`classify()` leaves it alone, and the `EmissiveGlow` on disk is still the AssetRipper stub. It stays
that way on purpose: being batch-tinted is a property of the material, not of the pairing, and
nothing in the shipped scenes selects it.

`Marmoset/Specular IBL Soft` and `Marmoset/Transparent/Specular IBL` are twins too, and both halves
are held back: their shading model is not the body's, so `VamShaderProvider` serves the
`ComputeBuff` name out of `z_sha` instead. The other four `Marmoset/*ComputeBuff` contracts are held
back for the same reason - the generator used to shade all of them with `VamGpuSkinning.cginc`, and
that was wrong. This also settles the open lightmap question those twins carried: it is not the body
library's question to answer. The 14 `Custom/Hair/*ComputeBuff` shaders were held back on that same
suspicion and are **not** held back any more: measured against the bytecode, they are the body's
model with fewer inputs - see *What the hair needed*.

### What the generator emits

`VAM_MESH_SKIN` in `shader-src\VamGpuSkinning.cginc`:

- suppresses the `StructuredBuffer` declarations (a plain pass binds none);
- replaces `VamSkin()`'s buffer fetch with `unity_ObjectToWorld` on `v.vertex` plus
  `UnityObjectToWorldNormal` / `UnityObjectToWorldDir` on the vertex normal and tangent. The
  original's second, `mul(UNITY_MATRIX_MVP, v.vertex)` world-position form is kept verbatim for
  fidelity, even though the two agree for a `w = 1` mesh;
- leaves everything from `vam_v2f` down shared with the `ComputeBuff` family.

`New-VaMShaders.py` classifies each contract as `skin`, `mesh` or pending, validates the pass's
buffer set against the family it chose, and prints the shaders it left behind. Current output:

```
wrote 57 shaders (168 passes, 15 tessellated)
```

- **45 `skin`** - 31 of the body's `*ComputeBuff` family and the 14 hair ones. Four of them do not
  shade in every pass: the hair's three layer-0 passes and `Custom/Subsurface/AlphaMaskComputeBuff`
  are `skin` for their vertex path but are given the mask-only fragment, selected per pass by
  `mask_only()`. The classification answers which vertex path a contract needs, not which fragment.
- **12 `mesh`** - `Custom/Subsurface/` `Cull`, `NoCull`, `GlossCull`, `GlossNoCull`, `GlossNMCull`,
  `GlossNMNoCull`, `CutoutSeparateAlpha`, `TransparentCutoutSeparateAlpha`,
  `TransparentGlossSeparateAlpha`, `TransparentGlossNoCullSeparateAlpha`,
  `TransparentGlossNMSeparateAlpha`, `TransparentGlossNMNoCullSeparateAlpha`.
- **25 AssetRipper stubs** left untouched - *plus* the stub of every family the generator claims:
  the stub and the rebuilt sibling are two files for the same family name, and the sibling is what
  Unity compiles, because it carries the same name and is the one in the compiled set. The 25 are
  the families nothing claims: Marmoset lit (7), sky/dome/overlay (3), `Marmoset/Projection Box`
  (1), the geometry-shader family (`GPUTools/MeshedVR/Hair`, `HairOpt`, `HairOptSinglePass`,
  `GPUTools/Painter`, `Hidden/NGSS_Directional` - 5), and 9 unlit gizmo/UI/cursor/silhouette
  shaders. A stub is only a hazard where a name is *looked up* instead of referenced - the
  `ComputeBuff` swap, or `DAZImportMaterial`'s JSON lookup - and `VamShaderProvider` now answers
  those from `z_sha`.

So `Assets\Shader\` holds two file families for many names. Counting the leftovers as "25 stubs"
hides the interesting half of that: 12 plain contracts that used to be stubs are now rebuilt
shaders, and `VaM_Rebuild\Assets\Shader\` reveals it as 82 files = 45 rebuilt `*ComputeBuff` + 12
rebuilt plain + 25 stubs.

## Verification

```powershell
python tools\check_shaders.py             # all 57 shaders
python tools\check_shaders.py GlossNMCull # one family
```

`tools\check_shaders.py` lifts every `CGPROGRAM` block out of the generated files and hands it to the
Windows SDK's `fxc.exe` with the profile the pragmas ask for, once per entry point *and once per
keyword set*. Compiling with one keyword set hides exactly the bugs above; with ten of them the sweep
runs **4414/4414 programs, 0 failures**. A pass that declares `VamFragment` is also compiled once as
`VamFragmentAdd` and vice versa, so an ordinary forward pass is counted twice per keyword set; a mask
pass declares `VamFragmentMask` and is counted once, which is why the mask fix moved the total 4444 ->
4414.

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
