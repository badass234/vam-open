using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using MVR.FileManagement;
using MVR.FileManagementSecure;
using MeshVR;
using UnityEngine;
using UnityEngine.UI;

public class MaterialOptions : JSONStorable, IBinaryStorable
{
	protected List<UnityEngine.Object> allocatedObjects;

	[HideInInspector]
	public Transform copyFromTransform;

	[HideInInspector]
	public int copyFromTextureGroup = 1;

	[HideInInspector]
	public int copyToTextureGroup = 1;

	[HideInInspector]
	public MaterialOptions copyUIFrom;

	[HideInInspector]
	public MaterialOptionsUI copyUIFromUI;

	public bool controlRawImage;

	public bool searchInChildren = true;

	public Transform materialContainer;

	public Transform materialContainer2;

	public Material materialForDefaults;

	public Shader hideShader;

	public int[] paramMaterialSlots;

	public int[] paramMaterialSlots2;

	public bool deregisterOnDisable;

	protected string origOverrideId;

	protected JSONStorableString customNameJSON;

	protected JSONStorableFloat renderQueueJSON;

	protected JSONStorableBool hideMaterialJSON;

	protected List<MaterialOptions> otherMaterialOptionsList;

	public bool allowLinkToOtherMaterials;

	[SerializeField]
	protected bool _linkToOtherMaterials = true;

	protected JSONStorableBool linkToOtherMaterialsJSON;

	protected JSONStorableColor color1JSONParam;

	public string color1Name = "_Color";

	public string color1Name2;

	public string color1DisplayName = "Diffuse Color";

	public Text color1DisplayNameText;

	public UnityEngine.Color color1CurrentColor;

	public HSVColor color1CurrentHSVColor;

	public HSVColorPicker color1Picker;

	public RectTransform color1Container;

	public float color1Alpha = 1f;

	protected bool materialHasColor1;

	protected JSONStorableColor color2JSONParam;

	public string color2Name = "_SpecColor";

	public string color2DisplayName = "Specular Color";

	public Text color2DisplayNameText;

	public UnityEngine.Color color2CurrentColor;

	public HSVColor color2CurrentHSVColor;

	public HSVColorPicker color2Picker;

	public RectTransform color2Container;

	public float color2Alpha = 1f;

	protected bool materialHasColor2;

	protected JSONStorableColor color3JSONParam;

	public string color3Name = "_SubdermisColor";

	public string color3DisplayName = "Subsurface Color";

	public Text color3DisplayNameText;

	public UnityEngine.Color color3CurrentColor;

	public HSVColor color3CurrentHSVColor;

	public HSVColorPicker color3Picker;

	public RectTransform color3Container;

	public float color3Alpha = 1f;

	protected bool materialHasColor3;

	protected JSONStorableFloat param1JSONParam;

	public string param1Name = "_SpecOffset";

	public string param1DisplayName = "Specular Texture Offset";

	public Text param1DisplayNameText;

	public Text param1DisplayNameTextAlt;

	public float param1CurrentValue;

	public float param1MinValue = -1f;

	public float param1MaxValue = 1f;

	public Slider param1Slider;

	public Slider param1SliderAlt;

	protected bool materialHasParam1;

	protected JSONStorableFloat param2JSONParam;

	public string param2Name = "_SpecInt";

	public string param2DisplayName = "Specular Intensity";

	public Text param2DisplayNameText;

	public Text param2DisplayNameTextAlt;

	public float param2CurrentValue;

	public float param2MinValue;

	public float param2MaxValue = 10f;

	public Slider param2Slider;

	public Slider param2SliderAlt;

	protected bool materialHasParam2;

	protected JSONStorableFloat param3JSONParam;

	public string param3Name = "_Shininess";

	public string param3DisplayName = "Gloss";

	public Text param3DisplayNameText;

	public Text param3DisplayNameTextAlt;

	public float param3CurrentValue;

	public float param3MinValue = 2f;

	public float param3MaxValue = 8f;

	public Slider param3Slider;

	public Slider param3SliderAlt;

	protected bool materialHasParam3;

	protected JSONStorableFloat param4JSONParam;

	public string param4Name = "_Fresnel";

	public string param4DisplayName = "Specular Fresnel";

	public Text param4DisplayNameText;

	public Text param4DisplayNameTextAlt;

	public float param4CurrentValue;

	public float param4MinValue;

	public float param4MaxValue = 1f;

	public Slider param4Slider;

	public Slider param4SliderAlt;

	protected bool materialHasParam4;

	protected JSONStorableFloat param5JSONParam;

	public string param5Name = "_GlossOffset";

	public string param5DisplayName = "Gloss Texture Offset";

	public Text param5DisplayNameText;

	public Text param5DisplayNameTextAlt;

	public float param5CurrentValue;

	public float param5MinValue = -1f;

	public float param5MaxValue = 1f;

	public Slider param5Slider;

	public Slider param5SliderAlt;

	protected bool materialHasParam5;

	protected JSONStorableFloat param6JSONParam;

	public string param6Name = "_IBLFilter";

	public string param6DisplayName = "Global Illumination Filter";

	public Text param6DisplayNameText;

	public Text param6DisplayNameTextAlt;

	public float param6CurrentValue;

	public float param6MinValue;

	public float param6MaxValue = 1f;

	public Slider param6Slider;

	public Slider param6SliderAlt;

	protected bool materialHasParam6;

	protected JSONStorableFloat param7JSONParam;

	public string param7Name = "_AlphaAdjust";

	public string param7DisplayName = "Alpha Adjust";

	public Text param7DisplayNameText;

	public Text param7DisplayNameTextAlt;

	public float param7CurrentValue;

	public float param7MinValue = -1f;

	public float param7MaxValue = 1f;

	public Slider param7Slider;

	public Slider param7SliderAlt;

	protected bool materialHasParam7;

	protected JSONStorableFloat param8JSONParam;

	public string param8Name = "_DiffOffset";

	public string param8DisplayName = "Diffuse Texture Offset";

	public Text param8DisplayNameText;

	public Text param8DisplayNameTextAlt;

	public float param8CurrentValue;

	public float param8MinValue = -1f;

	public float param8MaxValue = 1f;

	public Slider param8Slider;

	public Slider param8SliderAlt;

	protected bool materialHasParam8;

	protected JSONStorableFloat param9JSONParam;

	public string param9Name = "_DiffuseBumpiness";

	public string param9DisplayName = "Diffuse Bumpiness";

	public Text param9DisplayNameText;

	public Text param9DisplayNameTextAlt;

	public float param9CurrentValue;

	public float param9MinValue;

	public float param9MaxValue = 2f;

	public Slider param9Slider;

	public Slider param9SliderAlt;

	protected bool materialHasParam9;

	protected JSONStorableFloat param10JSONParam;

	public string param10Name = "_SpecularBumpiness";

	public string param10DisplayName = "Specular Bumpiness";

	public Text param10DisplayNameText;

	public Text param10DisplayNameTextAlt;

	public float param10CurrentValue;

	public float param10MinValue;

	public float param10MaxValue = 2f;

	public Slider param10Slider;

	public Slider param10SliderAlt;

	protected bool materialHasParam10;

	public MaterialOptionTextureGroup textureGroup1;

	protected JSONStorableStringChooser textureGroup1JSON;

	public UIPopup textureGroup1Popup;

	public UIPopup textureGroup1PopupAlt;

	public string startingTextureGroup1Set;

	public string currentTextureGroup1Set;

	protected bool hasTextureGroup1;

	public MaterialOptionTextureGroup textureGroup2;

	protected JSONStorableStringChooser textureGroup2JSON;

	public UIPopup textureGroup2Popup;

	public UIPopup textureGroup2PopupAlt;

	public string startingTextureGroup2Set;

	public string currentTextureGroup2Set;

	protected bool hasTextureGroup2;

	public MaterialOptionTextureGroup textureGroup3;

	protected JSONStorableStringChooser textureGroup3JSON;

	public UIPopup textureGroup3Popup;

	public UIPopup textureGroup3PopupAlt;

	public string startingTextureGroup3Set;

	public string currentTextureGroup3Set;

	protected bool hasTextureGroup3;

	public MaterialOptionTextureGroup textureGroup4;

	protected JSONStorableStringChooser textureGroup4JSON;

	public UIPopup textureGroup4Popup;

	public UIPopup textureGroup4PopupAlt;

	public string startingTextureGroup4Set;

	public string currentTextureGroup4Set;

	protected bool hasTextureGroup4;

	public MaterialOptionTextureGroup textureGroup5;

	protected JSONStorableStringChooser textureGroup5JSON;

	public UIPopup textureGroup5Popup;

	public UIPopup textureGroup5PopupAlt;

	public string startingTextureGroup5Set;

	public string currentTextureGroup5Set;

	protected bool hasTextureGroup5;

	public string customTextureFolder;

	protected string customTexturePackageFolder;

	protected JSONStorableAction openTextureFolderInExplorerAction;

	public bool customTexture1IsLinear;

	public bool customTexture1IsNormal;

	public bool customTexture1IsTransparency;

	public Button customTexture1FileBrowseButton;

	public Button customTexture1ReloadButton;

	public Button customTexture1ClearButton;

	public Button customTexture1NullButton;

	public Button customTexture1DefaultButton;

	public Text customTexture1UrlText;

	public Text customTexture1Label;

	protected Texture2D customTexture1;

	protected bool customTexture1IsNull;

	protected JSONStorableUrl customTexture1UrlJSON;

	protected JSONStorableFloat customTexture1TileXJSON;

	public Slider customTexture1TileXSlider;

	protected JSONStorableFloat customTexture1TileYJSON;

	public Slider customTexture1TileYSlider;

	protected JSONStorableFloat customTexture1OffsetXJSON;

	public Slider customTexture1OffsetXSlider;

	protected JSONStorableFloat customTexture1OffsetYJSON;

	public Slider customTexture1OffsetYSlider;

	public bool customTexture2IsLinear = true;

	public bool customTexture2IsNormal;

	public bool customTexture2IsTransparency;

	public Button customTexture2FileBrowseButton;

	public Button customTexture2ReloadButton;

	public Button customTexture2ClearButton;

	public Button customTexture2NullButton;

	public Button customTexture2DefaultButton;

	public Text customTexture2UrlText;

	public Text customTexture2Label;

	protected Texture2D customTexture2;

	protected bool customTexture2IsNull;

	protected JSONStorableUrl customTexture2UrlJSON;

	protected JSONStorableFloat customTexture2TileXJSON;

	public Slider customTexture2TileXSlider;

	protected JSONStorableFloat customTexture2TileYJSON;

	public Slider customTexture2TileYSlider;

	protected JSONStorableFloat customTexture2OffsetXJSON;

	public Slider customTexture2OffsetXSlider;

	protected JSONStorableFloat customTexture2OffsetYJSON;

	public Slider customTexture2OffsetYSlider;

	public bool customTexture3IsLinear = true;

	public bool customTexture3IsNormal;

	public bool customTexture3IsTransparency;

	public Button customTexture3FileBrowseButton;

	public Button customTexture3ReloadButton;

	public Button customTexture3ClearButton;

	public Button customTexture3NullButton;

	public Button customTexture3DefaultButton;

	public Text customTexture3UrlText;

	public Text customTexture3Label;

	protected Texture2D customTexture3;

	protected bool customTexture3IsNull;

	protected JSONStorableUrl customTexture3UrlJSON;

	protected JSONStorableFloat customTexture3TileXJSON;

	public Slider customTexture3TileXSlider;

	protected JSONStorableFloat customTexture3TileYJSON;

	public Slider customTexture3TileYSlider;

	protected JSONStorableFloat customTexture3OffsetXJSON;

	public Slider customTexture3OffsetXSlider;

	protected JSONStorableFloat customTexture3OffsetYJSON;

	public Slider customTexture3OffsetYSlider;

	public bool customTexture4IsLinear;

	public bool customTexture4IsNormal;

	public bool customTexture4IsTransparency = true;

	public Button customTexture4FileBrowseButton;

	public Button customTexture4ReloadButton;

	public Button customTexture4ClearButton;

	public Button customTexture4NullButton;

	public Button customTexture4DefaultButton;

	public Text customTexture4UrlText;

	public Text customTexture4Label;

	protected Texture2D customTexture4;

	protected bool customTexture4IsNull;

	protected JSONStorableUrl customTexture4UrlJSON;

	protected JSONStorableFloat customTexture4TileXJSON;

	public Slider customTexture4TileXSlider;

	protected JSONStorableFloat customTexture4TileYJSON;

	public Slider customTexture4TileYSlider;

	protected JSONStorableFloat customTexture4OffsetXJSON;

	public Slider customTexture4OffsetXSlider;

	protected JSONStorableFloat customTexture4OffsetYJSON;

	public Slider customTexture4OffsetYSlider;

	public bool customTexture5IsLinear = true;

	public bool customTexture5IsNormal = true;

	public bool customTexture5IsTransparency;

	public Button customTexture5FileBrowseButton;

	public Button customTexture5ReloadButton;

	public Button customTexture5ClearButton;

	public Button customTexture5NullButton;

	public Button customTexture5DefaultButton;

	public Text customTexture5UrlText;

	public Text customTexture5Label;

	protected Texture2D customTexture5;

	protected bool customTexture5IsNull;

	protected JSONStorableUrl customTexture5UrlJSON;

	protected JSONStorableFloat customTexture5TileXJSON;

	public Slider customTexture5TileXSlider;

	protected JSONStorableFloat customTexture5TileYJSON;

	public Slider customTexture5TileYSlider;

	protected JSONStorableFloat customTexture5OffsetXJSON;

	public Slider customTexture5OffsetXSlider;

	protected JSONStorableFloat customTexture5OffsetYJSON;

	public Slider customTexture5OffsetYSlider;

	public bool customTexture6IsLinear;

	public bool customTexture6IsNormal;

	public bool customTexture6IsTransparency;

	public Button customTexture6FileBrowseButton;

	public Button customTexture6ReloadButton;

	public Button customTexture6ClearButton;

	public Button customTexture6NullButton;

	public Button customTexture6DefaultButton;

	public Text customTexture6UrlText;

	public Text customTexture6Label;

	protected Texture2D customTexture6;

	protected bool customTexture6IsNull;

	protected JSONStorableUrl customTexture6UrlJSON;

	protected JSONStorableFloat customTexture6TileXJSON;

	public Slider customTexture6TileXSlider;

	protected JSONStorableFloat customTexture6TileYJSON;

	public Slider customTexture6TileYSlider;

	protected JSONStorableFloat customTexture6OffsetXJSON;

	public Slider customTexture6OffsetXSlider;

	protected JSONStorableFloat customTexture6OffsetYJSON;

	public Slider customTexture6OffsetYSlider;

	protected JSONStorableAction createUVTemplateTextureJSON;

	protected JSONStorableAction createSimTemplateTextureJSON;

	protected Material[] rawImageMaterials;

	protected MeshFilter meshFilter;

	protected Renderer[] renderers;

	protected Renderer[] renderers2;

	protected Dictionary<Material, Shader> materialToOriginalShader;

	protected Dictionary<Texture2D, int> textureUseCount;

	protected void RegisterAllocatedObject(UnityEngine.Object o)
	{
		if (Application.isPlaying)
		{
			if (allocatedObjects == null)
			{
				allocatedObjects = new List<UnityEngine.Object>();
			}
			allocatedObjects.Add(o);
		}
	}

	protected void DestroyAllocatedObjects()
	{
		if (!Application.isPlaying || allocatedObjects == null)
		{
			return;
		}
		foreach (UnityEngine.Object allocatedObject in allocatedObjects)
		{
			UnityEngine.Object.Destroy(allocatedObject);
		}
	}

	public bool LoadFromBinaryReader(BinaryReader binReader)
	{
		try
		{
			string text = binReader.ReadString();
			if (text != "MaterialOptions")
			{
				SuperController.LogError("Binary file corrupted. Tried to read MaterialOptions in wrong section");
				return false;
			}
			string text2 = binReader.ReadString();
			if (text2 != "1.0")
			{
				SuperController.LogError("MaterialOptions schema " + text2 + " is not compatible with this version of software");
				return false;
			}
			if (textureGroup1 == null)
			{
				textureGroup1 = new MaterialOptionTextureGroup();
			}
			overrideId = binReader.ReadString();
			int num = binReader.ReadInt32();
			paramMaterialSlots = new int[num];
			textureGroup1.materialSlots = new int[num];
			for (int i = 0; i < num; i++)
			{
				paramMaterialSlots[i] = binReader.ReadInt32();
				textureGroup1.materialSlots[i] = paramMaterialSlots[i];
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Error while loading MaterialOptions from binary reader " + ex);
			return false;
		}
		return true;
	}

	public bool LoadFromBinaryFile(string path)
	{
		bool result = false;
		try
		{
			using (FileEntryStream fileEntryStream = FileManager.OpenStream(path, true))
			{
				using (BinaryReader binReader = new BinaryReader(fileEntryStream.Stream))
				{
					result = LoadFromBinaryReader(binReader);
				}
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Error while loading MaterialOptions from binary file " + path + " " + ex);
		}
		return result;
	}

	public bool StoreToBinaryWriter(BinaryWriter binWriter)
	{
		try
		{
			binWriter.Write("MaterialOptions");
			binWriter.Write("1.0");
			binWriter.Write(overrideId);
			binWriter.Write(paramMaterialSlots.Length);
			for (int i = 0; i < paramMaterialSlots.Length; i++)
			{
				binWriter.Write(paramMaterialSlots[i]);
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Error while storing MaterialOptions to binary writer " + ex);
			return false;
		}
		return true;
	}

	public bool StoreToBinaryFile(string path)
	{
		bool result = false;
		try
		{
			FileManager.AssertNotCalledFromPlugin();
			using (FileStream output = FileManager.OpenStreamForCreate(path))
			{
				using (BinaryWriter binWriter = new BinaryWriter(output))
				{
					result = StoreToBinaryWriter(binWriter);
				}
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Error while storing MaterialOptions to binary file " + path + " " + ex);
		}
		return result;
	}

	protected void SyncCustomName(string s)
	{
		if (origOverrideId == null)
		{
			origOverrideId = overrideId;
		}
		if (s != null && s != string.Empty)
		{
			overrideId = "+Material" + s;
		}
		else
		{
			overrideId = origOverrideId;
		}
	}

	protected void SyncRenderQueue(float f)
	{
		int materialRenderQueue = Mathf.FloorToInt(f);
		SetMaterialRenderQueue(materialRenderQueue);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			otherMaterialOptions.renderQueueJSON.val = renderQueueJSON.val;
		}
	}

	protected void SyncHideMaterial(bool b)
	{
		SetMaterialHide(b);
		if (!b)
		{
			int materialRenderQueue = Mathf.FloorToInt(renderQueueJSON.val);
			SetMaterialRenderQueue(materialRenderQueue);
		}
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			otherMaterialOptions.hideMaterialJSON.val = hideMaterialJSON.val;
		}
	}

	protected virtual void SyncLinkToOtherMaterials(bool b)
	{
		_linkToOtherMaterials = b;
		if (_linkToOtherMaterials)
		{
			foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
			{
				otherMaterialOptions.renderQueueJSON.val = renderQueueJSON.val;
				if (otherMaterialOptions.hideMaterialJSON != null && hideMaterialJSON != null)
				{
					otherMaterialOptions.hideMaterialJSON.val = hideMaterialJSON.val;
				}
				if (otherMaterialOptions.color1JSONParam != null && color1JSONParam != null)
				{
					otherMaterialOptions.color1JSONParam.val = color1JSONParam.val;
				}
				if (otherMaterialOptions.color2JSONParam != null && color2JSONParam != null)
				{
					otherMaterialOptions.color2JSONParam.val = color2JSONParam.val;
				}
				if (otherMaterialOptions.color3JSONParam != null && color3JSONParam != null)
				{
					otherMaterialOptions.color3JSONParam.val = color3JSONParam.val;
				}
				if (otherMaterialOptions.param1JSONParam != null && param1JSONParam != null)
				{
					otherMaterialOptions.param1JSONParam.val = param1JSONParam.val;
				}
				if (otherMaterialOptions.param2JSONParam != null && param2JSONParam != null)
				{
					otherMaterialOptions.param2JSONParam.val = param2JSONParam.val;
				}
				if (otherMaterialOptions.param3JSONParam != null && param3JSONParam != null)
				{
					otherMaterialOptions.param3JSONParam.val = param3JSONParam.val;
				}
				if (otherMaterialOptions.param4JSONParam != null && param4JSONParam != null)
				{
					otherMaterialOptions.param4JSONParam.val = param4JSONParam.val;
				}
				if (otherMaterialOptions.param5JSONParam != null && param5JSONParam != null)
				{
					otherMaterialOptions.param5JSONParam.val = param5JSONParam.val;
				}
				if (otherMaterialOptions.param6JSONParam != null && param6JSONParam != null)
				{
					otherMaterialOptions.param6JSONParam.val = param6JSONParam.val;
				}
				if (otherMaterialOptions.param7JSONParam != null && param7JSONParam != null)
				{
					otherMaterialOptions.param7JSONParam.val = param7JSONParam.val;
				}
				if (otherMaterialOptions.param8JSONParam != null && param8JSONParam != null)
				{
					otherMaterialOptions.param8JSONParam.val = param8JSONParam.val;
				}
				if (otherMaterialOptions.param9JSONParam != null && param9JSONParam != null)
				{
					otherMaterialOptions.param9JSONParam.val = param9JSONParam.val;
				}
				if (otherMaterialOptions.param10JSONParam != null && param10JSONParam != null)
				{
					otherMaterialOptions.param10JSONParam.val = param10JSONParam.val;
				}
				if (otherMaterialOptions.textureGroup1JSON != null && textureGroup1JSON != null)
				{
					otherMaterialOptions.textureGroup1JSON.val = textureGroup1JSON.val;
				}
				if (otherMaterialOptions.textureGroup2JSON != null && textureGroup2JSON != null)
				{
					otherMaterialOptions.textureGroup2JSON.val = textureGroup2JSON.val;
				}
				if (otherMaterialOptions.textureGroup3JSON != null && textureGroup3JSON != null)
				{
					otherMaterialOptions.textureGroup3JSON.val = textureGroup3JSON.val;
				}
				if (otherMaterialOptions.textureGroup4JSON != null && textureGroup4JSON != null)
				{
					otherMaterialOptions.textureGroup4JSON.val = textureGroup4JSON.val;
				}
				if (otherMaterialOptions.textureGroup5JSON != null && textureGroup5JSON != null)
				{
					otherMaterialOptions.textureGroup5JSON.val = textureGroup5JSON.val;
				}
				if (otherMaterialOptions.customTexture1UrlJSON != null && customTexture1UrlJSON != null)
				{
					otherMaterialOptions.customTexture1UrlJSON.val = customTexture1UrlJSON.val;
					otherMaterialOptions.customTexture1TileXJSON.val = customTexture1TileXJSON.val;
					otherMaterialOptions.customTexture1TileYJSON.val = customTexture1TileYJSON.val;
					otherMaterialOptions.customTexture1OffsetXJSON.val = customTexture1OffsetXJSON.val;
					otherMaterialOptions.customTexture1OffsetYJSON.val = customTexture1OffsetYJSON.val;
				}
				if (otherMaterialOptions.customTexture2UrlJSON != null && customTexture2UrlJSON != null)
				{
					otherMaterialOptions.customTexture2UrlJSON.val = customTexture2UrlJSON.val;
					otherMaterialOptions.customTexture2TileXJSON.val = customTexture2TileXJSON.val;
					otherMaterialOptions.customTexture2TileYJSON.val = customTexture2TileYJSON.val;
					otherMaterialOptions.customTexture2OffsetXJSON.val = customTexture2OffsetXJSON.val;
					otherMaterialOptions.customTexture2OffsetYJSON.val = customTexture2OffsetYJSON.val;
				}
				if (otherMaterialOptions.customTexture3UrlJSON != null && customTexture3UrlJSON != null)
				{
					otherMaterialOptions.customTexture3UrlJSON.val = customTexture3UrlJSON.val;
					otherMaterialOptions.customTexture3TileXJSON.val = customTexture3TileXJSON.val;
					otherMaterialOptions.customTexture3TileYJSON.val = customTexture3TileYJSON.val;
					otherMaterialOptions.customTexture3OffsetXJSON.val = customTexture3OffsetXJSON.val;
					otherMaterialOptions.customTexture3OffsetYJSON.val = customTexture3OffsetYJSON.val;
				}
				if (otherMaterialOptions.customTexture4UrlJSON != null && customTexture4UrlJSON != null)
				{
					otherMaterialOptions.customTexture4UrlJSON.val = customTexture4UrlJSON.val;
					otherMaterialOptions.customTexture4TileXJSON.val = customTexture4TileXJSON.val;
					otherMaterialOptions.customTexture4TileYJSON.val = customTexture4TileYJSON.val;
					otherMaterialOptions.customTexture4OffsetXJSON.val = customTexture4OffsetXJSON.val;
					otherMaterialOptions.customTexture4OffsetYJSON.val = customTexture4OffsetYJSON.val;
				}
				if (otherMaterialOptions.customTexture5UrlJSON != null && customTexture5UrlJSON != null)
				{
					otherMaterialOptions.customTexture5UrlJSON.val = customTexture5UrlJSON.val;
					otherMaterialOptions.customTexture5TileXJSON.val = customTexture5TileXJSON.val;
					otherMaterialOptions.customTexture5TileYJSON.val = customTexture5TileYJSON.val;
					otherMaterialOptions.customTexture5OffsetXJSON.val = customTexture5OffsetXJSON.val;
					otherMaterialOptions.customTexture5OffsetYJSON.val = customTexture5OffsetYJSON.val;
				}
				if (otherMaterialOptions.customTexture6UrlJSON != null && customTexture6UrlJSON != null)
				{
					otherMaterialOptions.customTexture6UrlJSON.val = customTexture6UrlJSON.val;
					otherMaterialOptions.customTexture6TileXJSON.val = customTexture6TileXJSON.val;
					otherMaterialOptions.customTexture6TileYJSON.val = customTexture6TileYJSON.val;
					otherMaterialOptions.customTexture6OffsetXJSON.val = customTexture6OffsetXJSON.val;
					otherMaterialOptions.customTexture6OffsetYJSON.val = customTexture6OffsetYJSON.val;
				}
			}
		}
		foreach (MaterialOptions otherMaterialOptions2 in otherMaterialOptionsList)
		{
			otherMaterialOptions2.linkToOtherMaterialsJSON.val = linkToOtherMaterialsJSON.val;
		}
	}

	public void OpenTextureFolderInExplorer()
	{
		string input = customTextureFolder;
		input = Regex.Replace(input, ".*:\\\\", string.Empty);
		if (!FileManager.DirectoryExists(input) && FileManager.IsSecureWritePath(input))
		{
			FileManager.CreateDirectory(input);
		}
		if (FileManager.DirectoryExists(input))
		{
			SuperController.singleton.OpenFolderInExplorer(input);
		}
	}

	public virtual void SetCustomTextureFolder(string path)
	{
		if (customTextureFolder != null && FileManager.IsDirectoryInPackage(customTextureFolder))
		{
			customTexturePackageFolder = customTextureFolder;
		}
		customTextureFolder = path;
		if (customTexture1UrlJSON != null)
		{
			customTexture1UrlJSON.suggestedPath = customTextureFolder;
		}
		if (customTexture2UrlJSON != null)
		{
			customTexture2UrlJSON.suggestedPath = customTextureFolder;
		}
		if (customTexture3UrlJSON != null)
		{
			customTexture3UrlJSON.suggestedPath = customTextureFolder;
		}
		if (customTexture4UrlJSON != null)
		{
			customTexture4UrlJSON.suggestedPath = customTextureFolder;
		}
		if (customTexture5UrlJSON != null)
		{
			customTexture5UrlJSON.suggestedPath = customTextureFolder;
		}
		if (customTexture6UrlJSON != null)
		{
			customTexture6UrlJSON.suggestedPath = customTextureFolder;
		}
	}

	protected void QueueCustomTexture(string url, bool forceReload, bool isLinear, bool isNormalMap, bool isTransparency, ImageLoaderThreaded.ImageLoaderCallback callback)
	{
		if (ImageLoaderThreaded.singleton != null)
		{
			ImageLoaderThreaded.QueuedImage queuedImage = new ImageLoaderThreaded.QueuedImage();
			queuedImage.imgPath = url;
			queuedImage.forceReload = forceReload;
			queuedImage.createMipMaps = true;
			queuedImage.isNormalMap = isNormalMap;
			queuedImage.isThumbnail = false;
			queuedImage.linear = isLinear;
			queuedImage.createAlphaFromGrayscale = isTransparency;
			queuedImage.compress = !queuedImage.isNormalMap;
			queuedImage.callback = callback;
			ImageLoaderThreaded.singleton.QueueImage(queuedImage);
		}
	}

	protected void OnTexture1Loaded(ImageLoaderThreaded.QueuedImage qi)
	{
		if (!qi.hadError)
		{
			if (this != null)
			{
				RegisterTexture(qi.tex);
				DeregisterTexture(customTexture1);
				customTexture1 = qi.tex;
				customTexture1IsNull = qi.imgPath == "NULL";
				SetTextureGroupSet(textureGroup1, currentTextureGroup1Set, 0, qi.tex, customTexture1IsNull);
			}
		}
		else
		{
			SuperController.LogError("Error during texture load: " + qi.errorText);
		}
	}

	protected void SyncCustomTexture1Url(JSONStorableString jstr)
	{
		JSONStorableUrl jSONStorableUrl = jstr as JSONStorableUrl;
		string val = jSONStorableUrl.val;
		if (val != null && val != string.Empty)
		{
			if (Regex.IsMatch(val, "^http"))
			{
				SuperController.LogError("Texture load does not currently support http image urls");
			}
			else
			{
				QueueCustomTexture(val, jSONStorableUrl.valueSetFromBrowse, customTexture1IsLinear, customTexture1IsNormal, customTexture1IsTransparency, OnTexture1Loaded);
			}
		}
		else
		{
			QueueCustomTexture(null, jSONStorableUrl.valueSetFromBrowse, customTexture1IsLinear, customTexture1IsNormal, customTexture1IsTransparency, OnTexture1Loaded);
		}
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.customTexture1UrlJSON != null)
			{
				otherMaterialOptions.customTexture1UrlJSON.val = customTexture1UrlJSON.val;
			}
		}
	}

