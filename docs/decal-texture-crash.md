# Decal Maker texture crash under Unity 2021.3

A Play run of the rebuilt game on Unity **2021.3.45f2** ends the third-party plugin
**Chokaphi's Decal Maker** with an unhandled
`UnityException: Failed to create texture because of invalid parameters.` The throw site is a single
`new Texture2D(...)` inside the plugin's own `VAM_Decal_Maker.Decal_Maker.GetResource`, and the log
line printed just above it states the reason: the requested texture is a compressed DXT5/BC3 texture
whose width or height is not a multiple of 4.

This document identifies that call and traces each of its arguments back to its source. The
conclusion, argued in full below, is short:

* the failing texture is `new Texture2D(1, 1, TextureFormat.DXT5, linear)` at
  `VAM_Decal_Maker.cs:219` - three compile-time literals and one caller-supplied `bool`;
* it is a placeholder that the plugin creates *before* loading a 4096x4096 PNG over it with
  `Texture2D.LoadImage`, so the 1x1 size never had to be meaningful for a compressed format;
* nothing in the rebuilt game supplies that size - the plugin names its own resource file by a
  hard-coded path and picks the format itself;
* the same call already failed on Unity **2020.3.49f1** in our own baseline logs (rejected by D3D11
  with `E_INVALIDARG`), so this is a latent plugin defect that the 2021.3 upgrade promoted from a
  warning to a fatal exception.

#### The crash as logged

Recorded in `C:\Games\VaM_Updater\VAMOpen\artifacts\manual-play-prefix-leak.log`, a 1,647,591,770
byte log (about 21.6 M lines) of the 2021.3.45f2 run:

```text
Compressed TextureFormat RGBA Compressed DXT5|BC3 requires a texture size that is a multiple of 4
UnityEngine.StackTraceUtility:ExtractStackTrace ()
UnityEngine.Texture2D:Internal_Create (UnityEngine.Texture2D,int,int,int,UnityEngine.Experimental.Rendering.GraphicsFormat,UnityEngine.TextureColorSpace,UnityEngine.Experimental.Rendering.TextureCreationFlags,intptr)
UnityEngine.Texture2D:.ctor (int,int,UnityEngine.TextureFormat,int,bool,intptr)
UnityEngine.Texture2D:.ctor (int,int,UnityEngine.TextureFormat,bool)
VAM_Decal_Maker.Decal_Maker:GetResource (string,bool)
VAM_Decal_Maker.RenderPanelBase:ConvertNormal (UnityEngine.Texture2D)
VAM_Decal_Maker.ManagerPanel:UpdateSkinImage ()
VAM_Decal_Maker.ManagerPanel:CoreEvent (object,VAM_Decal_Maker.PanelEventArgs)
VAM_Decal_Maker.Decal_Maker:OnCoreChange (object,VAM_Decal_Maker.PanelEventArgs)
VAM_Decal_Maker.Decal_Maker/<CharacterUpdated>c__Iterator1:MoveNext ()
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

[C:\build\output\unity\unity\Runtime\Graphics\Texture2D.cpp line 563]

UnityException: Failed to create texture because of invalid parameters.
  at UnityEngine.Texture2D.Internal_Create (UnityEngine.Texture2D mono, System.Int32 w, System.Int32 h, System.Int32 mipCount, UnityEngine.Experimental.Rendering.GraphicsFormat format, UnityEngine.TextureColorSpace colorSpace, UnityEngine.Experimental.Rendering.TextureCreationFlags flags, System.IntPtr nativeTex) [0x00023] in <bc88c87c01184d1499d92d3b79a10ce6>:0
  at UnityEngine.Texture2D..ctor (System.Int32 width, System.Int32 height, UnityEngine.TextureFormat textureFormat, System.Int32 mipCount, System.Boolean linear, System.IntPtr nativeTex) [0x0004d] in <bc88c87c01184d1499d92d3b79a10ce6>:0
  at UnityEngine.Texture2D..ctor (System.Int32 width, System.Int32 height, UnityEngine.TextureFormat textureFormat, System.Boolean mipChain) [0x00000] in <bc88c87c01184d1499d92d3b79a10ce6>:0
  at VAM_Decal_Maker.Decal_Maker.GetResource (System.String path, System.Boolean linear) [0x0003a] in <1ac21d888e8345078245eb76d13b741e>:0
  at VAM_Decal_Maker.RenderPanelBase.ConvertNormal (UnityEngine.Texture2D mainTex) [0x00000] in <1ac21d888e8345078245eb76d13b741e>:0
  at VAM_Decal_Maker.ManagerPanel.UpdateSkinImage () [0x00073] in <1ac21d888e8345078245eb76d13b741e>:0
  at VAM_Decal_Maker.ManagerPanel.CoreEvent (System.Object o, VAM_Decal_Maker.PanelEventArgs e) [0x00158] in <1ac21d888e8345078245eb76d13b741e>:0
  at (wrapper delegate-invoke) System.EventHandler`1[VAM_Decal_Maker.PanelEventArgs].invoke_void_object_TEventArgs(object,VAM_Decal_Maker.PanelEventArgs)
  at VAM_Decal_Maker.Decal_Maker.OnCoreChange (System.Object o, VAM_Decal_Maker.PanelEventArgs e) [0x0000d] in <1ac21d888e8345078245eb76d13b741e>:0
  at VAM_Decal_Maker.Decal_Maker+<CharacterUpdated>c__Iterator1.MoveNext () [0x00115] in <1ac21d888e8345078245eb76d13b741e>:0
  at UnityEngine.SetupCoroutine.InvokeMoveNext (System.Collections.IEnumerator enumerator, System.IntPtr returnValueAddress) [0x00026] in <bc88c87c01184d1499d92d3b79a10ce6>:0
