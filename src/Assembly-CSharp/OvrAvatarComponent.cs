using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Oculus.Avatar;
using UnityEngine;

public class OvrAvatarComponent : MonoBehaviour
{
	private struct MeshThreadData
	{
		public Color[] MeshColors;

		public int VertexCount;

		public bool IsDarkMaterial;

		public bool UsesAlpha;
	}

	public static readonly string[] LayerKeywords = new string[9] { "LAYERS_0", "LAYERS_1", "LAYERS_2", "LAYERS_3", "LAYERS_4", "LAYERS_5", "LAYERS_6", "LAYERS_7", "LAYERS_8" };

	public static readonly string[] LayerSampleModeParameters = new string[8] { "_LayerSampleMode0", "_LayerSampleMode1", "_LayerSampleMode2", "_LayerSampleMode3", "_LayerSampleMode4", "_LayerSampleMode5", "_LayerSampleMode6", "_LayerSampleMode7" };

	public static readonly string[] LayerBlendModeParameters = new string[8] { "_LayerBlendMode0", "_LayerBlendMode1", "_LayerBlendMode2", "_LayerBlendMode3", "_LayerBlendMode4", "_LayerBlendMode5", "_LayerBlendMode6", "_LayerBlendMode7" };

	public static readonly string[] LayerMaskTypeParameters = new string[8] { "_LayerMaskType0", "_LayerMaskType1", "_LayerMaskType2", "_LayerMaskType3", "_LayerMaskType4", "_LayerMaskType5", "_LayerMaskType6", "_LayerMaskType7" };

	public static readonly string[] LayerColorParameters = new string[8] { "_LayerColor0", "_LayerColor1", "_LayerColor2", "_LayerColor3", "_LayerColor4", "_LayerColor5", "_LayerColor6", "_LayerColor7" };

	public static readonly string[] LayerSurfaceParameters = new string[8] { "_LayerSurface0", "_LayerSurface1", "_LayerSurface2", "_LayerSurface3", "_LayerSurface4", "_LayerSurface5", "_LayerSurface6", "_LayerSurface7" };

	public static readonly string[] LayerSampleParametersParameters = new string[8] { "_LayerSampleParameters0", "_LayerSampleParameters1", "_LayerSampleParameters2", "_LayerSampleParameters3", "_LayerSampleParameters4", "_LayerSampleParameters5", "_LayerSampleParameters6", "_LayerSampleParameters7" };

	public static readonly string[] LayerMaskParametersParameters = new string[8] { "_LayerMaskParameters0", "_LayerMaskParameters1", "_LayerMaskParameters2", "_LayerMaskParameters3", "_LayerMaskParameters4", "_LayerMaskParameters5", "_LayerMaskParameters6", "_LayerMaskParameters7" };

	public static readonly string[] LayerMaskAxisParameters = new string[8] { "_LayerMaskAxis0", "_LayerMaskAxis1", "_LayerMaskAxis2", "_LayerMaskAxis3", "_LayerMaskAxis4", "_LayerMaskAxis5", "_LayerMaskAxis6", "_LayerMaskAxis7" };

	public SkinnedMeshRenderer RootMeshComponent;

	private Dictionary<Material, ovrAvatarMaterialState> materialStates = new Dictionary<Material, ovrAvatarMaterialState>();

	public List<OvrAvatarRenderComponent> RenderParts = new List<OvrAvatarRenderComponent>();

	private bool DrawSkeleton;

	private bool IsVertexDataUpdating;

	private bool IsCombiningMeshes;

	private bool FirstMaterialUpdate = true;

	private ulong ClothingAlphaTexture;

	private Vector4 ClothingAlphaOffset;

	private Thread VertexThread;

	private MeshThreadData[] ThreadData;

	public void StartMeshCombining(ovrAvatarComponent component)
	{
		IsCombiningMeshes = true;
		base.gameObject.SetActive(false);
		ThreadData = new MeshThreadData[RenderParts.Count];
		for (uint num = 0u; num < RenderParts.Count; num++)
		{
			OvrAvatarRenderComponent ovrAvatarRenderComponent = RenderParts[(int)num];
			IntPtr renderPart = OvrAvatar.GetRenderPart(component, num);
			ovrAvatarMaterialState ovrAvatarMaterialState2 = CAPI.ovrAvatarSkinnedMeshRender_GetMaterialState(renderPart);
			ThreadData[num].VertexCount = ovrAvatarRenderComponent.mesh.sharedMesh.vertexCount;
			ThreadData[num].IsDarkMaterial = num != 0;
			if (ovrAvatarMaterialState2.alphaMaskTextureID != 0)
			{
				if (num != 0)
				{
					ClothingAlphaOffset = ovrAvatarMaterialState2.alphaMaskScaleOffset;
					ClothingAlphaTexture = ovrAvatarMaterialState2.alphaMaskTextureID;
				}
				ThreadData[num].UsesAlpha = true;
			}
			ThreadData[num].MeshColors = ovrAvatarRenderComponent.mesh.sharedMesh.colors;
		}
		VertexThread = new Thread((ThreadStart)delegate
		{
			UpdateVertices(ref ThreadData);
		});
		VertexThread.Start();
	}

