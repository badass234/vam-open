using System;
using Oculus.Avatar;
using UnityEngine;

public class OvrAvatarSkinnedMeshRenderComponent : OvrAvatarRenderComponent
{
	private Shader surface;

	private Shader surfaceSelfOccluding;

	private bool previouslyActive;

	internal void Initialize(ovrAvatarRenderPart_SkinnedMeshRender skinnedMeshRender, Shader surface, Shader surfaceSelfOccluding, int thirdPersonLayer, int firstPersonLayer, int sortOrder)
	{
		this.surfaceSelfOccluding = ((!(surfaceSelfOccluding != null)) ? Shader.Find("OvrAvatar/AvatarSurfaceShaderSelfOccluding") : surfaceSelfOccluding);
		this.surface = ((!(surface != null)) ? Shader.Find("OvrAvatar/AvatarSurfaceShader") : surface);
		mesh = CreateSkinnedMesh(skinnedMeshRender.meshAssetID, skinnedMeshRender.visibilityMask, thirdPersonLayer, firstPersonLayer, sortOrder);
		bones = mesh.bones;
		UpdateMeshMaterial(skinnedMeshRender.visibilityMask, mesh);
	}

	public void UpdateSkinnedMeshRender(OvrAvatarComponent component, OvrAvatar avatar, IntPtr renderPart)
	{
		ovrAvatarVisibilityFlags visibilityMask = CAPI.ovrAvatarSkinnedMeshRender_GetVisibilityMask(renderPart);
		ovrAvatarTransform localTransform = CAPI.ovrAvatarSkinnedMeshRender_GetTransform(renderPart);
		UpdateSkinnedMesh(avatar, bones, localTransform, visibilityMask, renderPart);
		UpdateMeshMaterial(visibilityMask, (!(mesh == null)) ? mesh : component.RootMeshComponent);
		bool activeSelf = base.gameObject.activeSelf;
		if (mesh != null && (CAPI.ovrAvatarSkinnedMeshRender_MaterialStateChanged(renderPart) || (!previouslyActive && activeSelf)))
		{
			ovrAvatarMaterialState matState = CAPI.ovrAvatarSkinnedMeshRender_GetMaterialState(renderPart);
			component.UpdateAvatarMaterial(mesh.sharedMaterial, matState);
		}
		previouslyActive = activeSelf;
	}

	private void UpdateMeshMaterial(ovrAvatarVisibilityFlags visibilityMask, SkinnedMeshRenderer rootMesh)
	{
		Shader shader = (((visibilityMask & ovrAvatarVisibilityFlags.SelfOccluding) == 0) ? surface : surfaceSelfOccluding);
		if (rootMesh.sharedMaterial == null || rootMesh.sharedMaterial.shader != shader)
		{
			rootMesh.sharedMaterial = CreateAvatarMaterial(base.gameObject.name + "_material", shader);
		}
	}
}
