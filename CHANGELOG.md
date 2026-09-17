# Changelog

Notable changes to **OpenVaM**, the open rebuild of Virt-a-Mate. The project is in alpha, so the version's
last number counts patches inside the `0.1` line while the first two stand still. Every number below is
reproduced by a gate or an instrument; the measurements behind them are in [`docs/`](docs/).

## 0.1.7-alpha

Everything since the first public alpha: the boot scene loads, and the character, its skin, hair, clothing and
eyes draw from this project's own code.

- **Clothing renders with its own colour, and transparent where the original is transparent.** The cause was
  the generator, not the data: it decoded `rtBlend*.colMask` with the bit order `(1, R) (2, G) (4, B) (8, A)`
  while Unity's `ColorWriteMask` is `Alpha = 1, Blue = 2, Green = 4, Red = 8`, so the transparent families'
  `14` became `ColorMask GBA` and the pass never wrote red. 44 pass states moved from `GBA` to `RGB`, gate
  3023/3023 programs.
- **Hair, lashes and the eye.** `Custom/Hair/*ComputeBuff` is transcribed (+14 shaders, +55 passes); pass 0 of
  `Main*` **writes black and keeps the alpha**, the lashes are `saturate(_AlphaTex.a + _AlphaAdjust)`, and the
  clipping and the gate come from the shipped programs: 4414/4414. The plain twins needed a second path -
  `DAZHairMesh` draws through `Graphics.DrawMesh` and never reaches the `ComputeBuff` lookup - and their
  contracts come from `z_sha` (317 names, none in both): 88 shaders, 256 passes, 6805/6805.
- **Materials draw with this project's shaders.** `VamShaderProvider.UseProjectShader` moved the character's
  materials off the bundle copies - those on a bundle copy fell from 84 to 76 - a shader looked up by name no
  longer answers with an AssetRipper placeholder where there is no transcription, and `VamSkin()` no longer
  orthogonalises the tangent, which the shipped programs never rotate.
- **The base pass, read back out of the shipped bytecode.** `DIRECTIONAL-MARMO_LINEAR` disassembled with `fxc`
  and walked against our compile: same arithmetic, same order, one divergence equal only at exposure 1.000, one
  dead instruction. Left outside: the specular IBL cube's import colour space, the `_SpecInt`/`_Shininess`/
  `_Fresnel` the game pushes against what the material holds, and the bloom threshold.
- **A standalone `VAMOpen.exe` builds in batch mode** (`RebuildPlayer.Build`), its verdict read from the
  `----- RebuildPlayer` markers rather than the exit code: ten scenes, 312 FPS. The missing packages were a
  missing key file - `Keys/1.21/key.json` - and with it the same build logs `Scanned 18 packages` instead of 9.
- **The editor and the installation had never rendered under the same settings.** `RebuildGate.PresetDump`
  prints the resolved quality level and the working directory's `prefs.json` graphics keys, and
  `scripts\New-RuntimeDataLinks.ps1` matched **5 of 9**: the installation runs the **High** preset of
  `UserPreferences.QualityLevels` to the digit while the editor ran **Max** with `msaaLevel` raised to 8. With
  them equal the two pictures measure 0.7144 against 0.7106 of mean lit-skin luminance (**+0.54 %**), 0.61 % of
  the frame differing by more than 24/255.
- **Self-shadowing: VaM's own point-light shadow filter, decoded from the released bytecode.**
  `VAM_LIGHT_ATTENUATION` now comes from the release build's `MARMO_LINEAR + POINT + SHADOWS_CUBE` - cube uv
  projection, the 25-tap Poisson disk, its taps averaged rather than lerped toward 1. At `shadowStrength` 0.10
  it darkens the lit body by **+8.26/255** of mean luminance, against the built-in path's +0.98.
- **Scene previews in the built player.** The lost previews were one wrong file: Mono 2.0's `System.Drawing.dll`
  (448 512 B) staged into `Managed\`, where its marshaller cannot start, so 469 previews threw; the `unityjit`
  build (483 840 B) is staged instead and the gate decodes a preview through it. In a fresh player 157 scenes
  each resolve to a decoded 512x512 `.jpg` (`uFileBrowser.ThumbnailDiagnostics`, dormant without
  `-vamopen-diag`).
- **A plugin the installation cannot run flooded the log**: 17 194 `NullReferenceException` lines, 8 595 each
  from `MacGruber.Breathing.Update` and `DriverBreathing.Update`. `MVRPluginManager` now destroys the
  half-built component, switches the plugin off and shows its first failure as a dialog, so that boot logs 12
  exception lines, none from `Update`.
- **The harness was attacking the workspace.** Setup deleted the gate apparatus with `VaM_Rebuild\`, recursed
  through the `StreamingAssets` junction into 16.47 GB of bundles (PowerShell 5.1 deletes files *behind* a
  junction) and made a stderr warning fatal; `RebuildPlayer` deleted its output through the same link, and setup
  wiped the `AddonPackages`/`Custom` junctions so the editor scanned 0 packages in silence, and a gate ran
  green on untracked generated shaders. Junctions unlink with `RemoveDirectory` now.
- **An in-game screen resolution setting.** `InitScreenResolutionUI` clones the `Physics Update Cap Popup` row
  at runtime, because the panels live in bundles and not here: `Screen.resolutions` above a 640x480 floor,
  within 0.02 of the display's aspect, six entries plus the current mode, hidden in VR. The value is
  `prefs.json`'s `screenResolution`, applied at boot only when the display still reports that mode; the popup
  is `AlertUI` with Keep/Revert and a ten-second countdown that replays the previous mode through
  `ApplyScreenResolution(..., prompt: false)`.
- **The project had been compiling a copy of the sources, and no gate could see it.** `src\` is where this
  project is edited (129 969 B `UserPreferences.cs`), `VaM_Rebuild\Assets\Scripts\` is what Unity compiles
  (118 911 B, **0** hits for `screenResolution`), and the compile gate answered `verdict: OK` about the unedited
  tree. `scripts\Sync-Sources.ps1` now mirrors the `*.cs` files, byte-verified, before every gate, and reports a
  stale assembly instead of trusting it.
- **The panel names the build**: it reads `VaMOpen 0.1.7-alpha (content version: 1.22.0.13)`, the first half
  from `VaMOpenBuild.Version` and the second from the installation's own `version` file. The compile gate is
  `verdict: OK` with 0 unique errors, and the player `verdict: OK` over 10 scenes.

## 0.1.0-alpha

First public alpha: the code compiles, boots, loads a scene and renders an animated, lit character; skin and
hair read close to the original, the cloth does not.

- **Works**: `Assembly-CSharp` (2809 files) and `Assembly-UnityScript` at **1421 errors down to 0**, parity
  **2753/2753**; the boot log matches the game's `output_log.txt`; the body follows an 84-bone animation; 43 of
  the 135 families from shipped DXBC (**3023/3023 programs**).
- **Does not work yet**: cloth (fixed in 0.1.7-alpha), the shoulder/back gloss-bump seam, the lashes and the
  eye; 92 of the 135 families, so the installation stays a runtime dependency; post-processing stubs; physics,
  UI and other scenes untested.
- **Known issues**: one scene per session crashed the player; 25 of the 68 `.shader` files are placeholders;
  the `Marmoset/Specular IBL*` fallbacks are missing; tessellation is D3D11-only.
- **Requirements**: Windows, Unity **2018.1.9f2**, your own installation.
