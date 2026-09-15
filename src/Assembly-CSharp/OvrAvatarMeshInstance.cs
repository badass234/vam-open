using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class OvrAvatarMeshInstance : MonoBehaviour
{
	private HashSet<ulong> AssetsToLoad;

	public ulong MeshID;

	private ulong MaterialID;

	private ulong FadeTextureID;

	public ovrAvatarBodyPartType MeshType;

	public ovrAvatarMaterialState MaterialState;

	private MeshFilter Mesh;

	private MeshRenderer MeshInstance;

	public void AssetLoadedCallback(OvrAvatarAsset asset)
	{
		AssetsToLoad.Remove(asset.assetID);
		HandleAssetAvailable(asset);
		if (AssetsToLoad.Count <= 0)
		{
			UpdateMaterial();
		}
	}

	public void SetMeshAssets(ulong fadeTexture, ulong meshID, ulong materialID, ovrAvatarBodyPartType type)
	{
		MaterialID = materialID;
		MeshID = meshID;
		FadeTextureID = fadeTexture;
		MeshType = type;
		AssetsToLoad = new HashSet<ulong>();
		RequestAsset(meshID);
		RequestAsset(materialID);
		RequestAsset(fadeTexture);
	}

	private void HandleAssetAvailable(OvrAvatarAsset asset)
	{
		if (asset.assetID == MeshID)
		{
			Mesh = base.gameObject.AddComponent<MeshFilter>();
			MeshInstance = base.gameObject.AddComponent<MeshRenderer>();
			MeshInstance.shadowCastingMode = ShadowCastingMode.Off;
			Mesh.sharedMesh = ((OvrAvatarAssetMesh)asset).mesh;
			Material material = new Material(Shader.Find("OvrAvatar/AvatarSurfaceShaderSelfOccluding"));
			MeshInstance.material = material;
		}
		if (asset.assetID == MaterialID)
		{
			MaterialState = ((OvrAvatarAssetMaterial)asset).material;
			MaterialState.alphaMaskTextureID = FadeTextureID;
			RequestMaterialTextures();
		}
	}

	public void ChangeMaterial(ulong assetID)
	{
		MaterialID = assetID;
		RequestAsset(MaterialID);
	}

	private void RequestAsset(ulong assetID)
	{
		if (assetID != 0)
		{
			OvrAvatarAsset asset = OvrAvatarSDKManager.Instance.GetAsset(assetID);
			if (asset == null)
			{
				OvrAvatarSDKManager.Instance.BeginLoadingAsset(assetID, AssetLoadedCallback);
				AssetsToLoad.Add(assetID);
			}
			else
			{
				HandleAssetAvailable(asset);
			}
		}
	}

	private void RequestMaterialTextures()
	{
		RequestAsset(MaterialState.normalMapTextureID);
		RequestAsset(MaterialState.parallaxMapTextureID);
		RequestAsset(MaterialState.roughnessMapTextureID);
		for (int i = 0; i < MaterialState.layerCount; i++)
		{
			RequestAsset(MaterialState.layers[i].sampleTexture);
		}
	}

	public void SetActive(bool active)
	{
		base.gameObject.SetActive(active);
		if (active)
		{
			UpdateMaterial();
		}
	}

	private void UpdateMaterial()
	{
		if (MeshInstance == null || MaterialID == 0)
		{
			return;
		}
		Material material = MeshInstance.material;
		ovrAvatarMaterialState materialState = MaterialState;
		material.SetColor("_BaseColor", materialState.baseColor);
		material.SetInt("_BaseMaskType", (int)materialState.baseMaskType);
		material.SetVector("_BaseMaskParameters", materialState.baseMaskParameters);
		material.SetVector("_BaseMaskAxis", materialState.baseMaskAxis);
		if (materialState.alphaMaskTextureID != 0)
		{
			material.SetTexture("_AlphaMask", OvrAvatarComponent.GetLoadedTexture(materialState.alphaMaskTextureID));
			material.SetTextureScale("_AlphaMask", new Vector2(materialState.alphaMaskScaleOffset.x, materialState.alphaMaskScaleOffset.y));
			material.SetTextureOffset("_AlphaMask", new Vector2(materialState.alphaMaskScaleOffset.z, materialState.alphaMaskScaleOffset.w));
		}
		if (materialState.normalMapTextureID != 0)
		{
			material.EnableKeyword("NORMAL_MAP_ON");
			material.SetTexture("_NormalMap", OvrAvatarComponent.GetLoadedTexture(materialState.normalMapTextureID));
			material.SetTextureScale("_NormalMap", new Vector2(materialState.normalMapScaleOffset.x, materialState.normalMapScaleOffset.y));
			material.SetTextureOffset("_NormalMap", new Vector2(materialState.normalMapScaleOffset.z, materialState.normalMapScaleOffset.w));
		}
		if (materialState.parallaxMapTextureID != 0)
		{
			material.SetTexture("_ParallaxMap", OvrAvatarComponent.GetLoadedTexture(materialState.parallaxMapTextureID));
			material.SetTextureScale("_ParallaxMap", new Vector2(materialState.parallaxMapScaleOffset.x, materialState.parallaxMapScaleOffset.y));
			material.SetTextureOffset("_ParallaxMap", new Vector2(materialState.parallaxMapScaleOffset.z, materialState.parallaxMapScaleOffset.w));
		}
		if (materialState.roughnessMapTextureID != 0)
		{
			material.EnableKeyword("ROUGHNESS_ON");
			material.SetTexture("_RoughnessMap", OvrAvatarComponent.GetLoadedTexture(materialState.roughnessMapTextureID));
			material.SetTextureScale("_RoughnessMap", new Vector2(materialState.roughnessMapScaleOffset.x, materialState.roughnessMapScaleOffset.y));
			material.SetTextureOffset("_RoughnessMap", new Vector2(materialState.roughnessMapScaleOffset.z, materialState.roughnessMapScaleOffset.w));
		}
		material.EnableKeyword(OvrAvatarComponent.LayerKeywords[materialState.layerCount]);
		for (ulong num = 0uL; num < materialState.layerCount; num++)
		{
			ovrAvatarMaterialLayerState ovrAvatarMaterialLayerState2 = materialState.layers[num];
			material.SetInt(OvrAvatarComponent.LayerSampleModeParameters[num], (int)ovrAvatarMaterialLayerState2.sampleMode);
			material.SetInt(OvrAvatarComponent.LayerBlendModeParameters[num], (int)ovrAvatarMaterialLayerState2.blendMode);
			material.SetInt(OvrAvatarComponent.LayerMaskTypeParameters[num], (int)ovrAvatarMaterialLayerState2.maskType);
			material.SetColor(OvrAvatarComponent.LayerColorParameters[num], ovrAvatarMaterialLayerState2.layerColor);
			if (ovrAvatarMaterialLayerState2.sampleMode != 0)
			{
				string text = OvrAvatarComponent.LayerSurfaceParameters[num];
				material.SetTexture(text, OvrAvatarComponent.GetLoadedTexture(ovrAvatarMaterialLayerState2.sampleTexture));
				material.SetTextureScale(text, new Vector2(ovrAvatarMaterialLayerState2.sampleScaleOffset.x, ovrAvatarMaterialLayerState2.sampleScaleOffset.y));
				material.SetTextureOffset(text, new Vector2(ovrAvatarMaterialLayerState2.sampleScaleOffset.z, ovrAvatarMaterialLayerState2.sampleScaleOffset.w));
			}
			if (ovrAvatarMaterialLayerState2.sampleMode == ovrAvatarMaterialLayerSampleMode.Parallax)
			{
				material.EnableKeyword("PARALLAX_ON");
			}
			material.SetColor(OvrAvatarComponent.LayerSampleParametersParameters[num], ovrAvatarMaterialLayerState2.sampleParameters);
			material.SetColor(OvrAvatarComponent.LayerMaskParametersParameters[num], ovrAvatarMaterialLayerState2.maskParameters);
			material.SetColor(OvrAvatarComponent.LayerMaskAxisParameters[num], ovrAvatarMaterialLayerState2.maskAxis);
		}
	}
}