```

Line numbers in this document are counted by splitting the log on LF. The file mixes LF, CRLF and a
handful of lone CR bytes (nine of them appear in the first 2,960 lines alone), so `Get-Content` and
most editors report the same warning **seven lines higher**: the warning is line **2935** and the
exception line **2950** by that count. Both counts were read back during this investigation; the LF
numbering is used here:

| LF | editor | content |
| --- | --- | --- |
| 2928 | 2935 | `Compressed TextureFormat RGBA Compressed DXT5\|BC3 requires a texture size that is a multiple of 4` |
| 2941 | 2948 | `[C:\build\output\unity\unity\Runtime\Graphics\Texture2D.cpp line 563]` |
| 2943 | 2950 | `UnityException: Failed to create texture because of invalid parameters.` |
| 2947 | 2954 | `at VAM_Decal_Maker.Decal_Maker.GetResource (...) [0x0003a]` |

Two things about the stack are worth stating explicitly, because they pin the source line:

* the frame immediately below `GetResource` is
  `UnityEngine.Texture2D..ctor (int, int, TextureFormat, bool)`, i.e. the **four-argument**
  constructor overload. As shown in the next sections, the plugin contains exactly one
  four-argument `Texture2D` construction - `VAM_Decal_Maker.cs:219` - and every other construction in
  the plugin passes a fifth argument (`:2658`, `:4033`, `:4148`, `:4266`, `:4276`).
* the `VAM_Decal_Maker` frames carry the assembly identity
  `<1ac21d888e8345078245eb76d13b741e>` instead of a source path. That is a runtime-compiled
  (DynamicCSharp) assembly, which is why the stack shows no `.cs` path - see the next
  section.

#### The plugin package

The plugin ships as `Chokaphi.DecalMaker.37.var` - a zip by VaM's convention - unpacked for this
investigation into `VAMOpen\artifacts\decal-maker\`.

| | |
| --- | --- |
| Package | `C:\Games\VaM_Updater\AddonPackages\Chokaphi.DecalMaker.37.var` |
| Package size | 2,867,016 bytes |
| Package SHA-256 | `F339B0ACD7549EFE3AB0F59F9622A37FE24E792CAA1C0BB95F91CEDBAFE1827B` |
| Creator / package name | `Chokaphi` / `DecalMaker` |
| License / target | `CC BY-SA` / `programVersion 1.22.0.3` |
| Description | `RC 9: Appearances are Deceiving` |
| Plugin version string | `RC 9` (`VAM_Decal_Maker.cs:27`, `private const string pluginVersion = "RC 9";`) |
| Script payload | `Custom\Scripts\Chokaphi\VAM_Decal_Maker\VAM_Decal_Maker.cs`, 220,502 bytes, 4,746 lines, SHA-256 `1E0C08860F25A3C707E5F2E1BEF69A5FFE26D05AF0A1DA1FA424CB19DA2A50E7` |

The payload is 47 extracted files: `meta.json`, one combined C# source, 16 PNG cutouts under
`Cutout\`, an `assetbundle` and a preset JSON plus 5 icon JPGs under `Icons\`, and 20 genital
textures under `Custom\Atom\Person\Textures\GenitalMaker\`.

**Correction of the premise this was diagnosed under.** The crash report described the plugin as
"a precompiled DLL shipped inside the `.var`", inferred from the bare GUID on the stack frames. That
is not the case, and the difference matters for a fix:

* **the package contains no DLL at all** - a full recursive listing of the extracted package shows
  exactly one script artefact, the `.cs` file above, and no `.dll`, no `.mdb`, no `.pdb`;
* the `.cs` is itself generated, not hand-written: its first lines are
  `File generated by SourceCombiner.exe using 31 source files.` / `Created On: 6/14/2023 8:59:18 PM`,
  so 31 original module files were concatenated into one. There is no `.cslist` in the package.
* VaM compiles plugin sources at load time with **DynamicCSharp / mcs**, which produces an in-memory
  assembly with no file name - that is what the bare GUID `<1ac21d888e8345078245eb76d13b741e>` on
  every `VAM_Decal_Maker` frame means. The same run's log shows the mechanism itself, e.g.
  `hop4-baseline-ladyclown-smoke.log` lines 6925-6928 with
  `DynamicCSharp.Compiler.McsDriver.Compile (...)`, `McsCompiler.CompileFromSettings (...)` and a
  `Compile of AcidBubbles.Timeline.283:/Custom/Scripts/.../VamTimeline.AtomAnimation.cslist failed`
  diagnostic; other plugins in the same logs carry their own GUIDs (for example
  `<128a4e623b5648eba04551ecf3eee232>` for `VRAdultFun.EmotionEngine`).

Consequence: this crash is debuggable from readable C# rather than from IL, and **a fix can be
shipped as a patch to the plugin's sources** (same single `.cs`, or the 31 originals plus a rebuild
through SourceCombiner) rather than as a repackaged binary with no source.

#### The throwing method, verbatim

`VAM_Decal_Maker.cs:209-223`. The listing below is the plugin's own shipped source, extracted from the
package and quoted unmodified, including the author's comment on line 217. Line numbers are file line
numbers in the extracted `VAM_Decal_Maker.cs`.

```csharp
        public Texture2D GetResource(string path, bool linear = false)
        {
            string cachepath = path + linear.ToString();
            Texture2D texture;
            if (resourceTextures.TryGetValue(cachepath, out texture))
            {
                return texture;
            }
            //not in dictionary
            byte[] tmppng = FileManagerSecure.ReadAllBytes(GetPackagePath(this) + path);
            texture = new Texture2D(1, 1, TextureFormat.DXT5, linear);
            texture.LoadImage(tmppng);
            resourceTextures.Add(cachepath, texture);
            return texture;
        }
```

**The throwing line is 219**:
`texture = new Texture2D(1, 1, TextureFormat.DXT5, linear);`.

It is the only texture construction in the whole plugin that uses a **compressed** format. The
stack's `[0x0003a]` offset on the `GetResource` frame is inside this statement - the exception frame
for the constructor call. `LoadImage` on line 220 is never reached: the constructor throws first.
There is no `try`/`catch` anywhere on this path (`GetResource`, `ConvertNormal`, `UpdateSkinImage`,
`CoreEvent`, `OnCoreChange`, `CharacterUpdated` are all unguarded), so the exception propagates out
of the coroutine and is reported as an unhandled coroutine exception.

Two details of the method are relevant to the trace:

* `resourceTextures` is `public static Dictionary<string, Texture2D>` (`VAM_Decal_Maker.cs:50`), and
  the cache key is `path + linear.ToString()`. The first call for a given `(path, linear)` pair wins;
  every later call returns the cached texture. In this crash the cache was empty for that key, which
  is why the file was read from disk and the constructor ran.
* `GetPackagePath(this) + path` is what tells the plugin where its own resource lives; it is
  resolved from the plugin manager's JSON, not from anything the engine hands in. Verbatim
  (`VAM_Decal_Maker.cs:3497-3512`):

```csharp
        public static string GetPluginPath(MVRScript self)
        {
            string id = self.name.Substring(0, self.name.IndexOf('_'));
            string filename = self.manager.GetJSON()["plugins"][id].Value;
            return filename.Substring(0, filename.LastIndexOfAny(PathSeparatorChars));
        }
        // Get path prefix of the package that contains our plugin.
        public static string GetPackagePath(MVRScript self)
        {
            string filename = GetPluginPath(self);
            int idx = filename.IndexOf(":/");
            if (idx >= 0)
                return filename.Substring(0, idx + 2);
            else
                return string.Empty;
        }