	private void UpdateVertices(ref MeshThreadData[] threadData)
	{
		IsVertexDataUpdating = true;
		MeshThreadData[] array = threadData;
		for (int i = 0; i < array.Length; i++)
		{
			MeshThreadData meshThreadData = array[i];
			for (int j = 0; j < meshThreadData.VertexCount; j++)
			{
				meshThreadData.MeshColors[j].a = 0f;
				if (meshThreadData.UsesAlpha)
				{
					meshThreadData.MeshColors[j].a = ((!meshThreadData.IsDarkMaterial) ? 0.5f : 1f);
				}
				meshThreadData.MeshColors[j].r = ((!meshThreadData.IsDarkMaterial) ? 0f : 1f);
			}
		}
		IsVertexDataUpdating = false;
	}

	public void CombineMeshes(ovrAvatarComponent component)
	{
		List<Transform> list = new List<Transform>();
		List<BoneWeight> list2 = new List<BoneWeight>();
		List<CombineInstance> list3 = new List<CombineInstance>();
		List<Matrix4x4> list4 = new List<Matrix4x4>();
		List<Color> list5 = new List<Color>();
		RootMeshComponent = base.gameObject.AddComponent<SkinnedMeshRenderer>();
		RootMeshComponent.quality = SkinQuality.Bone4;
		RootMeshComponent.updateWhenOffscreen = true;
		int num = 0;
		for (uint num2 = 0u; num2 < RenderParts.Count; num2++)
		{
			OvrAvatarRenderComponent ovrAvatarRenderComponent = RenderParts[(int)num2];
			if (RootMeshComponent.sharedMaterial == null)
			{
				RootMeshComponent.sharedMaterial = ovrAvatarRenderComponent.mesh.sharedMaterial;
				materialStates.Add(RootMeshComponent.sharedMaterial, default(ovrAvatarMaterialState));
				RootMeshComponent.sortingLayerID = ovrAvatarRenderComponent.mesh.sortingLayerID;
				RootMeshComponent.gameObject.layer = ovrAvatarRenderComponent.gameObject.layer;
			}
			list5.AddRange(ThreadData[num2].MeshColors);
			BoneWeight[] boneWeights = ovrAvatarRenderComponent.mesh.sharedMesh.boneWeights;
			foreach (BoneWeight boneWeight in boneWeights)
			{
				BoneWeight item = boneWeight;
				item.boneIndex0 += num;
				item.boneIndex1 += num;
				item.boneIndex2 += num;
				item.boneIndex3 += num;
				list2.Add(item);
			}
			num += ovrAvatarRenderComponent.mesh.bones.Length;
			Transform[] bones = ovrAvatarRenderComponent.mesh.bones;
			foreach (Transform item2 in bones)
			{
				list.Add(item2);
			}
			CombineInstance item3 = default(CombineInstance);
			item3.mesh = ovrAvatarRenderComponent.mesh.sharedMesh;
			item3.transform = ovrAvatarRenderComponent.mesh.transform.localToWorldMatrix;
			list3.Add(item3);
			for (int k = 0; k < ovrAvatarRenderComponent.mesh.bones.Length; k++)
			{
				list4.Add(ovrAvatarRenderComponent.mesh.sharedMesh.bindposes[k]);
			}
			UnityEngine.Object.Destroy(ovrAvatarRenderComponent.mesh);
			ovrAvatarRenderComponent.mesh = null;
		}
		RootMeshComponent.sharedMesh = new Mesh();
		RootMeshComponent.sharedMesh.name = base.transform.name + "_combined_mesh";
		RootMeshComponent.sharedMesh.CombineMeshes(list3.ToArray(), true, true);
		RootMeshComponent.bones = list.ToArray();
		RootMeshComponent.sharedMesh.boneWeights = list2.ToArray();
		RootMeshComponent.sharedMesh.bindposes = list4.ToArray();
		RootMeshComponent.sharedMesh.colors = list5.ToArray();
		RootMeshComponent.rootBone = list[0];
		RootMeshComponent.sharedMesh.RecalculateBounds();
	}

