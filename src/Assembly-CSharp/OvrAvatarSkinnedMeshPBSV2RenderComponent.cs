using System;
using Oculus.Avatar;
using UnityEngine;

public class OvrAvatarSkinnedMeshPBSV2RenderComponent : OvrAvatarRenderComponent
{
	private Shader surface;

	private bool previouslyActive;

	internal void Initialize(ovrAvatarRenderPart_SkinnedMeshRenderPBS_V2 skinnedMeshRender, Shader surface, int thirdPersonLayer, int firstPersonLayer, int sortOrder)
	{
		this.surface = ((!(surface != null)) ? Shader.Find("OvrAvatar/AvatarSurfaceShaderPBSV2") : surface);
		mesh = CreateSkinnedMesh(skinnedMeshRender.meshAssetID, skinnedMeshRender.visibilityMask, thirdPersonLayer, firstPersonLayer, sortOrder);
		bones = mesh.bones;
		UpdateMeshMaterial(skinnedMeshRender.visibilityMask, mesh);
	}

	public void UpdateSkinnedMeshRender(OvrAvatarComponent component, OvrAvatar avatar, IntPtr renderPart)
	{
		ovrAvatarVisibilityFlags visibilityMask = CAPI.ovrAvatarSkinnedMeshRenderPBSV2_GetVisibilityMask(renderPart);
		ovrAvatarTransform localTransform = CAPI.ovrAvatarSkinnedMeshRenderPBSV2_GetTransform(renderPart);
		UpdateSkinnedMesh(avatar, bones, localTransform, visibilityMask, renderPart);
		UpdateMeshMaterial(visibilityMask, (!(mesh == null)) ? mesh : component.RootMeshComponent);
		bool activeSelf = base.gameObject.activeSelf;
		if (mesh != null && (CAPI.ovrAvatarSkinnedMeshRenderPBSV2_MaterialStateChanged(renderPart) || (!previouslyActive && activeSelf)))
		{
			ovrAvatarPBSMaterialState ovrAvatarPBSMaterialState2 = CAPI.ovrAvatarSkinnedMeshRenderPBSV2_GetPBSMaterialState(renderPart);
			Material sharedMaterial = mesh.sharedMaterial;
			sharedMaterial.SetVector("_AlbedoMultiplier", ovrAvatarPBSMaterialState2.albedoMultiplier);
			sharedMaterial.SetTexture("_Albedo", OvrAvatarComponent.GetLoadedTexture(ovrAvatarPBSMaterialState2.albedoTextureID));
			sharedMaterial.SetTexture("_Metallicness", OvrAvatarComponent.GetLoadedTexture(ovrAvatarPBSMaterialState2.metallicnessTextureID));
			sharedMaterial.SetFloat("_GlossinessScale", ovrAvatarPBSMaterialState2.glossinessScale);
		}
		previouslyActive = activeSelf;
	}

	private void UpdateMeshMaterial(ovrAvatarVisibilityFlags visibilityMask, SkinnedMeshRenderer rootMesh)
	{
		if (rootMesh.sharedMaterial == null || rootMesh.sharedMaterial.shader != surface)
		{
			rootMesh.sharedMaterial = CreateAvatarMaterial(base.gameObject.name + "_material", surface);
		}
	}
}
