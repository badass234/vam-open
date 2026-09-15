using System.Collections.Generic;
using UnityEngine;

public class DAZCharacterMaterialOptions : MaterialOptions
{
	public enum TextureGroupDisableZone
	{
		None,
		Face,
		Limbs,
		Torso,
		Genitals,
		TorsoAndFace,
		TorsoAndLimbs,
		TorsoAndGenitals
	}

	[HideInInspector]
	public Transform skinContainer;

	[HideInInspector]
	[SerializeField]
	protected DAZSkinV2 _skin;

	public bool isPassthrough;

	public Transform passThroughToBucket1;

	public Transform passThroughToBucket2;

	public bool useSimpleMaterial;

	public TextureGroupDisableZone textureGroup1DisableZone;

	public TextureGroupDisableZone textureGroup2DisableZone;

	public TextureGroupDisableZone textureGroup3DisableZone;

	public TextureGroupDisableZone textureGroup4DisableZone;

	public TextureGroupDisableZone textureGroup5DisableZone;

	protected DAZCharacterTextureControl characterTextureControl;

	public DAZSkinV2 skin
	{
		get
		{
			return _skin;
		}
		set
		{
			if (!isPassthrough && _skin != value)
			{
				_skin = value;
				if (Application.isPlaying)
				{
					SetAllParameters();
				}
			}
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
		if (!(skin != null))
		{
			return;
		}
		if (useSimpleMaterial)
		{
			Material gPUsimpleMaterial = skin.GPUsimpleMaterial;
			SetMaterialHide(gPUsimpleMaterial, b);
		}
		else if (paramMaterialSlots != null)
		{
			for (int i = 0; i < paramMaterialSlots.Length; i++)
			{
				int num = paramMaterialSlots[i];
				if (num < skin.numMaterials)
				{
					Material material = skin.GPUmaterials[num];
					if (material != null)
					{
						SetMaterialHide(material, b);
					}
				}
			}
		}
		skin.FlushBuffers();
	}

	protected override void SetMaterialRenderQueue(int q)
	{
		if (!(skin != null))
		{
			return;
		}
		if (useSimpleMaterial)
		{
			Material gPUsimpleMaterial = skin.GPUsimpleMaterial;
			gPUsimpleMaterial.renderQueue = q;
		}
		else
		{
			if (paramMaterialSlots == null)
			{
				return;
			}
			for (int i = 0; i < paramMaterialSlots.Length; i++)
			{
				int num = paramMaterialSlots[i];
				if (num < skin.numMaterials)
				{
					Material material = skin.GPUmaterials[num];
					if (material != null)
					{
						material.renderQueue = q;
					}
				}
			}
		}
	}

	protected override void SetMaterialParam(string name, float value)
	{
		if (!(skin != null))
		{
			return;
		}
		if (useSimpleMaterial)
		{
			Material gPUsimpleMaterial = skin.GPUsimpleMaterial;
			if (gPUsimpleMaterial.HasProperty(name))
			{
				gPUsimpleMaterial.SetFloat(name, value);
			}
		}
		else
		{
			if (paramMaterialSlots == null)
			{
				return;
			}
			for (int i = 0; i < paramMaterialSlots.Length; i++)
			{
				int num = paramMaterialSlots[i];
				if (num < skin.numMaterials)
				{
					Material material = skin.GPUmaterials[num];
					if (material != null && material.HasProperty(name))
					{
						material.SetFloat(name, value);
					}
				}
			}
		}
	}

	protected override void SetMaterialColor(string name, Color c)
	{
		if (!(skin != null))
		{
			return;
		}
		if (useSimpleMaterial)
		{
			Material gPUsimpleMaterial = skin.GPUsimpleMaterial;
			if (gPUsimpleMaterial.HasProperty(name))
			{
				gPUsimpleMaterial.SetColor(name, c);
			}
		}
		else
		{
			if (paramMaterialSlots == null)
			{
				return;
			}
			for (int i = 0; i < paramMaterialSlots.Length; i++)
			{
				int num = paramMaterialSlots[i];
				if (num < skin.numMaterials)
				{
					Material material = skin.GPUmaterials[num];
					if (material != null && material.HasProperty(name))
					{
						material.SetColor(name, c);
					}
				}
			}
		}
	}

	protected override void SetMaterialTexture(int slot, string propName, Texture texture)
	{
		if (paramMaterialSlots != null && skin != null && slot < skin.numMaterials)
		{
			Material m = skin.GPUmaterials[slot];
			SetMaterialTexture(m, propName, texture);
		}
	}

	protected override void SetMaterialTexture2(int slot, string propName, Texture texture)
	{
		if (paramMaterialSlots != null && skin != null && slot < skin.numMaterials)
		{
			Material m = skin.GPUmaterials[slot];
			SetMaterialTexture(m, propName, texture);
		}
	}

	protected bool CheckNullifyTextureGroupSet(MaterialOptionTextureGroup textureGroup, TextureGroupDisableZone disableZone)
	{
		bool result = false;
		switch (disableZone)
		{
		case TextureGroupDisableZone.Face:
			if (textureGroup.mapTexturesToTextureNames)
			{
				if (characterTextureControl.hasFaceTextureSet)
				{
					result = true;
				}
			}
			else if (characterTextureControl.hasFaceDiffuseTextureSet)
			{
				result = true;
			}
			break;
		case TextureGroupDisableZone.Limbs:
			if (textureGroup.mapTexturesToTextureNames)
			{
				if (characterTextureControl.hasLimbsTextureSet)
				{
					result = true;
				}
			}
			else if (characterTextureControl.hasLimbsDiffuseTextureSet)
			{
				result = true;
			}
			break;
		case TextureGroupDisableZone.Torso:
			if (textureGroup.mapTexturesToTextureNames)
			{
				if (characterTextureControl.hasTorsoTextureSet)
				{
					result = true;
				}
			}
			else if (characterTextureControl.hasTorsoDiffuseTextureSet)
			{
				result = true;
			}
			break;
		case TextureGroupDisableZone.TorsoAndFace:
			if (textureGroup.mapTexturesToTextureNames)
			{
				if (characterTextureControl.hasTorsoTextureSet || characterTextureControl.hasFaceTextureSet)
				{
					result = true;
				}
			}
			else if (characterTextureControl.hasTorsoDiffuseTextureSet || characterTextureControl.hasFaceDiffuseTextureSet)
			{
				result = true;
			}
			break;
		case TextureGroupDisableZone.TorsoAndLimbs:
			if (textureGroup.mapTexturesToTextureNames)
			{
				if (characterTextureControl.hasTorsoTextureSet || characterTextureControl.hasLimbsTextureSet)
				{
					result = true;
				}
			}
			else if (characterTextureControl.hasTorsoDiffuseTextureSet || characterTextureControl.hasLimbsDiffuseTextureSet)
			{
				result = true;
			}
			break;
		case TextureGroupDisableZone.TorsoAndGenitals:
			if (textureGroup.mapTexturesToTextureNames)
			{
				if (characterTextureControl.hasTorsoTextureSet || characterTextureControl.hasGenitalsTextureSet)
				{
					result = true;
				}
			}
			else if (characterTextureControl.hasTorsoDiffuseTextureSet || characterTextureControl.hasGenitalsDiffuseTextureSet)
			{
				result = true;
			}
			break;
		case TextureGroupDisableZone.Genitals:
			if (textureGroup.mapTexturesToTextureNames)
			{
				if (characterTextureControl.hasGenitalsTextureSet)
				{
					result = true;
				}
			}
			else if (characterTextureControl.hasGenitalsDiffuseTextureSet)
			{
				result = true;
			}
			break;
		}
		return result;
	}

	protected override void SetTextureGroup1Set(string setName)
	{
		bool flag = false;
		if (textureGroup1DisableZone != 0 && characterTextureControl != null)
		{
			flag = CheckNullifyTextureGroupSet(textureGroup1, textureGroup1DisableZone);
		}
		if (flag)
		{
			textureGroup1JSON.valNoCallback = currentTextureGroup1Set;
		}
		else
		{
			base.SetTextureGroup1Set(setName);
		}
	}

	protected override void SetTextureGroup2Set(string setName)
	{
		bool flag = false;
		if (textureGroup2DisableZone != 0 && characterTextureControl != null)
		{
			flag = CheckNullifyTextureGroupSet(textureGroup2, textureGroup2DisableZone);
		}
		if (flag)
		{
			textureGroup2JSON.valNoCallback = currentTextureGroup2Set;
		}
		else
		{
			base.SetTextureGroup2Set(setName);
		}
	}

	protected override void SetTextureGroup3Set(string setName)
	{
		bool flag = false;
		if (textureGroup3DisableZone != 0 && characterTextureControl != null)
		{
			flag = CheckNullifyTextureGroupSet(textureGroup3, textureGroup3DisableZone);
		}
		if (flag)
		{
			textureGroup3JSON.valNoCallback = currentTextureGroup3Set;
		}
		else
		{
			base.SetTextureGroup3Set(setName);
		}
	}

	protected override void SetTextureGroup4Set(string setName)
	{
		bool flag = false;
		if (textureGroup4DisableZone != 0 && characterTextureControl != null)
		{
			flag = CheckNullifyTextureGroupSet(textureGroup4, textureGroup4DisableZone);
		}
		if (flag)
		{
			textureGroup4JSON.valNoCallback = currentTextureGroup4Set;
		}
		else
		{
			base.SetTextureGroup4Set(setName);
		}
	}

	protected override void SetTextureGroup5Set(string setName)
	{
		bool flag = false;
		if (textureGroup5DisableZone != 0 && characterTextureControl != null)
		{
			flag = CheckNullifyTextureGroupSet(textureGroup5, textureGroup5DisableZone);
		}
		if (flag)
		{
			textureGroup5JSON.valNoCallback = currentTextureGroup5Set;
		}
		else
		{
			base.SetTextureGroup5Set(setName);
		}
	}

	public void SetAllTextureGroupSetsToCurrent()
	{
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

	public override Mesh GetMesh()
	{
		Mesh result = null;
		if (_skin != null && _skin.dazMesh != null)
		{
			result = _skin.dazMesh.morphedUVMappedMesh;
		}
		return result;
	}

	public override void SetStartingValues(Dictionary<Texture2D, string> textureToSourcePath)
	{
		characterTextureControl = GetComponent<DAZCharacterTextureControl>();
		if (skin != null && materialForDefaults == null)
		{
			DAZMesh componentInChildren = skin.GetComponentInChildren<DAZMergedMesh>(true);
			if (componentInChildren == null)
			{
				componentInChildren = skin.GetComponentInChildren<DAZMesh>(true);
			}
			if (componentInChildren != null && paramMaterialSlots != null && paramMaterialSlots.Length > 0)
			{
				int num = paramMaterialSlots[0];
				if (num < componentInChildren.numMaterials)
				{
					materialForDefaults = componentInChildren.materials[num];
				}
				else
				{
					Debug.LogError("Material slot out of range");
				}
			}
		}
		base.SetStartingValues(textureToSourcePath);
	}

	protected void ConnectPassthroughBucket(Transform bucket)
	{
		if (!(bucket != null))
		{
			return;
		}
		DAZCharacterMaterialOptions[] componentsInChildren = bucket.GetComponentsInChildren<DAZCharacterMaterialOptions>();
		DAZCharacterMaterialOptions[] array = componentsInChildren;
		foreach (DAZCharacterMaterialOptions dAZCharacterMaterialOptions in array)
		{
			if (dAZCharacterMaterialOptions.storeId == base.storeId)
			{
				dAZCharacterMaterialOptions.SetCustomTextureFolder(customTextureFolder);
				dAZCharacterMaterialOptions.UITransform = UITransform;
				dAZCharacterMaterialOptions.InitUI();
			}
		}
	}

	public void ConnectPassthroughBuckets()
	{
		ConnectPassthroughBucket(passThroughToBucket1);
		ConnectPassthroughBucket(passThroughToBucket2);
	}

	public override void InitUI()
	{
		if (isPassthrough)
		{
			ConnectPassthroughBuckets();
		}
		else
		{
			base.InitUI();
		}
	}
}
