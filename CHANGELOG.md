# Changelog

Notable changes to **OpenVaM**, the open rebuild of Virt-a-Mate.

Versions follow [semantic versioning](https://semver.org/spec/v2.0.0.html). The project is in alpha,
so the minor number moves with each round of user-visible work and only the major/minor pair is
meant to be read as stable.

## Unreleased

The eye and the lashes now draw with the original's own masks. Both were the shared include shading a
surface the original had a small dedicated program for.

### What works

- **Lashes** - the cards render as strands instead of solid dark planes. The alpha was built from
  `_MainTex.a`, which these materials stub to white, and the mask was added on top of it, so
  `_AlphaTex` could never win; behind that the mask was read from `.r` instead of `.a` and sampled on
  the diffuse map's UV set. The shipped fragment is `saturate(_AlphaTex.a + _AlphaAdjust)` on the
  mask's own `_AlphaTex_ST`, with the colour premultiplied before the cutoff - and all 27 families
  that declare `_AlphaTex` read `.a` with `_AlphaAdjust` in an add, so it is family behaviour rather
  than a lash quirk.
- **Eyes** - `Custom/Subsurface/AlphaMaskComputeBuff` no longer shades. Its shipped fragment is three
  instructions: sample `_MainTex`, `mad_sat` the alpha against `_Color.a` and `_AlphaAdjust`, write
  black to RGB. Ours ran the full shading model through the first pass's `SrcAlpha` blend and then
  added a second lit colour under `One One`, which is why the eye read over-bright. Nothing in the
  contract distinguishes the family - its three properties are as few as a diffuse IBL family's, and
  its render state is an ordinary transparent pair - so the entry point is chosen by name, as the
  cutoffs are.
- **Clipping** - which passes discard is now read from the shipped programs instead of the render
  state. The old zWrite-and-opaque rule had no false positives but missed 10 passes, one of them the
  lash's own base pass; widening it leaves 4, and the states that differ only in the original source
  are named in the generator.

### Measurements re-run

- `python tools\check_shaders.py` - `3003/3003 programs compiled, 0 failed`, 43 shaders, 113 passes,
  15 of them tessellated. That is 20 below 0.1.1-alpha's number because the eye family's two passes
  are no longer compiled as the shared fragment as well as their own entry point.
- `python tools\verify_twins.py` - 112 pixel passes of the 14 twin families, all the same instruction
  stream up to renaming, 0 differing (`python tools\verify_twins.py --pairs-per-family 0` covers 580).

### Verified by hand

- The lashes draw as strands rather than solid dark cards, in the editor's play mode on the game's
  own boot scene (`Saves/scene/MeshedVR/default.json`, `scripts\Invoke-ManualPlay.ps1`). The eye's
  change is proven at the instruction level rather than by eye: both of its passes disassemble to
  the shipped sequence.

*Defect 3* in [`docs/verification.md`](docs/verification.md) carries the detail.

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
