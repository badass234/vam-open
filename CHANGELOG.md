# Changelog

Notable changes to **OpenVaM**, the open rebuild of Virt-a-Mate. The project is in alpha, so the minor number
moves with each round of user-visible work. Every number below is reproduced by a gate or an instrument; the
measurements behind them are in [`docs/`](docs/).

## 0.7.0-alpha - 2026-09-17

The in-game preferences had no screen resolution setting, so there is one now: a list of what the
display actually reports, a confirmation popup, and a ten-second revert for whoever picks a mode their
monitor cannot show. The panels live in the game's asset bundles and not in this repository, so the row
is not in any prefab this project owns - `UserPreferences.InitScreenResolutionUI` clones the
`Physics Update Cap Popup` row at runtime, places it below `Desktop Vsync Toggle`, renames its label to
`Screen Resolution` and drives it as the resolution control. What is listed is `Screen.resolutions`
filtered to a 640x480 floor and to the current display's aspect ratio within 0.02, deduplicated, sorted
by area and cut to six entries plus the current mode first; the value is written to `prefs.json` as
`screenResolution` and reapplied at boot only when the display still offers that mode. The popup is
`AlertUI` with Keep and Revert buttons and a countdown, `Reverting to the previous resolution in N
seconds...`, and a second change reuses the alert that is open instead of stacking another one. In VR
the row hides itself and a change is refused.