```

So the file that is read is
`Chokaphi.DecalMaker.37:/Custom/Scripts/Chokaphi/VAM_Decal_Maker/Cutout/Normal.png` - the plugin's
own resource inside its own package. Note the ordering inside `GetResource`: **`ReadAllBytes`
happens before the constructor, and the constructor's arguments do not depend on what was read.**

#### The chain that reaches it

Working upwards from the throw site, the following frames are all reproduced verbatim from the
plugin's own source; each matches the corresponding stack frame in the log, including the IL offsets.

`VAM_Decal_Maker.cs:4509-4518` - `RenderPanelBase.ConvertNormal`; the call into `GetResource` is the
first statement of the method, which is why the log shows `[0x00000]` for this frame:

```csharp
        //convert normal from packed version for UI button display
        public Texture2D ConvertNormal(Texture2D mainTex)
        {
            Texture2D _normTex = DM.GetResource("Custom/Scripts/Chokaphi/VAM_Decal_Maker/Cutout/Normal.png", true);
            if (mainTex == null)
                mainTex = _normTex;
            material.SetTexture("_BumpMap0", mainTex);
            material.SetFloat("_BumpMapBlend0", 1);
            GpuCombine(_normTex, material, true);
            return tempTexture;
        }
```

`VAM_Decal_Maker.cs:2953-2975` - `ManagerPanel.UpdateSkinImage`:

```csharp
        private void UpdateSkinImage()
        {
            //System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            Texture2D temp = null;
            int id = TextureIndex.GetFirstTextureID(TextureSlot, DM._isMale);
            if (MaterialSlot == MatSlotEnum.DecalTex)
            {
                temp = DM.GetCurrentGPUTexture(id, MatSlotEnum.MainTex);
            }
            else if (MaterialSlot == MatSlotEnum.BumpMap)
            {
                //convert any packed normals to rgb format
                temp = DM.GetCurrentGPUTexture(id, MaterialSlot);
                temp = renderPanel.ConvertNormal(temp);
            }
            else
            {
                temp = DM.GetCurrentGPUTexture(id, MaterialSlot);
            }
            ImagePanel.ApplyTexture(temp);
            // watch.Stop();
            //SuperController.LogError(MaterialSlot + " " +TextureSlot + " Skin panel took " + watch.ElapsedMilliseconds);
        }