	public void UpdateAvatar(ovrAvatarComponent component, OvrAvatar avatar)
	{
		if (IsCombiningMeshes)
		{
			if (!IsVertexDataUpdating)
			{
				CombineMeshes(component);
				IsCombiningMeshes = false;
				ThreadData = null;
			}
			return;
		}
		OvrAvatar.ConvertTransform(component.transform, base.transform);
		for (uint num = 0u; num < component.renderPartCount && RenderParts.Count > num; num++)
		{
			OvrAvatarRenderComponent ovrAvatarRenderComponent = RenderParts[(int)num];
			IntPtr renderPart = OvrAvatar.GetRenderPart(component, num);
			switch (CAPI.ovrAvatarRenderPart_GetType(renderPart))
			{
			case ovrAvatarRenderPartType.SkinnedMeshRender:
				((OvrAvatarSkinnedMeshRenderComponent)ovrAvatarRenderComponent).UpdateSkinnedMeshRender(this, avatar, renderPart);
				break;
			case ovrAvatarRenderPartType.SkinnedMeshRenderPBS:
			{
				Material mat = ((!(RootMeshComponent != null)) ? ovrAvatarRenderComponent.mesh.sharedMaterial : RootMeshComponent.sharedMaterial);
				((OvrAvatarSkinnedMeshRenderPBSComponent)ovrAvatarRenderComponent).UpdateSkinnedMeshRenderPBS(avatar, renderPart, mat);
				break;
			}
			case ovrAvatarRenderPartType.ProjectorRender:
				((OvrAvatarProjectorRenderComponent)ovrAvatarRenderComponent).UpdateProjectorRender(this, CAPI.ovrAvatarRenderPart_GetProjectorRender(renderPart));
				break;
			case ovrAvatarRenderPartType.SkinnedMeshRenderPBS_V2:
				((OvrAvatarSkinnedMeshPBSV2RenderComponent)ovrAvatarRenderComponent).UpdateSkinnedMeshRender(this, avatar, renderPart);
				break;
			}
		}
		if (RootMeshComponent != null)
		{
			IntPtr renderPart2 = OvrAvatar.GetRenderPart(component, 0u);
			switch (CAPI.ovrAvatarRenderPart_GetType(renderPart2))
			{
			case ovrAvatarRenderPartType.SkinnedMeshRender:
				UpdateActive(avatar, CAPI.ovrAvatarSkinnedMeshRender_GetVisibilityMask(renderPart2));
				if ((FirstMaterialUpdate && base.gameObject.activeSelf) || CAPI.ovrAvatarSkinnedMeshRender_MaterialStateChanged(renderPart2))
				{
					FirstMaterialUpdate = false;
					ovrAvatarMaterialState matState = CAPI.ovrAvatarSkinnedMeshRender_GetMaterialState(renderPart2);
					UpdateAvatarMaterial(RootMeshComponent.sharedMaterial, matState);
				}
				break;
			case ovrAvatarRenderPartType.SkinnedMeshRenderPBS:
				UpdateActive(avatar, CAPI.ovrAvatarSkinnedMeshRenderPBS_GetVisibilityMask(renderPart2));
				break;
			}
		}
		DebugDrawTransforms();
	}

	protected void UpdateActive(OvrAvatar avatar, ovrAvatarVisibilityFlags mask)
	{
		bool flag = avatar.ShowFirstPerson && (mask & ovrAvatarVisibilityFlags.FirstPerson) != 0;
		flag |= avatar.ShowThirdPerson && (mask & ovrAvatarVisibilityFlags.ThirdPerson) != 0;
		base.gameObject.SetActive(flag);
	}

	private void DebugDrawTransforms()
	{
		Color[] array = new Color[3]
		{
			Color.red,
			Color.white,
			Color.blue
		};
		int num = 0;
		if (!DrawSkeleton || !(RootMeshComponent != null) || RootMeshComponent.bones == null)
		{
			return;
		}
		Transform[] bones = RootMeshComponent.bones;
		foreach (Transform transform in bones)
		{
			if ((bool)transform.parent)
			{
				Debug.DrawLine(transform.position, transform.parent.position, array[num++ % array.Length]);
			}
		}
	}

