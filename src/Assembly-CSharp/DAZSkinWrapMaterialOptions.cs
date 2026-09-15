using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class DAZSkinWrapMaterialOptions : MaterialOptions
{
	public delegate void SimTextureLoaded();

	[HideInInspector]
	[SerializeField]
	protected DAZSkinWrap _skinWrap;

	[HideInInspector]
	[SerializeField]
	protected DAZSkinWrap _skinWrap2;

	public bool useSimpleMaterial;

	public SimTextureLoaded simTextureLoadedHandlers;

	public Button customSimTextureFileBrowseButton;

	public Button customSimTextureReloadButton;

	public Button customSimTextureClearButton;

	public Button customSimTextureNullButton;

	public Button customSimTextureDefaultButton;

	public Text customSimTextureUrlText;

	protected Texture2D defaultSimTexture;

	protected Texture2D customSimTexture;

	protected bool customSimTextureIsNull;

	protected JSONStorableUrl customSimTextureUrlJSON;

	public DAZSkinWrap skinWrap
	{
		get
		{
			if (_skinWrap == null)
			{
				DAZSkinWrap[] components = GetComponents<DAZSkinWrap>();
				DAZSkinWrap[] array = components;
				foreach (DAZSkinWrap dAZSkinWrap in array)
				{
					if (dAZSkinWrap.enabled && dAZSkinWrap.draw)
					{
						_skinWrap = dAZSkinWrap;
					}
				}
			}
			return _skinWrap;
		}
		set
		{
			if (_skinWrap != value)
			{
				_skinWrap = value;
				SetAllParameters();
			}
		}
	}

	public DAZSkinWrap skinWrap2
	{
		get
		{
			return _skinWrap2;
		}
		set
		{
			if (_skinWrap2 != value)
			{
				_skinWrap2 = value;
				SetAllParameters();
			}
		}
	}

	public bool hasCustomSimTexture
	{
		get
		{
			if (customSimTextureUrlJSON != null && customSimTextureUrlJSON.val != string.Empty)
			{
				return true;
			}
			return false;
		}
	}

	protected void OnSimTextureLoaded(ImageLoaderThreaded.QueuedImage qi)
	{
		if (!(this != null))
		{
			return;
		}
		RegisterTexture(qi.tex);
		DeregisterTexture(customSimTexture);
		customSimTexture = qi.tex;
		customSimTextureIsNull = qi.imgPath == "NULL";
		if (skinWrap != null && paramMaterialSlots != null)
		{
			for (int i = 0; i < paramMaterialSlots.Length; i++)
			{
				int num = paramMaterialSlots[i];
				if (num < skinWrap.simTextures.Length)
				{
					skinWrap.simTextures[num] = customSimTexture;
				}
			}
		}
		if (skinWrap2 != null && paramMaterialSlots2 != null)
		{
			for (int j = 0; j < paramMaterialSlots2.Length; j++)
			{
				int num2 = paramMaterialSlots2[j];
				if (num2 < skinWrap2.simTextures.Length)
				{
					skinWrap2.simTextures[num2] = customSimTexture;
				}
			}
		}
		if (simTextureLoadedHandlers != null)
		{
			simTextureLoadedHandlers();
		}
	}

	protected void SyncCustomSimTextureUrl(JSONStorableString jstr)
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
				QueueCustomTexture(val, jSONStorableUrl.valueSetFromBrowse, false, false, false, OnSimTextureLoaded);
			}
		}
		else
		{
			QueueCustomTexture(null, jSONStorableUrl.valueSetFromBrowse, false, false, false, OnSimTextureLoaded);
		}
		if (!allowLinkToOtherMaterials || !_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			DAZSkinWrapMaterialOptions dAZSkinWrapMaterialOptions = otherMaterialOptions as DAZSkinWrapMaterialOptions;
			if (dAZSkinWrapMaterialOptions != null && dAZSkinWrapMaterialOptions.customSimTextureUrlJSON != null)
			{
				dAZSkinWrapMaterialOptions.customSimTextureUrlJSON.val = customSimTextureUrlJSON.val;
			}
		}
	}

	protected void SetCustomSimTextureToNull()
	{
		if (customSimTextureUrlJSON != null)
		{
			customSimTextureUrlJSON.val = "NULL";
		}
	}

	protected override void SetMaterialHide(bool b)
	{
		if (!(hideShader != null))
		{
			return;
		}
		if (materialToOriginalShader == null)
		{
			materialToOriginalShader = new Dictionary<Material, Shader>();
		}
		if (skinWrap != null)
		{
			skinWrap.InitMaterials();
			if (useSimpleMaterial)
			{
				Material gPUsimpleMaterial = skinWrap.GPUsimpleMaterial;
				SetMaterialHide(gPUsimpleMaterial, b);
			}
			else if (paramMaterialSlots != null)
			{
				for (int i = 0; i < paramMaterialSlots.Length; i++)
				{
					int num = paramMaterialSlots[i];
					if (num < skinWrap.numMaterials)
					{
						Material m = skinWrap.GPUmaterials[num];
						SetMaterialHide(m, b);
					}
				}
			}
			skinWrap.FlushBuffers();
		}
		if (!(skinWrap2 != null))
		{
			return;
		}
		skinWrap2.InitMaterials();
		if (useSimpleMaterial)
		{
			Material gPUsimpleMaterial2 = skinWrap2.GPUsimpleMaterial;
			SetMaterialHide(gPUsimpleMaterial2, b);
		}
		else if (paramMaterialSlots2 != null)
		{
			for (int j = 0; j < paramMaterialSlots2.Length; j++)
			{
				int num2 = paramMaterialSlots2[j];
				if (num2 < skinWrap2.numMaterials)
				{
					Material m2 = skinWrap2.GPUmaterials[num2];
					SetMaterialHide(m2, b);
				}
			}
		}
		skinWrap2.FlushBuffers();
	}

	protected override void SetMaterialRenderQueue(int q)
	{
		if (skinWrap != null)
		{
			skinWrap.InitMaterials();
			if (useSimpleMaterial)
			{
				Material gPUsimpleMaterial = skinWrap.GPUsimpleMaterial;
				gPUsimpleMaterial.renderQueue = q;
			}
			else if (paramMaterialSlots != null)
			{
				for (int i = 0; i < paramMaterialSlots.Length; i++)
				{
					int num = paramMaterialSlots[i];
					if (num < skinWrap.numMaterials)
					{
						Material material = skinWrap.GPUmaterials[num];
						material.renderQueue = q;
					}
				}
			}
		}
		if (!(skinWrap2 != null))
		{
			return;
		}
		skinWrap2.InitMaterials();
		if (useSimpleMaterial)
		{
			Material gPUsimpleMaterial2 = skinWrap2.GPUsimpleMaterial;
			gPUsimpleMaterial2.renderQueue = q;
		}
		else
		{
			if (paramMaterialSlots2 == null)
			{
				return;
			}
			for (int j = 0; j < paramMaterialSlots2.Length; j++)
			{
				int num2 = paramMaterialSlots2[j];
				if (num2 < skinWrap2.numMaterials)
				{
					Material material2 = skinWrap2.GPUmaterials[num2];
					material2.renderQueue = q;
				}
			}
		}
	}

	protected override void SetMaterialParam(string name, float value)
	{
		if (skinWrap != null)
		{
			skinWrap.InitMaterials();
			if (useSimpleMaterial)
			{
				Material gPUsimpleMaterial = skinWrap.GPUsimpleMaterial;
				if (gPUsimpleMaterial.HasProperty(name))
				{
					gPUsimpleMaterial.SetFloat(name, value);
				}
			}
			else if (paramMaterialSlots != null)
			{
				for (int i = 0; i < paramMaterialSlots.Length; i++)
				{
					int num = paramMaterialSlots[i];
					if (num < skinWrap.numMaterials)
					{
						Material material = skinWrap.GPUmaterials[num];
						if (material != null && material.HasProperty(name))
						{
							material.SetFloat(name, value);
						}
					}
				}
			}
		}
		if (!(skinWrap2 != null))
		{
			return;
		}
		skinWrap2.InitMaterials();
		if (useSimpleMaterial)
		{
			Material gPUsimpleMaterial2 = skinWrap2.GPUsimpleMaterial;
			if (gPUsimpleMaterial2.HasProperty(name))
			{
				gPUsimpleMaterial2.SetFloat(name, value);
			}
		}
		else
		{
			if (paramMaterialSlots2 == null)
			{
				return;
			}
			for (int j = 0; j < paramMaterialSlots2.Length; j++)
			{
				int num2 = paramMaterialSlots2[j];
				if (num2 < skinWrap2.numMaterials)
				{
					Material material2 = skinWrap2.GPUmaterials[num2];
					if (material2 != null && material2.HasProperty(name))
					{
						material2.SetFloat(name, value);
					}
				}
			}
		}
	}

	protected override void SetMaterialColor(string name, Color c)
	{
		if (skinWrap != null)
		{
			skinWrap.InitMaterials();
			if (useSimpleMaterial)
			{
				Material gPUsimpleMaterial = skinWrap.GPUsimpleMaterial;
				if (gPUsimpleMaterial.HasProperty(name))
				{
					gPUsimpleMaterial.SetColor(name, c);
				}
			}
			else if (paramMaterialSlots != null)
			{
				for (int i = 0; i < paramMaterialSlots.Length; i++)
				{
					int num = paramMaterialSlots[i];
					if (num < skinWrap.numMaterials)
					{
						Material material = skinWrap.GPUmaterials[num];
						if (material != null && material.HasProperty(name))
						{
							material.SetColor(name, c);
						}
					}
				}
			}
		}
		if (!(skinWrap2 != null))
		{
			return;
		}
		skinWrap2.InitMaterials();
		if (useSimpleMaterial)
		{
			Material gPUsimpleMaterial2 = skinWrap2.GPUsimpleMaterial;
			if (gPUsimpleMaterial2.HasProperty(name))
			{
				gPUsimpleMaterial2.SetColor(name, c);
			}
		}
		else
		{
			if (paramMaterialSlots2 == null)
			{
				return;
			}
			for (int j = 0; j < paramMaterialSlots2.Length; j++)
			{
				int num2 = paramMaterialSlots2[j];
				if (num2 < skinWrap2.numMaterials)
				{
					Material material2 = skinWrap2.GPUmaterials[num2];
					if (material2 != null && material2.HasProperty(name))
					{
						material2.SetColor(name, c);
					}
				}
			}
		}
	}

	protected override void SetMaterialTexture(int slot, string propName, Texture texture)
	{
		if (skinWrap != null)
		{
			skinWrap.InitMaterials();
			if (slot < skinWrap.numMaterials)
			{
				Material m = skinWrap.GPUmaterials[slot];
				SetMaterialTexture(m, propName, texture);
			}
		}
	}

	protected override void SetMaterialTexture2(int slot, string propName, Texture texture)
	{
		if (skinWrap2 != null)
		{
			skinWrap2.InitMaterials();
			if (slot < skinWrap2.numMaterials)
			{
				Material m = skinWrap2.GPUmaterials[slot];
				SetMaterialTexture(m, propName, texture);
			}
		}
	}

	protected override void SetMaterialTextureScale(int slot, string propName, Vector2 scale)
	{
		if (skinWrap != null)
		{
			skinWrap.InitMaterials();
			if (slot < skinWrap.numMaterials)
			{
				Material m = skinWrap.GPUmaterials[slot];
				SetMaterialTextureScale(m, propName, scale);
			}
		}
	}

	protected override void SetMaterialTextureScale2(int slot, string propName, Vector2 scale)
	{
		if (skinWrap2 != null)
		{
			skinWrap2.InitMaterials();
			if (slot < skinWrap2.numMaterials)
			{
				Material m = skinWrap2.GPUmaterials[slot];
				SetMaterialTextureScale(m, propName, scale);
			}
		}
	}

	protected override void SetMaterialTextureOffset(int slot, string propName, Vector2 offset)
	{
		if (skinWrap != null)
		{
			skinWrap.InitMaterials();
			if (slot < skinWrap.numMaterials)
			{
				Material m = skinWrap.GPUmaterials[slot];
				SetMaterialTextureOffset(m, propName, offset);
			}
		}
	}

	protected override void SetMaterialTextureOffset2(int slot, string propName, Vector2 offset)
	{
		if (skinWrap2 != null)
		{
			skinWrap2.InitMaterials();
			if (slot < skinWrap2.numMaterials)
			{
				Material m = skinWrap2.GPUmaterials[slot];
				SetMaterialTextureOffset(m, propName, offset);
			}
		}
	}

	public override Mesh GetMesh()
	{
		Mesh result = null;
		if (skinWrap != null)
		{
			result = skinWrap.GetStartMesh();
		}
		return result;
	}

	public override void SetCustomTextureFolder(string path)
	{
		base.SetCustomTextureFolder(path);
		if (customSimTextureUrlJSON != null)
		{
			customSimTextureUrlJSON.suggestedPath = customTextureFolder;
		}
	}

	protected override void SyncLinkToOtherMaterials(bool b)
	{
		base.SyncLinkToOtherMaterials(b);
		if (!_linkToOtherMaterials)
		{
			return;
		}
		foreach (MaterialOptions otherMaterialOptions in otherMaterialOptionsList)
		{
			DAZSkinWrapMaterialOptions dAZSkinWrapMaterialOptions = otherMaterialOptions as DAZSkinWrapMaterialOptions;
			if (dAZSkinWrapMaterialOptions != null && dAZSkinWrapMaterialOptions.customSimTextureUrlJSON != null && customSimTextureUrlJSON != null)
			{
				dAZSkinWrapMaterialOptions.customSimTextureUrlJSON.val = customSimTextureUrlJSON.val;
			}
		}
	}

	public override void SetStartingValues(Dictionary<Texture2D, string> textureToSourcePath)
	{
		if (skinWrap != null && materialForDefaults == null)
		{
			skinWrap.InitMaterials();
			DAZMesh componentInChildren = skinWrap.GetComponentInChildren<DAZMergedMesh>(true);
			if (componentInChildren == null)
			{
				componentInChildren = skinWrap.GetComponentInChildren<DAZMesh>(true);
			}
			if (paramMaterialSlots != null && paramMaterialSlots.Length > 0)
			{
				int num = paramMaterialSlots[0];
				if (componentInChildren != null && num < componentInChildren.numMaterials)
				{
					materialForDefaults = componentInChildren.materials[num];
				}
				if (skinWrap.simTextures != null && num < skinWrap.simTextures.Length)
				{
					defaultSimTexture = skinWrap.simTextures[num];
				}
			}
		}
		base.SetStartingValues(textureToSourcePath);
		if (Application.isPlaying)
		{
			if (customSimTextureUrlJSON != null)
			{
				DeregisterUrl(customSimTextureUrlJSON);
				customSimTextureUrlJSON = null;
			}
			customSimTextureUrlJSON = new JSONStorableUrl("simTexture", string.Empty, SyncCustomSimTextureUrl, "jpg|jpeg|png|tif|tiff", customTextureFolder, true);
			customSimTextureUrlJSON.beginBrowseWithObjectCallback = base.BeginBrowse;
			RegisterUrl(customSimTextureUrlJSON);
		}
	}

	public override void InitUI()
	{
		base.InitUI();
		if (!(UITransform != null))
		{
			return;
		}
		DAZSkinWrapMaterialOptionsUI componentInChildren = UITransform.GetComponentInChildren<DAZSkinWrapMaterialOptionsUI>();
		if (componentInChildren != null)
		{
			customSimTextureFileBrowseButton = componentInChildren.customSimTextureFileBrowseButton;
			customSimTextureReloadButton = componentInChildren.customSimTextureReloadButton;
			customSimTextureClearButton = componentInChildren.customSimTextureClearButton;
			customSimTextureNullButton = componentInChildren.customSimTextureNullButton;
			customSimTextureDefaultButton = componentInChildren.customSimTextureDefaultButton;
			customSimTextureUrlText = componentInChildren.customSimTextureUrlText;
		}
		if (customSimTextureUrlJSON != null)
		{
			customSimTextureUrlJSON.fileBrowseButton = customSimTextureFileBrowseButton;
			customSimTextureUrlJSON.reloadButton = customSimTextureReloadButton;
			customSimTextureUrlJSON.clearButton = customSimTextureClearButton;
			customSimTextureUrlJSON.defaultButton = customSimTextureDefaultButton;
			customSimTextureUrlJSON.text = customSimTextureUrlText;
			if (customSimTextureNullButton != null)
			{
				customSimTextureNullButton.onClick.AddListener(SetCustomSimTextureToNull);
			}
		}
	}

	public override void DeregisterUI()
	{
		base.DeregisterUI();
		if (customSimTextureUrlJSON != null)
		{
			customSimTextureUrlJSON.fileBrowseButton = null;
			customSimTextureUrlJSON.reloadButton = null;
			customSimTextureUrlJSON.clearButton = null;
			customSimTextureUrlJSON.defaultButton = null;
			if (customSimTextureNullButton != null)
			{
				customSimTextureNullButton.onClick.RemoveListener(SetCustomSimTextureToNull);
			}
		}
	}
}