**The round's real find is that the project had been compiling a copy of the sources.** `src\` is where
this project is edited; `VaM_Rebuild\Assets\Scripts\` is where Unity compiles, and `Setup-RebuildProject.ps1`
makes the second out of the first once, so every edit in `src\` needed a copy that nothing performed.
Measured: `UserPreferences.cs` at 129 969 B in `src` against 118 911 B in the project, two days apart,
with zero occurrences of `screenResolution` in the project's copy, and a built `Assembly-CSharp.dll` of
6 180 864 B timestamped before the edits. The compile gate had returned `verdict: OK` for those edits -
correctly, about a tree nobody had edited. `scripts\Sync-Sources.ps1` is now the step between editing and
gating: it mirrors `*.cs` into the project for both assemblies, never touches a `.cs.meta` (a scene finds
its class by that GUID), verifies every file byte for byte (`copied 5, deleted 0, already identical 2820`,
`verdict: OK - 2825 source(s)`), and compares the newest source against the assembly's timestamp so a
stale build is reported rather than trusted - its first run said `Assembly-CSharp.dll: STALE - the
assembly predates the sources`.

Release `0.7.0-alpha`, tagged: sources in sync, compile gate `verdict: OK` with 0 unique errors,
`Assembly-CSharp.dll` at **6 188 544 B**, player `verdict: OK` over 10 scenes with `VAMOpen.exe` at
**648 704 B** and **231 024 085 B** of `VAMOpen_Data`. The new code is verified in the shipped
`VAMOpen_Data\Managed\Assembly-CSharp.dll` by byte search rather than in the editor - `0.7.0-alpha` three
times as UTF-16, `Keep this resolution?` and `Reverting to the previous resolution in ` once each -
because PowerShell's `Select-String -Encoding Unicode` answers "not found" on a binary that holds the
string. `New-PlayerRuntimeLinks.ps1` had never been run for this player and the build said so
(`runtime data missing: VAMOpen_Data\StreamingAssets`); the bundles are linked now and the player
registers all 18 `.var` packages. The reported result of the built player's own run: the setting works.

## 0.6.0-alpha - 2026-09-17

A plugin the installation cannot run used to flood the log for as long as its atom was loaded: one run
of the built player holds **17 194** `NullReferenceException` lines, of which **8 595** come from
`MacGruber.Breathing.Update` and **8 595** from `MacGruber.DriverBreathing.Update` - one per frame
each. The plugin is neither missing nor refused: its own dynamic assembly fails at the constructor
(`FieldAccessException` on a private field of its nested generic queue, `MiniQueue<T>.position`),
`AddComponent` leaves the half-built component on the object, `Init` throws on top of it, and Unity
keeps calling `Update`. `MVRPluginManager` now wraps creation and destroys the object it was building,
switches off a plugin that throws its way out of `Init` (the panel's toggle stays usable), and shows
the **first** failure of each plugin as a dialog instead of only a log line and a red button - the
warning window the user asked for. The same path covers "Plugin file X does not exist". Measured after
the change: the same boot logs **12** exception lines, **0** of them from either `Update`, and the
smoke run's own verdict counts **6** errors and exceptions for the whole session.

Those six are the plugin's own `LateRestoreFromJSON` throwing while `Atom.LateRestore` and
`Atom.RestoreFromLast` walk the atom's storables - the object that failed `Init` is still registered.
One-shots, not a loop.

The scene browser's previews are now **verified end to end in a fresh player**, not inferred from the
fix: 157 scenes listed, the loose scene and the `.var`-packaged ones alike resolving to a `.jpg` with
`exists: True`, `decoded 1 thumbnails ... 512x512`, and `8 of 8 scene browser thumbnails are in the
cache`. The instrument is `uFileBrowser.ThumbnailDiagnostics`, compiled in and dormant without
`-vamopen-diag`, which reports what a browser asked for, what it resolved to and whether the decode
succeeded.

A shader looked up **by name** answered with an AssetRipper placeholder whenever this project had not
transcribed that family, and the post-processing stack builds its materials by name - `Hidden/Post
FX/*` among them - so a post-processing material would have been built out of a one-pass blit while
the shipped implementation sat unread in `z_sha`. `VamShaderProvider.FindByName` now answers with this
project's reconstruction where there is one and with the shipped original everywhere else, and
`MaterialFactory` builds through it. The census found no drawn material in that state before or after,
so this closes a path rather than changing a frame - and it is the path the post-processing item needs.

`VamSkin()` no longer Gram-Schmidt-orthogonalises the tangent against the normal, because the shipped
vertex programs never do: the plain SM40 path, the GPU-skinning SM40 path, the tessellation control
point and the tessellation domain each spend one `dp3`/`rsq`/`mul` on the tangent and hand it to the
bitangent cross as it comes, and the pixel stage never re-orthogonalises either. The extra rotation
tilted every vertex's TBN away from the original's by the normal component the tangent carries. It is
a candidate for the gloss/bump seam that is still visible, and it is a hand check - nothing in this
round measured the seam.

Two harness traps were paid for and closed: `Setup-RebuildProject.ps1`'s wipe took the
`AddonPackages` and `Custom` junctions with it and left empty folders, so the editor scanned 0
packages and dropped every packaged scene load in silence (`requested=True, taken=False,
refused=False`); setup now re-makes the links and `Invoke-SmokeTest.ps1` refuses to start without them.

Release `0.6.0-alpha`: compile gate `verdict: OK`, 0 unique errors; player `verdict: OK`,
**281 890 175 B**, and the panel string is in the shipped assembly as a literal -
`VAMOpen_Data\Managed\Assembly-CSharp.dll` holds `VaMOpen 0.6.0-alpha` immediately followed by
`(content version: ` and no longer contains `0.5.0-alpha`.

## 0.5.0-alpha - 2026-09-17

Self-shadowing was missing because it did not exist: `VAM_LIGHT_ATTENUATION` was never transcribed, so point
lights fell through to AutoLight's built-in path. VaM's own filter is now transcribed out of the release
build's `MARMO_LINEAR + POINT + SHADOWS_CUBE` program: cube uv projection, bias twice floored at 0.005 instead
of 1e-5, the 25-tap Poisson disk (the blob's own `dcl_immediateConstantBuffer`, 0 mismatches), its rotating
hash, and taps **averaged** (`* 0.04`) rather than lerped toward 1, so a blurred edge is as dark as a hard one.
`_LightShadowData.x` sizes the disk rather than being the light's `shadowStrength`, and `_VamShadowUnity` is
the authorship control - a `Shader.SetGlobalFloat` uniform in no `Properties` block, so it adds no
`shader_feature`, and the frame drawn after it is put back is identical.

Measured on the lit body against the built-in path at the installation's own `shadowStrength` of 0.10: mean
**+8.26/255** against +0.98, 20.8 % of the mask darker than 10 levels against 1.9 %, and the darkest 8x8 cells
exactly where the body meets itself. One substitution, named: the shipped RNG samples an unidentified
full-screen texture at the screen-space uv, so the port seeds from the pixel coordinate instead, keeping the
blob's `m`/`sincos`/`frc` sequence.

**Two errors, kept because both were believed first.** A whole-frame average hid the effect (0.6/255), so
[`tools\measure_shadow.py`](tools/measure_shadow.py) masks the reference's lit pixels instead. And both gates
passed green on the *previous* round's shaders, because the project's shaders and their include copy are
generated by `scripts\New-VaMShaders.py` and untracked - now step 0 of the gate sequence.

The main panel names the build instead of the game: it reads **`VaMOpen 0.5.0-alpha (content version:
1.22.0.13)`**. The first half is one constant in `src\MeshVR\VaMOpenBuild.cs`; the second is the
installation's own `version` file, the same 26 bytes the game itself decrypts, which the player did not
have next to its exe until both link scripts started copying it - so a player built from this source used
to print a bare `VaMOpen 0.5.0-alpha`, and the `VAM_*` defines the content gates on now follow `1.22`
rather than the editor's `1.20` fallback. Said plainly, because it is easy to read the number as ours:
`1.22.0.13` appears nowhere in this source, and the shipped literal is `UNOFFICIAL`.

**Four ways the harness attacked the installation or itself, all closed.** `Setup-RebuildProject.ps1`
wiped `VaM_Rebuild\` and copied back only what AssetRipper produced, which deleted
`Assets\Editor\{RebuildGate,RebuildPlayer,VaMInspector}.cs` - the entire gate apparatus, leaving a
compile gate that answered `executeMethod class 'RebuildGate' could not be found`. It recursed through
`VaM_Rebuild\StreamingAssets`, a junction to the installation's **16.47 GB of bundles**, and PowerShell
5.1's `Remove-Item -Recurse` deletes the files *behind* a junction - verified on a scratch junction, so
one re-run would have destroyed the user's game. And its shader step turned the pending-compute warning
`New-VaMShaders.py` writes to stderr into a terminating error, so setup stopped short of `Project ready`.
The editor scripts are now stashed across the wipe and restored, the junction unlinked before the wipe
and relinked after it, and the shader step judged by exit code. The bundles are intact - **261 files,
16.47 GB** - and setup is re-runnable.

The fourth was on the player side and it made a second build impossible. `RebuildPlayer` deleted the
previous output with `Directory.Delete(path, true)`, and Mono treats a junction as an ordinary
subdirectory: non-recursively it throws `UnauthorizedAccessException` (measured - the first fix tried
exactly that and the build died on it), recursively it descends through the link into the user's
bundles. So the build after any linked one failed on `<exe>_Data\StreamingAssets`, first
`Directory ... is not empty`, then `Access ... is denied`. Junctions are now removed with
`RemoveDirectory`, and the size the build reports skips them, so 261 920 172 B is the player's own.
The two link scripts are unaffected: .NET Framework's `Directory.Delete` takes the name away and
nothing else, which is why only the editor-side code needed the Win32 call.

The built player's scene-selection menu showed no previews, and the cause was one file the harness had
been swapping in by mistake. Its log held **469** copies of a single exception, one per preview, out of
`System.Drawing.ComIStreamMarshaler+ManagedToNativeWrapper..cctor()` through
`System.Drawing.Bitmap..ctor (System.IO.Stream)` and
`ImageLoaderThreaded+QueuedImage.ProcessFromStream`, and the log names this very build just above the first
of them. `Setup-RebuildProject.ps1` was staging `VaM_Data\Managed\System.Drawing.dll`, Mono's .NET 2.0
build (448 512 B), which lands in the player's `Managed\`, and that assembly's stream marshaller cannot
initialise under the 4.x runtime, so every `new Bitmap(Stream)` threw. The player is Unity's `unityjit`
profile - its `System.dll`, `System.Core.dll` and `mscorlib.dll` are byte-identical to
`Editor\Data\MonoBleedingEdge\lib\mono\unityjit\` - and `System.Drawing` was the only file in
`Managed\` not from there, so the profile's own build (483 840 B) is staged instead, behind a guard that
refuses a 2.x identity. The `4.7.1-api` assembly is not an alternative: it is a 138 752 B forwarder, and
referencing it beside a plugin of the same simple name is `error CS1703` (measured), its target
`Facades\System.Drawing.Primitives.dll` carrying no `Bitmap`. Only `Bass.Net.dll` and `NAudio.dll` ever
asked for `2.0.0.0`, and Mono's binder unifies them onto the newer file - measured with a probe compiled
against the legacy assembly and run with this one beside it. `RebuildGate.Report` decodes one preview
through whatever it staged, so the next swap that breaks the menu names itself in the compile gate's log
rather than only in the game.

**Re-run:** 7479/7479 programs, 0 unique compile errors, play gate clean, a player that builds and starts
(261 955 527 B, `Scanned 18 packages`), and a compile gate that decodes a real scene preview through the
assembly it staged - `decoded ...default.jpg: 512x512 Format24bppRgb` - so the thumbnails are no longer a
hand check. The rebuilt player logs 101 lines and **no** exception at all, where the previous one logged 473
`Exception ` lines on the same startup path. And by hand the character darkens itself where it meets itself
at about 100 FPS. Open: whether the amount matches the installation is a hand-run judgement read as right,
and the user has not looked at the thumbnail menu himself yet; the seam, the sheen, the **Marmoset IBL
families** and the **post-processing stubs** (`Hidden/Post FX/*`) are untouched.

## 0.4.0-alpha - 2026-09-17

Every round so far had compared a picture taken here against one taken in the installation, and this one found
the two were never rendered under the same settings.

- `RebuildGate.PresetDump` prints the resolved `QualitySettings` level and the graphics keys of the working
  directory's `prefs.json` - read as raw text, because a missing or misspelled key falls back to the code's
  default in silence. `scripts\New-RuntimeDataLinks.ps1` matches those keys in place and matched **5 of 9**:
  `renderScale` 2 -> 1, `msaaLevel` 8 -> 4, `pixelLightCount` 4 -> 2, `smoothPasses` 4 -> 2, `glowEffects`
  High -> Low.
- Rendered side by side the two presets are nearly the same picture: mean lit-skin luminance 0.7144 against
  0.7106 (**+0.54 %**), 0.61 % of the frame differing by more than 24/255, every pixel of it inside the body.
  The capture cannot see `renderScale` or `msaaLevel` either - `RenderToFile` renders a fixed 1024x1024 target
  with no anti-aliasing - so the preset is not most of what the hand run sees.
- Still open: the sheen is diagnosed, not fixed; the seam has not been re-examined at the matched preset.

## 0.3.0-alpha - 2026-09-17

The first round that produces something other than an editor project, and the one that settled why the player
scanned nine of the eighteen packages the editor scans - which turned up the answer to the sheen.

- **The player builds in batch mode** (`RebuildPlayer.Build` from `scripts\Invoke-PlayerBuild.ps1`), its
  verdict read from the `----- RebuildPlayer` markers rather than the exit code, because Unity returns the same
  code for a failed build and for an editor that never reached the method. It runs - 258 972 055 B, ten
  scenes, D3D11, 312 FPS - and carries none of the game's data: `New-PlayerRuntimeLinks.ps1` junction-links
  `AddonPackages` and `Custom` (0.84 GB and 1.1 GB) and copies `Saves`, `AddonPackagesUserPrefs`, `prefs.json`.
- **The missing packages were a missing key file**: `SuperController` reads `Keys/1.21/key.json` relative to
  the working directory and fills in a restricted package set without a valid key - with it, the same build
  logs `Scanned 18 packages in 68.7 ms` instead of 9.
- **The sheen is at least partly the quality preset.** The installation's graphics keys are the **High** preset
  of `UserPreferences.QualityLevels` to the digit; the editor's are **Max** with `msaaLevel` raised by hand from
  the preset's 2 to 8. `pixelLightCount` is Unity's per-pixel light budget, and `smoothPasses` is
  `DAZSkinV2.smoothOuterLoops`, the Laplacian iterations applied to the skin mesh before its normals are
  rebuilt - the game's own quality settings applied by the game's own code, not this project's shading.
- Still open: the sheen is diagnosed, not fixed; the player build is a separate script, not one entry point.

## 0.2.0-alpha - 2026-09-17

Four rounds, and the release that closes the "basic functionality" milestone: the boot scene loads and the
character, skin, hair, clothing and eyes all draw from this project's own code, with the drawn body's shading
verified against the shipped programs rather than against a reading of them.

**1 - the hair.** The `Custom/Hair/*ComputeBuff` families are the body's shading model with fewer inputs - a uv
window, a per-pass vertex normal offset, `VAM_NO_TANGENTS`, the `_Cutoff1`/`_Cutoff2`/`_Pass1Cutoff` names,
buffers without `tangents` - giving +14 shaders and +55 passes. Three things were not a transcription:

- Pass 0 of the three `Main*` families **writes black and keeps the texel's alpha**, a dark layer for a later
  pass to draw over, where we emitted the shared shading model and drew a second fully lit layer. Every hair
  pass serialises `colMask = 14`, RGB masked off, so the render state cannot reveal it.
- Lashes are `saturate(_AlphaTex.a + _AlphaAdjust)` on the mask's own `_AlphaTex_ST`, not solid dark planes; the
  eyes' `AlphaMask` fragment writes black to RGB where ours ran the full shading model. A pass is a mask when
  its `LightMode` is lit and its `$Globals` are a subset of `{_Color, _AlphaAdjust}` - 5 of 136 emitted passes,
  all masks - and clipping now comes from the shipped programs too: the old rule missed 10 passes, one of them
  the lash's own base pass.
- The seam's two measurements - the GPU average of every map the drawn skin samples, and the handedness of the
  tangent basis - **both came back clean**, so the over-brightness stands against the game's own hair shader
  rather than ours. Gate: `4414/4414 programs`, 0 differing (*Defect 3* in
  [`docs/verification.md`](docs/verification.md)).

**2 - the plain twins.** `DAZHairMesh` draws through `Graphics.DrawMesh`, so a hair material carries the
**plain** `Custom/Hair/*` name and never reaches the `ComputeBuff` lookup. Contracts come from two places now -
`Extract-VaMShaders.py` also reads `VaM_Data\StreamingAssets\z_sha`, the bundle VaM ships its own copies in -
giving **135 from the engine data files, 182 from the bundle, 317 distinct names, none in both**, and 15 plain
`Custom/Hair/*` plus 26 plain `Custom/Subsurface/*` reconstructed from that DXBC: `88 shaders, 256 passes,
6805/6805 programs`. Two fixes came out of it:

- An empty ShaderLab file could shadow a working shader - two `Debug*ComputeBuff` families have no pass that
  reads the compute buffers, so the run wrote a file with an empty `SubShader { LOD 0 }`, and `Shader.Find`
  prefers the project's copy. The `ComputeBuff` suffix could also be applied twice.
- The unpacked bump normal is normalised before the flat direction is subtracted (`normalize(nTS) - nFlat`);
  without it the perturbation is shortened instead of rotated - a bump seam wherever the map carries a crease.

**3 - the game's own materials move onto the project's shaders.** `VamShaderProvider.UseProjectShader`
replaces a material's shader with the project's copy of the same name, from six call sites, with a generated
`VamProjectShaders.cs` listing the reconstructed names so an unreconstructed family keeps its AssetRipper
placeholder. Materials on a bundle copy fell from 84 to 76, and 33 of the character's 49 drawn materials are on
a project shader. Still open: the redirect is name-based and changes no drawn family's program, so it cannot be
judged by eye.

**4 - the base pass, read back out of the shipped bytecode.** `DIRECTIONAL-MARMO_LINEAR`, the ForwardBase
variant of `Custom/Subsurface/GlossNMTessMappedFixedComputeBuff`, was disassembled with `fxc`, this project's
HLSL compiled to the same profile, and the two streams walked together: the arithmetic is the same in the same
order. The one divergence is `_ExposureIBL.w` applied to the indirect half only where ours multiplied the
joined result - equal only while the exposure is one, and the scene has it at 1.000 - and the one omitted
instruction is proved dead: the shipped `r3.xyz * v7.xyz` term reads a varying that is exactly zero for every
vertex the tessellated pipeline produces. No visible change is claimed; the shading is *proved* to match, which
is what lets the next round skip the shader. What is left lives outside it: the specular IBL cube's import
colour space, the material's `_SpecInt`/`_Shininess`/`_Fresnel` values, and bloom thresholds in linear space.

## 0.1.1-alpha - 2026-09-16

Clothing renders with its own colour, and transparent where the original is transparent. The cause was the
generator, not the data: it decoded the serialized `rtBlend*.colMask` with the bit order `(1, R) (2, G)
(4, B) (8, A)`, while Unity's `ColorWriteMask` is `Alpha = 1, Blue = 2, Green = 4, Red = 8`. The transparent
families serialise `14`, i.e. `RGB`, so they were given `ColorMask GBA` - the pass never wrote red, the colour
behind it bled through, and the garment read as inverted. 44 pass states moved from `GBA` to `RGB`. Gate:
`3023/3023 programs`, 0 differences.

## 0.1.0-alpha - 2026-09-16

The first public alpha: Virt-a-Mate's code is decompiled, compiles, boots, loads a scene and renders an
animated, lit character. The body skin and the hair read close to the original, the cloth and part of the
character's materials do not, and the recovered code is still the raw decompilation.

### What works

- **Compilation** - `Assembly-CSharp` (2809 files) and `Assembly-UnityScript` build through Unity's own
  compiler: **1421 errors down to 0**, type parity **2753/2753**.
- **Boot and scenes** - `SuperController` comes up, the boot log matches the original game's own
  `output_log.txt` line for line, and `Saves/scene/MeshedVR/default.json` loads and renders.
- **Character** - the body is skinned on the GPU and follows the scene's 84-bone animation instead of hanging
  in a bind pose, lit by the scene's own lights and IBL, in Linear colour space.
- **Shaders** - 43 of the 135 families the asset contracts name are reconstructed from the shipped DXBC,
  **3023/3023 programs**, and `verify_twins.py` diffs 112 pixel passes with **0 differing**.
- **Measurement** - a compile gate, a play gate and a manual play launcher, so every claim above is
  reproducible rather than remembered ([`docs/verification.md`](docs/verification.md)).

### What does not work yet

- **Cloth** - rendered transparent with its colour inverted; **fixed in 0.1.1-alpha**.
- **Part of the character's materials** - a gloss and bump seam across the shoulder and the back, the
  eyelashes and the eye.
- **92 of the 135 shader families** are not transcribed, so the game installation stays a runtime dependency.
- **Post-processing** - the stage's shaders are stubs and no `PostProcessingBehaviour` is on the scene camera,
  so there is no bloom, tonemapping, colour grading, eye adaptation or FXAA.
- **Physics, UI and other scenes** - untested, on the boot scene only, because opening several scenes in one
  session crashes the player.
- **Readability** - the code is the decompilation as it came out: obfuscated identifiers, dead decompiler
  artifacts, monolithic classes. The refactoring pass has not started.

### Known issues

- Opening more than one scene in an editor session crashes the player.
- 25 of the 68 `.shader` files are AssetRipper placeholders that declare no properties and transform only the
  mesh's own vertices.
- The `Marmoset/Specular IBL Soft{,NoCull}ComputeBuff` fallbacks our generated shaders name do not exist yet,
  so Unity logs `fallback shader ... not found` for them.
- Tessellation is D3D11-only: the `HLSLSupport.cginc` branches refuse the tessellation attributes on GLES.

### Requirements

Windows, Unity **2018.1.9f2**, and your own Virt-a-Mate installation - no game content ships with this
repository. See [How to try it](README.md#how-to-try-it) in the README.