	public void UpdateAvatarMaterial(Material mat, ovrAvatarMaterialState matState)
	{
		mat.SetColor("_BaseColor", matState.baseColor);
		mat.SetInt("_BaseMaskType", (int)matState.baseMaskType);
		mat.SetVector("_BaseMaskParameters", matState.baseMaskParameters);
		mat.SetVector("_BaseMaskAxis", matState.baseMaskAxis);
		if (matState.alphaMaskTextureID != 0)
		{
			mat.SetTexture("_AlphaMask", GetLoadedTexture(matState.alphaMaskTextureID));
			mat.SetTextureScale("_AlphaMask", new Vector2(matState.alphaMaskScaleOffset.x, matState.alphaMaskScaleOffset.y));
			mat.SetTextureOffset("_AlphaMask", new Vector2(matState.alphaMaskScaleOffset.z, matState.alphaMaskScaleOffset.w));
		}
		if (ClothingAlphaTexture != 0)
		{
			mat.EnableKeyword("VERTALPHA_ON");
			mat.SetTexture("_AlphaMask2", GetLoadedTexture(ClothingAlphaTexture));
			mat.SetTextureScale("_AlphaMask2", new Vector2(ClothingAlphaOffset.x, ClothingAlphaOffset.y));
			mat.SetTextureOffset("_AlphaMask2", new Vector2(ClothingAlphaOffset.z, ClothingAlphaOffset.w));
		}
		if (matState.normalMapTextureID != 0)
		{
			mat.EnableKeyword("NORMAL_MAP_ON");
			mat.SetTexture("_NormalMap", GetLoadedTexture(matState.normalMapTextureID));
			mat.SetTextureScale("_NormalMap", new Vector2(matState.normalMapScaleOffset.x, matState.normalMapScaleOffset.y));
			mat.SetTextureOffset("_NormalMap", new Vector2(matState.normalMapScaleOffset.z, matState.normalMapScaleOffset.w));
		}
		if (matState.parallaxMapTextureID != 0)
		{
			mat.SetTexture("_ParallaxMap", GetLoadedTexture(matState.parallaxMapTextureID));
			mat.SetTextureScale("_ParallaxMap", new Vector2(matState.parallaxMapScaleOffset.x, matState.parallaxMapScaleOffset.y));
			mat.SetTextureOffset("_ParallaxMap", new Vector2(matState.parallaxMapScaleOffset.z, matState.parallaxMapScaleOffset.w));
		}
		if (matState.roughnessMapTextureID != 0)
		{
			mat.EnableKeyword("ROUGHNESS_ON");
			mat.SetTexture("_RoughnessMap", GetLoadedTexture(matState.roughnessMapTextureID));
			mat.SetTextureScale("_RoughnessMap", new Vector2(matState.roughnessMapScaleOffset.x, matState.roughnessMapScaleOffset.y));
			mat.SetTextureOffset("_RoughnessMap", new Vector2(matState.roughnessMapScaleOffset.z, matState.roughnessMapScaleOffset.w));
		}
		mat.EnableKeyword(LayerKeywords[matState.layerCount]);
		for (ulong num = 0uL; num < matState.layerCount; num++)
		{
			ovrAvatarMaterialLayerState ovrAvatarMaterialLayerState2 = matState.layers[num];
			mat.SetInt(LayerSampleModeParameters[num], (int)ovrAvatarMaterialLayerState2.sampleMode);
			mat.SetInt(LayerBlendModeParameters[num], (int)ovrAvatarMaterialLayerState2.blendMode);
			mat.SetInt(LayerMaskTypeParameters[num], (int)ovrAvatarMaterialLayerState2.maskType);
			mat.SetColor(LayerColorParameters[num], ovrAvatarMaterialLayerState2.layerColor);
			if (ovrAvatarMaterialLayerState2.sampleMode != 0)
			{
				string text = LayerSurfaceParameters[num];
				mat.SetTexture(text, GetLoadedTexture(ovrAvatarMaterialLayerState2.sampleTexture));
				mat.SetTextureScale(text, new Vector2(ovrAvatarMaterialLayerState2.sampleScaleOffset.x, ovrAvatarMaterialLayerState2.sampleScaleOffset.y));
				mat.SetTextureOffset(text, new Vector2(ovrAvatarMaterialLayerState2.sampleScaleOffset.z, ovrAvatarMaterialLayerState2.sampleScaleOffset.w));
			}
			if (ovrAvatarMaterialLayerState2.sampleMode == ovrAvatarMaterialLayerSampleMode.Parallax)
			{
				mat.EnableKeyword("PARALLAX_ON");
			}
			mat.SetColor(LayerSampleParametersParameters[num], ovrAvatarMaterialLayerState2.sampleParameters);
			mat.SetColor(LayerMaskParametersParameters[num], ovrAvatarMaterialLayerState2.maskParameters);
			mat.SetColor(LayerMaskAxisParameters[num], ovrAvatarMaterialLayerState2.maskAxis);
		}
		materialStates[mat] = matState;
	}

	public static Texture2D GetLoadedTexture(ulong assetId)
	{
		if (assetId == 0)
		{
			return null;
		}
		OvrAvatarAssetTexture ovrAvatarAssetTexture = (OvrAvatarAssetTexture)OvrAvatarSDKManager.Instance.GetAsset(assetId);
		if (ovrAvatarAssetTexture == null)
		{
			throw new Exception("Could not find texture for asset " + assetId);
		}
		return ovrAvatarAssetTexture.texture;
	}
}
public struct ovrAvatarComponent
{
	public ovrAvatarTransform transform;

	public uint renderPartCount;

	public IntPtr renderParts;

	[MarshalAs(UnmanagedType.LPStr)]
	public string name;
}