```

`VAM_Decal_Maker.cs:2835-2858` (head of `ManagerPanel.CoreEvent`, and the branch taken) - the log's
`[0x00158]` places the frame in the `CoreNewCharacterSelected` case:

```csharp
        public void CoreEvent(object o, PanelEventArgs e)
        {
            //SuperController.LogError("CORE EVENT " + e.EventName + " " + o.ToString());
            switch (e.EventName)
            {   //set Torso as active slot
                ...
                case EventEnum.CoreNewCharacterSelected:
                    if (e.Bool)
                    {
                        //DeregisterDAZCharacterTextureControl();
                        //RegisterDAZCharacterTextureControl();
                    }
                    UpdateSkinImage();
                    renderPanel.IsDirty = true;
                    break;
```

`VAM_Decal_Maker.cs:313-317` - `Decal_Maker.OnCoreChange`, the event forwarder:

```csharp
        public void OnCoreChange(object o, PanelEventArgs e)
        {
            //Fire the event - notifying all subscribers
            CoreEvent?.Invoke(o, e);
        }
```

`VAM_Decal_Maker.cs:517-530` - `CharacterUpdated`, compiled as the iterator `c__Iterator1` named in
the stack (the method is a coroutine, and `MoveNext` is the frame):

```csharp
        private IEnumerator CharacterUpdated(bool newCharacter = false)
        {
            if (!newCharacter)
                yield return new WaitForSeconds(1);
            LogError("CharacterUpdated");
            //loadingIcon.gameObject stays true until all texture/cloathing load process is finished
            LogError("yielding till all textures load " + SuperController.singleton.loadingIcon.gameObject.activeSelf);
            yield return new WaitWhile(() => SuperController.singleton.loadingIcon.gameObject.activeSelf);
            LogError("textures load finished continue coroutine");
            //should store the current skin textures.
            LogError("STORE Current Skin Textures");
            StoreGPUMats();
            OnCoreChange(this, new PanelEventArgs(EventEnum.CoreNewCharacterSelected, newCharacter));
            _processingCharacterChange = false;
        }
```

So the sequence is: a character change starts the `CharacterUpdated` coroutine (`StartCoroutine(
CharacterUpdated(false))` at line 485 for a reload, `StartCoroutine(CharacterUpdated(true))` from
`CharacterChanged()` at line 547 for a real change); the coroutine waits for the loading icon to
clear, stores the GPU materials, and raises `CoreNewCharacterSelected`; every manager panel
receives it through `OnCoreChange`; the panel whose `MaterialSlot` is `BumpMap` calls
`UpdateSkinImage`, which asks the render panel to convert a packed normal map; `ConvertNormal`'s
first action is to fetch its own `Normal.png` placeholder, and that fetch throws.

Note that the game-side texture `DM.GetCurrentGPUTexture(id, MaterialSlot)` (line 452-455,
`return (Texture2D)_dazSkin.GPUmaterials[id].GetTexture(MaterialSlot);`) is only ever bound to the
`mainTex` parameter, and `mainTex` is used solely by the lines after the throwing call. It is
**not** an argument to the constructor.

#### Where `w`, `h` and the format come from

The three size/format arguments of the failing constructor are literals in the plugin, and the fourth
is the method's own parameter:

| argument | bound to | source |
| --- | --- | --- |
| `width` | the integer literal `1` | `VAM_Decal_Maker.cs:219` |
| `height` | the integer literal `1` | `VAM_Decal_Maker.cs:219` |
| `textureFormat` | the constant `TextureFormat.DXT5` | `VAM_Decal_Maker.cs:219` |
| `mipChain` | the parameter `linear`, which is `true` at this call site | the `linear` argument of `GetResource`, passed as `true` at `VAM_Decal_Maker.cs:4511` |

and `path` at the failing call is the string literal
`"Custom/Scripts/Chokaphi/VAM_Decal_Maker/Cutout/Normal.png"` (again line 4511), which
`GetPackagePath` turns into `Chokaphi.DecalMaker.37:/Custom/Scripts/Chokaphi/VAM_Decal_Maker/Cutout/Normal.png`
- the plugin's own file, whose extracted size is **4096x4096**.

In other words, the arguments are fully determined before any I/O happens:

* `w` and `h` are **not** derived from `mainTex`, not from the file on disk, not from the current
  decal, not from any game API, and not from a cached format of a source texture. They are two
  constants in the plugin's own text.
* the format is **not** the result of a `switch` on a name or extension. The whole plugin contains
  seven `new Texture2D(` sites - lines 219, 2658, 4033, 4148, 4266, 4276 and a commented-out entry at
  4539 - of which only line 219 names a compressed format; the others are `RGBA32`/`ARGB32`, and
  those are all 4096x4096 or the incoming video texture's own size.
* the 4096x4096 PNG is read on line 218 and handed to `LoadImage` on line 220, i.e. the placeholder
  is deliberately created *small* and then overwritten by the decoded image. The placeholder's
  dimensions are irrelevant to the plugin's intent - but they are not irrelevant to the engine, for a
  compressed format.

The plugin is therefore **size-agnostic about the image and hard-coded about the placeholder**. It
copies nothing that was given to it: it invents a 1x1 compressed texture of its own. Every one of the
41 images the package ships is a multiple of 4 - fifteen 4096x4096 cutouts (including the
`Normal.png` fetched here), `_FemaleGenitals.png` at 8192x8192, five 512x512 icons, and twenty
4096x4096 genital textures - which is consistent with the author's intent to re-upload into a
block-aligned surface through `LoadImage` - and it is precisely why the 1x1 placeholder is a mistake
rather than a size that occasionally happens to be correct. There is no input under which
`1, 1, TextureFormat.DXT5` is legal in Unity 2021.3: the answer to "could the size ever have been a
multiple of 4 in the original game" is that it never was, in any version, for this call.

**No game-side size participates.** Nothing in `src\` hands the plugin this dimension, and the
plugin's `mainTex` - the only game-supplied texture on the path - is not an argument to the
constructor. The reconstructed game code does not contribute to this failure. (Verified by grepping
the reconstruction for `TextureFormat.DXT5|BC3|DXT1`: the only sites are
`DAZCharacterTextureControl.cs:1396/1400`, `ImageLoaderThreaded.cs:565/570`, `ImageControl.cs:880`,
`SkyshopLightController.cs:364`, `OvrAvatarAssetTexture.cs:23-27` and `TextureLoader.cs:28/31/35`, and
none of them is reachable from `GetResource`.)

#### What the shipped game does with the same format

The idiom the plugin deviates from is visible in the original game itself. Dumping the shipped
`C:\Games\VaM_Updater\VaM_Data\Managed\Assembly-CSharp.dll` (5,833,216 bytes) and decompiling it
shows that the game constructs a `TextureFormat.DXT5` texture in exactly two places, and both use
**4x4**:

```csharp
// ImageControl.cs:876 (decompiled from the shipped DAC)
	private IEnumerator SyncImage()
	{
		Texture2D tex = new Texture2D(4, 4, TextureFormat.DXT5, mipmap: true);

// SkyshopLightController.cs:364 (decompiled from the shipped DAC)
	private IEnumerator SyncImage()
	{
		Texture2D tex = new Texture2D(4, 4, TextureFormat.DXT5, mipmap: false)
		{
			wrapMode = TextureWrapMode.Clamp
		};
```

The reconstruction in `src\Assembly-CSharp\ImageControl.cs:880` and
`src\Assembly-CSharp\SkyshopLightController.cs:364` matches those lines exactly, i.e. our rebuilt
code reproduces the game's legal idiom and adds nothing. Both are 4x4 placeholders that are later
filled by `LoadImage` from a URL - structurally identical to what the plugin is trying to do, with a
size that satisfies the block granularity of a BC3 surface. `1, 1` is the plugin's own deviation from
the pattern it was copied from.

For background, the game's own loader also refuses to put a compressed format on an unaligned
surface: `ImageLoaderThreaded` computes the format only after checking `IsPowerOfTwo` on both
dimensions (`src\Assembly-CSharp\ImageLoaderThreaded.cs`, around lines 531-580), and the DDS path
(`TextureLoader.LoadDDSManual`) takes the size from the DDS header, which is block-aligned by
construction.

#### The same call already failed on Unity 2020.3

This is the most important piece of context, because it decides where the defect lives. The plugin
assembly was **not** compiled by the editor, so its behaviour cannot have been changed by the upgrade
itself; and the baseline logs from before the upgrade show that the texture was already invalid
there. In `C:\Games\VaM_Updater\VAMOpen\artifacts\logs\hop4-baseline-ladyclown-smoke.log` (852,549
bytes; line 16: `Built from '2020.3/release' branch; Version is '2020.3.49f1 (18249dd5551b)...'`), on
the *identical* stack:

```text
1223: Assertion failed on expression: 'mipLevel < m_MipCount'
1236: [C:\build\output\unity\unity\Runtime/Graphics/SharedTextureData.cpp line 99]
1253: d3d11: failed to create 2D texture id=2645 width=1 height=1 mips=3 dxgifmt=29 [D3D error was 80070057]
1266: [C:\build\output\unity\unity\Runtime/GfxDevice/d3d11/TexturesD3D11.cpp line 939]
1268: d3d11: failed to create 2D texture shader resource view id=2645 [D3D error was 80070057]
6932:   [Assert] d3d11: Failed to create 2D texture in GfxDeviceD3D11
```

with the same `GetResource / ConvertNormal / UpdateSkinImage / CoreEvent / OnCoreChange /
<CharacterUpdated>c__Iterator1` frames beneath each of them (lines 1228-1234). The same triple
appears in `hop4-baseline2-ladyclown-run1.log` at 1021/1036/1051 and in
`hop4-baseline2-ladyclown-run2.log` at 1192/1207/1222.

Two facts follow.

* **The dimensions in that message are the plugin's own arguments, printed numerically by the
  graphics device**: `width=1 height=1`. That is the 1x1 of `VAM_Decal_Maker.cs:219`, confirmed
  independently of the source by the 2020.3 D3D11 layer.
* **`80070057` is `E_INVALIDARG`** - the same request that Unity 2021.3.45f2 rejects with a managed
  `UnityException` was already rejected by D3D11 under 2020.3.49f1. In 2020.3 Unity let the
  construction proceed after logging an assertion and the D3D failure, so the outcome was a broken
  (never-uploaded) texture plus log noise; the plugin then called `LoadImage` on it and continued. In
  2021.3 the same condition raises a hard `UnityException`, which - with no `try`/`catch` anywhere on
  the path - terminates the coroutine.

So the bug is **not** newly introduced and **not** caused by our port: it is a latent plugin defect
that was previously masked. Note also that the 2020.3 logs contain no "multiple of 4" message at all
(grepping all three baseline logs for `multiple of 4` returns nothing), while the 2021.3.45f2 log
prints one for this very call; that check is therefore new relative to 2020.3.49f1 (the versions in
between were not tested), and it is what turns a silent degradation into a fatal error.

The `mips=3` in the D3D11 message is worth one remark: the request was a *mip-chained* 1x1 surface,
and the device reports three mip levels - consistent with the mip count being clamped to the 4x4
block granularity of the compressed format rather than to the 1x1 pixel size. This is an inference
from the log line only; the plugin's source cannot show engine internals, and the exact mip-count
rule was not proven here.

#### What this means for us

* **The defect is the plugin's.** The crash is produced by a compile-time constant triple
  `(1, 1, TextureFormat.DXT5)` written into `VAM_Decal_Maker.GetResource` in the shipped package,
  independent of any input, any character and any file. It is not a size the plugin received.
* **Our reconstruction contributes nothing to it.** No game-side API supplies those dimensions; the
  only game-supplied texture on the path (`GetCurrentGPUTexture`) is bound to a different parameter
  and cannot reach the constructor. The reconstructed `ImageControl` / `SkyshopLightController`
  reproduce the game's own legal 4x4 DXT5 idiom.
* **The engine's new strictness is the trigger, not the cause.** Unity 2021.3 fails the construction
  where 2020.3 logged and moved on, and the plugin has no error handling, so the exception escapes the
  coroutine. Had Unity 2021 kept the old behaviour, the plugin's placeholder would still have been
  invalid and the decal's normal map would still have been drawn without a valid bump texture.
* **It is fixable without touching the game.** Because the package ships readable C# (and no DLL), the
  one-line change `new Texture2D(4, 4, TextureFormat.DXT5, linear)` - or simply an uncompressed format
  such as `TextureFormat.RGBA32`, since the texture is overwritten by `LoadImage` immediately
  afterwards - restores the plugin's intended behaviour on 2021.3. That is a third-party plugin fix
  and is shipped as a patch to the user's package rather than folded into our reconstruction - see
  *The delivered patch* below.

Not proven in this document:

* which specific 2021.2-cycle build introduced the "requires a texture size that is a multiple of 4"
  check (only the 2020.3 and 2021.3 sides were observed directly);
* the exact engine rule that produced `mips=3` for a 1x1 mip-chained compressed texture;
* the author's reason for choosing `1, 1` rather than `4, 4` - the surrounding code only shows that
  the placeholder is meant to be replaced by `LoadImage` on the very next line;
* the compiler's choice of the iterator name `c__Iterator1` for `CharacterUpdated` (the source has a
  single `CharacterUpdated`; the iterator naming is an mcs/DynamicCSharp artefact observed in the
  stack, not derived from the source).

#### Reproduction and evidence files

* Crash: `VAMOpen\artifacts\manual-play-prefix-leak.log` - Unity 2021.3.45f2, LF lines 2928 (warning),
  2941 (`Texture2D.cpp line 563`), 2943 (exception), 2944-2954 (managed stack).
* Pre-upgrade baseline: `VAMOpen\artifacts\logs\hop4-baseline-ladyclown-smoke.log`,
  `hop4-baseline2-ladyclown-run1.log`, `hop4-baseline2-ladyclown-run2.log` - Unity 2020.3.49f1, the
  same stack plus `width=1 height=1 mips=3 dxgifmt=29 [D3D error was 80070057]`, and its summary copy
  in the corresponding `*.report.txt`.
* Plugin: `VAMOpen\artifacts\decal-maker\` - extracted `Chokaphi.DecalMaker.37.var`
  (`meta.json`, `Custom\Scripts\Chokaphi\VAM_Decal_Maker\VAM_Decal_Maker.cs`, cutouts, icons,
  genital textures).
* The game's own DXT5 idiom: `VAMOpen\artifacts\decal-maker\orig-ImageControl.cs` (59,070 bytes) and
  `orig-SkyshopLightController.cs` (28,552 bytes), decompiled from the shipped
  `VaM_Data\Managed\Assembly-CSharp.dll`, cross-checked against
  `src\Assembly-CSharp\ImageControl.cs:880` and `src\Assembly-CSharp\SkyshopLightController.cs:364`.




### The delivered patch (2026-09-19)

The fix above is no longer a recommendation; it is in the user's installation as a sibling package, made
by `VAMOpen\scripts\New-DecalMakerPatch.ps1`.

* **What it writes.** `AddonPackages\Chokaphi.DecalMaker.38.var` - the 46 entries of `.37` copied as raw
  bytes with exactly one of them rewritten, the script, `new Texture2D(1, 1, ...)` becoming
  `new Texture2D(4, 4, ...)`. 1 350 901 bytes, against the source package's 2 867 016, because the script
  entry (220 502 bytes) is unchanged in size. The shipped `.37` is untouched.
* **Why `4, 4` and not `RGBA32`.** Both work. `4, 4` keeps the author's format, is exactly one DXT5
  block, and is the number the shipped game uses for the same idiom (`ImageControl.cs:880`,
  `SkyshopLightController.cs:364`). `TextureFormat.RGBA32` has no size constraint at all, but it would
  change the format the texture reports before `LoadImage` replaces its surface, which is a wider change
  than the defect needs.
* **Why a new version, and why that is not enough on its own.** The patch is a sibling package rather than
  an edit of the shipped file, so the original stays byte-intact and rollback is deleting one file. It was
  first written up here as superseding `.37` for every consumer, on the strength of
  `VarPackageGroup.NewestVersion`, the absence of a `Chokaphi.DecalMaker.*` pin in
  `AddonPackagesUserPrefs`, and the package's own `meta.json` declaring
  `"standardReferenceVersionOption": "Latest"`. **That is refuted, both from the source and from a run.**
  `FileManager.GetPackage(string packageUidOrPath)` (`src\Assembly-CSharp\FileManager.cs:1560-1592`) enters
  the package *group* for exactly two spellings - `^([^\.]+\.[^\.]+)\.latest$` ->
  `packageGroup.NewestPackage`, and `^([^\.]+\.[^\.]+)\.min([0-9]+)$` ->
  `GetClosestMatchingPackageVersion(requestVersion, false, false)` - and resolves everything else by an
  exact `ContainsKey`/`TryGetValue` on the uid or the path. The uid of a file inside a package *is* the
  version-qualified string (`VarFileEntry.Uid = vp.Uid + ":/" + InternalSlashPath`, `VarFileEntry.cs:29`;
  `VarDirectoryEntry.cs:88`), so `Chokaphi.DecalMaker.37:/...` addresses revision 37 and nothing else.
  Operationally: the A/B's first attempt kept loading `.37` with `.38` sitting beside it. A new revision is
  therefore not merely unconfigured for existing scenes, it is unreferenced by construction, and reaching
  them needs the scene's own version token repointed (which is what the A/B did, on a copy) or a genuine
  rewrite of the shipped `.37` - `scripts\New-DecalMakerPatch.ps1 -InPlace`, which saves the original to
  `Chokaphi.DecalMaker.37.var.original` first and restores it on `-InPlace -Remove`. The second is a route
  this project does not take: the package belongs to its author, so the delivery is the new revision and
  the scene-side token is the consumer's own edit. The second gate is the
  package's confirmation state: `VarPackage.LoadUserPrefs` (`VarPackage.cs:314-335`) reads
  `<userPrefsFolder>/<Uid>.prefs` and defaults `pluginsAlwaysEnabled` to `false` when it is absent, after
  which `MVRPluginManager` calls `UserConfirm` instead of compiling - a panel a headless run cannot
  answer. The script therefore writes that 125-byte file too, in both working-directory roots.
* **One file, three consumers.** `VAMOpen\VaM_Rebuild\AddonPackages` and
  `VAMOpen\artifacts\player\AddonPackages` are junctions onto the installation's `AddonPackages`, so the
  editor, the standalone player and the original game all see one directory and one list.
* **The script is guard-railed.** It asserts the entry census and exactly one occurrence of the defect
  before writing, supports `-Verify` (report only), `-Remove` (rollback) and `-InPlace` (rewrite the
  shipped package, after backing it up), and self-checks by hashing all 46 entries on both sides after the
  repack. `-InPlace` is a capability of the tool rather than the project's route: the package is its
  author's, so the delivery is the new revision and repointing a scene's token is the consumer's own edit.
  The archive is built into a temporary file, verified entry by entry, and only then moved over the
  target, which matters most in `-InPlace`, where the target is the file the game may be loading. Source
  entry timestamps are carried over rather than stamped with the moment of the repack, so two runs produce
  the same bytes.

**Evidence that nothing else moved.** The two script entries were compared programmatically: equal
length, identical first and last 16 bytes, and differences at **exactly two offsets**, 11 662 and 11 665,
each `0x31` -> `0x34` (`'1'` -> `'4'`). Three unrelated entries (`meta.json`, `Cutout\Normal.png`,
`Icons\chokpahi-decal.assetbundle`) hash identically on both sides, the entry list is the same 46 names in
the same order, and the patched entry is pure ASCII with CRLF endings like its source.

### The acceptance, measured

An A/B, `.37` against `.38`, on one scene, run in the editor by the sub-agent that held it; the pass is
recorded in `artifacts\decal-repro\` (`REPORT.md` with the per-line table, `step2-acceptance.txt` with the
machine analysis, `decal-repro.log` for the control and `decal-repro-38b.log` for the candidate).

**Both gates had to be answered to get there, and the first attempt failed because of them.** Run 1 put the
`.38` package in place and reproduced the crash unchanged: the scene's own JSON hardcodes
`"plugin#2" : "Chokaphi.DecalMaker.37:/Custom/Scripts/Chokaphi/VAM_Decal_Maker/VAM_Decal_Maker.cs"`, so the
exact-resolution rule above kept the old revision in use, and `MVRPluginManager` refuses a `.cs` from a
package the prefs have not confirmed. Run 2 passed with two additions: `Saves\scene\decal-ab\ladyclown38.json`
(the same scene, version token changed and nothing else) and
`VaM_Rebuild\AddonPackagesUserPrefs\Chokaphi.DecalMaker.38.prefs`. That is a real trap for anyone following
this fix, and it is why the script now handles both halves.

**The result, read per block rather than per line.** The raw count of lines matching the plugin's names
*rises*, 28 -> 42, because the eight benign `GetCurrentGPUTexture` blocks the fixed plugin now reaches are
matches too - and those blocks are the positive evidence the earlier caution asked for: the plugin's own
skin-image path is entered instead of throwing on its first cache miss.

| reading | control (`.37`) | candidate (`.38`) |
| --- | --- | --- |
| `Failed to create texture` | 2 | **0** |
| the plugin's `multiple of 4` block | 1 | **0** |
| `at VAM_Decal_Maker....GetResource` frames | 6 | **0** |
| `GetResource` / `ConvertNormal` anywhere | 2 / 2 | **0 / 0** |
| `UpdateSkinImage` frames | 1 | **8** |
| `GetCurrentGPUTexture` frames | 0 | **8** |

Four further markers support it: the compiled plugin assembly's md5 is `b91c3b6a...` in the candidate
against `6ba98b00...` in the control, so a different revision really was compiled; `plugin#2
url=Chokaphi.DecalMaker.38:... scriptControllers=1` is reported; the candidate unloads the new revision's
own bundle (`Unloading unused asset bundle Chokaphi.DecalMaker.38:`) and then destroys the component
(`VAM_Decal_Maker.Decal_Maker:OnDestroy ()`); and the `GetCurrentGPUTexture <- UpdateSkinImage <- CoreEvent
<- OnCoreChange <- <CharacterUpdated>` chain appears 8 times against 0.

**The mechanism was then isolated on its own, and the pair re-run.** A throwaway edit-mode probe
(`DecalSizeProbe`, editor-side scratch, deleted after the run - its output survives as
`artifacts\decal-repro\step1-texture-probe.txt`) built the very constructor the plugin calls: DXT5 is
**refused at 1x1, 2x2 and 3x3, with and without a mip chain**, and **accepted at 4x4, 8x8, 4x4 DXT1 and
1x1 RGBA32** - so `1, 1` -> `4, 4` is necessary and sufficient, and the
refusal is a size rule rather than a mip or a format one. `LoadImage` onto a 4x4 DXT5 placeholder then
returned `True` on six real plugin images, every time reallocating the texture to the image's own size
(4096x4096, and 8192x8192 for `GenitalMaker/_FemaleGenitals.png`) with a rebuilt thirteen-level mip chain,
which is the "only the format ever mattered" sentence measured instead of argued. The re-run as a pair
(`decal-repro-A37.log` for the control, `decal-repro-B38.log` for the candidate, census `step3-ab-pair.txt`)
gives the same verdict on 1375 and 2151 lines: the control prints the rule at `:1132`, the exception at
`:1147` and the six-frame stack at `:1151-1157`, with `GetCurrentGPUTexture` at 0, and the candidate prints
none of them with `UpdateSkinImage` / `GetCurrentGPUTexture` at **8 / 8**. One reading from that pair is
worth keeping: the control crashed **three times in one session**, because a stray mouse click
(`LookInputModule:ProcessMousePressAlt` -> `MVRPluginManager:RemoveAllPlugins`) loaded a second scene - so a
raw crash count tracks how many scenes were loaded rather than the defect, and the per-block reading is the
one that means something.

**The loop was then closed by the user's own hand, which is the same A/B in a stronger form.** The run that
produced the fourth paste - `artifacts\manual-play.log`, 3 605 lines / 363 186 B, static since 16:42:33 - is
one editor session on one user-loaded scene, `SoftEros777.Lady_Clown.1:/Saves/scene/ladyclown.json` (`:937`,
picked in the scene file browser at `:943`; the boot scene `MeshedVR/default.json` at `:909` is the session's
own and no third scene is loaded), and it names the revisions, so it reads as a measurement rather than as one
more paste. Its `.37` instances throw six times (`:1468`, `:1673`, `:1895`, `:2119` with the deleted probe's
dumps interleaved at `:1637`, `:1859`, `:2081`, `:2305`, then `:2358` and `:2413`) and are torn down as
`Unloading unused asset bundle Chokaphi.DecalMaker.37:...` (`:2316`, `:2371`, `:2426`). The first two teardowns
are the URL control's **Reload** - `JSONStorableUrl:Reload` (`:2329`) <- `CreatePluginWithId`'s `b__0`
(`:2328`) <- `Button:Press` (`:2332`) <- `LookInputModule:ProcessMousePressAlt` (`:2336`) - and each Reload
re-creates the same `.37` URL, which is why the instance that follows throws again (`:2413`); the third teardown
is the plugin's own **Remove** button, `MVRPluginManager:RemovePlugin` (`:2436`) <- `b__1` (`:2437`) <-
`Button:Press` (`:2440`). After it the surviving instance runs the skin-image path **eight** times
(`ManagerPanel:UpdateSkinImage` at `:2455`, `:2467`, `:2479`, `:2491`, `:2503`, `:2515`, `:2527`, `:2539`, each
one a `Decal_Maker:GetCurrentGPUTexture` block at `-1`) with **no** `Failed to create texture` and **no**
`multiple of 4` after `:2413`, and its own teardown unloads `Chokaphi.DecalMaker.38:...` (`:2632`). A bundle
can only be unloaded if it was loaded, so the surviving instance was the delivered revision - **`.37` throws,
`.38` does not, inside one session on one scene, with no scene-side edit at all**, because the user reached the
new revision from the plugin's own URL control and its Reload. And the session's own save corroborates it from
a second source: `Save Saves\scene\1789828695.json` at `:2547`, whose line 894 is `"plugin#2" :
"Chokaphi.DecalMaker.38:/Custom/Scripts/Chokaphi/VAM_Decal_Maker/VAM_Decal_Maker.cs"` - so the live URL held
the delivered revision by the moment of the save, with the eight clean blocks just before it (`:2454`-`:2538`)
and the `.38` teardown after (`:2632`). This supersedes the caveat that the working shape must be "copy the
scene out, repoint the token, load the copy": the repoint is reachable live.

**The line numbers this document carried for that log before were 9-10 lines early throughout, and the count
of rule lines is not a count of crashes.** The numbers came from a mid-run snapshot of about 2 567 lines, so
every one of them shifted: the scene load `:928` -> `:937`, the throws `:1459`/`:1664`/`:1886`/`:2109`/`:2348`/
`:2403` -> `:1468`/`:1673`/`:1895`/`:2119`/`:2358`/`:2413`, the `.37` teardowns `:2306`/`:2361`/`:2416` ->
`:2316`/`:2371`/`:2426`, the clean blocks `:2444`-`:2529` -> `:2455`-`:2539`, the `.38` teardown `:2622` ->
`:2632`. Nothing should be re-derived from the old set. The engine's own `requires a texture size that is a
multiple of 4` prints **54** times (30 DXT5, 24 DXT1), but only **six** of those are the plugin's own - one per
round of the probe era (`:1453`, `:1648`, `:1870`, `:2094`) and two live (`:2343`, `:2398`) - while the other
48 are the throwaway probe's deliberate grid, six DXT5 and six DXT1 per round. The 10-line gap between a rule
line and its throw is the log's frame aliasing: the `MethodName (args)` frame, then the IL frame carrying the
MVID about ten lines further down.

Three limits, in the same spirit as the ones below: the log prints no plugin URL string, so the attribution
rests on the bundle identity, on the saved scene and on the absence of any later `.37` teardown - and the
creation of the surviving instance is logged nowhere, because a plugin whose source compiles succeeds
silently, while the two Reloads are visible only through the teardown each one causes; the session
**straddles** commit `fe96e9a`, which deleted the probe sources at 16:39 while this editor was open (the log
records the deletion and the following recompilation), so only the crashes from `:2358` on are live-plugin
evidence; and each reload recompiles the plugin through `DynamicCSharp`, which is why the crashing MVIDs
progress `de7b9d90...` (four throws, probe era) -> `828f997d...` (after the first Reload) -> `a036ba03...`
(after the second).

**What the acceptance does not show**, recorded because an over-read claim is how a wrong cause gets
inherited. The exception's absence proves nothing by itself - the 1.6 GB crash log names the package nowhere
(the smaller surviving log does, above), and
the `The referenced script ... is missing!` block is not decal-specific: it sits some twenty-two thousand
lines after the crash in that log, and it reads 0 in *both* runs of the paired A/B, so it is not a marker in
either direction. `Cache\Textures` received no new files and `resourceTextures`
reads 0 in both runs, so the plugin's cache writes are not part of the evidence. The candidate's clean
unload line exists only because the editor was closed gracefully, which makes it asymmetric with the
control. And the control loaded through the package while the candidate loaded a loose copy of the same
scene, a consequence of the package's internal JSON being un-editable in place.

**The state of the delivery on disk.** `-Verify` after the acceptance reads: mode
`new revision - the shipped package is left alone`; source `Chokaphi.DecalMaker.37.var`, 46 entries,
2 867 016 B, sha256 `F339B0AC...`; the scoped script 220 502 B / 4 747 lines; exactly **1** occurrence of
the defect literal; confirmation `pluginsAlwaysEnabled True` in **both** prefs locations (the install
root's `AddonPackagesUserPrefs`, which the standalone player reads, and `VaM_Rebuild\AddonPackagesUserPrefs`,
which the editor reads); nothing written. The delivered package is
`Chokaphi.DecalMaker.38.var`, 1 350 901 B, sha256 `DA62BD4A...` - the regenerated byte-reproducible form,
whose only difference from the package the acceptance actually loaded (`89C7EE67...`) is entry timestamps:
46 of 46 entries content-identical. A copy of that reading is `artifacts\decal-repro\verify-after-restore.txt`.

**The decision, taken, and what it rules out.** The install-side route is the **new revision**, and the
plugin itself is left alone: `Chokaphi.DecalMaker.37.var` is somebody else's package, so rewriting it in
place is out of bounds regardless of the backup the script keeps. What that leaves is the scene side -
`.38` works for scenes that name `.38`, and a scene that names `.37` reaches the fix by having its own
version token repointed, which the A/B did on a copy and which is a one-line edit on a loose scene file
(the scene in the A/B control lives inside `SoftEros777.Lady_Clown.1`, whose internal JSON cannot be
edited in place, so the working shape is: copy the scene out, repoint the token, load the copy). None of
this is repository content - it is local install state - and `-InPlace`, which rewrites the shipped package
after saving `Chokaphi.DecalMaker.37.var.original` beside it, stays in the script as a tested capability
for an install owner who wants it, **not** as this project's route.
#### The class of defect, enumerated install-wide

Every `new Texture2D(...)` the installation can reach was enumerated as text, independently of this diagnosis:
all 80 `*.var` archives unpacked and scanned as ZIPs (12 758 entries, 1 221 text files read) plus the loose
`Custom\Scripts\` sources - 24 textual matches over **18 distinct sites**, 16 of them live. Exactly **one** of
them can construct a compressed texture at a size the engine refuses, and it is the line this document is
about. The rest:

- **14 provably valid** - the 4 096-square RGBA32 icons each revision carries, other ARGB32 / RGB24 / RGBA32
  placeholders, and the delivered `DXT5` at 4x4.
- **1 indeterminate rather than invalid** - `MacGruber.Essentials.16`'s `MacGruber_SkyMagicLoader.cs:223`,
  `new Texture2D(mipsize, mipsize, cube.format, false)`: the size is a power of two, so safe by construction,
  but `cube.format` is read from `mySkyProbe.customBakedTexture as Cubemap` at runtime and cannot be resolved
  from text. It is the only opaque site left on the machine.
- **2 commented out.**

Two further readings from that sweep. The delivered revision differs from the shipped one in **this line
alone** - one changed line, the md5 the compiled plugin assembly name embeds is `6BA98B00...` in the shipped
`.37` against `B91C3B6A...` in the delivered `.38` - which is an independent confirmation of the byte-level
minimality argument above. And reachability is corroborated the other way too: the plugin
carries no `.disabled` marker, `VaM_Rebuild\AddonPackagesUserPrefs\Chokaphi.DecalMaker.37.prefs` exists and
already reads `pluginsAlwaysEnabled: true` - so the second gate this document names was satisfied in the
install before the delivery script wrote anything - and the only packages that pin `.37` are the Lady Clown
scene pair, `SoftEros777.Lady_Clown.1` (`ladyclown.json:893`, `meta.json:33`) plus the A/B scene
`VaM_Rebuild\Saves\scene\decal-ab\ladyclown37.json`. The boot scene names `MacGruber.Life.12` and no decal
plugin at all, which is why the defect needs the Lady Clown scene to appear. Full record:
`artifacts\plugin-census\REPORT.md` and `census.csv`.

#### And the plugin does not crash the game

Worth stating, because the pasted stacks read as fatal. The plugin's own `Setup()` **catches** the exception
(`VAM_Decal_Maker.cs:393`) and sets `_SetupError = true` (`:397`), after which `Update()` returns immediately
(`:281`): the plugin ends up half-initialised and silently dead, with nothing raised beyond the four lines the
engine prints for the throw itself. The exception reaches a stack only on the *other* path, which is a
coroutine: `Init()` -> `StartCoroutine(CharacterChanged())` -> `Update()` -> `Setup()` is the startup route and
swallows it, while every stack the user pasted comes from `CharacterUpdated`'s iterator through `OnCoreChange`
-> `CoreEvent` -> `UpdateSkinImage` -> `ConvertNormal` -> `GetResource`, where nothing catches. The same line
therefore reports itself in two shapes, and only the second escapes.
