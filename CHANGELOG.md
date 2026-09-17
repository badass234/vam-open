# Changelog

Notable changes to **OpenVaM**, the open rebuild of Virt-a-Mate.

Versions follow [semantic versioning](https://semver.org/spec/v2.0.0.html). The project is in alpha,
so the minor number moves with each round of user-visible work and only the major/minor pair is
meant to be read as stable.

## 0.5.0-alpha - 2026-09-17

The report was that the rebuilt character casts no shadow on itself. The claim turned out to be true
of the port's shading model in a stronger sense than the report implies: `VAM_LIGHT_ATTENUATION` did
not exist, so every point light in the scene went through AutoLight's built-in path, and the round
before this one had measured the body against a shadows-off frame and read 0.6/255 - which is about
what "no shadow term" looks like. VaM's own filter is now transcribed out of the release build's
`MARMO_LINEAR + POINT + SHADOWS_CUBE` fragment program, and a one-frame switch that hands the same
pixel back to Unity measures what it is worth. Reaching that measurement took two mistakes, both kept
below because both were believed first, and one of them was made by the gates.

### Round 7 - VaM's own point-light shadow filter, and the two errors that hid it

#### What works

- **The shipped filter is transcribed instruction for instruction.** `artifacts\_tmp\ship_point.asm`
  is that fragment program, and the shadow half of it is lines 190-246: the cube uv projection, the
  bias applied twice (once along the tap's own direction, once again inside the projection, floored at
  0.005 rather than Unity's 1e-5), the 25-tap Poisson disk, the hash that rotates it, and the mix.
  Two details are deliberately not Unity's, and both are the difference an eye can see:

  | | VaM | Unity's built-in path |
  | --- | --- | --- |
  | what `_LightShadowData.x` does | sizes the disk, `(1 - x) * 0.1` | is the light's `shadowStrength` |
  | what a tap contributes | averaged, `* 0.04` | lerped toward 1 by the shadow |
  | consequence | a blurred edge is as dark as a hard one | a blurred edge is lighter |

  The rest of `_LightShadowData` is AutoLight's own realtime-to-baked fade, so the port keeps
  `UnityMixRealtimeAndBakedShadows` instead of reimplementing it.
- **The disk is the shipped disk, verified rather than eyeballed.** The 25 taps in
  `shader-src\VamGpuSkinning.cginc` are copied out of the blob's `dcl_immediateConstantBuffer`, and a
  script compared all 25 pairs: **zero mismatches**.
- **The control that proves authorship is a plain uniform, not a keyword.** `_VamShadowUnity` is
  declared in the include and arrives through `Shader.SetGlobalFloat`; it is listed in no `Properties`
  block and so adds no `shader_feature`, because a feature would double an already large variant set
  to answer a question that lasts one frame. With it set, `VamPointShadowAttenuation` returns
  `UnityComputeForwardShadows` and the same session draws Unity's filter.
- **On lit body pixels the two filters are not the same picture.** The reference is the shadows-off
  frame, the mask is every reference pixel above luma 32 (73 494 px), the delta is signed so positive
  means darker, and both frames come from one build, one session and one camera:

  | measurement, framed on the body | VaM's filter | Unity's filter |
  | --- | --- | --- |
  | mean delta on the lit mask | **+8.26/255** | +0.98/255 |
  | median / p90 / p99 / max | +2.43 / +18.89 / +121.55 / +206.35 | +0.07 / +1.00 / +18.91 / +138.00 |
  | share of the mask darker by more than 2 / 10 | 52.3 % / 20.8 % | 6.4 % / 1.9 % |
  | pixels darker by more than 40 | **2 196** | 240 |
  | 8x8 cell means, range | +0.00 ~ +30.36 | +0.00 ~ +6.30 |

  So the darkening is 8.4x the mean and 9x the deep pixels of the built-in path, and its cells are
  structured the way a contact shadow is - the deep ones sit where the body meets itself - rather than
  the way an exposure change is. Under the installation's own `shadowStrength` of 0.10, the built-in
  path had almost nothing to show, which is why the report was simply "there is no self-shadowing".
  Re-running the pair on regenerated builds reproduces the mean to a few hundredths (+8.26, +8.26,
  +8.23 over three runs against +0.98, +0.97, +0.96) and the cell range to a few tenths, but the counts
  drift by about a percent: the reference frame is not bit-identical run to run either, which is what
  the drifting mask size says (73 494, 73 435, 73 413 px) before any deep-pixel count is taken. So
  across runs compare the mean, and read the counts as the run in front of you.
- **The frame taken after the switch is put back is identical to the session frame** (max
  |difference| 0), so the pair measures the filter and nothing else.
- **One substitution, and it is named.** The shipped RNG begins by sampling a full-screen texture at
  the screen-space uv, and that texture could not be identified: no ShaderLab text survives in the
  bundle and no VaM script binds it. The port derives the seed chain from the pixel coordinate
  instead, keeping the blob's own `m`, `sincos` and `frc` sequence, constants and order.
  `AutoLight.cginc` is what makes the coordinate free: for a cube-shadow point light it declares
  `unityShadowCoord3` holding `worldPos - _LightPositionRange.xyz` and leaves the slot it uses for
  screen/depth shadows alone, so `i.pos` - the pixel coordinate - can carry the seed without stealing a
  varying.

#### The two errors that stood in the way, kept because both were believed first

- **A statistic taken over the wrong pixels.** The 0.6/255 of the previous round came from averaging
  the delta over the *whole frame*, dark background included, which dilutes a real contact shadow into
  nothing. The question needs the statistic taken only where there was light to lose:
  `tools\measure_shadow.py` masks on the reference's lit pixels, reports the signed delta there and an
  8x8 cell breakdown, and writes a composite for the eye.
- **A gate measuring a generated copy.** `VaM_Rebuild\Assets\VaMShaders\VamGpuSkinning.cginc` and
  `VaM_Rebuild\Assets\Shader\*.shader` are **generated** by `scripts\New-VaMShaders.py` and are not
  tracked; `shader-src\VamGpuSkinning.cginc` is the source of truth. A round that edits the include and
  then runs the compile and play gates without running the generator **measures the previous round's
  shaders**, and both gates are green while it does. That is what the control frame's bit-identical
  result meant: the switch was not in the build, not that the filter was absent.
  `scripts\New-VaMShaders.py` is now step 0 in the gate sequence `plan.md` documents, and the README
  already listed it.

#### Measurements re-run

- `python scripts\New-VaMShaders.py`, then `python tools\check_shaders.py` -
  **7479/7479 programs compiled, 0 failed**.
- `scripts\Invoke-CompileGate.ps1` - verdict `OK`, **0 unique errors**,
  `Assembly-CSharp.dll` 6 172 160 B, `VaMUnityScript.dll` 16 896 B.
- `scripts\Invoke-SmokeTest.ps1 -Method Play -Seconds 60 -WarmupSeconds 20` on
  `Saves/scene/MeshedVR/default.json` - 26 877-line log, report at `artifacts\play.report.txt`, all 30
  shadow captures written, and no `Shader error`, no `Shader warning` and no `error CS` anywhere in it.
- `tools\measure_shadow.py` on the frame pairs above, for every number in this section.
- `scripts\Invoke-ManualPlay.ps1` on `Saves/scene/MeshedVR/default.json` - the hand run that closes the
  report: the character darkens itself where it meets itself, at about 100 FPS, with no shader or
  compile error in the log.

#### Known issues

- **The filter is proven, not tuned.** Whether its amount matches the installation's look is a hand-run
  question - the scene has three point lights at `shadowStrength` 0.10 - and the deepest pixels
  (p99 121/255 on 3.0 % of the lit body) are where the eye should look first, not the mean. The hand
  run of 2026-09-17 looked there and read the amount as right; what no run can turn into a number is
  the comparison itself, because the repository holds no reference frames from the installation.
- **A reflection-level comparison of the two filters can never be pixel-exact**, because of the RNG
  substitution above. The comparison this round makes is of the darkening they produce, which is
  measurable.
- The gloss/bump seam and the sheen are untouched by this round, and a hand run taken before it is no
  longer a baseline for one taken after: the light term moved.
- `_VamShadowUnity` is a global and is therefore live for every shader that includes the library. It is
  set and restored inside one probe and left at 0 in every other frame, but it is a switch that can be
  left on, so the probe owns both ends.

#### Not done

- The remaining **Marmoset IBL families** - the last entry in `UNTRANSCRIBED_FAMILIES`, and the only
  thing left between the shipped materials and the project's shaders.
- A side-by-side against the installation, which is a human task: the repository holds no reference
  frames from it, so the comparison stays where the reference is. The instrument measurements cover
  what can be covered from inside one build - two settings of one switch, one session, one camera.

## 0.4.0-alpha - 2026-09-17

Every round so far has judged a picture taken in this project against a picture taken in the
installation, and this one found that the two were never rendered under the same settings. The fix is
five keys in one file and a gate that says which file it read, and it takes the sheen that the two
rounds before it had put down to the quality preset and hands most of it back to the shader and to
what is bound to it.

### Round 6 - the same preset on both sides, and the part of the sheen the preset does not explain

#### What works

- **The gate reports the preset it ran at.** `RebuildGate.PresetDump`, called from `EnvironmentDump`
  and therefore present in every play report, prints the resolved `QualitySettings` level with its
  `antiAliasing` and `pixelLightCount`, the `UserPreferences.singleton` values, and the graphics keys
  of the `prefs.json` in the working directory. That last one is read as raw text rather than through
  SimpleJSON, because the half that could disagree is the file itself:
  `UserPreferences.RestorePreferences` reads `prefs.json` relative to the process working directory,
  so the editor reads the project's copy and a built player reads its own, and a key that is missing
  or misspelled falls back to the code's default in silence.
- **`scripts\New-RuntimeDataLinks.ps1` matches the editor's graphics keys to the installation's** and
  prints each one. Nine keys are compared - the five a `QualityLevel` is made of, plus `shaderLOD`,
  `mirrorReflections`, `realtimeReflectionProbes` and `softBodyPhysics` - and the ones that differ are
  rewritten in place, so the project's `prefs.json` keeps its shape and keeps its input, HUD and
  plugin settings. A second run prints `the editor already runs the installation preset`.
  `-MatchGraphicsPrefs:$false` reports the differences without applying them. On this machine it
  matched **5 of 9**:

  | key | installation | editor project, before | editor project, after |
  | --- | --- | --- | --- |
  | `renderScale` | 1 | 2 | 1 |
  | `msaaLevel` | 4 | 8 | 4 |
  | `pixelLightCount` | 2 | 4 | 2 |
  | `smoothPasses` | 2 | 4 | 2 |
  | `glowEffects` | Low | High | Low |

- **The two presets were then rendered side by side, and they are nearly the same picture.** The same
  gate, the same scene (`Saves/scene/MeshedVR/default.json`), the same 40 s and the same camera,
  once at the installation preset and once at the project's old one:

  | measurement, framed on the body | installation preset | project preset, before |
  | --- | --- | --- |
  | mean luminance of the frame | 0.1447 | 0.1450 |
  | share of the frame above 0.90 luminance | 1.471 % | 1.532 % |
  | mean luminance over the lit skin | 0.7106 | 0.7144 (**+0.54 %**) |
  | lit skin above 0.75 luminance | 45.81 % | **47.25 %** |
  | pixels differing by more than 24/255 | 0.61 % of the frame, every one of them inside the body's bounding box | |

  So the preset moves the surface in the direction the hand run reports - a flatter skin under
  `smoothPasses` 4 and more per-pixel lights under `pixelLightCount` 4 - and it moves it by about
  half a percent of mean luminance and 1.4 points of highlight coverage. Round 5 called the sheen *at
  least partly* the preset and left the rest open; the size of that part is now known, and it is
  small. The three candidates Round 4 listed as living outside the shader - the specular cube's
  import colour space, the material's own intensities, and the bloom thresholds - are still the main
  ones, and the two settings are still beside them.
- **The capture has a known blind spot, and it is the resolution.** `RebuildGate.RenderToFile` renders
  the camera into a fixed 1024x1024 temporary target with no anti-aliasing, so the two keys that
  describe the resolution of a *view* rather than of a surface - `renderScale` and `msaaLevel` -
  cannot appear in a framed capture at all. Three of the five keys, `pixelLightCount`, `smoothPasses`
  and `glowEffects`, are the ones the comparison above actually exercised.

#### Measurements re-run

- `scripts\New-RuntimeDataLinks.ps1` - 5 of 9 graphics keys matched on the first run, and the second
  run reports the editor already at the installation preset, with the installation's own `prefs.json`
  untouched.
- `scripts\Invoke-SmokeTest.ps1 -Method Play -Seconds 40 -Scene Saves/scene/MeshedVR/default.json`,
  twice, once per preset - both runs `18 declared, 18 present, 0 missing` atoms and no error before
  the load, and both now print the `preset:` block that says which of the two they were.

#### Known issues

- **The sheen is still diagnosed, not fixed**, and the round moved it rather than closing it. The
  preset is now equal on both sides and the gate states it, so the next comparison is fair - but the
  preset is not most of what the hand run sees, and the suspects are back where they were: the
  specular cube's import colour space, the material's specular and fresnel intensities, the bloom
  threshold, and the reconstructed gloss and Fresnel terms themselves.
- The gloss/bump seam the hand run still reports at very low visibility has not been re-examined at
  the lower preset either.
- The hand run at the matched preset has not happened yet; this round measured with the gate's own
  camera, not with the eye that reported the defect.

#### Not done

- The player's `prefs.json` is seeded from the installation by
  `scripts\New-PlayerRuntimeLinks.ps1` and is not matched key by key the way the editor's now is, so a
  player built from a project file that has drifted would drift with it.
- No release page is published for this or the previous round.

## 0.3.0-alpha - 2026-09-17

The first round that produces something other than an editor project: `VAMOpen.exe` builds in batch
mode and starts on its own. Building a player is not the same as running one, and the difference is
where the round's second subject came from - the player scanned nine of the eighteen packages the
editor scans for the same project, and settling that turned up the answer to the sheen that the three
rounds before this one had been circling.

### Round 5 - the standalone player, and a sheen that is a settings difference

#### What works

- **The player builds in batch mode.** `RebuildPlayer.Build` is the entry point and
  `scripts\Invoke-PlayerBuild.ps1` is the driver: `-batchmode -nographics -quit -executeMethod`,
  output to `artifacts\player`, verdict from the `----- RebuildPlayer` markers rather than from the
  exit code, because Unity returns the same code for a failed build and for an editor that never
  reached the method. A build that fails on an `UnityEditor` reference the editor compiles perfectly
  well is the one class of error this adds to the gate's, and the marker is what reports it.
- **The player runs.** Ten scenes, `VAMOpen_Data`, 258 972 055 B of output, and a window titled
  `VaM`; its log is at `%USERPROFILE%\AppData\LocalLow\MeshedVR\VaM\output_log.txt`, not next to the
  exe. The log shows the asset manager ready, D3D11 on the RX 9070 XT, and a 312 FPS benchmark.
- **The build deliberately does not carry the game's data.** The player resolves all of it relative to
  its own directory - the game computes its root as the parent of `Application.dataPath`, and the
  bundles come from `Application.streamingAssetsPath`, which is `<exe>_Data\StreamingAssets` - so
  `scripts\New-PlayerRuntimeLinks.ps1` links `AddonPackages` and `Custom` in (junctions: 0.84 GB and
  1.1 GB that a copy would only let drift out of date), copies `Saves`,
  `AddonPackagesUserPrefs` and `prefs.json` so that a run of this build cannot write into the
  installation it borrowed them from, and refreshes `Keys`.
- **The nine-against-eighteen packages are the key file.** The player's log read
  `Scanned 9 packages in 60.8 ms` where the editor reads `Scanned 18 packages in 158.0 ms` for the
  same `AddonPackages`, and 18 `.var` files are what is actually in there. `FileManager.Refresh`
  logs `packagesByUid.Count`, and the count follows the key: `SuperController` reads
  `keyFilePath` (`Keys/1.21/key.json`) relative to the working directory, and with no valid key it
  fills in the restricted package set. Copying `Keys` into the player folder is the whole fix - the
  same build then logs `Scanned 18 packages in 68.7 ms` - and the script now refuses to leave the
  file out silently.
- **The sheen is at least partly the quality preset, and the numbers are in two files.** The editor
  project's `prefs.json` and the installation's hold the same settings, and the graphics ones among
  them are not the same preset. The installation's five are the **High** preset of
  `UserPreferences.QualityLevels` to the digit; the editor's are **Max**, with `msaaLevel` raised by
  hand from the preset's 2 to 8:

  | key | installation (High) | editor project (Max) | what it does in the code |
  | --- | --- | --- | --- |
  | `renderScale` | 1 | 2 | the internal render target's resolution |
  | `msaaLevel` | 4 | 8 | `QualitySettings.antiAliasing` (`UserPreferences.cs:2886`) |
  | `pixelLightCount` | 2 | 4 | `QualitySettings.pixelLightCount` (`UserPreferences.cs:3020`) |
  | `smoothPasses` | 2 | 4 | `DAZSkinV2.smoothOuterLoops`, the Laplacian smoothing iterations on the skin mesh (`DAZSkinV2.cs:2612`) |
  | `glowEffects` | Low | High | `MKGlow.Samples`, 3 against 2 (`UserPreferences.cs:3361`) |

  Two of those move a character in the "smoother and lit harder" direction, which is what an oiled
  look is: `pixelLightCount` is Unity's per-pixel light budget, so a light that was outside it
  contributes no per-pixel highlight at 2 and does at 4, and `smoothPasses` is how many times the
  skin's vertices are Laplacian smoothed before the normals are rebuilt, so 4 passes leave a flatter
  and more mirror-like surface than 2. Both are the game's own quality settings applied by the
  game's own code; neither is this project's shading.
- **The glow level is not an on-off switch, which is what makes it a weak suspect.** `SyncGlow`
  enables every registered `MKGlow` whenever `glowObjectCount > 0` and the level is not `Off`, and
  `Low` and `High` differ only in `Samples` - 3 against 2. The installation is not running with its
  bloom off.

#### Measurements re-run

- `scripts\Invoke-PlayerBuild.ps1` - `----- RebuildPlayer OK -----`, 258 972 055 B, 10 scenes, 0
  `error CS`, 0 `Shader error`.
- The built player's own log - asset manager ready, `Refresh Handlers took 0.6 ms`, packages
  scanned, `Benchmark complete. Avg. FPS: 312.18`, D3D11.
- `scripts\New-PlayerRuntimeLinks.ps1` - 18 `.var` packages visible, the manifest bundle present, the
  key file in place.

#### Known issues

- **The sheen is diagnosed, not fixed.** The two `prefs.json` files say the editor sessions that
  produced the report were running at a higher preset than the installation the report was compared
  against, and until the same preset is run on both sides, whatever is left over cannot be attributed
  to the shader. The three candidates Round 4 listed as living outside the shader - the specular
  cube's import colour space, the material's own intensities, and the bloom thresholds - all stay
  open, and two settings are now joined to them.
- The gloss/bump seam the hand run still reports at very low visibility has not been re-examined at
  the lower preset either.

#### Not done

- The player is not part of a single entry point: `scripts\Setup-RebuildProject.ps1` builds the
  project, and the player build is a separate script that has to be called on purpose.
- No release page is published for this or the previous round.

## 0.2.0-alpha - 2026-09-17

Four rounds are in this release. The first transcribed the hair, and the whole hair family now renders
from this project's code. The second reconstructed the plain twins - the half of every family that no
`ComputeBuff` lookup reaches - and closed a defect that was silently disabling two of the game's own
shaders. The third makes the game's own materials use those twins instead of the shipped bundle's copy.
The fourth reads the body shader back out of the shipped bytecode instruction by instruction and finds
the one place the reconstruction had been shading the frame differently.

The hair is the body's shading model with fewer inputs, and one pass of every hair family does not
shade at all: it writes black and keeps the texel's alpha. The eye and the lashes had already started
drawing with the original's own masks; the hair carries the same kind of mask pass, and it is now read
from each pass's own fragment rather than from a name table.

**This is the release that closes the "basic functionality" milestone**: the boot scene loads, the
character, the skin, the hair, the clothing and the eyes draw from this project's own code, and the
shading model of the drawn body has been verified against the shipped programs rather than against a
reading of them. What is deliberately left for later is named under *Known issues*.

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

### Round 3 - the game's own materials move onto the project's shaders

Round 2 left the plain twins defined and unused: a material instantiated from the shipped bundle keeps
the bundle's shader object, so the reconstruction was compiled and never asked for. This round adds the
redirection, and then measures what it moved rather than what it could move.

#### What works

- **`VamShaderProvider` can redirect a material.** `UseProjectShader(Material)` replaces a material's
  shader with the project's own copy of the same name and touches nothing else - properties, maps and
  per-renderer bindings stay - and `UseProjectShaders(Material[])`, `UseProjectShaders(GameObject)` and
  `UseProjectShadersEverywhere()` walk the collections the draw paths actually use.
- **A gate that keeps it off the placeholders.** `New-VaMShaders.py` emits a third artefact,
  `MeshVR\VamProjectShaders.cs` - the 88 names the run wrote, plus `Defines(name)` - generated from the
  shaders that were produced rather than from the contracts that were found. Without it the redirect
  would move a material onto an AssetRipper placeholder whenever a family is not reconstructed, and one
  of those is the hair's optimised path (`GPUTools/MeshedVR/HairOpt`), whose naive takeover draws *less*
  than the shipped shader it stands in for. Verified: none of the 88 written names collides with the
  25 placeholders.
- **Six call sites, at the moments the materials appear.** `SuperController` sweeps every live scene
  renderer when the load coroutine returns and again per atom in `AddAtom`; `DAZSkinV2` sweeps the
  `GPUmaterials` and the simple material it is about to draw with, in `SkinMeshGPUMaterialInit` and in
  `CopyMaterials`, because `Awake` runs before the renderer's materials are handed over; `DAZSkinWrap`
  does the same in its own `CopyMaterials` and swap; `DAZHairMesh` redirects `hairMaterial` before it
  copies it into `hairMaterialRuntime`, which is the material its `Graphics.DrawMesh` draws with and the
  one path that never goes through a renderer slot at all.
- **Measured**: the census's `material(s) on a bundle copy` fell from 84 to 76, and the eight that
  moved are the interaction rig's hand meshes (`Hands_Mat_01_MVR`, `Hands_Mat_02_MVR`), the equipped
  clothing's simulation meshes (`hu_pty_body-1`, `hu_skt_body1/2-1`, `hu_skt_metal-1`, `hu_skt_str-1`)
  and the hair tools (`HairTool`) - `Custom/Subsurface/GlossNMCull`, `TransparentSeparateAlpha`,
  `TransparentGlossNMNoCullSeparateAlpha` and `TransparentGlossNMDetailNoCullSeparateAlpha` now draw
  their original `*ComputeBuff` twin's plain half rather than the bundle's copy of it.
- **The census stopped calling a built-in a family the project defines.** `ShaderOrigin` asked
  `ProjectDefinesShader`, which accepts anything `Shader.Find` answers: Unity's own `Standard`,
  `Unlit/Texture` and `GPUTools/Painter`, and every placeholder. 36 materials were counted as
  take-overs waiting to happen; they are now reported as `bundle (only a stub or a built-in answers)`,
  which is exactly the set the redirection has to leave alone.
- **A new line accounts for what is left on a skin.** `skins still holding a bundle shader` reports
  each `DAZSkinV2` with its `inHierarchy` and its off-bundle ratio.

#### Measurements re-run

- `scripts\Invoke-SmokeTest.ps1` on the boot scene - `----- RebuildGate OK -----`, 0 errors before the
  load, `scene atoms: 18 declared, 18 present, 0 missing`, and the frame is the same frame: 118
  renderers (14 disabled), 154 material slots, 120 distinct materials, 31 distinct shaders. 0 materials
  with an unsupported shader, 0 on a stub, 0 `Shader error` lines, and the log's 12 distinct exception
  lines are the pre-existing `MacGruber.Breathing` plugin ones - the same 12 as the round before.
- The character is unchanged, and the new line explains it: `character materials: 49 drawn, 0 on an
  unsupported shader, 16 on a bundle copy, 33 on a project shader`, where the 16 are **not** 16 drawn
  materials. They are the 19 `GPUmaterials` of the unequipped `VictoriaElitePonytailHair*` template -
  `active=True inHierarchy=False`, so `Awake` never ran and `SkinMeshGPUMaterialInit` never built them
  - plus the live skin's single `EyeReflection-1`, which
  `Marmoset/Transparent/Simple Glass/Specular IBLComputeBuff` leaves with nothing to redirect to. No
  drawn material of the character still uses a bundle copy of a family this project defines.

#### Not done

- The redirection is name-based and deliberately changes no drawn family's program, so it cannot be
  judged by eye: the hands and the hair tools are the only visible parts that changed owner, and both
  draw through twins that `tools\verify_twins.py` already pairs. A visible run on the boot scene is
  still the missing check before defect 1's height-dependent brightness is re-measured.
- 21 materials are still `bundle only (not in project)`: the untranscribed `Marmoset/` set, the
  overlay helpers and `Unlit/UnlitOverlayShader`.

### Round 4 - the base pass, read back out of the shipped bytecode

The three rounds before this one all ended the same way: a hand run, a chart of what looked right, and
one complaint left over. The complaint is that the characters shine slightly more than the original
does. That is a *shading* difference, and the shading had until now been ported by reading the
disassembly, not by comparing against it - so this round disassembles the drawn body's own programs,
compiles this project's HLSL for the same sequences, and puts the two instruction streams side by side
until the divergence is either located or excluded.

#### What works

- **The base pass is compared instruction by instruction, and agrees everywhere but one line.** The
  shipped `..._064` blob (`DIRECTIONAL-MARMO_LINEAR`, the ForwardBase variant of
  `Custom/Subsurface/GlossNMTessMappedFixedComputeBuff`) was disassembled with `fxc`, this project's
  HLSL was compiled to the same profile with the same include path, and the two were walked together:
  the tangent frame, the normal-map decode, the albedo override blend, the `_DiffOffset`/`_SpecOffset`/
  `_GlossOffset` adds, the gloss mip and exponent curve (`0.159155 * exp(x) + 0.318310`), the Fresnel
  curve, the `_IBLFilter` split, the L1 and L2 SH ambient, the subdermis warp and the shadow
  coordinates are the same arithmetic in the same order.
- **One divergence found: `_ExposureIBL.w` is applied to the indirect half only, and ours was applied
  to the sum.** The shipped blob's last arithmetic instruction is
  `mad o0.xyz, r2.xyzx, cb0[74].wwww, r0.xyzx` - the accumulated ambient and reflection times the
  master exposure, then the direct term added untouched. We multiplied the joined result, which is
  only equal while the exposure is one. The census reads the drawn scene's global as
  `_ExposureIBL = (0.100, 0.100, 1.000, 1.000)`, so today's frame does not change: the fix removes a
  hidden dependency on a scene value rather than a visible error.
- **The one instruction we omit is now proved to be dead rather than assumed to be.** The shipped
  fragment adds `r3.xyz * v7.xyz` into the colour before that exposure line. The source comment
  claimed the interpolator "hard-codes to zero"; it does not, and that claim was re-verified from the
  vertex program: `vs_000.asm` ends `mov o4.x, l(0)`, the domain shader emits that same control point
  as `and o7.xyz, r0.xyzx, vicp[0][4].xxxx`, and the bitwise and against the zeroed mask makes
  `v7.xyz` exactly zero for every vertex the tessellated pipeline produces. The term contributes
  nothing, and the comment now says how that was established.
- **The additive pass is verified equivalent too, and its blob is a lit one.** The body's forward-add
  program (`..._268`) is a separate DXBC blob for point and spot lights, and our additive pass was
  derived from the base pass rather than transcribed. It re-derives the whole surface - albedo, the
  override blend `albedo * (1 - a) + texel.rgb * a` on the same `_SpecTex.w`, the tangent frame, the
  specular map with `_SpecOffset`, the gloss map with `_GlossOffset` - and then has nothing but the
  diffuse and highlight lobes: no cube, no SH, no ambient. Ours does the same, and the exposure it
  applies to both of its lobes on the way out is arithmetically the same place. The pass also skips
  `_ExposureIBL.w` for the *sum* in the shipped program (two separate multiplies), so the additive
  side needed no change.
- **The comments now carry the evidence.** Every deviation listed at the top of
  `shader-src\VamGpuSkinning.cginc` names the instruction it was decided from, so the next reader can
  start from a proof instead of from the previous reader's confidence.

#### Measurements re-run

- `python tools\check_shaders.py` - `6805/6805 programs compiled, 0 failed`, 88 shaders, 168 passes,
  15 of them tessellated, unchanged by this round except in the composite it compiles.
- `scripts\Invoke-CompileGate.ps1` - `verdict: OK`, 0 unique errors, `Assembly-CSharp.dll`
  6 172 160 B, `VaMUnityScript.dll` 16 896 B.

#### Known issues - what the gloss difference is *not*

The reported over-gloss is **not** a wrong term in the fragment program. The base pass, the additive
pass, the ambient, the reflection, the Fresnel curve, the highlight exponent and the composite have all
now been compared against the shipped bytecode and agree, and the two candidate arithmetic differences
that were left (`_ExposureIBL.w` and the masked `v7` term) are both either inert or dead. What is left
lives outside the shader, and the next round starts from this list:

- the specular IBL cube's *import* colour space. `_SpecCubeIBL` is a cube VaM builds at load
  (`Sky.SpecularCube`), and the two cubes we import (`museum_SPEC`, `untitled_SPEC1`) are pulled in as
  sRGB with trilinear filtering while the two fallbacks (`cube`, `blackCube`) are sRGB with bilinear.
  `CubeBuffer`/`SkyProbe` do their own gamma, so which of the two conversions the original applied to
  the stored texels has to be settled before the cube can be blamed or cleared.
- the material's own `_SpecInt`/`_Shininess`/`_Fresnel` values as the game pushes them, against what
  the material asset holds - the shader reads the slots correctly, which says nothing about the numbers
  in them.
- bloom: `BloomComponent` thresholds in linear space, so a threshold that survives the rebuild
  differently would read as a sheen over every character at once.

#### Not done

- No visible change is claimed for this round. The character's shading is *proved* to match the shipped
  programs, which is the precondition for the next round's search to skip the shader entirely.

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
