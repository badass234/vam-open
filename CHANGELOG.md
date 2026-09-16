# Changelog

Notable changes to **OpenVaM**, the open rebuild of Virt-a-Mate.

Versions follow [semantic versioning](https://semver.org/spec/v2.0.0.html). The project is in alpha,
so the minor number moves with each round of user-visible work and only the major/minor pair is
meant to be read as stable.

## Unreleased

Two rounds are in this section. The first transcribed the hair, and the whole hair family now renders
from this project's code. The second reconstructed the plain twins - the half of every family that no
`ComputeBuff` lookup reaches - and closed a defect that was silently disabling two of the game's own
shaders.

The hair is the body's shading model with fewer inputs, and one pass of every hair family does not
shade at all: it writes black and keeps the texel's alpha. The eye and the lashes had already started
drawing with the original's own masks; the hair carries the same kind of mask pass, and it is now read
from each pass's own fragment rather than from a name table.

### Round 1 - the hair, and a pass that does not shade

#### What works

- **Hair** - the `Custom/Hair/*ComputeBuff` families are reconstructed like the body's. They were
  held back for two rounds on the suspicion of being a second shading model, and they are not: the
  fragment globals they add are the four `_uvXMin`/`_uvXMax`/`_uvYMin`/`_uvYMax` window uniforms and
  the `_Cutoff1`/`_Cutoff2`/`_Pass1Cutoff` names, their buffers are the body's minus `tangents`, and
  their light modes are the body's three. Four mechanisms carry the difference - the uv window, the
  per-pass vertex normal offset, `VAM_NO_TANGENTS`, and the cutoff - and the two thicken families use
  the first two.
- **The hair's under-layer pass** - pass 0 of `MainComputeBuff`, `MainThickenComputeBuff` and
  `MainThickenSeparateAlphaComputeBuff` writes black and keeps the texel's alpha; it lays a dark layer
  down for a later pass to draw over. We were emitting the shared shading model for it, so it wrote a
  second fully lit layer. The render state cannot reveal this - every hair pass serialises
  `colMask = 14`, RGB masked off, so the shipped `mov o0.xyz, l(0,0,0,0)` is dropped and the pass is
  visible only as coverage. It is found by what the fragment reads instead.
- **Lashes** - the cards render as strands instead of solid dark planes. The alpha was built from
  `_MainTex.a`, which these materials stub to white, and the mask was added on top of it, so
  `_AlphaTex` could never win; behind that the mask was read from `.r` instead of `.a` and sampled on
  the diffuse map's UV set. The shipped fragment is `saturate(_AlphaTex.a + _AlphaAdjust)` on the
  mask's own `_AlphaTex_ST`, with the colour premultiplied before the cutoff - and all 27 families
  that declare `_AlphaTex` read `.a` with `_AlphaAdjust` in an add, so it is family behaviour rather
  than a lash quirk.
- **Eyes** - the same rule now covers `Custom/Subsurface/AlphaMaskComputeBuff`, which the previous
  round had pinned by name. Its shipped fragment is three instructions: sample `_MainTex`, `mad_sat`
  the alpha against `_Color.a` and `_AlphaAdjust`, write black to RGB. Ours ran the full shading model
  through the first pass's `SrcAlpha` blend and then added a second lit colour under `One One`, which
  is why the eye read over-bright. A pass is a mask when its `LightMode` is a lit one and its
  `$Globals` are a subset of `{_Color, _AlphaAdjust}`; over the 136 emitted passes that selects exactly
  5, all of them masks, against a nearest non-match of 29 globals.
- **Clipping** - which passes discard is now read from the shipped programs instead of the render
  state. The old zWrite-and-opaque rule had no false positives but missed 10 passes, one of them the
  lash's own base pass; widening it leaves 4, and the states that differ only in the original source
  are named in the generator.

#### Measurements re-run

- `scripts\Invoke-CompileGate.ps1` - `----- RebuildGate OK -----`, 0 errors, 0 unique errors,
  `Assembly-CSharp.dll` 6 163 968 B.
- `python tools\check_shaders.py` - `4414/4414 programs compiled, 0 failed`, 57 shaders, 168 passes,
  15 of them tessellated. The hair is +14 shaders and +55 passes, and three of those passes now
  compile as `VamFragmentMask` once per keyword set instead of twice as `VamFragment`/`VamFragmentAdd`,
  which is where 4444 - 30 comes from. These are the counts as of this round; the plain twins below
  take them to `6805/6805` over 88 shaders.
- `python tools\verify_twins.py` - 112 pixel passes of the hair's twin families, all the same
  instruction stream up to renaming, 0 differing (`python tools\verify_twins.py
  --pairs-per-family 0` covers 580). The gate is one-source here and two-source by the round below.
- The run's own report now carries two measurements defect 1 needed and did not have: the GPU average
  of every map the drawn skin's 30 `GPUmaterials` slots sample, and the handedness of the tangent
  basis the shaders are given.

#### Defect 1 - the two unmeasured suspects come back clean

The shoulder and back gloss seam had been narrowed to two things that were never measured, and both
come back clean:

- **The skin maps are right.** Every slot's `_MainTex` averages a plausible warm skin -
  `Lexi_TorsoD`, the map the broken submeshes share, is `(0.64, 0.41, 0.33)` against the working
  `Lexi_LimbsD` at `(0.64, 0.43, 0.38)` - the gloss maps read `0.04-0.07` (glossy) and the normal maps
  `(1.00, 0.49, 0.49)` (intact DXT5nm). The one near-white map is the cornea's `S6EyesTr`, a
  transparency mask whose family no longer shades at all.
- **The tangent basis is right where the seam is.** All 24928 CPU tangents are left handed and
  perpendicular to their normals; the tangent buffer's 205 non-left-handed slots sit **181 below the
  hip** - the region that renders correctly - against 3 in the torso and 2 in the neck and head.

With the shaders already refuted, the skin now has no remaining suspect. What the same run locates is
where the brightness comes from: the legs are the only band with no blown pixels and the only band
whose colour is not clipped, and above them the white rises with height (hip 6.9 %, torso 11.2 %, head
13.8 %) - additive, and not something a wrong map or a wrong tangent frame can do. The hair is the
part of that which is not in the project: `Custom/Hair/MainSeparateAlphaLayer1` resolved as
`origin=bundle only (not in project)` on that run, since `New-VaMShaders.py` then listed `Custom/Hair/`
and `Marmoset/` as untranscribed. The hair is transcribed as of this entry, so that reading no longer
holds and the over-brightness stands against the game's own hair shader rather than ours.

#### Verified by hand

- The lashes draw as strands rather than solid dark cards, in the editor's play mode on the game's
  own boot scene (`Saves/scene/MeshedVR/default.json`, `scripts\Invoke-ManualPlay.ps1`). The eye's
  change is proven at the instruction level rather than by eye: both of its passes disassemble to
  the shipped sequence.

*Defect 3* in [`docs/verification.md`](docs/verification.md) carries the detail.

### Round 2 - the plain twins, and an empty file that was shadowing two shipped shaders

`VamShaderProvider` is the one place a material reaches a shader by name, and the gate's report says
which side answered: `project`, `bundle (project defines one)`, or `bundle only (not in project)`. The
hair round's run still read `bundle only (not in project)` for `Custom/Hair/MainSeparateAlphaLayer1`,
and the reason is structural: `DAZHairMesh` skins its meshes on the CPU and draws them with
`Graphics.DrawMesh` under the material it already holds, so a hair material carries the **plain**
`Custom/Hair/*` name and never goes through the `ComputeBuff` lookup at all. The engine data files
carry no `Custom/Hair/*` contract, so nothing in the project could answer that name.

#### What works

- **A second contract source.** `scripts\Extract-VaMShaders.py` also reads
  `VaM_Data\StreamingAssets\z_sha`, the bundle VaM ships its own copies of the shaders in. Contracts
  now come from two places - **135 from the engine data files and 182 from the bundle, 317 distinct
  names, none of them in both** - and `New-VaMShaders.py` reads both directories, first writer winning
  per name.
- **The plain halves.** 15 plain `Custom/Hair/*` and 26 plain `Custom/Subsurface/*` are reconstructed
  from the bundle's own DXBC: `88 shaders, 256 passes, 15 of them tessellated`, against 57 and 168 in
  the round above. Every family the hair and the skin look up is now defined in the project, so
  `bundle only (not in project)` is gone for them. Two `*ComputeBuff` families the earlier run had left
  pending are picked up now that the classifier sees the second source.
- **The five plain `*TessMapped*` twins stay pending, deliberately.** A tessellated pass would have to
  read the compute buffers on its hull and its domain, and only the skinned half of a family has a
  tessellation path here, so `classify()` refuses to shade them from the wrong library rather than
  guessing a vertex stage they do not have.
- **What the census still shows.** The materials themselves are instantiated from the bundle and keep
  its shader object, so the report counts 84 material slots `origin=bundle (project defines one)`, 16
  of them on the character. The plain twins are now defined; redirecting those materials to them is
  the next step, and the skin's `ComputeBuff` swap is the pattern for it. The family the skin actually
  draws with, `Custom/Subsurface/GlossNMTessMappedFixedComputeBuff`, resolves as `origin=project`.

#### Fixed

- **An empty ShaderLab file could shadow a working shader.** `classify()` claims a family from its
  `*ComputeBuff` name, and `Custom/DebugTangentsComputeBuff` and `Custom/DebugUVsComputeBuff` have no
  pass that reads the compute buffers, so `emit_shader()` produced nothing while the run still wrote a
  file with an empty `SubShader { LOD 0 }`. `Shader.Find` prefers the project's own copy, so that file
  shadowed the shipped shader the bundle would have answered with. A contract that emits no pass is now
  left pending, the stale file is deleted, and the run counts the shaders it actually wrote.
- **The `ComputeBuff` suffix could be applied twice.** `FindComputeBuff` appended it unconditionally,
  and a skin re-initialises its materials more than once, so the second pass asked for
  `...ComputeBuffComputeBuff` - a name that exists nowhere. An already-suffixed name is now looked up
  as it is.
- **The unpacked bump normal is normalised before the flat direction is subtracted.** The shipped
  fragment does `normalize(nTS) - nFlat`, the library did `nTS - nFlat`, and the two are not
  equivalent: unpacking leaves the vector longer than unit wherever both slope channels are steep (the
  `z` term is clamped), so skipping the normalise *shortens* the perturbation instead of rotating it -
  a bump seam wherever the map carries a crease, growing with the bumpiness sliders.

#### Measurements re-run

- `python tools\check_shaders.py` - `6805/6805 programs compiled, 0 failed`, 88 shaders, 256 passes,
  15 of them tessellated.
- `python tools\verify_twins.py` - the gate now pairs the sources and knows that 37 of the 55 twin
  families pair a bundle plain half with an engine-data `ComputeBuff` half, where a difference is
  evidence about the two builds rather than a failure. 646 programs classified: `451 identical / 69
  canonical / 84 mask_only / 38 lane_assignment / 2 operand_diff / 2 opcode_diff`, 604 of them proven
  the same instruction stream up to renaming, and the last four classes occur only inside two-build
  pairs. `--pairs-per-family 0` covers 2027 programs and reports the same shape, 1853 of them proven.
- **The alpha cutoff** - 256 emitted passes, 3119 fragment programs disassembled, 0 refusals:
  `157 passes carry a cutoff and every one of their variants discards; 99 carry neither`, and
  `disagreements (cutoff xor discard): 0`.
- `scripts\Invoke-SmokeTest.ps1` on the boot scene - `----- RebuildGate OK -----`, 0 errors before the
  load, 18 of 18 atoms present, the scene loaded in 36.7 s, and **0 `Shader error` lines** in the log.
  48 of the 49 compute-buffer swaps name a project shader; the one that does not is the untranscribed
  `Marmoset/Transparent/Simple Glass/Specular IBLComputeBuff`.

## 0.1.1-alpha - 2026-09-16

Clothing now renders with its own colour, and transparent where the original is transparent. The
cause was ours and not the data's: three lines of the shader generator.

### What works

- **Cloth** - the materials of `Custom/Subsurface/TransparentGlossNMDetailNoCullSeparateAlpha` draw
  like the original's. The generator was decoding the serialized `rtBlend*.colMask` with the bit order
  `(1, R) (2, G) (4, B) (8, A)`, while Unity's `ColorWriteMask` is `Alpha = 1, Blue = 2, Green = 4,
  Red = 8`. The transparent families serialise `14`, i.e. `RGB`, so they were given `ColorMask GBA`
  instead: the pass never wrote red, the colour behind it bled through, and the garment read as
  inverted and transparent. 44 pass states moved from `GBA` to `RGB`
  ([`docs/verification.md`](docs/verification.md), *Defect 4*).

### Known issues

- The same disassembly shows one layer of that family still missing: the shipped program samples
  `_DetailMap` and perturbs the shading normal with it, where our library reads the detail layer from
  `_DecalTex`. It is a bump layer rather than a colour one, so the garments' colour does not depend on
  it.

### Measurements re-run

- `python tools\check_shaders.py` - `3023/3023 programs compiled, 0 failed`, 43 shaders, 113 passes,
  15 of them tessellated.
- `python tools\verify_twins.py --pairs-per-family 0` - 580 pixel passes compared, 0 with different
  operands or opcodes; `python tools\verify_twins.py` samples every family and reports the same 0 on
  its 112 passes.

## 0.1.0-alpha - 2026-09-16

The first public alpha. Virt-a-Mate's code is decompiled, compiles, boots, loads a scene and renders
an animated, lit character. The body skin and the hair read close to the original; the cloth and
part of the character's materials do not, and the recovered code is still the raw decompilation.

### What works

- **Compilation** - `Assembly-CSharp` (2809 files) and `Assembly-UnityScript` build through Unity's
  own compiler: **1421 errors down to 0**, with type parity against the shipped assembly of
  **2753/2753** ([`docs/parity-report.md`](docs/parity-report.md)).
- **Boot** - the game starts, `SuperController` comes up, `isLoading` falls back to `false`, and the
  boot log matches the original game's own `output_log.txt` line for line.
- **Scenes** - `Saves/scene/MeshedVR/default.json`, the scene the original boots into, loads and
  renders. `CyberDemoAlt` loads with all 17 atoms present, 1883 `MonoBehaviour`s and no unresolved
  reference.
- **Character** - the body is skinned on the GPU and follows the scene's 84-bone animation instead
  of hanging in a bind pose, lit by the scene's own lights and IBL: colour space is Linear and the
  IBL globals match the scene JSON.
- **Shaders** - 43 of the 135 shader families the asset contracts name are reconstructed from the
  shipped DXBC: 31 `*ComputeBuff` skinning families and their 12 plain `Custom/Subsurface` twins,
  including the `*TessMapped*` family's hull and domain. Unity compiles **3023/3023 programs**, and
  `tools\verify_twins.py` diffs the 112 pixel passes of the 14 twin families against the shipped
  bytecode: **0 differing**
  ([`docs/shader-reconstruction.md`](docs/shader-reconstruction.md)).
- **Hair** - the scalp and the hair cards sit on the head with the original's colour.
- **Measurement** - a compile gate, a play gate and a manual play launcher, so every claim above is
  reproducible rather than remembered ([`docs/verification.md`](docs/verification.md)).

### What does not work yet

- **Cloth** - clothing renders transparent with its colour inverted; **fixed in 0.1.1-alpha**. The
  family the cloth materials want, `Custom/Subsurface/TransparentGlossNMDetailNoCullSeparateAlpha`, is
  not transcribed, and the `_AlphaTex` of the bundle copy is empty where that family reads its alpha
  from it. *(Both halves of this explanation were refuted by the disassembly: the family does draw
  through our reconstructed twin, and the empty `_AlphaTex` is the original's own state.)*
- **Part of the character's materials** - a gloss and bump seam across the shoulder and the back,
  and the eyelashes and the eye, where only some of the material slots fall back to our
  reconstructions.
- **92 of the 135 shader families** are still not transcribed - the hair set, the Marmoset IBL set
  and the cutout set among them. They are read back out of the shipped bundle at runtime by
  `MeshVR.VamShaderProvider`, so those parts of the scene do draw: they draw the original shader
  rather than ours, and they keep the game installation a runtime dependency.
- **Post-processing** - no bloom, tonemapping, colour grading, eye adaptation or FXAA yet: the
  stage's shaders are stubs and no `PostProcessingBehaviour` is attached to the scene camera.
- **Physics, UI and other scenes** - untested. Testing is done on the boot scene
  (`Saves/scene/MeshedVR/default.json`), because opening several scenes in one session crashes the
  player.
- **Readability** - the code is the decompilation as it came out: obfuscated identifiers, dead
  decompiler artifacts, monolithic classes. The refactoring pass towards readable code has not
  started.

### Known issues

- Opening more than one scene in an editor session crashes the player. Under investigation;
  `Saves/scene/MeshedVR/default.json` is the scene manual testing uses.
- 25 of the 68 `.shader` files in the project are still AssetRipper placeholders that declare no
  properties and transform only the mesh's own vertices.
- The `Marmoset/Specular IBL Soft{,NoCull}ComputeBuff` fallbacks our generated shaders name do not
  exist yet, so Unity logs `fallback shader ... not found` for them.
- Tessellation is D3D11-only: the `HLSLSupport.cginc` branches refuse the tessellation attributes on
  GLES, so the tessellated family's passes are skipped there.

### Requirements

Windows, Unity **2018.1.9f2**, and your own Virt-a-Mate installation - no game content ships with
this repository. See [How to try it](README.md#how-to-try-it) in the README.