	protected void SetCustomTexture1ToNull()
	{
		if (customTexture1UrlJSON != null)
		{
			customTexture1UrlJSON.val = "NULL";
		}
	}

	protected void SyncCustomTexture1Tile(float f)
	{
		Vector2 scale = default(Vector2);
		scale.x = customTexture1TileXJSON.val;
		scale.y = customTexture1TileYJSON.val;
		SetTextureGroupTextureScale(textureGroup1, 0, scale);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.customTexture1TileXJSON != null)
			{
				otherMaterialOptions.customTexture1TileXJSON.val = customTexture1TileXJSON.val;
				otherMaterialOptions.customTexture1TileYJSON.val = customTexture1TileYJSON.val;
			}
		}
	}

	protected void SyncCustomTexture1Offset(float f)
	{
		Vector2 offset = default(Vector2);
		offset.x = customTexture1OffsetXJSON.val;
		offset.y = customTexture1OffsetYJSON.val;
		SetTextureGroupTextureOffset(textureGroup1, 0, offset);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.customTexture1OffsetXJSON != null)
			{
				otherMaterialOptions.customTexture1OffsetXJSON.val = customTexture1OffsetXJSON.val;
				otherMaterialOptions.customTexture1OffsetYJSON.val = customTexture1OffsetYJSON.val;
			}
		}
	}

	protected void OnTexture2Loaded(ImageLoaderThreaded.QueuedImage qi)
	{
		if (!qi.hadError)
		{
			if (this != null)
			{
				RegisterTexture(qi.tex);
				DeregisterTexture(customTexture2);
				customTexture2 = qi.tex;
				customTexture2IsNull = qi.imgPath == "NULL";
				SetTextureGroupSet(textureGroup1, currentTextureGroup1Set, 1, qi.tex, customTexture2IsNull);
			}
		}
		else
		{
			SuperController.LogError("Error during texture load: " + qi.errorText);
		}
	}

	protected void SyncCustomTexture2Url(JSONStorableString jstr)
	{
		JSONStorableUrl jSONStorableUrl = jstr as JSONStorableUrl;
		string val = jSONStorableUrl.val;
		if (val != null && val != string.Empty)
		{
			if (Regex.IsMatch(val, "^http"))
			{
				SuperController.LogError("Texture load does not currently support http image urls");
			}
			else
			{
				QueueCustomTexture(val, jSONStorableUrl.valueSetFromBrowse, customTexture2IsLinear, customTexture2IsNormal, customTexture2IsTransparency, OnTexture2Loaded);
			}
		}
		else
		{
			QueueCustomTexture(null, jSONStorableUrl.valueSetFromBrowse, customTexture2IsLinear, customTexture2IsNormal, customTexture2IsTransparency, OnTexture2Loaded);
		}
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.customTexture2UrlJSON != null)
			{
				otherMaterialOptions.customTexture2UrlJSON.val = customTexture2UrlJSON.val;
			}
		}
	}

	protected void SetCustomTexture2ToNull()
	{
		if (customTexture2UrlJSON != null)
		{
			customTexture2UrlJSON.val = "NULL";
		}
	}

	protected void SyncCustomTexture2Tile(float f)
	{
		Vector2 scale = default(Vector2);
		scale.x = customTexture2TileXJSON.val;
		scale.y = customTexture2TileYJSON.val;
		SetTextureGroupTextureScale(textureGroup1, 1, scale);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.customTexture2TileXJSON != null)
			{
				otherMaterialOptions.customTexture2TileXJSON.val = customTexture2TileXJSON.val;
				otherMaterialOptions.customTexture2TileYJSON.val = customTexture2TileYJSON.val;
			}
		}
	}

	protected void SyncCustomTexture2Offset(float f)
	{
		Vector2 offset = default(Vector2);
		offset.x = customTexture2OffsetXJSON.val;
		offset.y = customTexture2OffsetYJSON.val;
		SetTextureGroupTextureOffset(textureGroup1, 1, offset);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.customTexture2OffsetXJSON != null)
			{
				otherMaterialOptions.customTexture2OffsetXJSON.val = customTexture2OffsetXJSON.val;
				otherMaterialOptions.customTexture2OffsetYJSON.val = customTexture2OffsetYJSON.val;
			}
		}
	}

	protected void OnTexture3Loaded(ImageLoaderThreaded.QueuedImage qi)
	{
		if (!qi.hadError)
		{
			if (this != null)
			{
				RegisterTexture(qi.tex);
				DeregisterTexture(customTexture3);
				customTexture3 = qi.tex;
				customTexture3IsNull = qi.imgPath == "NULL";
				SetTextureGroupSet(textureGroup1, currentTextureGroup1Set, 2, qi.tex, customTexture3IsNull);
			}
		}
		else
		{
			SuperController.LogError("Error during texture load: " + qi.errorText);
		}
	}

	protected void SyncCustomTexture3Url(JSONStorableString jstr)
	{
		JSONStorableUrl jSONStorableUrl = jstr as JSONStorableUrl;
		string val = jSONStorableUrl.val;
		if (val != null && val != string.Empty)
		{
			if (Regex.IsMatch(val, "^http"))
			{
				SuperController.LogError("Texture load does not currently support http image urls");
			}
			else
			{
				QueueCustomTexture(val, jSONStorableUrl.valueSetFromBrowse, customTexture3IsLinear, customTexture3IsNormal, customTexture3IsTransparency, OnTexture3Loaded);
			}
		}
		else
		{
			QueueCustomTexture(null, jSONStorableUrl.valueSetFromBrowse, customTexture3IsLinear, customTexture3IsNormal, customTexture3IsTransparency, OnTexture3Loaded);
		}
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.customTexture3UrlJSON != null)
			{
				otherMaterialOptions.customTexture3UrlJSON.val = customTexture3UrlJSON.val;
			}
		}
	}

	protected void SetCustomTexture3ToNull()
	{
		if (customTexture3UrlJSON != null)
		{
			customTexture3UrlJSON.val = "NULL";
		}
	}

	protected void SyncCustomTexture3Tile(float f)
	{
		Vector2 scale = default(Vector2);
		scale.x = customTexture3TileXJSON.val;
		scale.y = customTexture3TileYJSON.val;
		SetTextureGroupTextureScale(textureGroup1, 2, scale);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.customTexture3TileXJSON != null)
			{
				otherMaterialOptions.customTexture3TileXJSON.val = customTexture3TileXJSON.val;
				otherMaterialOptions.customTexture3TileYJSON.val = customTexture3TileYJSON.val;
			}
		}
	}

	protected void SyncCustomTexture3Offset(float f)
	{
		Vector2 offset = default(Vector2);
		offset.x = customTexture3OffsetXJSON.val;
		offset.y = customTexture3OffsetYJSON.val;
		SetTextureGroupTextureOffset(textureGroup1, 2, offset);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.customTexture3OffsetXJSON != null)
			{
				otherMaterialOptions.customTexture3OffsetXJSON.val = customTexture3OffsetXJSON.val;
				otherMaterialOptions.customTexture3OffsetYJSON.val = customTexture3OffsetYJSON.val;
			}
		}
	}

	protected void OnTexture4Loaded(ImageLoaderThreaded.QueuedImage qi)
	{
		if (!qi.hadError)
		{
			if (this != null)
			{
				RegisterTexture(qi.tex);
				DeregisterTexture(customTexture4);
				customTexture4 = qi.tex;
				customTexture4IsNull = qi.imgPath == "NULL";
				SetTextureGroupSet(textureGroup1, currentTextureGroup1Set, 3, qi.tex, customTexture4IsNull);
			}
		}
		else
		{
			SuperController.LogError("Error during texture load: " + qi.errorText);
		}
	}

	protected void SyncCustomTexture4Url(JSONStorableString jstr)
	{
		JSONStorableUrl jSONStorableUrl = jstr as JSONStorableUrl;
		string val = jSONStorableUrl.val;
		if (val != null && val != string.Empty)
		{
			if (Regex.IsMatch(val, "^http"))
			{
				SuperController.LogError("Texture load does not currently support http image urls");
			}
			else
			{
				QueueCustomTexture(val, jSONStorableUrl.valueSetFromBrowse, customTexture4IsLinear, customTexture4IsNormal, customTexture4IsTransparency, OnTexture4Loaded);
			}
		}
		else
		{
			QueueCustomTexture(null, jSONStorableUrl.valueSetFromBrowse, customTexture4IsLinear, customTexture4IsNormal, customTexture4IsTransparency, OnTexture4Loaded);
		}
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.customTexture4UrlJSON != null)
			{
				otherMaterialOptions.customTexture4UrlJSON.val = customTexture4UrlJSON.val;
			}
		}
	}

	protected void SetCustomTexture4ToNull()
	{
		if (customTexture4UrlJSON != null)
		{
			customTexture4UrlJSON.val = "NULL";
		}
	}

	protected void SyncCustomTexture4Tile(float f)
	{
		Vector2 scale = default(Vector2);
		scale.x = customTexture4TileXJSON.val;
		scale.y = customTexture4TileYJSON.val;
		SetTextureGroupTextureScale(textureGroup1, 3, scale);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.customTexture4TileXJSON != null)
			{
				otherMaterialOptions.customTexture4TileXJSON.val = customTexture4TileXJSON.val;
				otherMaterialOptions.customTexture4TileYJSON.val = customTexture4TileYJSON.val;
			}
		}
	}

	protected void SyncCustomTexture4Offset(float f)
	{
		Vector2 offset = default(Vector2);
		offset.x = customTexture4OffsetXJSON.val;
		offset.y = customTexture4OffsetYJSON.val;
		SetTextureGroupTextureOffset(textureGroup1, 3, offset);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.customTexture4OffsetXJSON != null)
			{
				otherMaterialOptions.customTexture4OffsetXJSON.val = customTexture4OffsetXJSON.val;
				otherMaterialOptions.customTexture4OffsetYJSON.val = customTexture4OffsetYJSON.val;
			}
		}
	}

	protected void OnTexture5Loaded(ImageLoaderThreaded.QueuedImage qi)
	{
		if (!qi.hadError)
		{
			if (this != null)
			{
				RegisterTexture(qi.tex);
				DeregisterTexture(customTexture5);
				customTexture5 = qi.tex;
				customTexture5IsNull = qi.imgPath == "NULL";
				SetTextureGroupSet(textureGroup1, currentTextureGroup1Set, 4, qi.tex, customTexture5IsNull);
			}
		}
		else
		{
			SuperController.LogError("Error during texture load: " + qi.errorText);
		}
	}

	protected void SyncCustomTexture5Url(JSONStorableString jstr)
	{
		JSONStorableUrl jSONStorableUrl = jstr as JSONStorableUrl;
		string val = jSONStorableUrl.val;
		if (val != null && val != string.Empty)
		{
			if (Regex.IsMatch(val, "^http"))
			{
				SuperController.LogError("Texture load does not currently support http image urls");
			}
			else
			{
				QueueCustomTexture(val, jSONStorableUrl.valueSetFromBrowse, customTexture5IsLinear, customTexture5IsNormal, customTexture5IsTransparency, OnTexture5Loaded);
			}
		}
		else
		{
			QueueCustomTexture(null, jSONStorableUrl.valueSetFromBrowse, customTexture5IsLinear, customTexture5IsNormal, customTexture5IsTransparency, OnTexture5Loaded);
		}
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.customTexture5UrlJSON != null)
			{
				otherMaterialOptions.customTexture5UrlJSON.val = customTexture5UrlJSON.val;
			}
		}
	}

	protected void SetCustomTexture5ToNull()
	{
		if (customTexture5UrlJSON != null)
		{
			customTexture5UrlJSON.val = "NULL";
		}
	}

	protected void SyncCustomTexture5Tile(float f)
	{
		Vector2 scale = default(Vector2);
		scale.x = customTexture5TileXJSON.val;
		scale.y = customTexture5TileYJSON.val;
		SetTextureGroupTextureScale(textureGroup1, 4, scale);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.customTexture5TileXJSON != null)
			{
				otherMaterialOptions.customTexture5TileXJSON.val = customTexture5TileXJSON.val;
				otherMaterialOptions.customTexture5TileYJSON.val = customTexture5TileYJSON.val;
			}
		}
	}

	protected void SyncCustomTexture5Offset(float f)
	{
		Vector2 offset = default(Vector2);
		offset.x = customTexture5OffsetXJSON.val;
		offset.y = customTexture5OffsetYJSON.val;
		SetTextureGroupTextureOffset(textureGroup1, 4, offset);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.customTexture5OffsetXJSON != null)
			{
				otherMaterialOptions.customTexture5OffsetXJSON.val = customTexture5OffsetXJSON.val;
				otherMaterialOptions.customTexture5OffsetYJSON.val = customTexture5OffsetYJSON.val;
			}
		}
	}

	protected void OnTexture6Loaded(ImageLoaderThreaded.QueuedImage qi)
	{
		if (!qi.hadError)
		{
			if (this != null)
			{
				RegisterTexture(qi.tex);
				DeregisterTexture(customTexture6);
				customTexture6 = qi.tex;
				customTexture6IsNull = qi.imgPath == "NULL";
				SetTextureGroupSet(textureGroup1, currentTextureGroup1Set, 5, qi.tex, customTexture6IsNull);
			}
		}
		else
		{
			SuperController.LogError("Error during texture load: " + qi.errorText);
		}
	}

	protected void SyncCustomTexture6Url(JSONStorableString jstr)
	{
		JSONStorableUrl jSONStorableUrl = jstr as JSONStorableUrl;
		string val = jSONStorableUrl.val;
		if (val != null && val != string.Empty)
		{
			if (Regex.IsMatch(val, "^http"))
			{
				SuperController.LogError("Texture load does not currently support http image urls");
			}
			else
			{
				QueueCustomTexture(val, jSONStorableUrl.valueSetFromBrowse, customTexture6IsLinear, customTexture6IsNormal, customTexture6IsTransparency, OnTexture6Loaded);
			}
		}
		else
		{
			QueueCustomTexture(null, jSONStorableUrl.valueSetFromBrowse, customTexture6IsLinear, customTexture6IsNormal, customTexture6IsTransparency, OnTexture6Loaded);
		}
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.customTexture6UrlJSON != null)
			{
				otherMaterialOptions.customTexture6UrlJSON.val = customTexture6UrlJSON.val;
			}
		}
	}

	protected void SetCustomTexture6ToNull()
	{
		if (customTexture6UrlJSON != null)
		{
			customTexture6UrlJSON.val = "NULL";
		}
	}

	protected void SyncCustomTexture6Tile(float f)
	{
		Vector2 scale = default(Vector2);
		scale.x = customTexture6TileXJSON.val;
		scale.y = customTexture6TileYJSON.val;
		SetTextureGroupTextureScale(textureGroup1, 5, scale);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.customTexture6TileXJSON != null)
			{
				otherMaterialOptions.customTexture6TileXJSON.val = customTexture6TileXJSON.val;
				otherMaterialOptions.customTexture6TileYJSON.val = customTexture6TileYJSON.val;
			}
		}
	}

	protected void SyncCustomTexture6Offset(float f)
	{
		Vector2 offset = default(Vector2);
		offset.x = customTexture6OffsetXJSON.val;
		offset.y = customTexture6OffsetYJSON.val;
		SetTextureGroupTextureOffset(textureGroup1, 5, offset);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.customTexture6OffsetXJSON != null)
			{
				otherMaterialOptions.customTexture6OffsetXJSON.val = customTexture6OffsetXJSON.val;
				otherMaterialOptions.customTexture6OffsetYJSON.val = customTexture6OffsetYJSON.val;
			}
		}
	}

	protected void BeginBrowse(JSONStorableUrl jsurl)
	{
		if (!FileManager.IsDirectoryInPackage(customTextureFolder) && !customTextureFolder.Contains(":") && FileManager.IsSecureWritePath(customTextureFolder) && !FileManager.DirectoryExists(customTextureFolder))
		{
			FileManager.CreateDirectory(customTextureFolder);
		}
		List<ShortCut> shortCutsForDirectory = FileManager.GetShortCutsForDirectory(customTextureFolder, true, true, true, true);
		ShortCut shortCut = new ShortCut();
		shortCut.displayName = "Root";
		shortCut.path = Path.GetFullPath(".");
		shortCutsForDirectory.Insert(0, shortCut);
		if (customTexturePackageFolder != null)
		{
			VarDirectoryEntry varDirectoryEntry = FileManager.GetVarDirectoryEntry(customTexturePackageFolder);
			if (varDirectoryEntry != null)
			{
				ShortCut shortCut2 = new ShortCut();
				shortCut2.package = varDirectoryEntry.Package.Uid;
				shortCut2.displayName = varDirectoryEntry.InternalSlashPath;
				shortCut2.path = varDirectoryEntry.SlashPath;
				shortCutsForDirectory.Add(shortCut2);
			}
		}
		jsurl.shortCuts = shortCutsForDirectory;
	}

	protected void CreateUVTemplateTexture(Mesh mesh, System.Drawing.Color backgroundColor, System.Drawing.Color lineColor, string suffix = "UVTemplate")
	{
		Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
		int[] array = paramMaterialSlots;
		foreach (int key in array)
		{
			dictionary.Add(key, true);
		}
		int num = 4096;
		int num2 = 4096;
		Texture2D texture2D = new Texture2D(num, num2, TextureFormat.BGRA32, false, false);
		string input = base.storeId + suffix;
		input = (texture2D.name = Regex.Replace(input, ".*:", string.Empty));
		Bitmap bitmap = new Bitmap(4096, 4096, PixelFormat.Format32bppArgb);
		System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);
		Rectangle rect = new Rectangle(0, 0, num, num2);
		Brush brush = new SolidBrush(backgroundColor);
		Brush brush2 = new SolidBrush(lineColor);
		Pen pen = new Pen(brush2, 2f);
		graphics.FillRectangle(brush, rect);
		Vector2[] uv = mesh.uv;
		for (int j = 0; j < mesh.subMeshCount; j++)
		{
			if (dictionary.ContainsKey(j))
			{
				int[] triangles = mesh.GetTriangles(j);
				for (int k = 0; k < triangles.Length; k += 3)
				{
					int num3 = triangles[k];
					int num4 = triangles[k + 1];
					int num5 = triangles[k + 2];
					Vector2 vector = uv[num3];
					vector.x *= num;
					vector.y *= num2;
					Vector2 vector2 = uv[num4];
					vector2.x *= num;
					vector2.y *= num2;
					Vector2 vector3 = uv[num5];
					vector3.x *= num;
					vector3.y *= num2;
					graphics.DrawLine(pen, vector.x, vector.y, vector2.x, vector2.y);
					graphics.DrawLine(pen, vector2.x, vector2.y, vector3.x, vector3.y);
					graphics.DrawLine(pen, vector3.x, vector3.y, vector.x, vector.y);
					graphics.DrawLine(pen, vector.x + (float)num, vector.y, vector2.x + (float)num, vector2.y);
					graphics.DrawLine(pen, vector2.x + (float)num, vector2.y, vector3.x + (float)num, vector3.y);
					graphics.DrawLine(pen, vector3.x + (float)num, vector3.y, vector.x + (float)num, vector.y);
					graphics.DrawLine(pen, vector.x + (float)num, vector.y + (float)num2, vector2.x + (float)num, vector2.y + (float)num2);
					graphics.DrawLine(pen, vector2.x + (float)num, vector2.y + (float)num2, vector3.x + (float)num, vector3.y + (float)num2);
					graphics.DrawLine(pen, vector3.x + (float)num, vector3.y + (float)num2, vector.x + (float)num, vector.y + (float)num2);
					graphics.DrawLine(pen, vector.x + (float)num, vector.y - (float)num2, vector2.x + (float)num, vector2.y - (float)num2);
					graphics.DrawLine(pen, vector2.x + (float)num, vector2.y - (float)num2, vector3.x + (float)num, vector3.y - (float)num2);
					graphics.DrawLine(pen, vector3.x + (float)num, vector3.y - (float)num2, vector.x + (float)num, vector.y - (float)num2);
					graphics.DrawLine(pen, vector.x - (float)num, vector.y, vector2.x - (float)num, vector2.y);
					graphics.DrawLine(pen, vector2.x - (float)num, vector2.y, vector3.x - (float)num, vector3.y);
					graphics.DrawLine(pen, vector3.x - (float)num, vector3.y, vector.x - (float)num, vector.y);
					graphics.DrawLine(pen, vector.x - (float)num, vector.y - (float)num2, vector2.x - (float)num, vector2.y - (float)num2);
					graphics.DrawLine(pen, vector2.x - (float)num, vector2.y - (float)num2, vector3.x - (float)num, vector3.y - (float)num2);
					graphics.DrawLine(pen, vector3.x - (float)num, vector3.y - (float)num2, vector.x - (float)num, vector.y - (float)num2);
					graphics.DrawLine(pen, vector.x - (float)num, vector.y + (float)num2, vector2.x - (float)num, vector2.y + (float)num2);
					graphics.DrawLine(pen, vector2.x - (float)num, vector2.y + (float)num2, vector3.x - (float)num, vector3.y + (float)num2);
					graphics.DrawLine(pen, vector3.x - (float)num, vector3.y + (float)num2, vector.x - (float)num, vector.y + (float)num2);
					graphics.DrawLine(pen, vector.x, vector.y + (float)num2, vector2.x, vector2.y + (float)num2);
					graphics.DrawLine(pen, vector2.x, vector2.y + (float)num2, vector3.x, vector3.y + (float)num2);
					graphics.DrawLine(pen, vector3.x, vector3.y + (float)num2, vector.x, vector.y + (float)num2);
					graphics.DrawLine(pen, vector.x, vector.y - (float)num2, vector2.x, vector2.y - (float)num2);
					graphics.DrawLine(pen, vector2.x, vector2.y - (float)num2, vector3.x, vector3.y - (float)num2);
					graphics.DrawLine(pen, vector3.x, vector3.y - (float)num2, vector.x, vector.y - (float)num2);
				}
			}
		}
		BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
		int size = num * num2 * 4;
		texture2D.LoadRawTextureData(bitmapData.Scan0, size);
		bitmap.UnlockBits(bitmapData);
		byte[] bytes = texture2D.EncodeToPNG();
		string input2 = customTextureFolder;
		input2 = Regex.Replace(input2, ".*:\\\\", string.Empty);
		if (input2 != string.Empty && !Regex.IsMatch(input2, "/$"))
		{
			input2 += "/";
		}
		if (!FileManager.DirectoryExists(input2) && FileManager.IsSecureWritePath(input2))
		{
			FileManager.CreateDirectory(input2);
		}
		if (FileManager.DirectoryExists(input2))
		{
			FileManager.WriteAllBytes(input2 + input + ".png", bytes);
		}
		pen.Dispose();
		brush.Dispose();
		brush2.Dispose();
		graphics.Dispose();
		bitmap.Dispose();
		UnityEngine.Object.Destroy(texture2D);
	}

	public virtual Mesh GetMesh()
	{
		Mesh result = null;
		if (meshFilter != null)
		{
			result = meshFilter.mesh;
		}
		return result;
	}

	public virtual void CreateUVTemplateTexture()
	{
		Mesh mesh = GetMesh();
		if (mesh != null)
		{
			CreateUVTemplateTexture(mesh, System.Drawing.Color.White, System.Drawing.Color.Black);
		}
	}

	public virtual void CreateSimTemplateTexture()
	{
		Mesh mesh = GetMesh();
		if (mesh != null)
		{
			CreateUVTemplateTexture(mesh, System.Drawing.Color.Black, System.Drawing.Color.Blue, "SimTemplate");
		}
	}

	public void CopyUI()
	{
		if (copyUIFrom != null)
		{
			color1DisplayNameText = copyUIFrom.color1DisplayNameText;
			color1Picker = copyUIFrom.color1Picker;
			color1Container = copyUIFrom.color1Container;
			color2DisplayNameText = copyUIFrom.color2DisplayNameText;
			color2Picker = copyUIFrom.color2Picker;
			color2Container = copyUIFrom.color2Container;
			color3DisplayNameText = copyUIFrom.color3DisplayNameText;
			color3Picker = copyUIFrom.color3Picker;
			color3Container = copyUIFrom.color3Container;
			param1DisplayNameText = copyUIFrom.param1DisplayNameText;
			param1DisplayNameTextAlt = copyUIFrom.param1DisplayNameTextAlt;
			param1Slider = copyUIFrom.param1Slider;
			param1SliderAlt = copyUIFrom.param1SliderAlt;
			param2DisplayNameText = copyUIFrom.param2DisplayNameText;
			param2DisplayNameTextAlt = copyUIFrom.param2DisplayNameTextAlt;
			param2Slider = copyUIFrom.param2Slider;
			param2SliderAlt = copyUIFrom.param2SliderAlt;
			param3DisplayNameText = copyUIFrom.param3DisplayNameText;
			param3DisplayNameTextAlt = copyUIFrom.param3DisplayNameTextAlt;
			param3Slider = copyUIFrom.param3Slider;
			param3SliderAlt = copyUIFrom.param3SliderAlt;
			param4DisplayNameText = copyUIFrom.param4DisplayNameText;
			param4DisplayNameTextAlt = copyUIFrom.param4DisplayNameTextAlt;
			param4Slider = copyUIFrom.param4Slider;
			param4SliderAlt = copyUIFrom.param4SliderAlt;
			param5DisplayNameText = copyUIFrom.param5DisplayNameText;
			param5DisplayNameTextAlt = copyUIFrom.param5DisplayNameTextAlt;
			param5Slider = copyUIFrom.param5Slider;
			param5SliderAlt = copyUIFrom.param5SliderAlt;
			param6DisplayNameText = copyUIFrom.param6DisplayNameText;
			param6DisplayNameTextAlt = copyUIFrom.param6DisplayNameTextAlt;
			param6Slider = copyUIFrom.param6Slider;
			param6SliderAlt = copyUIFrom.param6SliderAlt;
			param7DisplayNameText = copyUIFrom.param7DisplayNameText;
			param7DisplayNameTextAlt = copyUIFrom.param7DisplayNameTextAlt;
			param7Slider = copyUIFrom.param7Slider;
			param7SliderAlt = copyUIFrom.param7SliderAlt;
			param8DisplayNameText = copyUIFrom.param8DisplayNameText;
			param8DisplayNameTextAlt = copyUIFrom.param8DisplayNameTextAlt;
			param8Slider = copyUIFrom.param8Slider;
			param8SliderAlt = copyUIFrom.param8SliderAlt;
			param9DisplayNameText = copyUIFrom.param9DisplayNameText;
			param9DisplayNameTextAlt = copyUIFrom.param9DisplayNameTextAlt;
			param9Slider = copyUIFrom.param9Slider;
			param9SliderAlt = copyUIFrom.param9SliderAlt;
			param10DisplayNameText = copyUIFrom.param10DisplayNameText;
			param10DisplayNameTextAlt = copyUIFrom.param10DisplayNameTextAlt;
			param10Slider = copyUIFrom.param10Slider;
			param10SliderAlt = copyUIFrom.param10SliderAlt;
			textureGroup1Popup = copyUIFrom.textureGroup1Popup;
			textureGroup1PopupAlt = copyUIFrom.textureGroup1PopupAlt;
			textureGroup2Popup = copyUIFrom.textureGroup2Popup;
			textureGroup2PopupAlt = copyUIFrom.textureGroup2PopupAlt;
			textureGroup3Popup = copyUIFrom.textureGroup3Popup;
			textureGroup3PopupAlt = copyUIFrom.textureGroup3PopupAlt;
			textureGroup4Popup = copyUIFrom.textureGroup4Popup;
			textureGroup4PopupAlt = copyUIFrom.textureGroup4PopupAlt;
			textureGroup5Popup = copyUIFrom.textureGroup5Popup;
			textureGroup5PopupAlt = copyUIFrom.textureGroup5PopupAlt;
		}
	}

	public void CopyParams()
	{
		if (copyUIFrom != null)
		{
			paramMaterialSlots = (int[])copyUIFrom.paramMaterialSlots.Clone();
			color1Name = copyUIFrom.color1Name;
			color1Name2 = copyUIFrom.color1Name2;
			color1DisplayName = copyUIFrom.color1DisplayName;
			color2Name = copyUIFrom.color2Name;
			color2DisplayName = copyUIFrom.color2DisplayName;
			color3Name = copyUIFrom.color3Name;
			color3DisplayName = copyUIFrom.color3DisplayName;
			param1Name = copyUIFrom.param1Name;
			param1DisplayName = copyUIFrom.param1DisplayName;
			param1MaxValue = copyUIFrom.param1MaxValue;
			param1MinValue = copyUIFrom.param1MinValue;
			param2Name = copyUIFrom.param2Name;
			param2DisplayName = copyUIFrom.param2DisplayName;
			param2MaxValue = copyUIFrom.param2MaxValue;
			param2MinValue = copyUIFrom.param2MinValue;
			param3Name = copyUIFrom.param3Name;
			param3DisplayName = copyUIFrom.param3DisplayName;
			param3MaxValue = copyUIFrom.param3MaxValue;
			param3MinValue = copyUIFrom.param3MinValue;
			param4Name = copyUIFrom.param4Name;
			param4DisplayName = copyUIFrom.param4DisplayName;
			param4MaxValue = copyUIFrom.param4MaxValue;
			param4MinValue = copyUIFrom.param4MinValue;
			param5Name = copyUIFrom.param5Name;
			param5DisplayName = copyUIFrom.param5DisplayName;
			param5MaxValue = copyUIFrom.param5MaxValue;
			param5MinValue = copyUIFrom.param5MinValue;
			param6Name = copyUIFrom.param6Name;
			param6DisplayName = copyUIFrom.param6DisplayName;
			param6MaxValue = copyUIFrom.param6MaxValue;
			param6MinValue = copyUIFrom.param6MinValue;
			param7Name = copyUIFrom.param7Name;
			param7DisplayName = copyUIFrom.param7DisplayName;
			param7MaxValue = copyUIFrom.param7MaxValue;
			param7MinValue = copyUIFrom.param7MinValue;
			param8Name = copyUIFrom.param8Name;
			param8DisplayName = copyUIFrom.param8DisplayName;
			param8MaxValue = copyUIFrom.param8MaxValue;
			param8MinValue = copyUIFrom.param8MinValue;
			param9Name = copyUIFrom.param9Name;
			param9DisplayName = copyUIFrom.param9DisplayName;
			param9MaxValue = copyUIFrom.param9MaxValue;
			param9MinValue = copyUIFrom.param9MinValue;
			param10Name = copyUIFrom.param10Name;
			param10DisplayName = copyUIFrom.param10DisplayName;
			param10MaxValue = copyUIFrom.param10MaxValue;
			param10MinValue = copyUIFrom.param10MinValue;
		}
	}

	public void CopyTextureGroup()
	{
		if (!(copyUIFrom != null))
		{
			return;
		}
		MaterialOptionTextureGroup materialOptionTextureGroup;
		string text;
		switch (copyFromTextureGroup)
		{
		default:
			return;
		case 1:
			materialOptionTextureGroup = copyUIFrom.textureGroup1;
			text = copyUIFrom.startingTextureGroup1Set;
			break;
		case 2:
			materialOptionTextureGroup = copyUIFrom.textureGroup2;
			text = copyUIFrom.startingTextureGroup2Set;
			break;
		case 3:
			materialOptionTextureGroup = copyUIFrom.textureGroup3;
			text = copyUIFrom.startingTextureGroup3Set;
			break;
		case 4:
			materialOptionTextureGroup = copyUIFrom.textureGroup4;
			text = copyUIFrom.startingTextureGroup4Set;
			break;
		case 5:
			materialOptionTextureGroup = copyUIFrom.textureGroup5;
			text = copyUIFrom.startingTextureGroup5Set;
			break;
		}
		MaterialOptionTextureGroup materialOptionTextureGroup2;
		switch (copyToTextureGroup)
		{
		default:
			return;
		case 1:
			materialOptionTextureGroup2 = textureGroup1;
			startingTextureGroup1Set = text;
			break;
		case 2:
			materialOptionTextureGroup2 = textureGroup2;
			startingTextureGroup2Set = text;
			break;
		case 3:
			materialOptionTextureGroup2 = textureGroup3;
			startingTextureGroup3Set = text;
			break;
		case 4:
			materialOptionTextureGroup2 = textureGroup4;
			startingTextureGroup4Set = text;
			break;
		case 5:
			materialOptionTextureGroup2 = textureGroup5;
			startingTextureGroup5Set = text;
			break;
		}
		if (materialOptionTextureGroup != null && materialOptionTextureGroup2 != null)
		{
			materialOptionTextureGroup2.name = materialOptionTextureGroup.name;
			materialOptionTextureGroup2.textureName = materialOptionTextureGroup.textureName;
			materialOptionTextureGroup2.secondaryTextureName = materialOptionTextureGroup.secondaryTextureName;
			materialOptionTextureGroup2.thirdTextureName = materialOptionTextureGroup.thirdTextureName;
			materialOptionTextureGroup2.fourthTextureName = materialOptionTextureGroup.fourthTextureName;
			materialOptionTextureGroup2.fifthTextureName = materialOptionTextureGroup.fifthTextureName;
			materialOptionTextureGroup2.sixthTextureName = materialOptionTextureGroup.sixthTextureName;
			materialOptionTextureGroup2.mapTexturesToTextureNames = materialOptionTextureGroup.mapTexturesToTextureNames;
			materialOptionTextureGroup2.autoCreateDefaultTexture = materialOptionTextureGroup.autoCreateDefaultTexture;
			materialOptionTextureGroup2.autoCreateDefaultSetName = materialOptionTextureGroup.autoCreateDefaultSetName;
			materialOptionTextureGroup2.autoCreateTextureFilePrefix = materialOptionTextureGroup.autoCreateTextureFilePrefix;
			materialOptionTextureGroup2.autoCreateSetNamePrefix = materialOptionTextureGroup.autoCreateSetNamePrefix;
			materialOptionTextureGroup2.sets = new MaterialOptionTextureSet[materialOptionTextureGroup.sets.Length];
			for (int i = 0; i < materialOptionTextureGroup.sets.Length; i++)
			{
				materialOptionTextureGroup2.sets[i] = new MaterialOptionTextureSet();
				materialOptionTextureGroup2.sets[i].name = materialOptionTextureGroup.sets[i].name;
				materialOptionTextureGroup2.sets[i].textures = (Texture[])materialOptionTextureGroup.sets[i].textures.Clone();
				materialOptionTextureGroup2.sets[i].textures2 = (Texture[])materialOptionTextureGroup.sets[i].textures2.Clone();
			}
		}
	}

	public void MergeTextureGroup()
	{
		if (!(copyUIFrom != null))
		{
			return;
		}
		MaterialOptionTextureGroup materialOptionTextureGroup;
		switch (copyFromTextureGroup)
		{
		default:
			return;
		case 1:
			materialOptionTextureGroup = copyUIFrom.textureGroup1;
			break;
		case 2:
			materialOptionTextureGroup = copyUIFrom.textureGroup2;
			break;
		case 3:
			materialOptionTextureGroup = copyUIFrom.textureGroup3;
			break;
		case 4:
			materialOptionTextureGroup = copyUIFrom.textureGroup4;
			break;
		case 5:
			materialOptionTextureGroup = copyUIFrom.textureGroup5;
			break;
		}
		MaterialOptionTextureGroup materialOptionTextureGroup2;
		switch (copyToTextureGroup)
		{
		default:
			return;
		case 1:
			materialOptionTextureGroup2 = textureGroup1;
			break;
		case 2:
			materialOptionTextureGroup2 = textureGroup2;
			break;
		case 3:
			materialOptionTextureGroup2 = textureGroup3;
			break;
		case 4:
			materialOptionTextureGroup2 = textureGroup4;
			break;
		case 5:
			materialOptionTextureGroup2 = textureGroup5;
			break;
		}
		if (materialOptionTextureGroup == null || materialOptionTextureGroup2 == null)
		{
			return;
		}
		int num = materialOptionTextureGroup.sets.Length;
		if (num > 0)
		{
			Debug.Log("Merging in " + num + " sets from " + copyFromTransform.name + " " + materialOptionTextureGroup.name);
			MaterialOptionTextureSet[] array = new MaterialOptionTextureSet[materialOptionTextureGroup2.sets.Length + num];
			int num2 = 0;
			for (int i = 0; i < materialOptionTextureGroup2.sets.Length; i++)
			{
				array[i] = materialOptionTextureGroup2.sets[i];
				num2++;
			}
			for (int j = 0; j < materialOptionTextureGroup.sets.Length; j++)
			{
				array[num2] = new MaterialOptionTextureSet();
				array[num2].name = materialOptionTextureGroup2.autoCreateSetNamePrefix + " " + (num2 + 1);
				array[num2].textures = (Texture[])materialOptionTextureGroup.sets[j].textures.Clone();
				num2++;
			}
			materialOptionTextureGroup2.sets = array;
		}
	}

	public virtual void SetMaterialHide(Material m, bool b)
	{
		if (!(m != null))
		{
			return;
		}
		Shader value;
		if (b)
		{
			if (!materialToOriginalShader.ContainsKey(m))
			{
				materialToOriginalShader.Add(m, m.shader);
			}
			m.shader = hideShader;
		}
		else if (materialToOriginalShader.TryGetValue(m, out value))
		{
			m.shader = value;
		}
	}

	protected virtual void SetMaterialHide(bool b)
	{
		if (!(hideShader != null))
		{
			return;
		}
		if (materialToOriginalShader == null)
		{
			materialToOriginalShader = new Dictionary<Material, Shader>();
		}
		if (paramMaterialSlots != null)
		{
			Renderer[] array = renderers;
			foreach (Renderer renderer in array)
			{
				for (int j = 0; j < paramMaterialSlots.Length; j++)
				{
					int num = paramMaterialSlots[j];
					Material[] array2 = ((!Application.isPlaying) ? renderer.sharedMaterials : renderer.materials);
					if (num < array2.Length)
					{
						Material m = array2[num];
						SetMaterialHide(m, b);
					}
				}
			}
		}
		if (paramMaterialSlots2 == null)
		{
			return;
		}
		Renderer[] array3 = renderers2;
		foreach (Renderer renderer2 in array3)
		{
			for (int l = 0; l < paramMaterialSlots2.Length; l++)
			{
				int num2 = paramMaterialSlots2[l];
				Material[] array4 = ((!Application.isPlaying) ? renderer2.sharedMaterials : renderer2.materials);
				if (num2 < array4.Length)
				{
					Material m2 = array4[num2];
					SetMaterialHide(m2, b);
				}
			}
		}
	}

	protected virtual void SetMaterialRenderQueue(int q)
	{
		if (paramMaterialSlots != null)
		{
			Renderer[] array = renderers;
			foreach (Renderer renderer in array)
			{
				for (int j = 0; j < paramMaterialSlots.Length; j++)
				{
					int num = paramMaterialSlots[j];
					Material[] array2 = ((!Application.isPlaying) ? renderer.sharedMaterials : renderer.materials);
					if (num < array2.Length)
					{
						Material material = array2[num];
						material.renderQueue = q;
					}
				}
			}
			if (rawImageMaterials != null)
			{
				Material[] array3 = rawImageMaterials;
				foreach (Material material2 in array3)
				{
					material2.renderQueue = q;
				}
			}
		}
		if (paramMaterialSlots2 == null)
		{
			return;
		}
		Renderer[] array4 = renderers2;
		foreach (Renderer renderer2 in array4)
		{
			for (int m = 0; m < paramMaterialSlots2.Length; m++)
			{
				int num2 = paramMaterialSlots2[m];
				Material[] array5 = ((!Application.isPlaying) ? renderer2.sharedMaterials : renderer2.materials);
				if (num2 < array5.Length)
				{
					Material material3 = array5[num2];
					material3.renderQueue = q;
				}
			}
		}
	}

	protected virtual void SetMaterialParam(string name, float value)
	{
		if (paramMaterialSlots != null)
		{
			Renderer[] array = renderers;
			foreach (Renderer renderer in array)
			{
				for (int j = 0; j < paramMaterialSlots.Length; j++)
				{
					int num = paramMaterialSlots[j];
					Material[] array2 = ((!Application.isPlaying) ? renderer.sharedMaterials : renderer.materials);
					if (num < array2.Length)
					{
						Material material = array2[num];
						if (material.HasProperty(name))
						{
							material.SetFloat(name, value);
						}
					}
				}
			}
			if (rawImageMaterials != null)
			{
				Material[] array3 = rawImageMaterials;
				foreach (Material material2 in array3)
				{
					if (material2.HasProperty(name))
					{
						material2.SetFloat(name, value);
					}
				}
			}
		}
		if (paramMaterialSlots2 == null)
		{
			return;
		}
		Renderer[] array4 = renderers2;
		foreach (Renderer renderer2 in array4)
		{
			for (int m = 0; m < paramMaterialSlots2.Length; m++)
			{
				int num2 = paramMaterialSlots2[m];
				Material[] array5 = ((!Application.isPlaying) ? renderer2.sharedMaterials : renderer2.materials);
				if (num2 < array5.Length)
				{
					Material material3 = array5[num2];
					if (material3.HasProperty(name))
					{
						material3.SetFloat(name, value);
					}
				}
			}
		}
	}

	protected virtual void SetMaterialColor(string name, UnityEngine.Color c)
	{
		if (paramMaterialSlots != null)
		{
			Renderer[] array = renderers;
			foreach (Renderer renderer in array)
			{
				for (int j = 0; j < paramMaterialSlots.Length; j++)
				{
					int num = paramMaterialSlots[j];
					Material[] array2 = ((!Application.isPlaying) ? renderer.sharedMaterials : renderer.materials);
					if (num < array2.Length)
					{
						Material material = array2[num];
						if (material.HasProperty(name))
						{
							material.SetColor(name, c);
						}
					}
				}
			}
			if (rawImageMaterials != null)
			{
				Material[] array3 = rawImageMaterials;
				foreach (Material material2 in array3)
				{
					if (material2.HasProperty(name))
					{
						material2.SetColor(name, c);
					}
				}
			}
		}
		if (paramMaterialSlots2 == null)
		{
			return;
		}
		Renderer[] array4 = renderers2;
		foreach (Renderer renderer2 in array4)
		{
			for (int m = 0; m < paramMaterialSlots2.Length; m++)
			{
				int num2 = paramMaterialSlots2[m];
				Material[] array5 = ((!Application.isPlaying) ? renderer2.sharedMaterials : renderer2.materials);
				if (num2 < array5.Length)
				{
					Material material3 = array5[num2];
					if (material3.HasProperty(name))
					{
						material3.SetColor(name, c);
					}
				}
			}
		}
	}

	protected void RegisterTexture(Texture2D tex)
	{
		if (tex != null && ImageLoaderThreaded.singleton != null && ImageLoaderThreaded.singleton.RegisterTextureUse(tex))
		{
			if (textureUseCount == null)
			{
				textureUseCount = new Dictionary<Texture2D, int>();
			}
			int value = 0;
			if (textureUseCount.TryGetValue(tex, out value))
			{
				textureUseCount.Remove(tex);
			}
			value++;
			textureUseCount.Add(tex, value);
		}
	}

	protected void DeregisterTexture(Texture2D tex)
	{
		if (!(tex != null))
		{
			return;
		}
		int value = 0;
		if (ImageLoaderThreaded.singleton != null && textureUseCount != null && textureUseCount.TryGetValue(tex, out value))
		{
			ImageLoaderThreaded.singleton.DeregisterTextureUse(tex);
			textureUseCount.Remove(tex);
			value--;
			if (value > 0)
			{
				textureUseCount.Add(tex, value);
			}
		}
	}

	protected void DeregisterAllTextures()
	{
		if (!(ImageLoaderThreaded.singleton != null) || textureUseCount == null)
		{
			return;
		}
		foreach (Texture2D key in textureUseCount.Keys)
		{
			int value = 0;
			if (textureUseCount.TryGetValue(key, out value))
			{
				for (int i = 0; i < value; i++)
				{
					ImageLoaderThreaded.singleton.DeregisterTextureUse(key);
				}
			}
		}
		textureUseCount.Clear();
	}

	protected virtual void SetMaterialTexture(Material m, string propName, Texture texture)
	{
		if (m != null && m.HasProperty(propName))
		{
			Texture texture2 = m.GetTexture(propName);
			m.SetTexture(propName, texture);
			if (texture != null && texture is Texture2D)
			{
				RegisterTexture(texture as Texture2D);
			}
			if (texture2 != null && texture2 is Texture2D)
			{
				DeregisterTexture(texture2 as Texture2D);
			}
		}
	}

	protected virtual void SetMaterialTexture(int slot, string propName, Texture texture)
	{
		Renderer[] array = renderers;
		foreach (Renderer renderer in array)
		{
			Material[] array2 = ((!Application.isPlaying) ? renderer.sharedMaterials : renderer.materials);
			if (slot < array2.Length)
			{
				Material m = array2[slot];
				SetMaterialTexture(m, propName, texture);
			}
		}
	}

	protected virtual void SetMaterialTexture2(int slot, string propName, Texture texture)
	{
		Renderer[] array = renderers2;
		foreach (Renderer renderer in array)
		{
			Material[] array2 = ((!Application.isPlaying) ? renderer.sharedMaterials : renderer.materials);
			if (slot < array2.Length)
			{
				Material m = array2[slot];
				SetMaterialTexture(m, propName, texture);
			}
		}
	}

	protected virtual void SetMaterialTextureScale(Material m, string propName, Vector2 scale)
	{
		if (m != null && m.HasProperty(propName))
		{
			m.SetTextureScale(propName, scale);
		}
	}

	protected virtual void SetMaterialTextureScale(int slot, string propName, Vector2 scale)
	{
		Renderer[] array = renderers;
		foreach (Renderer renderer in array)
		{
			Material[] array2 = ((!Application.isPlaying) ? renderer.sharedMaterials : renderer.materials);
			if (slot < array2.Length)
			{
				Material m = array2[slot];
				SetMaterialTextureScale(m, propName, scale);
			}
		}
	}

	protected virtual void SetMaterialTextureScale2(int slot, string propName, Vector2 scale)
	{
		Renderer[] array = renderers2;
		foreach (Renderer renderer in array)
		{
			Material[] array2 = ((!Application.isPlaying) ? renderer.sharedMaterials : renderer.materials);
			if (slot < array2.Length)
			{
				Material m = array2[slot];
				SetMaterialTextureScale(m, propName, scale);
			}
		}
	}

	protected virtual void SetMaterialTextureOffset(Material m, string propName, Vector2 offset)
	{
		if (m != null && m.HasProperty(propName))
		{
			m.SetTextureOffset(propName, offset);
		}
	}

	protected virtual void SetMaterialTextureOffset(int slot, string propName, Vector2 offset)
	{
		Renderer[] array = renderers;
		foreach (Renderer renderer in array)
		{
			Material[] array2 = ((!Application.isPlaying) ? renderer.sharedMaterials : renderer.materials);
			if (slot < array2.Length)
			{
				Material m = array2[slot];
				SetMaterialTextureOffset(m, propName, offset);
			}
		}
	}

	protected virtual void SetMaterialTextureOffset2(int slot, string propName, Vector2 offset)
	{
		Renderer[] array = renderers2;
		foreach (Renderer renderer in array)
		{
			Material[] array2 = ((!Application.isPlaying) ? renderer.sharedMaterials : renderer.materials);
			if (slot < array2.Length)
			{
				Material m = array2[slot];
				SetMaterialTextureOffset(m, propName, offset);
			}
		}
	}

	protected virtual void SetParam1CurrentValue(float val)
	{
		param1CurrentValue = val;
		SetMaterialParam(param1Name, param1CurrentValue);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.param1JSONParam != null)
			{
				otherMaterialOptions.param1JSONParam.val = param1JSONParam.val;
			}
		}
	}

	protected virtual void SetParam2CurrentValue(float val)
	{
		param2CurrentValue = val;
		SetMaterialParam(param2Name, param2CurrentValue);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.param2JSONParam != null)
			{
				otherMaterialOptions.param2JSONParam.val = param2JSONParam.val;
			}
		}
	}

	protected virtual void SetParam3CurrentValue(float val)
	{
		param3CurrentValue = val;
		SetMaterialParam(param3Name, param3CurrentValue);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.param3JSONParam != null)
			{
				otherMaterialOptions.param3JSONParam.val = param3JSONParam.val;
			}
		}
	}

	protected virtual void SetParam4CurrentValue(float val)
	{
		param4CurrentValue = val;
		SetMaterialParam(param4Name, param4CurrentValue);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.param4JSONParam != null)
			{
				otherMaterialOptions.param4JSONParam.val = param4JSONParam.val;
			}
		}
	}

	protected virtual void SetParam5CurrentValue(float val)
	{
		param5CurrentValue = val;
		SetMaterialParam(param5Name, param5CurrentValue);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.param5JSONParam != null)
			{
				otherMaterialOptions.param5JSONParam.val = param5JSONParam.val;
			}
		}
	}

	protected virtual void SetParam6CurrentValue(float val)
	{
		param6CurrentValue = val;
		SetMaterialParam(param6Name, param6CurrentValue);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.param6JSONParam != null)
			{
				otherMaterialOptions.param6JSONParam.val = param6JSONParam.val;
			}
		}
	}

	protected virtual void SetParam7CurrentValue(float val)
	{
		param7CurrentValue = val;
		SetMaterialParam(param7Name, param7CurrentValue);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.param7JSONParam != null)
			{
				otherMaterialOptions.param7JSONParam.val = param7JSONParam.val;
			}
		}
	}

	protected virtual void SetParam8CurrentValue(float val)
	{
		param8CurrentValue = val;
		SetMaterialParam(param8Name, param8CurrentValue);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.param8JSONParam != null)
			{
				otherMaterialOptions.param8JSONParam.val = param8JSONParam.val;
			}
		}
	}

	protected virtual void SetParam9CurrentValue(float val)
	{
		param9CurrentValue = val;
		SetMaterialParam(param9Name, param9CurrentValue);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.param9JSONParam != null)
			{
				otherMaterialOptions.param9JSONParam.val = param1JSONParam.val;
			}
		}
	}

	protected virtual void SetParam10CurrentValue(float val)
	{
		param10CurrentValue = val;
		SetMaterialParam(param10Name, param10CurrentValue);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.param10JSONParam != null)
			{
				otherMaterialOptions.param10JSONParam.val = param10JSONParam.val;
			}
		}
	}

	public virtual void SetColor1(UnityEngine.Color c)
	{
		if (color1JSONParam != null)
		{
			color1JSONParam.SetColor(c);
		}
	}

	public virtual void SetColor2(UnityEngine.Color c)
	{
		if (color2JSONParam != null)
		{
			color2JSONParam.SetColor(c);
		}
	}

	public virtual void SetColor3(UnityEngine.Color c)
	{
		if (color3JSONParam != null)
		{
			color3JSONParam.SetColor(c);
		}
	}

	protected virtual void SetColor1FromHSV(float h, float s, float v)
	{
		color1CurrentHSVColor.H = h;
		color1CurrentHSVColor.S = s;
		color1CurrentHSVColor.V = v;
		color1CurrentColor = HSVColorPicker.HSVToRGB(h, s, v);
		color1CurrentColor.a = color1Alpha;
		SetMaterialColor(color1Name, color1CurrentColor);
		if (color1Name2 != null && color1Name2 != string.Empty)
		{
			SetMaterialColor(color1Name2, color1CurrentColor);
		}
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.color1JSONParam != null)
			{
				otherMaterialOptions.color1JSONParam.val = color1JSONParam.val;
			}
		}
	}

	protected virtual void SetColor2FromHSV(float h, float s, float v)
	{
		color2CurrentHSVColor.H = h;
		color2CurrentHSVColor.S = s;
		color2CurrentHSVColor.V = v;
		color2CurrentColor = HSVColorPicker.HSVToRGB(h, s, v);
		color2CurrentColor.a = color2Alpha;
		SetMaterialColor(color2Name, color2CurrentColor);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.color2JSONParam != null)
			{
				otherMaterialOptions.color2JSONParam.val = color2JSONParam.val;
			}
		}
	}

	protected virtual void SetColor3FromHSV(float h, float s, float v)
	{
		color3CurrentHSVColor.H = h;
		color3CurrentHSVColor.S = s;
		color3CurrentHSVColor.V = v;
		color3CurrentColor = HSVColorPicker.HSVToRGB(h, s, v);
		color3CurrentColor.a = color1Alpha;
		SetMaterialColor(color3Name, color3CurrentColor);
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.color3JSONParam != null)
			{
				otherMaterialOptions.color3JSONParam.val = color3JSONParam.val;
			}
		}
	}

	protected virtual void SetTextureGroupTextureScale(MaterialOptionTextureGroup tg, int textureNumber, Vector2 scale)
	{
		string text;
		switch (textureNumber)
		{
		case 0:
			text = tg.textureName;
			break;
		case 1:
			text = tg.secondaryTextureName;
			break;
		case 2:
			text = tg.thirdTextureName;
			break;
		case 3:
			text = tg.fourthTextureName;
			break;
		case 4:
			text = tg.fifthTextureName;
			break;
		case 5:
			text = tg.sixthTextureName;
			break;
		default:
			text = null;
			break;
		}
		int[] materialSlots = tg.materialSlots;
		int[] materialSlots2 = tg.materialSlots2;
		if (text == null)
		{
			return;
		}
		if (materialSlots != null)
		{
			for (int i = 0; i < materialSlots.Length; i++)
			{
				SetMaterialTextureScale(materialSlots[i], text, scale);
			}
		}
		if (materialSlots2 != null)
		{
			for (int j = 0; j < materialSlots2.Length; j++)
			{
				SetMaterialTextureScale2(materialSlots2[j], text, scale);
			}
		}
	}

	protected virtual void SetTextureGroupTextureOffset(MaterialOptionTextureGroup tg, int textureNumber, Vector2 offset)
	{
		string text;
		switch (textureNumber)
		{
		case 0:
			text = tg.textureName;
			break;
		case 1:
			text = tg.secondaryTextureName;
			break;
		case 2:
			text = tg.thirdTextureName;
			break;
		case 3:
			text = tg.fourthTextureName;
			break;
		case 4:
			text = tg.fifthTextureName;
			break;
		case 5:
			text = tg.sixthTextureName;
			break;
		default:
			text = null;
			break;
		}
		int[] materialSlots = tg.materialSlots;
		int[] materialSlots2 = tg.materialSlots2;
		if (text == null)
		{
			return;
		}
		if (materialSlots != null)
		{
			for (int i = 0; i < materialSlots.Length; i++)
			{
				SetMaterialTextureOffset(materialSlots[i], text, offset);
			}
		}
		if (materialSlots2 != null)
		{
			for (int j = 0; j < materialSlots2.Length; j++)
			{
				SetMaterialTextureOffset2(materialSlots2[j], text, offset);
			}
		}
	}

	protected virtual void SetTextureGroupTexture(MaterialOptionTextureGroup tg, int textureNumber, Texture texture1, Texture texture2)
	{
		string text;
		switch (textureNumber)
		{
		case 0:
			text = tg.textureName;
			break;
		case 1:
			text = tg.secondaryTextureName;
			break;
		case 2:
			text = tg.thirdTextureName;
			break;
		case 3:
			text = tg.fourthTextureName;
			break;
		case 4:
			text = tg.fifthTextureName;
			break;
		case 5:
			text = tg.sixthTextureName;
			break;
		default:
			text = null;
			break;
		}
		int[] materialSlots = tg.materialSlots;
		int[] materialSlots2 = tg.materialSlots2;
		if (text == null)
		{
			return;
		}
		if (materialSlots != null)
		{
			for (int i = 0; i < materialSlots.Length; i++)
			{
				SetMaterialTexture(materialSlots[i], text, texture1);
			}
		}
		if (materialSlots2 != null)
		{
			for (int j = 0; j < materialSlots2.Length; j++)
			{
				SetMaterialTexture2(materialSlots2[j], text, texture2);
			}
		}
	}

	protected virtual void SetTextureGroupSet(MaterialOptionTextureGroup tg, string setName, int onlyTextureNumber = -1, Texture customTexture = null, bool allowNull = false)
	{
		MaterialOptionTextureSet materialOptionTextureSet = null;
		if (setName != null)
		{
			materialOptionTextureSet = tg.GetSetByName(setName);
		}
		if (materialOptionTextureSet != null)
		{
			Texture[] textures = materialOptionTextureSet.textures;
			Texture[] textures2 = materialOptionTextureSet.textures2;
			int[] materialSlots = tg.materialSlots;
			if (tg.mapTexturesToTextureNames)
			{
				if (onlyTextureNumber != -1)
				{
					Texture texture = null;
					Texture texture2 = null;
					if (customTexture != null || allowNull)
					{
						texture = customTexture;
						texture2 = customTexture;
					}
					else
					{
						if (textures != null && onlyTextureNumber < textures.Length)
						{
							texture = textures[onlyTextureNumber];
						}
						if (textures2 != null && onlyTextureNumber < textures2.Length)
						{
							texture2 = textures2[onlyTextureNumber];
						}
					}
					SetTextureGroupTexture(tg, onlyTextureNumber, texture, texture2);
					return;
				}
				for (int i = 0; i < textures.Length; i++)
				{
					Texture texture3 = null;
					Texture texture4 = null;
					if (textures != null && i < textures.Length)
					{
						texture3 = textures[i];
					}
					if (textures2 != null && i < textures2.Length)
					{
						texture4 = textures2[i];
					}
					SetTextureGroupTexture(tg, i, texture3, texture4);
				}
			}
			else
			{
				if (materialSlots.Length != textures.Length)
				{
					return;
				}
				for (int j = 0; j < materialSlots.Length; j++)
				{
					SetMaterialTexture(materialSlots[j], tg.textureName, textures[j]);
					if (tg.secondaryTextureName != null && tg.secondaryTextureName != string.Empty)
					{
						SetMaterialTexture(materialSlots[j], tg.secondaryTextureName, textures[j]);
					}
				}
			}
		}
		else if (tg.mapTexturesToTextureNames && onlyTextureNumber != -1)
		{
			SetTextureGroupTexture(tg, onlyTextureNumber, customTexture, customTexture);
		}
	}

	protected virtual void SetTextureGroup1Set(string setName)
	{
		if (textureGroup1 != null && textureGroup1.sets != null)
		{
			SetTextureGroupSet(textureGroup1, setName);
			currentTextureGroup1Set = setName;
			if (customTexture1 != null || customTexture1IsNull)
			{
				SetTextureGroupSet(textureGroup1, currentTextureGroup1Set, 0, customTexture1, customTexture1IsNull);
			}
			if (customTexture2 != null || customTexture2IsNull)
			{
				SetTextureGroupSet(textureGroup1, currentTextureGroup1Set, 1, customTexture2, customTexture2IsNull);
			}
			if (customTexture3 != null || customTexture3IsNull)
			{
				SetTextureGroupSet(textureGroup1, currentTextureGroup1Set, 2, customTexture3, customTexture3IsNull);
			}
			if (customTexture4 != null || customTexture4IsNull)
			{
				SetTextureGroupSet(textureGroup1, currentTextureGroup1Set, 3, customTexture4, customTexture4IsNull);
			}
			if (customTexture5 != null || customTexture5IsNull)
			{
				SetTextureGroupSet(textureGroup1, currentTextureGroup1Set, 4, customTexture5, customTexture5IsNull);
			}
			if (customTexture6 != null || customTexture6IsNull)
			{
				SetTextureGroupSet(textureGroup1, currentTextureGroup1Set, 5, customTexture6, customTexture6IsNull);
			}
		}
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.textureGroup1JSON != null)
			{
				otherMaterialOptions.textureGroup1JSON.val = textureGroup1JSON.val;
			}
		}
	}

	protected virtual void SetTextureGroup2Set(string setName)
	{
		if (textureGroup2 != null && textureGroup2.sets != null)
		{
			SetTextureGroupSet(textureGroup2, setName);
			currentTextureGroup2Set = setName;
		}
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.textureGroup2JSON != null)
			{
				otherMaterialOptions.textureGroup2JSON.val = textureGroup2JSON.val;
			}
		}
	}

	protected virtual void SetTextureGroup3Set(string setName)
	{
		if (textureGroup3 != null && textureGroup3.sets != null)
		{
			SetTextureGroupSet(textureGroup3, setName);
			currentTextureGroup3Set = setName;
		}
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.textureGroup3JSON != null)
			{
				otherMaterialOptions.textureGroup3JSON.val = textureGroup3JSON.val;
			}
		}
	}

	protected virtual void SetTextureGroup4Set(string setName)
	{
		if (textureGroup4 != null && textureGroup4.sets != null)
		{
			SetTextureGroupSet(textureGroup4, setName);
			currentTextureGroup4Set = setName;
		}
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.textureGroup4JSON != null)
			{
				otherMaterialOptions.textureGroup4JSON.val = textureGroup4JSON.val;
			}
		}
	}

	protected virtual void SetTextureGroup5Set(string setName)
	{
		if (textureGroup5 != null && textureGroup5.sets != null)
		{
			SetTextureGroupSet(textureGroup5, setName);
			currentTextureGroup5Set = setName;
		}
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			if (otherMaterialOptions.textureGroup5JSON != null)
			{
				otherMaterialOptions.textureGroup5JSON.val = textureGroup5JSON.val;
			}
		}
	}

	public void CopyUI(MaterialOptionsUI moui)
	{
		color1Container = moui.color1Container;
		color1DisplayNameText = moui.color1DisplayNameText;
		color1Picker = moui.color1Picker;
		color2Container = moui.color2Container;
		color2DisplayNameText = moui.color2DisplayNameText;
		color2Picker = moui.color2Picker;
		color3Container = moui.color3Container;
		color3DisplayNameText = moui.color3DisplayNameText;
		color3Picker = moui.color3Picker;
		param1Slider = moui.param1Slider;
		param1DisplayNameText = moui.param1DisplayNameText;
		param2Slider = moui.param2Slider;
		param2DisplayNameText = moui.param2DisplayNameText;
		param3Slider = moui.param3Slider;
		param3DisplayNameText = moui.param3DisplayNameText;
		param4Slider = moui.param4Slider;
		param4DisplayNameText = moui.param4DisplayNameText;
		param5Slider = moui.param5Slider;
		param5DisplayNameText = moui.param5DisplayNameText;
		param6Slider = moui.param6Slider;
		param6DisplayNameText = moui.param6DisplayNameText;
		param7Slider = moui.param7Slider;
		param7DisplayNameText = moui.param7DisplayNameText;
		param8Slider = moui.param8Slider;
		param8DisplayNameText = moui.param8DisplayNameText;
		param9Slider = moui.param9Slider;
		param9DisplayNameText = moui.param9DisplayNameText;
		param10Slider = moui.param10Slider;
		param10DisplayNameText = moui.param10DisplayNameText;
		textureGroup1Popup = moui.textureGroup1Popup;
		textureGroup2Popup = moui.textureGroup2Popup;
		textureGroup3Popup = moui.textureGroup3Popup;
		textureGroup4Popup = moui.textureGroup4Popup;
		textureGroup5Popup = moui.textureGroup5Popup;
		customTexture1FileBrowseButton = moui.customTexture1FileBrowseButton;
		customTexture1ReloadButton = moui.customTexture1ReloadButton;
		customTexture1ClearButton = moui.customTexture1ClearButton;
		customTexture1NullButton = moui.customTexture1NullButton;
		customTexture1DefaultButton = moui.customTexture1DefaultButton;
		customTexture1UrlText = moui.customTexture1UrlText;
		customTexture1Label = moui.customTexture1Label;
		customTexture2FileBrowseButton = moui.customTexture2FileBrowseButton;
		customTexture2ReloadButton = moui.customTexture2ReloadButton;
		customTexture2ClearButton = moui.customTexture2ClearButton;
		customTexture2NullButton = moui.customTexture2NullButton;
		customTexture2DefaultButton = moui.customTexture2DefaultButton;
		customTexture2UrlText = moui.customTexture2UrlText;
		customTexture2Label = moui.customTexture2Label;
		customTexture3FileBrowseButton = moui.customTexture3FileBrowseButton;
		customTexture3ReloadButton = moui.customTexture3ReloadButton;
		customTexture3ClearButton = moui.customTexture3ClearButton;
		customTexture3NullButton = moui.customTexture3NullButton;
		customTexture3DefaultButton = moui.customTexture3DefaultButton;
		customTexture3UrlText = moui.customTexture3UrlText;
		customTexture3Label = moui.customTexture3Label;
		customTexture4FileBrowseButton = moui.customTexture4FileBrowseButton;
		customTexture4ReloadButton = moui.customTexture4ReloadButton;
		customTexture4ClearButton = moui.customTexture4ClearButton;
		customTexture4NullButton = moui.customTexture4NullButton;
		customTexture4DefaultButton = moui.customTexture4DefaultButton;
		customTexture4UrlText = moui.customTexture4UrlText;
		customTexture4Label = moui.customTexture4Label;
		customTexture5FileBrowseButton = moui.customTexture5FileBrowseButton;
		customTexture5ReloadButton = moui.customTexture5ReloadButton;
		customTexture5ClearButton = moui.customTexture5ClearButton;
		customTexture5NullButton = moui.customTexture5NullButton;
		customTexture5DefaultButton = moui.customTexture5DefaultButton;
		customTexture5UrlText = moui.customTexture5UrlText;
		customTexture5Label = moui.customTexture5Label;
		customTexture6FileBrowseButton = moui.customTexture6FileBrowseButton;
		customTexture6ReloadButton = moui.customTexture6ReloadButton;
		customTexture6ClearButton = moui.customTexture6ClearButton;
		customTexture6NullButton = moui.customTexture6NullButton;
		customTexture6DefaultButton = moui.customTexture6DefaultButton;
		customTexture6UrlText = moui.customTexture6UrlText;
		customTexture6Label = moui.customTexture6Label;
		customTexture1TileXSlider = moui.customTexture1TileXSlider;
		customTexture1TileYSlider = moui.customTexture1TileYSlider;
		customTexture1OffsetXSlider = moui.customTexture1OffsetXSlider;
		customTexture1OffsetYSlider = moui.customTexture1OffsetYSlider;
		customTexture2TileXSlider = moui.customTexture2TileXSlider;
		customTexture2TileYSlider = moui.customTexture2TileYSlider;
		customTexture2OffsetXSlider = moui.customTexture2OffsetXSlider;
		customTexture2OffsetYSlider = moui.customTexture2OffsetYSlider;
		customTexture3TileXSlider = moui.customTexture3TileXSlider;
		customTexture3TileYSlider = moui.customTexture3TileYSlider;
		customTexture3OffsetXSlider = moui.customTexture3OffsetXSlider;
		customTexture3OffsetYSlider = moui.customTexture3OffsetYSlider;
		customTexture4TileXSlider = moui.customTexture4TileXSlider;
		customTexture4TileYSlider = moui.customTexture4TileYSlider;
		customTexture4OffsetXSlider = moui.customTexture4OffsetXSlider;
		customTexture4OffsetYSlider = moui.customTexture4OffsetYSlider;
		customTexture5TileXSlider = moui.customTexture5TileXSlider;
		customTexture5TileYSlider = moui.customTexture5TileYSlider;
		customTexture5OffsetXSlider = moui.customTexture5OffsetXSlider;
		customTexture5OffsetYSlider = moui.customTexture5OffsetYSlider;
		customTexture6TileXSlider = moui.customTexture6TileXSlider;
		customTexture6TileYSlider = moui.customTexture6TileYSlider;
		customTexture6OffsetXSlider = moui.customTexture6OffsetXSlider;
		customTexture6OffsetYSlider = moui.customTexture6OffsetYSlider;
	}

	public override void InitUI()
	{
		if (UITransform != null)
		{
			MaterialOptionsUI componentInChildren = UITransform.GetComponentInChildren<MaterialOptionsUI>(true);
			if (componentInChildren != null)
			{
				CopyUI(componentInChildren);
				restoreAllFromDefaultsAction.button = componentInChildren.restoreFromDefaultsButton;
				saveToStore1Action.button = componentInChildren.saveToStore1Button;
				restoreAllFromStore1Action.button = componentInChildren.restoreFromStore1Button;
				saveToStore2Action.button = componentInChildren.saveToStore2Button;
				restoreAllFromStore2Action.button = componentInChildren.restoreFromStore2Button;
				saveToStore3Action.button = componentInChildren.saveToStore3Button;
				restoreAllFromStore3Action.button = componentInChildren.restoreFromStore3Button;
				if (createUVTemplateTextureJSON != null)
				{
					createUVTemplateTextureJSON.button = componentInChildren.createUVTemplateTextureButton;
					if (componentInChildren.createUVTemplateTextureButton != null)
					{
						componentInChildren.createUVTemplateTextureButton.gameObject.SetActive(true);
					}
				}
				else if (componentInChildren.createUVTemplateTextureButton != null)
				{
					componentInChildren.createUVTemplateTextureButton.gameObject.SetActive(false);
				}
				if (createSimTemplateTextureJSON != null)
				{
					createSimTemplateTextureJSON.button = componentInChildren.createSimTemplateTextureButton;
					if (componentInChildren.createSimTemplateTextureButton != null)
					{
						componentInChildren.createSimTemplateTextureButton.gameObject.SetActive(true);
					}
				}
				else if (componentInChildren.createSimTemplateTextureButton != null)
				{
					componentInChildren.createSimTemplateTextureButton.gameObject.SetActive(false);
				}
				if (openTextureFolderInExplorerAction != null)
				{
					openTextureFolderInExplorerAction.button = componentInChildren.openTextureFolderInExplorerButton;
					if (componentInChildren.openTextureFolderInExplorerButton != null)
					{
						componentInChildren.openTextureFolderInExplorerButton.gameObject.SetActive(true);
					}
				}
				else if (componentInChildren.openTextureFolderInExplorerButton != null)
				{
					componentInChildren.openTextureFolderInExplorerButton.gameObject.SetActive(false);
				}
				customNameJSON.inputField = componentInChildren.customNameField;
				if (renderQueueJSON != null)
				{
					renderQueueJSON.slider = componentInChildren.renderQueueSlider;
				}
				if (hideMaterialJSON != null)
				{
					hideMaterialJSON.toggle = componentInChildren.hideMaterialToggle;
					if (componentInChildren.hideMaterialToggle != null)
					{
						componentInChildren.hideMaterialToggle.gameObject.SetActive(true);
					}
				}
				else if (componentInChildren.hideMaterialToggle != null)
				{
					componentInChildren.hideMaterialToggle.gameObject.SetActive(false);
				}
				if (linkToOtherMaterialsJSON != null)
				{
					linkToOtherMaterialsJSON.toggle = componentInChildren.linkToOtherMaterialsToggle;
					if (componentInChildren.linkToOtherMaterialsToggle != null)
					{
						componentInChildren.linkToOtherMaterialsToggle.gameObject.SetActive(true);
					}
				}
				else if (componentInChildren.linkToOtherMaterialsToggle != null)
				{
					componentInChildren.linkToOtherMaterialsToggle.gameObject.SetActive(false);
				}
			}
		}
		if (color1Picker != null)
		{
			if (color1Name != null && color1Name != string.Empty && materialHasColor1)
			{
				if (color1Container != null)
				{
					color1Container.gameObject.SetActive(true);
				}
				if (color1DisplayNameText != null)
				{
					color1DisplayNameText.text = color1DisplayName;
				}
				if (color1JSONParam != null)
				{
					color1JSONParam.colorPicker = color1Picker;
				}
			}
			else if (color1Container != null)
			{
				color1Container.gameObject.SetActive(false);
			}
		}
		if (color2Picker != null)
		{
			if (color2Name != null && color2Name != string.Empty && materialHasColor2)
			{
				if (color2Container != null)
				{
					color2Container.gameObject.SetActive(true);
				}
				if (color2DisplayNameText != null)
				{
					color2DisplayNameText.text = color2DisplayName;
				}
				if (color2JSONParam != null)
				{
					color2JSONParam.colorPicker = color2Picker;
				}
			}
			else if (color2Container != null)
			{
				color2Container.gameObject.SetActive(false);
			}
		}
		if (color3Picker != null)
		{
			if (color3Name != null && color3Name != string.Empty && materialHasColor3)
			{
				if (color3Container != null)
				{
					color3Container.gameObject.SetActive(true);
				}
				if (color3DisplayNameText != null)
				{
					color3DisplayNameText.text = color3DisplayName;
				}
				if (color3JSONParam != null)
				{
					color3JSONParam.colorPicker = color3Picker;
				}
			}
			else if (color3Container != null)
			{
				color3Container.gameObject.SetActive(false);
			}
		}
		if (param1Slider != null)
		{
			if (materialHasParam1)
			{
				param1Slider.transform.parent.gameObject.SetActive(true);
				if (param1DisplayNameText != null)
				{
					param1DisplayNameText.text = param1DisplayName;
				}
				if (param1JSONParam != null)
				{
					param1JSONParam.slider = param1Slider;
				}
			}
			else
			{
				param1Slider.transform.parent.gameObject.SetActive(false);
			}
		}
		if (param2Slider != null)
		{
			if (materialHasParam2)
			{
				param2Slider.transform.parent.gameObject.SetActive(true);
				if (param2DisplayNameText != null)
				{
					param2DisplayNameText.text = param2DisplayName;
				}
				if (param2JSONParam != null)
				{
					param2JSONParam.slider = param2Slider;
				}
			}
			else
			{
				param2Slider.transform.parent.gameObject.SetActive(false);
			}
		}
		if (param3Slider != null)
		{
			if (materialHasParam3)
			{
				param3Slider.transform.parent.gameObject.SetActive(true);
				if (param3DisplayNameText != null)
				{
					param3DisplayNameText.text = param3DisplayName;
				}
				if (param3JSONParam != null)
				{
					param3JSONParam.slider = param3Slider;
				}
			}
			else
			{
				param3Slider.transform.parent.gameObject.SetActive(false);
			}
		}
		if (param4Slider != null)
		{
			if (materialHasParam4)
			{
				param4Slider.transform.parent.gameObject.SetActive(true);
				if (param4DisplayNameText != null)
				{
					param4DisplayNameText.text = param4DisplayName;
				}
				if (param4JSONParam != null)
				{
					param4JSONParam.slider = param4Slider;
				}
			}
			else
			{
				param4Slider.transform.parent.gameObject.SetActive(false);
			}
		}
		if (param5Slider != null)
		{
			if (materialHasParam5)
			{
				param5Slider.transform.parent.gameObject.SetActive(true);
				if (param5DisplayNameText != null)
				{
					param5DisplayNameText.text = param5DisplayName;
				}
				if (param5JSONParam != null)
				{
					param5JSONParam.slider = param5Slider;
				}
			}
			else
			{
				param5Slider.transform.parent.gameObject.SetActive(false);
			}
		}
		if (param6Slider != null)
		{
			if (materialHasParam6)
			{
				param6Slider.transform.parent.gameObject.SetActive(true);
				if (param6DisplayNameText != null)
				{
					param6DisplayNameText.text = param6DisplayName;
				}
				if (param6JSONParam != null)
				{
					param6JSONParam.slider = param6Slider;
				}
			}
			else
			{
				param6Slider.transform.parent.gameObject.SetActive(false);
			}
		}
		if (param7Slider != null)
		{
			if (materialHasParam7)
			{
				param7Slider.transform.parent.gameObject.SetActive(true);
				if (param7DisplayNameText != null)
				{
					param7DisplayNameText.text = param7DisplayName;
				}
				if (param7JSONParam != null)
				{
					param7JSONParam.slider = param7Slider;
				}
			}
			else
			{
				param7Slider.transform.parent.gameObject.SetActive(false);
			}
		}
		if (param8Slider != null)
		{
			if (materialHasParam8)
			{
				param8Slider.transform.parent.gameObject.SetActive(true);
				if (param8DisplayNameText != null)
				{
					param8DisplayNameText.text = param8DisplayName;
				}
				if (param8JSONParam != null)
				{
					param8JSONParam.slider = param8Slider;
				}
			}
			else
			{
				param8Slider.transform.parent.gameObject.SetActive(false);
			}
		}
		if (param9Slider != null)
		{
			if (materialHasParam9)
			{
				param9Slider.transform.parent.gameObject.SetActive(true);
				if (param9DisplayNameText != null)
				{
					param9DisplayNameText.text = param9DisplayName;
				}
				if (param9JSONParam != null)
				{
					param9JSONParam.slider = param9Slider;
				}
			}
			else
			{
				param9Slider.transform.parent.gameObject.SetActive(false);
			}
		}
		if (param10Slider != null)
		{
			if (materialHasParam10)
			{
				param10Slider.transform.parent.gameObject.SetActive(true);
				if (param10DisplayNameText != null)
				{
					param10DisplayNameText.text = param10DisplayName;
				}
				if (param10JSONParam != null)
				{
					param10JSONParam.slider = param10Slider;
				}
			}
			else
			{
				param10Slider.transform.parent.gameObject.SetActive(false);
			}
		}
		if (textureGroup1Popup != null)
		{
			if (hasTextureGroup1)
			{
				textureGroup1Popup.gameObject.SetActive(true);
				if (textureGroup1JSON != null)
				{
					textureGroup1JSON.popup = textureGroup1Popup;
				}
			}
			else
			{
				textureGroup1Popup.gameObject.SetActive(false);
			}
		}
		if (textureGroup2Popup != null)
		{
			if (hasTextureGroup2)
			{
				textureGroup2Popup.gameObject.SetActive(true);
				if (textureGroup2JSON != null)
				{
					textureGroup2JSON.popup = textureGroup2Popup;
				}
			}
			else
			{
				textureGroup2Popup.gameObject.SetActive(false);
			}
		}
		if (textureGroup3Popup != null)
		{
			if (hasTextureGroup3)
			{
				textureGroup3Popup.gameObject.SetActive(true);
				if (textureGroup3JSON != null)
				{
					textureGroup3JSON.popup = textureGroup3Popup;
				}
			}
			else
			{
				textureGroup3Popup.gameObject.SetActive(false);
			}
		}
		if (textureGroup4Popup != null)
		{
			if (hasTextureGroup4)
			{
				textureGroup4Popup.gameObject.SetActive(true);
				if (textureGroup4JSON != null)
				{
					textureGroup4JSON.popup = textureGroup4Popup;
				}
			}
			else
			{
				textureGroup4Popup.gameObject.SetActive(false);
			}
		}
		if (textureGroup5Popup != null)
		{
			if (hasTextureGroup5)
			{
				textureGroup5Popup.gameObject.SetActive(true);
				if (textureGroup5JSON != null)
				{
					textureGroup5JSON.popup = textureGroup5Popup;
				}
			}
			else
			{
				textureGroup5Popup.gameObject.SetActive(false);
			}
		}
		if (customTexture1TileXJSON != null)
		{
			customTexture1TileXJSON.slider = customTexture1TileXSlider;
			if (customTexture1TileXSlider != null)
			{
				customTexture1TileXSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture1TileXSlider != null)
		{
			customTexture1TileXSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture1TileYJSON != null)
		{
			customTexture1TileYJSON.slider = customTexture1TileYSlider;
			if (customTexture1TileYSlider != null)
			{
				customTexture1TileYSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture1TileYSlider != null)
		{
			customTexture1TileYSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture1OffsetXJSON != null)
		{
			customTexture1OffsetXJSON.slider = customTexture1OffsetXSlider;
			if (customTexture1OffsetXSlider != null)
			{
				customTexture1OffsetXSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture1OffsetXSlider != null)
		{
			customTexture1OffsetXSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture1OffsetYJSON != null)
		{
			customTexture1OffsetYJSON.slider = customTexture1OffsetYSlider;
			if (customTexture1OffsetYSlider != null)
			{
				customTexture1OffsetYSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture1OffsetYSlider != null)
		{
			customTexture1OffsetYSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture1UrlJSON != null)
		{
			customTexture1UrlJSON.fileBrowseButton = customTexture1FileBrowseButton;
			customTexture1UrlJSON.reloadButton = customTexture1ReloadButton;
			customTexture1UrlJSON.clearButton = customTexture1ClearButton;
			customTexture1UrlJSON.defaultButton = customTexture1DefaultButton;
			customTexture1UrlJSON.text = customTexture1UrlText;
			if (customTexture1FileBrowseButton != null)
			{
				customTexture1FileBrowseButton.gameObject.SetActive(true);
			}
			if (customTexture1ReloadButton != null)
			{
				customTexture1ReloadButton.gameObject.SetActive(true);
			}
			if (customTexture1ClearButton != null)
			{
				customTexture1ClearButton.gameObject.SetActive(true);
			}
			if (customTexture1DefaultButton != null)
			{
				customTexture1DefaultButton.gameObject.SetActive(true);
			}
			if (customTexture1UrlText != null)
			{
				customTexture1UrlText.gameObject.SetActive(true);
			}
			if (customTexture1Label != null)
			{
				customTexture1Label.gameObject.SetActive(true);
				if (textureGroup1 != null)
				{
					customTexture1Label.text = textureGroup1.textureName;
				}
				else
				{
					customTexture1Label.text = string.Empty;
				}
			}
			if (customTexture1NullButton != null)
			{
				customTexture1NullButton.gameObject.SetActive(true);
				customTexture1NullButton.onClick.AddListener(SetCustomTexture1ToNull);
			}
		}
		else
		{
			if (customTexture1FileBrowseButton != null)
			{
				customTexture1FileBrowseButton.gameObject.SetActive(false);
			}
			if (customTexture1ReloadButton != null)
			{
				customTexture1ReloadButton.gameObject.SetActive(false);
			}
			if (customTexture1ClearButton != null)
			{
				customTexture1ClearButton.gameObject.SetActive(false);
			}
			if (customTexture1DefaultButton != null)
			{
				customTexture1DefaultButton.gameObject.SetActive(false);
			}
			if (customTexture1UrlText != null)
			{
				customTexture1UrlText.gameObject.SetActive(false);
			}
			if (customTexture1Label != null)
			{
				customTexture1Label.gameObject.SetActive(false);
			}
			if (customTexture1NullButton != null)
			{
				customTexture1NullButton.gameObject.SetActive(false);
			}
		}
		if (customTexture2TileXJSON != null)
		{
			customTexture2TileXJSON.slider = customTexture2TileXSlider;
			if (customTexture2TileXSlider != null)
			{
				customTexture2TileXSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture2TileXSlider != null)
		{
			customTexture2TileXSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture2TileYJSON != null)
		{
			customTexture2TileYJSON.slider = customTexture2TileYSlider;
			if (customTexture2TileYSlider != null)
			{
				customTexture2TileYSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture2TileYSlider != null)
		{
			customTexture2TileYSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture2OffsetXJSON != null)
		{
			customTexture2OffsetXJSON.slider = customTexture2OffsetXSlider;
			if (customTexture2OffsetXSlider != null)
			{
				customTexture2OffsetXSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture2OffsetXSlider != null)
		{
			customTexture2OffsetXSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture2OffsetYJSON != null)
		{
			customTexture2OffsetYJSON.slider = customTexture2OffsetYSlider;
			if (customTexture2OffsetYSlider != null)
			{
				customTexture2OffsetYSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture2OffsetYSlider != null)
		{
			customTexture2OffsetYSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture2UrlJSON != null)
		{
			customTexture2UrlJSON.fileBrowseButton = customTexture2FileBrowseButton;
			customTexture2UrlJSON.reloadButton = customTexture2ReloadButton;
			customTexture2UrlJSON.clearButton = customTexture2ClearButton;
			customTexture2UrlJSON.defaultButton = customTexture2DefaultButton;
			customTexture2UrlJSON.text = customTexture2UrlText;
			if (customTexture2FileBrowseButton != null)
			{
				customTexture2FileBrowseButton.gameObject.SetActive(true);
			}
			if (customTexture2ReloadButton != null)
			{
				customTexture2ReloadButton.gameObject.SetActive(true);
			}
			if (customTexture2ClearButton != null)
			{
				customTexture2ClearButton.gameObject.SetActive(true);
			}
			if (customTexture2DefaultButton != null)
			{
				customTexture2DefaultButton.gameObject.SetActive(true);
			}
			if (customTexture2UrlText != null)
			{
				customTexture2UrlText.gameObject.SetActive(true);
			}
			if (customTexture2Label != null)
			{
				customTexture2Label.gameObject.SetActive(true);
				if (textureGroup1 != null)
				{
					customTexture2Label.text = textureGroup1.secondaryTextureName;
				}
				else
				{
					customTexture2Label.text = string.Empty;
				}
			}
			if (customTexture2NullButton != null)
			{
				customTexture2NullButton.gameObject.SetActive(true);
				customTexture2NullButton.onClick.AddListener(SetCustomTexture2ToNull);
			}
		}
		else
		{
			if (customTexture2FileBrowseButton != null)
			{
				customTexture2FileBrowseButton.gameObject.SetActive(false);
			}
			if (customTexture2ReloadButton != null)
			{
				customTexture2ReloadButton.gameObject.SetActive(false);
			}
			if (customTexture2ClearButton != null)
			{
				customTexture2ClearButton.gameObject.SetActive(false);
			}
			if (customTexture2DefaultButton != null)
			{
				customTexture2DefaultButton.gameObject.SetActive(false);
			}
			if (customTexture2UrlText != null)
			{
				customTexture2UrlText.gameObject.SetActive(false);
			}
			if (customTexture2Label != null)
			{
				customTexture2Label.gameObject.SetActive(false);
			}
			if (customTexture2NullButton != null)
			{
				customTexture2NullButton.gameObject.SetActive(false);
			}
		}
		if (customTexture3TileXJSON != null)
		{
			customTexture3TileXJSON.slider = customTexture3TileXSlider;
			if (customTexture3TileXSlider != null)
			{
				customTexture3TileXSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture3TileXSlider != null)
		{
			customTexture3TileXSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture3TileYJSON != null)
		{
			customTexture3TileYJSON.slider = customTexture3TileYSlider;
			if (customTexture3TileYSlider != null)
			{
				customTexture3TileYSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture3TileYSlider != null)
		{
			customTexture3TileYSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture3OffsetXJSON != null)
		{
			customTexture3OffsetXJSON.slider = customTexture3OffsetXSlider;
			if (customTexture3OffsetXSlider != null)
			{
				customTexture3OffsetXSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture3OffsetXSlider != null)
		{
			customTexture3OffsetXSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture3OffsetYJSON != null)
		{
			customTexture3OffsetYJSON.slider = customTexture3OffsetYSlider;
			if (customTexture3OffsetYSlider != null)
			{
				customTexture3OffsetYSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture3OffsetYSlider != null)
		{
			customTexture3OffsetYSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture3UrlJSON != null)
		{
			customTexture3UrlJSON.fileBrowseButton = customTexture3FileBrowseButton;
			customTexture3UrlJSON.reloadButton = customTexture3ReloadButton;
			customTexture3UrlJSON.clearButton = customTexture3ClearButton;
			customTexture3UrlJSON.defaultButton = customTexture3DefaultButton;
			customTexture3UrlJSON.text = customTexture3UrlText;
			if (customTexture3FileBrowseButton != null)
			{
				customTexture3FileBrowseButton.gameObject.SetActive(true);
			}
			if (customTexture3ReloadButton != null)
			{
				customTexture3ReloadButton.gameObject.SetActive(true);
			}
			if (customTexture3ClearButton != null)
			{
				customTexture3ClearButton.gameObject.SetActive(true);
			}
			if (customTexture3DefaultButton != null)
			{
				customTexture3DefaultButton.gameObject.SetActive(true);
			}
			if (customTexture3UrlText != null)
			{
				customTexture3UrlText.gameObject.SetActive(true);
			}
			if (customTexture3Label != null)
			{
				customTexture3Label.gameObject.SetActive(true);
				if (textureGroup1 != null)
				{
					customTexture3Label.text = textureGroup1.thirdTextureName;
				}
				else
				{
					customTexture3Label.text = string.Empty;
				}
			}
			if (customTexture3NullButton != null)
			{
				customTexture3NullButton.gameObject.SetActive(true);
				customTexture3NullButton.onClick.AddListener(SetCustomTexture3ToNull);
			}
		}
		else
		{
			if (customTexture3FileBrowseButton != null)
			{
				customTexture3FileBrowseButton.gameObject.SetActive(false);
			}
			if (customTexture3ReloadButton != null)
			{
				customTexture3ReloadButton.gameObject.SetActive(false);
			}
			if (customTexture3ClearButton != null)
			{
				customTexture3ClearButton.gameObject.SetActive(false);
			}
			if (customTexture3DefaultButton != null)
			{
				customTexture3DefaultButton.gameObject.SetActive(false);
			}
			if (customTexture3UrlText != null)
			{
				customTexture3UrlText.gameObject.SetActive(false);
			}
			if (customTexture3Label != null)
			{
				customTexture3Label.gameObject.SetActive(false);
			}
			if (customTexture3NullButton != null)
			{
				customTexture3NullButton.gameObject.SetActive(false);
			}
		}
		if (customTexture4TileXJSON != null)
		{
			customTexture4TileXJSON.slider = customTexture4TileXSlider;
			if (customTexture4TileXSlider != null)
			{
				customTexture4TileXSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture4TileXSlider != null)
		{
			customTexture4TileXSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture4TileYJSON != null)
		{
			customTexture4TileYJSON.slider = customTexture4TileYSlider;
			if (customTexture4TileYSlider != null)
			{
				customTexture4TileYSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture4TileYSlider != null)
		{
			customTexture4TileYSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture4OffsetXJSON != null)
		{
			customTexture4OffsetXJSON.slider = customTexture4OffsetXSlider;
			if (customTexture4OffsetXSlider != null)
			{
				customTexture4OffsetXSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture4OffsetXSlider != null)
		{
			customTexture4OffsetXSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture4OffsetYJSON != null)
		{
			customTexture4OffsetYJSON.slider = customTexture4OffsetYSlider;
			if (customTexture4OffsetYSlider != null)
			{
				customTexture4OffsetYSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture4OffsetYSlider != null)
		{
			customTexture4OffsetYSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture4UrlJSON != null)
		{
			customTexture4UrlJSON.fileBrowseButton = customTexture4FileBrowseButton;
			customTexture4UrlJSON.reloadButton = customTexture4ReloadButton;
			customTexture4UrlJSON.clearButton = customTexture4ClearButton;
			customTexture4UrlJSON.defaultButton = customTexture4DefaultButton;
			customTexture4UrlJSON.text = customTexture4UrlText;
			if (customTexture4FileBrowseButton != null)
			{
				customTexture4FileBrowseButton.gameObject.SetActive(true);
			}
			if (customTexture4ReloadButton != null)
			{
				customTexture4ReloadButton.gameObject.SetActive(true);
			}
			if (customTexture4ClearButton != null)
			{
				customTexture4ClearButton.gameObject.SetActive(true);
			}
			if (customTexture4DefaultButton != null)
			{
				customTexture4DefaultButton.gameObject.SetActive(true);
			}
			if (customTexture4UrlText != null)
			{
				customTexture4UrlText.gameObject.SetActive(true);
			}
			if (customTexture4Label != null)
			{
				customTexture4Label.gameObject.SetActive(true);
				if (textureGroup1 != null)
				{
					customTexture4Label.text = textureGroup1.fourthTextureName;
				}
				else
				{
					customTexture4Label.text = string.Empty;
				}
			}
			if (customTexture4NullButton != null)
			{
				customTexture4NullButton.gameObject.SetActive(true);
				customTexture4NullButton.onClick.AddListener(SetCustomTexture4ToNull);
			}
		}
		else
		{
			if (customTexture4FileBrowseButton != null)
			{
				customTexture4FileBrowseButton.gameObject.SetActive(false);
			}
			if (customTexture4ReloadButton != null)
			{
				customTexture4ReloadButton.gameObject.SetActive(false);
			}
			if (customTexture4ClearButton != null)
			{
				customTexture4ClearButton.gameObject.SetActive(false);
			}
			if (customTexture4DefaultButton != null)
			{
				customTexture4DefaultButton.gameObject.SetActive(false);
			}
			if (customTexture4UrlText != null)
			{
				customTexture4UrlText.gameObject.SetActive(false);
			}
			if (customTexture4Label != null)
			{
				customTexture4Label.gameObject.SetActive(false);
			}
			if (customTexture4NullButton != null)
			{
				customTexture4NullButton.gameObject.SetActive(false);
			}
		}
		if (customTexture5TileXJSON != null)
		{
			customTexture5TileXJSON.slider = customTexture5TileXSlider;
			if (customTexture5TileXSlider != null)
			{
				customTexture5TileXSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture5TileXSlider != null)
		{
			customTexture5TileXSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture5TileYJSON != null)
		{
			customTexture5TileYJSON.slider = customTexture5TileYSlider;
			if (customTexture5TileYSlider != null)
			{
				customTexture5TileYSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture5TileYSlider != null)
		{
			customTexture5TileYSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture5OffsetXJSON != null)
		{
			customTexture5OffsetXJSON.slider = customTexture5OffsetXSlider;
			if (customTexture5OffsetXSlider != null)
			{
				customTexture5OffsetXSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture5OffsetXSlider != null)
		{
			customTexture5OffsetXSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture5OffsetYJSON != null)
		{
			customTexture5OffsetYJSON.slider = customTexture5OffsetYSlider;
			if (customTexture5OffsetYSlider != null)
			{
				customTexture5OffsetYSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture5OffsetYSlider != null)
		{
			customTexture5OffsetYSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture5UrlJSON != null)
		{
			customTexture5UrlJSON.fileBrowseButton = customTexture5FileBrowseButton;
			customTexture5UrlJSON.reloadButton = customTexture5ReloadButton;
			customTexture5UrlJSON.clearButton = customTexture5ClearButton;
			customTexture5UrlJSON.defaultButton = customTexture5DefaultButton;
			customTexture5UrlJSON.text = customTexture5UrlText;
			if (customTexture5FileBrowseButton != null)
			{
				customTexture5FileBrowseButton.gameObject.SetActive(true);
			}
			if (customTexture5ReloadButton != null)
			{
				customTexture5ReloadButton.gameObject.SetActive(true);
			}
			if (customTexture5ClearButton != null)
			{
				customTexture5ClearButton.gameObject.SetActive(true);
			}
			if (customTexture5DefaultButton != null)
			{
				customTexture5DefaultButton.gameObject.SetActive(true);
			}
			if (customTexture5UrlText != null)
			{
				customTexture5UrlText.gameObject.SetActive(true);
			}
			if (customTexture5Label != null)
			{
				customTexture5Label.gameObject.SetActive(true);
				if (textureGroup1 != null)
				{
					customTexture5Label.text = textureGroup1.fifthTextureName;
				}
				else
				{
					customTexture5Label.text = string.Empty;
				}
			}
			if (customTexture5NullButton != null)
			{
				customTexture5NullButton.gameObject.SetActive(true);
				customTexture5NullButton.onClick.AddListener(SetCustomTexture5ToNull);
			}
		}
		else
		{
			if (customTexture5FileBrowseButton != null)
			{
				customTexture5FileBrowseButton.gameObject.SetActive(false);
			}
			if (customTexture5ReloadButton != null)
			{
				customTexture5ReloadButton.gameObject.SetActive(false);
			}
			if (customTexture5ClearButton != null)
			{
				customTexture5ClearButton.gameObject.SetActive(false);
			}
			if (customTexture5DefaultButton != null)
			{
				customTexture5DefaultButton.gameObject.SetActive(false);
			}
			if (customTexture5UrlText != null)
			{
				customTexture5UrlText.gameObject.SetActive(false);
			}
			if (customTexture5Label != null)
			{
				customTexture5Label.gameObject.SetActive(false);
			}
			if (customTexture5NullButton != null)
			{
				customTexture5NullButton.gameObject.SetActive(false);
			}
		}
		if (customTexture6TileXJSON != null)
		{
			customTexture6TileXJSON.slider = customTexture6TileXSlider;
			if (customTexture6TileXSlider != null)
			{
				customTexture6TileXSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture6TileXSlider != null)
		{
			customTexture6TileXSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture6TileYJSON != null)
		{
			customTexture6TileYJSON.slider = customTexture6TileYSlider;
			if (customTexture6TileYSlider != null)
			{
				customTexture6TileYSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture6TileYSlider != null)
		{
			customTexture6TileYSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture6OffsetXJSON != null)
		{
			customTexture6OffsetXJSON.slider = customTexture6OffsetXSlider;
			if (customTexture6OffsetXSlider != null)
			{
				customTexture6OffsetXSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture6OffsetXSlider != null)
		{
			customTexture6OffsetXSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture6OffsetYJSON != null)
		{
			customTexture6OffsetYJSON.slider = customTexture6OffsetYSlider;
			if (customTexture6OffsetYSlider != null)
			{
				customTexture6OffsetYSlider.transform.parent.gameObject.SetActive(true);
			}
		}
		else if (customTexture6OffsetYSlider != null)
		{
			customTexture6OffsetYSlider.transform.parent.gameObject.SetActive(false);
		}
		if (customTexture6UrlJSON != null)
		{
			customTexture6UrlJSON.fileBrowseButton = customTexture6FileBrowseButton;
			customTexture6UrlJSON.reloadButton = customTexture6ReloadButton;
			customTexture6UrlJSON.clearButton = customTexture6ClearButton;
			customTexture6UrlJSON.defaultButton = customTexture6DefaultButton;
			customTexture6UrlJSON.text = customTexture6UrlText;
			if (customTexture6FileBrowseButton != null)
			{
				customTexture6FileBrowseButton.gameObject.SetActive(true);
			}
			if (customTexture6ReloadButton != null)
			{
				customTexture6ReloadButton.gameObject.SetActive(true);
			}
			if (customTexture6ClearButton != null)
			{
				customTexture6ClearButton.gameObject.SetActive(true);
			}
			if (customTexture6DefaultButton != null)
			{
				customTexture6DefaultButton.gameObject.SetActive(true);
			}
			if (customTexture6UrlText != null)
			{
				customTexture6UrlText.gameObject.SetActive(true);
			}
			if (customTexture6Label != null)
			{
				customTexture6Label.gameObject.SetActive(true);
				if (textureGroup1 != null)
				{
					customTexture6Label.text = textureGroup1.sixthTextureName;
				}
				else
				{
					customTexture6Label.text = string.Empty;
				}
			}
			if (customTexture6NullButton != null)
			{
				customTexture6NullButton.gameObject.SetActive(true);
				customTexture6NullButton.onClick.AddListener(SetCustomTexture6ToNull);
			}
		}
		else
		{
			if (customTexture6FileBrowseButton != null)
			{
				customTexture6FileBrowseButton.gameObject.SetActive(false);
			}
			if (customTexture6ReloadButton != null)
			{
				customTexture6ReloadButton.gameObject.SetActive(false);
			}
			if (customTexture6ClearButton != null)
			{
				customTexture6ClearButton.gameObject.SetActive(false);
			}
			if (customTexture6DefaultButton != null)
			{
				customTexture6DefaultButton.gameObject.SetActive(false);
			}
			if (customTexture6UrlText != null)
			{
				customTexture6UrlText.gameObject.SetActive(false);
			}
			if (customTexture6Label != null)
			{
				customTexture6Label.gameObject.SetActive(false);
			}
			if (customTexture6NullButton != null)
			{
				customTexture6NullButton.gameObject.SetActive(false);
			}
		}
	}

	public override void InitUIAlt()
	{
		if (UITransformAlt != null)
		{
			MaterialOptionsUI componentInChildren = UITransformAlt.GetComponentInChildren<MaterialOptionsUI>(true);
			param1SliderAlt = componentInChildren.param1Slider;
			param1DisplayNameTextAlt = componentInChildren.param1DisplayNameText;
			param2SliderAlt = componentInChildren.param2Slider;
			param2DisplayNameTextAlt = componentInChildren.param2DisplayNameText;
			param3SliderAlt = componentInChildren.param3Slider;
			param3DisplayNameTextAlt = componentInChildren.param3DisplayNameText;
			param4SliderAlt = componentInChildren.param4Slider;
			param4DisplayNameTextAlt = componentInChildren.param4DisplayNameText;
			param5SliderAlt = componentInChildren.param5Slider;
			param5DisplayNameTextAlt = componentInChildren.param5DisplayNameText;
			param6SliderAlt = componentInChildren.param6Slider;
			param6DisplayNameTextAlt = componentInChildren.param6DisplayNameText;
			param7SliderAlt = componentInChildren.param7Slider;
			param7DisplayNameTextAlt = componentInChildren.param7DisplayNameText;
			param8SliderAlt = componentInChildren.param8Slider;
			param8DisplayNameTextAlt = componentInChildren.param8DisplayNameText;
			param9SliderAlt = componentInChildren.param9Slider;
			param9DisplayNameTextAlt = componentInChildren.param9DisplayNameText;
			param10SliderAlt = componentInChildren.param10Slider;
			param10DisplayNameTextAlt = componentInChildren.param10DisplayNameText;
			textureGroup1PopupAlt = componentInChildren.textureGroup1Popup;
			textureGroup2PopupAlt = componentInChildren.textureGroup2Popup;
			textureGroup3PopupAlt = componentInChildren.textureGroup3Popup;
			textureGroup4PopupAlt = componentInChildren.textureGroup4Popup;
			textureGroup5PopupAlt = componentInChildren.textureGroup5Popup;
			restoreAllFromDefaultsAction.buttonAlt = componentInChildren.restoreFromDefaultsButton;
			saveToStore1Action.buttonAlt = componentInChildren.saveToStore1Button;
			restoreAllFromStore1Action.buttonAlt = componentInChildren.restoreFromStore1Button;
			saveToStore2Action.buttonAlt = componentInChildren.saveToStore2Button;
			restoreAllFromStore2Action.buttonAlt = componentInChildren.restoreFromStore2Button;
			saveToStore3Action.buttonAlt = componentInChildren.saveToStore3Button;
			restoreAllFromStore3Action.buttonAlt = componentInChildren.restoreFromStore3Button;
		}
		if (param1SliderAlt != null)
		{
			if (materialHasParam1)
			{
				param1SliderAlt.transform.parent.gameObject.SetActive(true);
				if (param1DisplayNameTextAlt != null)
				{
					param1DisplayNameTextAlt.text = param1DisplayName;
				}
				if (param1JSONParam != null)
				{
					param1JSONParam.sliderAlt = param1SliderAlt;
				}
			}
			else
			{
				param1SliderAlt.transform.parent.gameObject.SetActive(false);
			}
		}
		if (param2SliderAlt != null)
		{
			if (materialHasParam2)
			{
				param2SliderAlt.transform.parent.gameObject.SetActive(true);
				if (param2DisplayNameTextAlt != null)
				{
					param2DisplayNameTextAlt.text = param2DisplayName;
				}
				if (param2JSONParam != null)
				{
					param2JSONParam.sliderAlt = param2SliderAlt;
				}
			}
			else
			{
				param2SliderAlt.transform.parent.gameObject.SetActive(false);
			}
		}
		if (param3SliderAlt != null)
		{
			if (materialHasParam3)
			{
				param3SliderAlt.transform.parent.gameObject.SetActive(true);
				if (param3DisplayNameTextAlt != null)
				{
					param3DisplayNameTextAlt.text = param3DisplayName;
				}
				if (param3JSONParam != null)
				{
					param3JSONParam.sliderAlt = param3SliderAlt;
				}
			}
			else
			{
				param3SliderAlt.transform.parent.gameObject.SetActive(false);
			}
		}
		if (param4SliderAlt != null)
		{
			if (materialHasParam4)
			{
				param4SliderAlt.transform.parent.gameObject.SetActive(true);
				if (param4DisplayNameTextAlt != null)
				{
					param4DisplayNameTextAlt.text = param4DisplayName;
				}
				if (param4JSONParam != null)
				{
					param4JSONParam.sliderAlt = param4SliderAlt;
				}
			}
			else
			{
				param4SliderAlt.transform.parent.gameObject.SetActive(false);
			}
		}
		if (param5SliderAlt != null)
		{
			if (materialHasParam5)
			{
				param5SliderAlt.transform.parent.gameObject.SetActive(true);
				if (param5DisplayNameTextAlt != null)
				{
					param5DisplayNameTextAlt.text = param5DisplayName;
				}
				if (param5JSONParam != null)
				{
					param5JSONParam.sliderAlt = param5SliderAlt;
				}
			}
			else
			{
				param5SliderAlt.transform.parent.gameObject.SetActive(false);
			}
		}
		if (param6SliderAlt != null)
		{
			if (materialHasParam6)
			{
				param6SliderAlt.transform.parent.gameObject.SetActive(true);
				if (param6DisplayNameTextAlt != null)
				{
					param6DisplayNameTextAlt.text = param6DisplayName;
				}
				if (param6JSONParam != null)
				{
					param6JSONParam.sliderAlt = param6SliderAlt;
				}
			}
			else
			{
				param6SliderAlt.transform.parent.gameObject.SetActive(false);
			}
		}
		if (param7SliderAlt != null)
		{
			if (materialHasParam7)
			{
				param7SliderAlt.transform.parent.gameObject.SetActive(true);
				if (param7DisplayNameTextAlt != null)
				{
					param7DisplayNameTextAlt.text = param7DisplayName;
				}
				if (param7JSONParam != null)
				{
					param7JSONParam.sliderAlt = param7SliderAlt;
				}
			}
			else
			{
				param7SliderAlt.transform.parent.gameObject.SetActive(false);
			}
		}
		if (param8SliderAlt != null)
		{
			if (materialHasParam8)
			{
				param8SliderAlt.transform.parent.gameObject.SetActive(true);
				if (param8DisplayNameTextAlt != null)
				{
					param8DisplayNameTextAlt.text = param8DisplayName;
				}
				if (param8JSONParam != null)
				{
					param8JSONParam.sliderAlt = param8SliderAlt;
				}
			}
			else
			{
				param8SliderAlt.transform.parent.gameObject.SetActive(false);
			}
		}
		if (param9SliderAlt != null)
		{
			if (materialHasParam9)
			{
				param9SliderAlt.transform.parent.gameObject.SetActive(true);
				if (param9DisplayNameTextAlt != null)
				{
					param9DisplayNameTextAlt.text = param9DisplayName;
				}
				if (param9JSONParam != null)
				{
					param9JSONParam.sliderAlt = param9SliderAlt;
				}
			}
			else
			{
				param9SliderAlt.transform.parent.gameObject.SetActive(false);
			}
		}
		if (param10SliderAlt != null)
		{
			if (materialHasParam10)
			{
				param10SliderAlt.transform.parent.gameObject.SetActive(true);
				if (param10DisplayNameTextAlt != null)
				{
					param10DisplayNameTextAlt.text = param10DisplayName;
				}
				if (param10JSONParam != null)
				{
					param10JSONParam.sliderAlt = param10SliderAlt;
				}
			}
			else
			{
				param10SliderAlt.transform.parent.gameObject.SetActive(false);
			}
		}
		if (textureGroup1PopupAlt != null)
		{
			if (hasTextureGroup1)
			{
				textureGroup1PopupAlt.gameObject.SetActive(true);
				if (textureGroup1JSON != null)
				{
					textureGroup1JSON.popupAlt = textureGroup1PopupAlt;
				}
			}
			else
			{
				textureGroup1PopupAlt.gameObject.SetActive(false);
			}
		}
		if (textureGroup2PopupAlt != null)
		{
			if (hasTextureGroup2)
			{
				textureGroup2PopupAlt.gameObject.SetActive(true);
				if (textureGroup2JSON != null)
				{
					textureGroup2JSON.popupAlt = textureGroup2PopupAlt;
				}
			}
			else
			{
				textureGroup2PopupAlt.gameObject.SetActive(false);
			}
		}
		if (textureGroup3PopupAlt != null)
		{
			if (hasTextureGroup3)
			{
				textureGroup3PopupAlt.gameObject.SetActive(true);
				if (textureGroup3JSON != null)
				{
					textureGroup3JSON.popupAlt = textureGroup3PopupAlt;
				}
			}
			else
			{
				textureGroup3PopupAlt.gameObject.SetActive(false);
			}
		}
		if (textureGroup4PopupAlt != null)
		{
			if (hasTextureGroup4)
			{
				textureGroup4PopupAlt.gameObject.SetActive(true);
				if (textureGroup4JSON != null)
				{
					textureGroup4JSON.popupAlt = textureGroup4PopupAlt;
				}
			}
			else
			{
				textureGroup4PopupAlt.gameObject.SetActive(false);
			}
		}
		if (!(textureGroup5PopupAlt != null))
		{
			return;
		}
		if (hasTextureGroup5)
		{
			textureGroup5PopupAlt.gameObject.SetActive(true);
			if (textureGroup5JSON != null)
			{
				textureGroup5JSON.popupAlt = textureGroup5PopupAlt;
			}
		}
		else
		{
			textureGroup5PopupAlt.gameObject.SetActive(false);
		}
	}

	public virtual void DeregisterUI()
	{
		if (customNameJSON != null)
		{
			customNameJSON.inputField = null;
		}
		if (renderQueueJSON != null)
		{
			renderQueueJSON.slider = null;
		}
		if (hideMaterialJSON != null)
		{
			hideMaterialJSON.toggle = null;
		}
		if (color1JSONParam != null)
		{
			color1JSONParam.colorPicker = null;
			color1JSONParam.colorPickerAlt = null;
		}
		if (color2JSONParam != null)
		{
			color2JSONParam.colorPicker = null;
			color2JSONParam.colorPickerAlt = null;
		}
		if (color3JSONParam != null)
		{
			color3JSONParam.colorPicker = null;
			color3JSONParam.colorPickerAlt = null;
		}
		if (param1JSONParam != null)
		{
			param1JSONParam.slider = null;
			param1JSONParam.sliderAlt = null;
		}
		if (param2JSONParam != null)
		{
			param2JSONParam.slider = null;
			param2JSONParam.sliderAlt = null;
		}
		if (param3JSONParam != null)
		{
			param3JSONParam.slider = null;
			param3JSONParam.sliderAlt = null;
		}
		if (param4JSONParam != null)
		{
			param4JSONParam.slider = null;
			param4JSONParam.sliderAlt = null;
		}
		if (param5JSONParam != null)
		{
			param5JSONParam.slider = null;
			param5JSONParam.sliderAlt = null;
		}
		if (param6JSONParam != null)
		{
			param6JSONParam.slider = null;
			param6JSONParam.sliderAlt = null;
		}
		if (param7JSONParam != null)
		{
			param7JSONParam.slider = null;
			param7JSONParam.sliderAlt = null;
		}
		if (param8JSONParam != null)
		{
			param8JSONParam.slider = null;
			param8JSONParam.sliderAlt = null;
		}
		if (param9JSONParam != null)
		{
			param9JSONParam.slider = null;
			param9JSONParam.sliderAlt = null;
		}
		if (param10JSONParam != null)
		{
			param10JSONParam.slider = null;
			param10JSONParam.sliderAlt = null;
		}
		if (textureGroup1JSON != null)
		{
			textureGroup1JSON.popup = null;
			textureGroup1JSON.popupAlt = null;
		}
		if (textureGroup2JSON != null)
		{
			textureGroup2JSON.popup = null;
			textureGroup2JSON.popupAlt = null;
		}
		if (textureGroup3JSON != null)
		{
			textureGroup3JSON.popup = null;
			textureGroup3JSON.popupAlt = null;
		}
		if (textureGroup4JSON != null)
		{
			textureGroup4JSON.popup = null;
			textureGroup4JSON.popupAlt = null;
		}
		if (textureGroup5JSON != null)
		{
			textureGroup5JSON.popup = null;
			textureGroup5JSON.popupAlt = null;
		}
		if (restoreAllFromDefaultsAction != null)
		{
			restoreAllFromDefaultsAction.button = null;
			restoreAllFromDefaultsAction.buttonAlt = null;
		}
		if (saveToStore1Action != null)
		{
			saveToStore1Action.button = null;
			saveToStore1Action.buttonAlt = null;
		}
		if (restoreAllFromStore1Action != null)
		{
			restoreAllFromStore1Action.button = null;
			restoreAllFromStore1Action.buttonAlt = null;
		}
		if (saveToStore2Action != null)
		{
			saveToStore2Action.button = null;
			saveToStore2Action.buttonAlt = null;
		}
		if (restoreAllFromStore2Action != null)
		{
			restoreAllFromStore2Action.button = null;
			restoreAllFromStore2Action.buttonAlt = null;
		}
		if (saveToStore3Action != null)
		{
			saveToStore3Action.button = null;
			saveToStore3Action.buttonAlt = null;
		}
		if (restoreAllFromStore3Action != null)
		{
			restoreAllFromStore3Action.button = null;
			restoreAllFromStore3Action.buttonAlt = null;
		}
		if (customTexture1UrlJSON != null)
		{
			customTexture1UrlJSON.fileBrowseButton = null;
			customTexture1UrlJSON.fileBrowseButtonAlt = null;
			customTexture1UrlJSON.clearButton = null;
			customTexture1UrlJSON.clearButtonAlt = null;
			customTexture1UrlJSON.text = null;
			customTexture1UrlJSON.textAlt = null;
		}
		if (customTexture2UrlJSON != null)
		{
			customTexture2UrlJSON.fileBrowseButton = null;
			customTexture2UrlJSON.fileBrowseButtonAlt = null;
			customTexture2UrlJSON.clearButton = null;
			customTexture2UrlJSON.clearButtonAlt = null;
			customTexture2UrlJSON.text = null;
			customTexture2UrlJSON.textAlt = null;
		}
		if (customTexture3UrlJSON != null)
		{
			customTexture3UrlJSON.fileBrowseButton = null;
			customTexture3UrlJSON.fileBrowseButtonAlt = null;
			customTexture3UrlJSON.clearButton = null;
			customTexture3UrlJSON.clearButtonAlt = null;
			customTexture3UrlJSON.text = null;
			customTexture3UrlJSON.textAlt = null;
		}
		if (customTexture4UrlJSON != null)
		{
			customTexture4UrlJSON.fileBrowseButton = null;
			customTexture4UrlJSON.fileBrowseButtonAlt = null;
			customTexture4UrlJSON.clearButton = null;
			customTexture4UrlJSON.clearButtonAlt = null;
			customTexture4UrlJSON.text = null;
			customTexture4UrlJSON.textAlt = null;
		}
		if (customTexture5UrlJSON != null)
		{
			customTexture5UrlJSON.fileBrowseButton = null;
			customTexture5UrlJSON.fileBrowseButtonAlt = null;
			customTexture5UrlJSON.clearButton = null;
			customTexture5UrlJSON.clearButtonAlt = null;
			customTexture5UrlJSON.text = null;
			customTexture5UrlJSON.textAlt = null;
		}
		if (customTexture6UrlJSON != null)
		{
			customTexture6UrlJSON.fileBrowseButton = null;
			customTexture6UrlJSON.fileBrowseButtonAlt = null;
			customTexture6UrlJSON.clearButton = null;
			customTexture6UrlJSON.clearButtonAlt = null;
			customTexture6UrlJSON.text = null;
			customTexture6UrlJSON.textAlt = null;
		}
	}

	public override void SetUI(Transform t)
	{
		if (deregisterOnDisable)
		{
			if (UITransform != t)
			{
				UITransform = t;
				if (base.isActiveAndEnabled)
				{
					InitUI();
				}
			}
		}
		else
		{
			base.SetUI(t);
		}
	}

	public override void SetUIAlt(Transform t)
	{
		if (deregisterOnDisable)
		{
			if (UITransformAlt != t)
			{
				UITransformAlt = t;
				if (base.isActiveAndEnabled)
				{
					InitUIAlt();
				}
			}
		}
		else
		{
			base.SetUIAlt(t);
		}
	}

	public virtual void SetStartingValues()
	{
		SetStartingValues(null);
	}

	public virtual void SetStartingValues(Dictionary<Texture2D, string> textureToSourcePath)
	{
		if (Application.isPlaying)
		{
			otherMaterialOptionsList = new List<MaterialOptions>();
			if (allowLinkToOtherMaterials)
			{
				MaterialOptions[] components = GetComponents<MaterialOptions>();
				MaterialOptions[] array = components;
				foreach (MaterialOptions materialOptions in array)
				{
					if (materialOptions != this && materialOptions.allowLinkToOtherMaterials)
					{
						otherMaterialOptionsList.Add(materialOptions);
					}
				}
			}
			if (color1JSONParam != null)
			{
				DeregisterColor(color1JSONParam);
				color1JSONParam = null;
			}
			if (color2JSONParam != null)
			{
				DeregisterColor(color2JSONParam);
				color2JSONParam = null;
			}
			if (color3JSONParam != null)
			{
				DeregisterColor(color3JSONParam);
				color3JSONParam = null;
			}
			if (param1JSONParam != null)
			{
				DeregisterFloat(param1JSONParam);
				param1JSONParam = null;
			}
			if (param2JSONParam != null)
			{
				DeregisterFloat(param2JSONParam);
				param2JSONParam = null;
			}
			if (param3JSONParam != null)
			{
				DeregisterFloat(param3JSONParam);
				param3JSONParam = null;
			}
			if (param4JSONParam != null)
			{
				DeregisterFloat(param4JSONParam);
				param4JSONParam = null;
			}
			if (param5JSONParam != null)
			{
				DeregisterFloat(param5JSONParam);
				param5JSONParam = null;
			}
			if (param6JSONParam != null)
			{
				DeregisterFloat(param6JSONParam);
				param6JSONParam = null;
			}
			if (param7JSONParam != null)
			{
				DeregisterFloat(param7JSONParam);
				param7JSONParam = null;
			}
			if (param8JSONParam != null)
			{
				DeregisterFloat(param8JSONParam);
				param8JSONParam = null;
			}
			if (param9JSONParam != null)
			{
				DeregisterFloat(param9JSONParam);
				param9JSONParam = null;
			}
			if (param10JSONParam != null)
			{
				DeregisterFloat(param10JSONParam);
				param10JSONParam = null;
			}
			if (textureGroup1JSON != null)
			{
				DeregisterStringChooser(textureGroup1JSON);
				textureGroup1JSON = null;
			}
			if (textureGroup2JSON != null)
			{
				DeregisterStringChooser(textureGroup2JSON);
				textureGroup2JSON = null;
			}
			if (textureGroup3JSON != null)
			{
				DeregisterStringChooser(textureGroup3JSON);
				textureGroup3JSON = null;
			}
			if (textureGroup4JSON != null)
			{
				DeregisterStringChooser(textureGroup4JSON);
				textureGroup4JSON = null;
			}
			if (textureGroup5JSON != null)
			{
				DeregisterStringChooser(textureGroup5JSON);
				textureGroup5JSON = null;
			}
			if (createUVTemplateTextureJSON != null)
			{
				DeregisterAction(createUVTemplateTextureJSON);
				createUVTemplateTextureJSON = null;
			}
			if (createSimTemplateTextureJSON != null)
			{
				DeregisterAction(createSimTemplateTextureJSON);
				createSimTemplateTextureJSON = null;
			}
			if (openTextureFolderInExplorerAction != null)
			{
				DeregisterAction(openTextureFolderInExplorerAction);
				openTextureFolderInExplorerAction = null;
			}
			if (customTexture1UrlJSON != null)
			{
				DeregisterUrl(customTexture1UrlJSON);
				customTexture1UrlJSON = null;
			}
			if (customTexture1TileXJSON != null)
			{
				DeregisterFloat(customTexture1TileXJSON);
				customTexture1TileXJSON = null;
			}
			if (customTexture1TileYJSON != null)
			{
				DeregisterFloat(customTexture1TileYJSON);
				customTexture1TileYJSON = null;
			}
			if (customTexture1OffsetXJSON != null)
			{
				DeregisterFloat(customTexture1OffsetXJSON);
				customTexture1OffsetXJSON = null;
			}
			if (customTexture1OffsetYJSON != null)
			{
				DeregisterFloat(customTexture1OffsetYJSON);
				customTexture1OffsetYJSON = null;
			}
			if (customTexture2UrlJSON != null)
			{
				DeregisterUrl(customTexture2UrlJSON);
				customTexture2UrlJSON = null;
			}
			if (customTexture2TileXJSON != null)
			{
				DeregisterFloat(customTexture2TileXJSON);
				customTexture2TileXJSON = null;
			}
			if (customTexture2TileYJSON != null)
			{
				DeregisterFloat(customTexture2TileYJSON);
				customTexture2TileYJSON = null;
			}
			if (customTexture2OffsetXJSON != null)
			{
				DeregisterFloat(customTexture2OffsetXJSON);
				customTexture2OffsetXJSON = null;
			}
			if (customTexture2OffsetYJSON != null)
			{
				DeregisterFloat(customTexture2OffsetYJSON);
				customTexture2OffsetYJSON = null;
			}
			if (customTexture3UrlJSON != null)
			{
				DeregisterUrl(customTexture3UrlJSON);
				customTexture3UrlJSON = null;
			}
			if (customTexture3TileXJSON != null)
			{
				DeregisterFloat(customTexture3TileXJSON);
				customTexture3TileXJSON = null;
			}
			if (customTexture3TileYJSON != null)
			{
				DeregisterFloat(customTexture3TileYJSON);
				customTexture3TileYJSON = null;
			}
			if (customTexture3OffsetXJSON != null)
			{
				DeregisterFloat(customTexture3OffsetXJSON);
				customTexture3OffsetXJSON = null;
			}
			if (customTexture3OffsetYJSON != null)
			{
				DeregisterFloat(customTexture3OffsetYJSON);
				customTexture3OffsetYJSON = null;
			}
			if (customTexture4UrlJSON != null)
			{
				DeregisterUrl(customTexture4UrlJSON);
				customTexture4UrlJSON = null;
			}
			if (customTexture4TileXJSON != null)
			{
				DeregisterFloat(customTexture4TileXJSON);
				customTexture4TileXJSON = null;
			}
			if (customTexture4TileYJSON != null)
			{
				DeregisterFloat(customTexture4TileYJSON);
				customTexture4TileYJSON = null;
			}
			if (customTexture4OffsetXJSON != null)
			{
				DeregisterFloat(customTexture4OffsetXJSON);
				customTexture4OffsetXJSON = null;
			}
			if (customTexture4OffsetYJSON != null)
			{
				DeregisterFloat(customTexture4OffsetYJSON);
				customTexture4OffsetYJSON = null;
			}
			if (customTexture5UrlJSON != null)
			{
				DeregisterUrl(customTexture5UrlJSON);
				customTexture5UrlJSON = null;
			}
			if (customTexture5TileXJSON != null)
			{
				DeregisterFloat(customTexture5TileXJSON);
				customTexture5TileXJSON = null;
			}
			if (customTexture5TileYJSON != null)
			{
				DeregisterFloat(customTexture5TileYJSON);
				customTexture5TileYJSON = null;
			}
			if (customTexture5OffsetXJSON != null)
			{
				DeregisterFloat(customTexture5OffsetXJSON);
				customTexture5OffsetXJSON = null;
			}
			if (customTexture5OffsetYJSON != null)
			{
				DeregisterFloat(customTexture5OffsetYJSON);
				customTexture5OffsetYJSON = null;
			}
			if (customTexture6UrlJSON != null)
			{
				DeregisterUrl(customTexture6UrlJSON);
				customTexture6UrlJSON = null;
			}
			if (customTexture6TileXJSON != null)
			{
				DeregisterFloat(customTexture6TileXJSON);
				customTexture6TileXJSON = null;
			}
			if (customTexture6TileYJSON != null)
			{
				DeregisterFloat(customTexture6TileYJSON);
				customTexture6TileYJSON = null;
			}
			if (customTexture6OffsetXJSON != null)
			{
				DeregisterFloat(customTexture6OffsetXJSON);
				customTexture6OffsetXJSON = null;
			}
			if (customTexture6OffsetYJSON != null)
			{
				DeregisterFloat(customTexture6OffsetYJSON);
				customTexture6OffsetYJSON = null;
			}
		}
		customNameJSON = new JSONStorableString("customName", string.Empty, SyncCustomName);
		customNameJSON.isStorable = false;
		customNameJSON.isRestorable = false;
		Renderer[] array2 = ((materialContainer != null) ? ((!searchInChildren) ? materialContainer.GetComponents<Renderer>() : materialContainer.GetComponentsInChildren<Renderer>()) : ((!searchInChildren) ? GetComponents<Renderer>() : GetComponentsInChildren<Renderer>()));
		List<Renderer> list = new List<Renderer>();
		Renderer[] array3 = array2;
		foreach (Renderer renderer in array3)
		{
			MaterialOptionsIgnore component = renderer.GetComponent<MaterialOptionsIgnore>();
			if (component == null)
			{
				list.Add(renderer);
			}
		}
		renderers = list.ToArray();
		Renderer[] array4 = null;
		if (materialContainer2 != null)
		{
			array4 = ((!searchInChildren) ? materialContainer2.GetComponents<Renderer>() : materialContainer2.GetComponentsInChildren<Renderer>());
		}
		List<Renderer> list2 = new List<Renderer>();
		if (array4 != null)
		{
			Renderer[] array5 = array4;
			foreach (Renderer renderer2 in array5)
			{
				MaterialOptionsIgnore component2 = renderer2.GetComponent<MaterialOptionsIgnore>();
				if (component2 == null)
				{
					list2.Add(renderer2);
				}
			}
		}
		renderers2 = list2.ToArray();
		if (materialForDefaults == null && renderers.Length > 0 && paramMaterialSlots != null && paramMaterialSlots.Length > 0)
		{
			Renderer renderer3 = renderers[0];
			int num = paramMaterialSlots[0];
			if (num < renderer3.materials.Length)
			{
				materialForDefaults = renderer3.materials[num];
			}
		}
		if (renderers.Length > 0)
		{
			Renderer renderer4 = renderers[0];
			meshFilter = renderer4.GetComponent<MeshFilter>();
		}
		if (Application.isPlaying && textureGroup1 != null && paramMaterialSlots != null && paramMaterialSlots.Length > 0 && (textureGroup1.materialSlots == null || textureGroup1.materialSlots.Length == 0))
		{
			textureGroup1.materialSlots = new int[paramMaterialSlots.Length];
			for (int l = 0; l < paramMaterialSlots.Length; l++)
			{
				textureGroup1.materialSlots[l] = paramMaterialSlots[l];
			}
		}
		if (controlRawImage)
		{
			RawImage[] array6 = ((materialContainer != null) ? ((!searchInChildren) ? materialContainer.GetComponents<RawImage>() : materialContainer.GetComponentsInChildren<RawImage>()) : ((!searchInChildren) ? GetComponents<RawImage>() : GetComponentsInChildren<RawImage>()));
			List<Material> list3 = new List<Material>();
			RawImage[] array7 = array6;
			foreach (RawImage rawImage in array7)
			{
				MaterialOptionsIgnore component3 = rawImage.GetComponent<MaterialOptionsIgnore>();
				if (component3 == null)
				{
					if (Application.isPlaying)
					{
						Material material = new Material(rawImage.material);
						RegisterAllocatedObject(material);
						rawImage.material = material;
						list3.Add(material);
					}
					else
					{
						list3.Add(rawImage.material);
					}
				}
			}
			if (materialForDefaults == null && array6.Length > 0)
			{
				materialForDefaults = array6[0].material;
			}
			rawImageMaterials = list3.ToArray();
		}
		if (materialForDefaults != null)
		{
			if (Application.isPlaying)
			{
				renderQueueJSON = new JSONStorableFloat("renderQueue", materialForDefaults.renderQueue, SyncRenderQueue, -1f, 5000f);
				RegisterFloat(renderQueueJSON);
				if (hideShader != null)
				{
					hideMaterialJSON = new JSONStorableBool("hideMaterial", false, SyncHideMaterial);
					RegisterBool(hideMaterialJSON);
				}
				if (allowLinkToOtherMaterials)
				{
					linkToOtherMaterialsJSON = new JSONStorableBool("linkToOtherMaterials", _linkToOtherMaterials, SyncLinkToOtherMaterials);
					RegisterBool(linkToOtherMaterialsJSON);
				}
			}
			if (color1Name != null && color1Name != string.Empty && materialForDefaults.HasProperty(color1Name))
			{
				materialHasColor1 = true;
				color1CurrentColor = materialForDefaults.GetColor(color1Name);
				color1Alpha = color1CurrentColor.a;
				color1CurrentHSVColor = HSVColorPicker.RGBToHSV(color1CurrentColor.r, color1CurrentColor.g, color1CurrentColor.b);
				if (Application.isPlaying)
				{
					color1JSONParam = new JSONStorableColor(color1DisplayName, color1CurrentHSVColor, SetColor1FromHSV);
					RegisterColor(color1JSONParam);
				}
			}
			else
			{
				materialHasColor1 = false;
				color1CurrentColor = UnityEngine.Color.black;
				color1Alpha = 1f;
				color1CurrentHSVColor = HSVColorPicker.RGBToHSV(0f, 0f, 0f);
			}
			if (color2Name != null && color2Name != string.Empty && materialForDefaults.HasProperty(color2Name))
			{
				materialHasColor2 = true;
				color2CurrentColor = materialForDefaults.GetColor(color2Name);
				color2Alpha = color2CurrentColor.a;
				color2CurrentHSVColor = HSVColorPicker.RGBToHSV(color2CurrentColor.r, color2CurrentColor.g, color2CurrentColor.b);
				if (Application.isPlaying)
				{
					color2JSONParam = new JSONStorableColor(color2DisplayName, color2CurrentHSVColor, SetColor2FromHSV);
					RegisterColor(color2JSONParam);
				}
			}
			else
			{
				materialHasColor2 = false;
				color2CurrentColor = UnityEngine.Color.black;
				color2Alpha = 1f;
				color2CurrentHSVColor = HSVColorPicker.RGBToHSV(0f, 0f, 0f);
			}
			if (color3Name != null && color3Name != string.Empty && materialForDefaults.HasProperty(color3Name))
			{
				materialHasColor3 = true;
				color3CurrentColor = materialForDefaults.GetColor(color3Name);
				color3Alpha = color3CurrentColor.a;
				color3CurrentHSVColor = HSVColorPicker.RGBToHSV(color3CurrentColor.r, color3CurrentColor.g, color3CurrentColor.b);
				if (Application.isPlaying)
				{
					color3JSONParam = new JSONStorableColor(color3DisplayName, color3CurrentHSVColor, SetColor3FromHSV);
					RegisterColor(color3JSONParam);
				}
			}
			else
			{
				materialHasColor3 = false;
				color3CurrentColor = UnityEngine.Color.black;
				color3Alpha = 1f;
				color3CurrentHSVColor = HSVColorPicker.RGBToHSV(0f, 0f, 0f);
			}
			materialHasParam1 = false;
			materialHasParam2 = false;
			materialHasParam3 = false;
			materialHasParam4 = false;
			materialHasParam5 = false;
			materialHasParam6 = false;
			materialHasParam7 = false;
			materialHasParam8 = false;
			materialHasParam9 = false;
			materialHasParam10 = false;
			if (param1Name != null && param1Name != string.Empty && materialForDefaults.HasProperty(param1Name))
			{
				materialHasParam1 = true;
				param1CurrentValue = materialForDefaults.GetFloat(param1Name);
				if (Application.isPlaying)
				{
					param1JSONParam = new JSONStorableFloat(param1DisplayName, param1CurrentValue, SetParam1CurrentValue, param1MinValue, param1MaxValue);
					RegisterFloat(param1JSONParam);
				}
			}
			if (param2Name != null && param2Name != string.Empty && materialForDefaults.HasProperty(param2Name))
			{
				materialHasParam2 = true;
				param2CurrentValue = materialForDefaults.GetFloat(param2Name);
				if (Application.isPlaying)
				{
					param2JSONParam = new JSONStorableFloat(param2DisplayName, param2CurrentValue, SetParam2CurrentValue, param2MinValue, param2MaxValue);
					RegisterFloat(param2JSONParam);
				}
			}
			if (param3Name != null && param3Name != string.Empty && materialForDefaults.HasProperty(param3Name))
			{
				materialHasParam3 = true;
				param3CurrentValue = materialForDefaults.GetFloat(param3Name);
				if (Application.isPlaying)
				{
					param3JSONParam = new JSONStorableFloat(param3DisplayName, param3CurrentValue, SetParam3CurrentValue, param3MinValue, param3MaxValue);
					RegisterFloat(param3JSONParam);
				}
			}
			if (param4Name != null && param4Name != string.Empty && materialForDefaults.HasProperty(param4Name))
			{
				materialHasParam4 = true;
				param4CurrentValue = materialForDefaults.GetFloat(param4Name);
				if (Application.isPlaying)
				{
					param4JSONParam = new JSONStorableFloat(param4DisplayName, param4CurrentValue, SetParam4CurrentValue, param4MinValue, param4MaxValue);
					RegisterFloat(param4JSONParam);
				}
			}
			if (param5Name != null && param5Name != string.Empty && materialForDefaults.HasProperty(param5Name))
			{
				materialHasParam5 = true;
				param5CurrentValue = materialForDefaults.GetFloat(param5Name);
				if (Application.isPlaying)
				{
					param5JSONParam = new JSONStorableFloat(param5DisplayName, param5CurrentValue, SetParam5CurrentValue, param5MinValue, param5MaxValue);
					RegisterFloat(param5JSONParam);
				}
			}
			if (param6Name != null && param6Name != string.Empty && materialForDefaults.HasProperty(param6Name))
			{
				materialHasParam6 = true;
				param6CurrentValue = materialForDefaults.GetFloat(param6Name);
				if (Application.isPlaying)
				{
					param6JSONParam = new JSONStorableFloat(param6DisplayName, param6CurrentValue, SetParam6CurrentValue, param6MinValue, param6MaxValue);
					RegisterFloat(param6JSONParam);
				}
			}
			if (param7Name != null && param7Name != string.Empty && materialForDefaults.HasProperty(param7Name))
			{
				materialHasParam7 = true;
				param7CurrentValue = materialForDefaults.GetFloat(param7Name);
				if (Application.isPlaying)
				{
					param7JSONParam = new JSONStorableFloat(param7DisplayName, param7CurrentValue, SetParam7CurrentValue, param7MinValue, param7MaxValue);
					RegisterFloat(param7JSONParam);
				}
			}
			if (param8Name != null && param8Name != string.Empty && materialForDefaults.HasProperty(param8Name))
			{
				materialHasParam8 = true;
				param8CurrentValue = materialForDefaults.GetFloat(param8Name);
				if (Application.isPlaying)
				{
					param8JSONParam = new JSONStorableFloat(param8DisplayName, param8CurrentValue, SetParam8CurrentValue, param8MinValue, param8MaxValue);
					RegisterFloat(param8JSONParam);
				}
			}
			if (param9Name != null && param9Name != string.Empty && materialForDefaults.HasProperty(param9Name))
			{
				materialHasParam9 = true;
				param9CurrentValue = materialForDefaults.GetFloat(param9Name);
				if (Application.isPlaying)
				{
					param9JSONParam = new JSONStorableFloat(param9DisplayName, param9CurrentValue, SetParam9CurrentValue, param9MinValue, param9MaxValue);
					RegisterFloat(param9JSONParam);
				}
			}
			if (param10Name != null && param10Name != string.Empty && materialForDefaults.HasProperty(param10Name))
			{
				materialHasParam10 = true;
				param10CurrentValue = materialForDefaults.GetFloat(param10Name);
				if (Application.isPlaying)
				{
					param10JSONParam = new JSONStorableFloat(param10DisplayName, param10CurrentValue, SetParam10CurrentValue, param10MinValue, param10MaxValue);
					RegisterFloat(param10JSONParam);
				}
			}
		}
		hasTextureGroup1 = false;
		hasTextureGroup2 = false;
		hasTextureGroup3 = false;
		hasTextureGroup4 = false;
		hasTextureGroup5 = false;
		currentTextureGroup1Set = startingTextureGroup1Set;
		if (textureGroup1 != null && textureGroup1.name != null && textureGroup1.name != string.Empty && textureGroup1.sets != null && textureGroup1.sets.Length > 1)
		{
			hasTextureGroup1 = true;
			if (Application.isPlaying)
			{
				List<string> list4 = new List<string>();
				for (int n = 0; n < textureGroup1.sets.Length; n++)
				{
					list4.Add(textureGroup1.sets[n].name);
				}
				textureGroup1JSON = new JSONStorableStringChooser(textureGroup1.name, list4, startingTextureGroup1Set, textureGroup1.name, SetTextureGroup1Set);
				RegisterStringChooser(textureGroup1JSON);
			}
		}
		currentTextureGroup2Set = startingTextureGroup2Set;
		if (textureGroup2 != null && textureGroup2.name != null && textureGroup2.name != string.Empty && textureGroup2.sets != null && textureGroup2.sets.Length > 1)
		{
			hasTextureGroup2 = true;
			if (Application.isPlaying)
			{
				List<string> list5 = new List<string>();
				for (int num2 = 0; num2 < textureGroup2.sets.Length; num2++)
				{
					list5.Add(textureGroup2.sets[num2].name);
				}
				textureGroup2JSON = new JSONStorableStringChooser(textureGroup2.name, list5, startingTextureGroup2Set, textureGroup2.name, SetTextureGroup2Set);
				RegisterStringChooser(textureGroup2JSON);
			}
		}
		currentTextureGroup3Set = startingTextureGroup3Set;
		if (textureGroup3 != null && textureGroup3.name != null && textureGroup3.name != string.Empty && textureGroup3.sets != null && textureGroup3.sets.Length > 1)
		{
			hasTextureGroup3 = true;
			if (Application.isPlaying)
			{
				List<string> list6 = new List<string>();
				for (int num3 = 0; num3 < textureGroup3.sets.Length; num3++)
				{
					list6.Add(textureGroup3.sets[num3].name);
				}
				textureGroup3JSON = new JSONStorableStringChooser(textureGroup3.name, list6, startingTextureGroup3Set, textureGroup3.name, SetTextureGroup3Set);
				RegisterStringChooser(textureGroup3JSON);
			}
		}
		currentTextureGroup4Set = startingTextureGroup4Set;
		if (textureGroup4 != null && textureGroup4.name != null && textureGroup4.name != string.Empty && textureGroup4.sets != null && textureGroup4.sets.Length > 1)
		{
			hasTextureGroup4 = true;
			if (Application.isPlaying)
			{
				List<string> list7 = new List<string>();
				for (int num4 = 0; num4 < textureGroup4.sets.Length; num4++)
				{
					list7.Add(textureGroup4.sets[num4].name);
				}
				textureGroup4JSON = new JSONStorableStringChooser(textureGroup4.name, list7, startingTextureGroup4Set, textureGroup4.name, SetTextureGroup4Set);
				RegisterStringChooser(textureGroup4JSON);
			}
		}
		currentTextureGroup5Set = startingTextureGroup5Set;
		if (textureGroup5 != null && textureGroup5.name != null && textureGroup5.name != string.Empty && textureGroup5.sets != null && textureGroup5.sets.Length > 1)
		{
			hasTextureGroup5 = true;
			if (Application.isPlaying)
			{
				List<string> list8 = new List<string>();
				for (int num5 = 0; num5 < textureGroup5.sets.Length; num5++)
				{
					list8.Add(textureGroup5.sets[num5].name);
				}
				textureGroup5JSON = new JSONStorableStringChooser(textureGroup5.name, list8, startingTextureGroup5Set, textureGroup5.name, SetTextureGroup5Set);
				RegisterStringChooser(textureGroup5JSON);
			}
		}
		if (!Application.isPlaying || textureGroup1 == null || !textureGroup1.mapTexturesToTextureNames || !(materialForDefaults != null))
		{
			return;
		}
		MaterialOptionTextureSet materialOptionTextureSet = null;
		if (textureGroup1.sets == null || textureGroup1.sets.Length == 0)
		{
			textureGroup1.sets = new MaterialOptionTextureSet[1];
			materialOptionTextureSet = new MaterialOptionTextureSet();
			materialOptionTextureSet.name = "Default";
			materialOptionTextureSet.textures = new Texture[6];
			textureGroup1.sets[0] = materialOptionTextureSet;
			startingTextureGroup1Set = "Default";
			currentTextureGroup1Set = startingTextureGroup1Set;
		}
		createUVTemplateTextureJSON = new JSONStorableAction("CreateUVTemplateTexture", CreateUVTemplateTexture);
		RegisterAction(createUVTemplateTextureJSON);
		createSimTemplateTextureJSON = new JSONStorableAction("CreateSimTemplateTexture", CreateSimTemplateTexture);
		RegisterAction(createSimTemplateTextureJSON);
		openTextureFolderInExplorerAction = new JSONStorableAction("OpenTextureFolderInExplorer", OpenTextureFolderInExplorer);
		RegisterAction(openTextureFolderInExplorerAction);
		if (textureGroup1.textureName != null && textureGroup1.textureName != string.Empty && materialForDefaults.HasProperty(textureGroup1.textureName))
		{
			customTexture1UrlJSON = new JSONStorableUrl("customTexture" + textureGroup1.textureName, string.Empty, SyncCustomTexture1Url, "jpg|jpeg|png|tif|tiff", customTextureFolder, true);
			customTexture1UrlJSON.beginBrowseWithObjectCallback = BeginBrowse;
			Texture2D texture2D = (Texture2D)materialForDefaults.GetTexture(textureGroup1.textureName);
			if (texture2D != null)
			{
				if (materialOptionTextureSet != null)
				{
					materialOptionTextureSet.textures[0] = texture2D;
				}
				string value;
				if (textureToSourcePath != null && textureToSourcePath.TryGetValue(texture2D, out value))
				{
					customTexture1UrlJSON.valNoCallback = value;
				}
			}
			RegisterUrl(customTexture1UrlJSON);
			Vector2 textureScale = materialForDefaults.GetTextureScale(textureGroup1.textureName);
			Vector2 textureOffset = materialForDefaults.GetTextureOffset(textureGroup1.textureName);
			customTexture1TileXJSON = new JSONStorableFloat("customTexture1TileX", textureScale.x, SyncCustomTexture1Tile, 0f, 10f, false);
			RegisterFloat(customTexture1TileXJSON);
			customTexture1TileYJSON = new JSONStorableFloat("customTexture1TileY", textureScale.y, SyncCustomTexture1Tile, 0f, 10f, false);
			RegisterFloat(customTexture1TileYJSON);
			customTexture1OffsetXJSON = new JSONStorableFloat("customTexture1OffsetX", textureOffset.x, SyncCustomTexture1Offset, -1f, 1f, false);
			RegisterFloat(customTexture1OffsetXJSON);
			customTexture1OffsetYJSON = new JSONStorableFloat("customTexture1OffsetY", textureOffset.y, SyncCustomTexture1Offset, -1f, 1f, false);
			RegisterFloat(customTexture1OffsetYJSON);
		}
		if (textureGroup1.secondaryTextureName != null && textureGroup1.secondaryTextureName != string.Empty && materialForDefaults.HasProperty(textureGroup1.secondaryTextureName))
		{
			customTexture2UrlJSON = new JSONStorableUrl("customTexture" + textureGroup1.secondaryTextureName, string.Empty, SyncCustomTexture2Url, "jpg|jpeg|png|tif|tiff", customTextureFolder, true);
			customTexture2UrlJSON.beginBrowseWithObjectCallback = BeginBrowse;
			Texture2D texture2D2 = (Texture2D)materialForDefaults.GetTexture(textureGroup1.secondaryTextureName);
			if (texture2D2 != null)
			{
				if (materialOptionTextureSet != null)
				{
					materialOptionTextureSet.textures[1] = texture2D2;
				}
				string value2;
				if (textureToSourcePath != null && textureToSourcePath.TryGetValue(texture2D2, out value2))
				{
					customTexture2UrlJSON.valNoCallback = value2;
				}
			}
			RegisterUrl(customTexture2UrlJSON);
			Vector2 textureScale2 = materialForDefaults.GetTextureScale(textureGroup1.secondaryTextureName);
			Vector2 textureOffset2 = materialForDefaults.GetTextureOffset(textureGroup1.secondaryTextureName);
			customTexture2TileXJSON = new JSONStorableFloat("customTexture2TileX", textureScale2.x, SyncCustomTexture2Tile, 0f, 10f, false);
			RegisterFloat(customTexture2TileXJSON);
			customTexture2TileYJSON = new JSONStorableFloat("customTexture2TileY", textureScale2.y, SyncCustomTexture2Tile, 0f, 10f, false);
			RegisterFloat(customTexture2TileYJSON);
			customTexture2OffsetXJSON = new JSONStorableFloat("customTexture2OffsetX", textureOffset2.x, SyncCustomTexture2Offset, -1f, 1f, false);
			RegisterFloat(customTexture2OffsetXJSON);
			customTexture2OffsetYJSON = new JSONStorableFloat("customTexture2OffsetY", textureOffset2.y, SyncCustomTexture2Offset, -1f, 1f, false);
			RegisterFloat(customTexture2OffsetYJSON);
		}
		if (textureGroup1.thirdTextureName != null && textureGroup1.thirdTextureName != string.Empty && materialForDefaults.HasProperty(textureGroup1.thirdTextureName))
		{
			customTexture3UrlJSON = new JSONStorableUrl("customTexture" + textureGroup1.thirdTextureName, string.Empty, SyncCustomTexture3Url, "jpg|jpeg|png|tif|tiff", customTextureFolder, true);
			customTexture3UrlJSON.beginBrowseWithObjectCallback = BeginBrowse;
			Texture2D texture2D3 = (Texture2D)materialForDefaults.GetTexture(textureGroup1.thirdTextureName);
			if (texture2D3 != null)
			{
				if (materialOptionTextureSet != null)
				{
					materialOptionTextureSet.textures[2] = texture2D3;
				}
				string value3;
				if (textureToSourcePath != null && textureToSourcePath.TryGetValue(texture2D3, out value3))
				{
					customTexture3UrlJSON.valNoCallback = value3;
				}
			}
			RegisterUrl(customTexture3UrlJSON);
			Vector2 textureScale3 = materialForDefaults.GetTextureScale(textureGroup1.thirdTextureName);
			Vector2 textureOffset3 = materialForDefaults.GetTextureOffset(textureGroup1.thirdTextureName);
			customTexture3TileXJSON = new JSONStorableFloat("customTexture3TileX", textureScale3.x, SyncCustomTexture3Tile, 0f, 10f, false);
			RegisterFloat(customTexture3TileXJSON);
			customTexture3TileYJSON = new JSONStorableFloat("customTexture3TileY", textureScale3.y, SyncCustomTexture3Tile, 0f, 10f, false);
			RegisterFloat(customTexture3TileYJSON);
			customTexture3OffsetXJSON = new JSONStorableFloat("customTexture3OffsetX", textureOffset3.x, SyncCustomTexture3Offset, -1f, 1f, false);
			RegisterFloat(customTexture3OffsetXJSON);
			customTexture3OffsetYJSON = new JSONStorableFloat("customTexture3OffsetY", textureOffset3.y, SyncCustomTexture3Offset, -1f, 1f, false);
			RegisterFloat(customTexture3OffsetYJSON);
		}
		if (textureGroup1.fourthTextureName != null && textureGroup1.fourthTextureName != string.Empty && materialForDefaults.HasProperty(textureGroup1.fourthTextureName))
		{
			customTexture4UrlJSON = new JSONStorableUrl("customTexture" + textureGroup1.fourthTextureName, string.Empty, SyncCustomTexture4Url, "jpg|jpeg|png|tif|tiff", customTextureFolder, true);
			customTexture4UrlJSON.beginBrowseWithObjectCallback = BeginBrowse;
			Texture2D texture2D4 = (Texture2D)materialForDefaults.GetTexture(textureGroup1.fourthTextureName);
			if (texture2D4 != null)
			{
				if (materialOptionTextureSet != null)
				{
					materialOptionTextureSet.textures[3] = texture2D4;
				}
				string value4;
				if (textureToSourcePath != null && textureToSourcePath.TryGetValue(texture2D4, out value4))
				{
					customTexture4UrlJSON.valNoCallback = value4;
				}
			}
			RegisterUrl(customTexture4UrlJSON);
			Vector2 textureScale4 = materialForDefaults.GetTextureScale(textureGroup1.fourthTextureName);
			Vector2 textureOffset4 = materialForDefaults.GetTextureOffset(textureGroup1.fourthTextureName);
			customTexture4TileXJSON = new JSONStorableFloat("customTexture4TileX", textureScale4.x, SyncCustomTexture4Tile, 0f, 10f, false);
			RegisterFloat(customTexture4TileXJSON);
			customTexture4TileYJSON = new JSONStorableFloat("customTexture4TileY", textureScale4.y, SyncCustomTexture4Tile, 0f, 10f, false);
			RegisterFloat(customTexture4TileYJSON);
			customTexture4OffsetXJSON = new JSONStorableFloat("customTexture4OffsetX", textureOffset4.x, SyncCustomTexture4Offset, -1f, 1f, false);
			RegisterFloat(customTexture4OffsetXJSON);
			customTexture4OffsetYJSON = new JSONStorableFloat("customTexture4OffsetY", textureOffset4.y, SyncCustomTexture4Offset, -1f, 1f, false);
			RegisterFloat(customTexture4OffsetYJSON);
		}
		if (textureGroup1.fifthTextureName != null && textureGroup1.fifthTextureName != string.Empty && materialForDefaults.HasProperty(textureGroup1.fifthTextureName))
		{
			customTexture5UrlJSON = new JSONStorableUrl("customTexture" + textureGroup1.fifthTextureName, string.Empty, SyncCustomTexture5Url, "jpg|jpeg|png|tif|tiff", customTextureFolder, true);
			customTexture5UrlJSON.beginBrowseWithObjectCallback = BeginBrowse;
			Texture2D texture2D5 = (Texture2D)materialForDefaults.GetTexture(textureGroup1.fifthTextureName);
			if (texture2D5 != null)
			{
				if (materialOptionTextureSet != null)
				{
					materialOptionTextureSet.textures[4] = texture2D5;
				}
				string value5;
				if (textureToSourcePath != null && textureToSourcePath.TryGetValue(texture2D5, out value5))
				{
					customTexture5UrlJSON.valNoCallback = value5;
				}
			}
			RegisterUrl(customTexture5UrlJSON);
			Vector2 textureScale5 = materialForDefaults.GetTextureScale(textureGroup1.fifthTextureName);
			Vector2 textureOffset5 = materialForDefaults.GetTextureOffset(textureGroup1.fifthTextureName);
			customTexture5TileXJSON = new JSONStorableFloat("customTexture5TileX", textureScale5.x, SyncCustomTexture5Tile, 0f, 10f, false);
			RegisterFloat(customTexture5TileXJSON);
			customTexture5TileYJSON = new JSONStorableFloat("customTexture5TileY", textureScale5.y, SyncCustomTexture5Tile, 0f, 10f, false);
			RegisterFloat(customTexture5TileYJSON);
			customTexture5OffsetXJSON = new JSONStorableFloat("customTexture5OffsetX", textureOffset5.x, SyncCustomTexture5Offset, -1f, 1f, false);
			RegisterFloat(customTexture5OffsetXJSON);
			customTexture5OffsetYJSON = new JSONStorableFloat("customTexture5OffsetY", textureOffset5.y, SyncCustomTexture5Offset, -1f, 1f, false);
			RegisterFloat(customTexture5OffsetYJSON);
		}
		if (textureGroup1.sixthTextureName == null || !(textureGroup1.sixthTextureName != string.Empty) || !materialForDefaults.HasProperty(textureGroup1.sixthTextureName))
		{
			return;
		}
		customTexture6UrlJSON = new JSONStorableUrl("customTexture" + textureGroup1.sixthTextureName, string.Empty, SyncCustomTexture6Url, "jpg|jpeg|png|tif|tiff", customTextureFolder, true);
		customTexture6UrlJSON.beginBrowseWithObjectCallback = BeginBrowse;
		Texture2D texture2D6 = (Texture2D)materialForDefaults.GetTexture(textureGroup1.sixthTextureName);
		if (texture2D6 != null)
		{
			if (materialOptionTextureSet != null)
			{
				materialOptionTextureSet.textures[5] = texture2D6;
			}
			string value6;
			if (textureToSourcePath != null && textureToSourcePath.TryGetValue(texture2D6, out value6))
			{
				customTexture6UrlJSON.valNoCallback = value6;
			}
		}
		RegisterUrl(customTexture6UrlJSON);
		Vector2 textureScale6 = materialForDefaults.GetTextureScale(textureGroup1.sixthTextureName);
		Vector2 textureOffset6 = materialForDefaults.GetTextureOffset(textureGroup1.sixthTextureName);
		customTexture6TileXJSON = new JSONStorableFloat("customTexture6TileX", textureScale6.x, SyncCustomTexture6Tile, 0f, 10f, false);
		RegisterFloat(customTexture6TileXJSON);
		customTexture6TileYJSON = new JSONStorableFloat("customTexture6TileY", textureScale6.y, SyncCustomTexture6Tile, 0f, 10f, false);
		RegisterFloat(customTexture6TileYJSON);
		customTexture6OffsetXJSON = new JSONStorableFloat("customTexture6OffsetX", textureOffset6.x, SyncCustomTexture6Offset, -1f, 1f, false);
		RegisterFloat(customTexture6OffsetXJSON);
		customTexture6OffsetYJSON = new JSONStorableFloat("customTexture6OffsetY", textureOffset6.y, SyncCustomTexture6Offset, -1f, 1f, false);
		RegisterFloat(customTexture6OffsetYJSON);
	}

	protected virtual void SetAllStartingValuesToCurrentValues()
	{
	}

	protected virtual void SetAllParameters()
	{
		if (materialForDefaults != null)
		{
			if (color1Name != null && color1Name != string.Empty && materialForDefaults.HasProperty(color1Name))
			{
				SetMaterialColor(color1Name, color1CurrentColor);
			}
			if (color1Name2 != null && color1Name2 != string.Empty && materialForDefaults.HasProperty(color1Name2))
			{
				SetMaterialColor(color1Name2, color1CurrentColor);
			}
			if (color2Name != null && color2Name != string.Empty && materialForDefaults.HasProperty(color2Name))
			{
				SetMaterialColor(color2Name, color2CurrentColor);
			}
			if (color3Name != null && color3Name != string.Empty && materialForDefaults.HasProperty(color3Name))
			{
				SetMaterialColor(color3Name, color3CurrentColor);
			}
			if (param1Name != null && param1Name != string.Empty && materialForDefaults.HasProperty(param1Name))
			{
				SetMaterialParam(param1Name, param1CurrentValue);
			}
			if (param2Name != null && param2Name != string.Empty && materialForDefaults.HasProperty(param2Name))
			{
				SetMaterialParam(param2Name, param2CurrentValue);
			}
			if (param3Name != null && param3Name != string.Empty && materialForDefaults.HasProperty(param3Name))
			{
				SetMaterialParam(param3Name, param3CurrentValue);
			}
			if (param4Name != null && param4Name != string.Empty && materialForDefaults.HasProperty(param4Name))
			{
				SetMaterialParam(param4Name, param4CurrentValue);
			}
			if (param5Name != null && param5Name != string.Empty && materialForDefaults.HasProperty(param5Name))
			{
				SetMaterialParam(param5Name, param5CurrentValue);
			}
			if (param6Name != null && param6Name != string.Empty && materialForDefaults.HasProperty(param6Name))
			{
				SetMaterialParam(param6Name, param6CurrentValue);
			}
			if (param7Name != null && param7Name != string.Empty && materialForDefaults.HasProperty(param7Name))
			{
				SetMaterialParam(param7Name, param7CurrentValue);
			}
			if (param8Name != null && param8Name != string.Empty && materialForDefaults.HasProperty(param8Name))
			{
				SetMaterialParam(param8Name, param8CurrentValue);
			}
			if (param9Name != null && param9Name != string.Empty && materialForDefaults.HasProperty(param9Name))
			{
				SetMaterialParam(param9Name, param9CurrentValue);
			}
			if (param10Name != null && param10Name != string.Empty && materialForDefaults.HasProperty(param10Name))
			{
				SetMaterialParam(param10Name, param10CurrentValue);
			}
			if (renderQueueJSON != null)
			{
				SyncRenderQueue(renderQueueJSON.val);
			}
			if (hideMaterialJSON != null)
			{
				SyncHideMaterial(hideMaterialJSON.val);
			}
		}
		if (textureGroup1 != null && textureGroup1.sets != null && textureGroup1.sets.Length > 0)
		{
			SetTextureGroup1Set(currentTextureGroup1Set);
		}
		if (textureGroup2 != null && textureGroup2.sets != null && textureGroup2.sets.Length > 0)
		{
			SetTextureGroup2Set(currentTextureGroup2Set);
		}
		if (textureGroup3 != null && textureGroup3.sets != null && textureGroup3.sets.Length > 0)
		{
			SetTextureGroup3Set(currentTextureGroup3Set);
		}
		if (textureGroup4 != null && textureGroup4.sets != null && textureGroup4.sets.Length > 0)
		{
			SetTextureGroup4Set(currentTextureGroup4Set);
		}
		if (textureGroup5 != null && textureGroup5.sets != null && textureGroup5.sets.Length > 0)
		{
			SetTextureGroup5Set(currentTextureGroup5Set);
		}
	}

	public virtual void SyncAllParameters()
	{
		SetStartingValues();
		SetAllParameters();
	}

	private void Start()
	{
		if (!deregisterOnDisable)
		{
			InitUI();
			InitUIAlt();
		}
	}

	public void CheckAwake()
	{
		Awake();
	}

	protected override void Awake()
	{
		if (awakecalled)
		{
			return;
		}
		base.Awake();
		if (base.enabled)
		{
			if (Application.isPlaying && hideShader == null)
			{
				hideShader = Shader.Find("Custom/Discard");
			}
			SetStartingValues();
		}
	}

	protected virtual void OnEnable()
	{
		if (deregisterOnDisable)
		{
			InitUI();
			InitUIAlt();
		}
	}

	protected virtual void OnDisable()
	{
		if (deregisterOnDisable)
		{
			DeregisterUI();
		}
	}

	protected virtual void OnDestroy()
	{
		DestroyAllocatedObjects();
		DeregisterAllTextures();
	}
}
