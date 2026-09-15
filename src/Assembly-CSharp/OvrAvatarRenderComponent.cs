using System;
using Oculus.Avatar;
using UnityEngine;
using UnityEngine.Rendering;

public class OvrAvatarRenderComponent : MonoBehaviour
{
	private bool firstSkinnedUpdate = true;

	public SkinnedMeshRenderer mesh;

	public Transform[] bones;

	protected void UpdateActive(OvrAvatar avatar, ovrAvatarVisibilityFlags mask)
	{
		bool flag = avatar.ShowFirstPerson && (mask & ovrAvatarVisibilityFlags.FirstPerson) != 0;
		flag |= avatar.ShowThirdPerson && (mask & ovrAvatarVisibilityFlags.ThirdPerson) != 0;
		base.gameObject.SetActive(flag);
	}

	protected SkinnedMeshRenderer CreateSkinnedMesh(ulong assetID, ovrAvatarVisibilityFlags visibilityMask, int thirdPersonLayer, int firstPersonLayer, int sortingOrder)
	{
		OvrAvatarAssetMesh ovrAvatarAssetMesh = (OvrAvatarAssetMesh)OvrAvatarSDKManager.Instance.GetAsset(assetID);
		if (ovrAvatarAssetMesh == null)
		{
			throw new Exception("Couldn't find mesh for asset " + assetID);
		}
		if ((visibilityMask & ovrAvatarVisibilityFlags.ThirdPerson) != 0)
		{
			base.gameObject.layer = thirdPersonLayer;
		}
		else
		{
			base.gameObject.layer = firstPersonLayer;
		}
		SkinnedMeshRenderer skinnedMeshRenderer = ovrAvatarAssetMesh.CreateSkinnedMeshRendererOnObject(base.gameObject);
		skinnedMeshRenderer.quality = SkinQuality.Bone4;
		skinnedMeshRenderer.sortingOrder = sortingOrder;
		skinnedMeshRenderer.updateWhenOffscreen = true;
		if ((visibilityMask & ovrAvatarVisibilityFlags.SelfOccluding) == 0)
		{
			skinnedMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		}
		return skinnedMeshRenderer;
	}

	protected void UpdateSkinnedMesh(OvrAvatar avatar, Transform[] bones, ovrAvatarTransform localTransform, ovrAvatarVisibilityFlags visibilityMask, IntPtr renderPart)
	{
		UpdateActive(avatar, visibilityMask);
		OvrAvatar.ConvertTransform(localTransform, base.transform);
		ovrAvatarRenderPartType ovrAvatarRenderPartType2 = CAPI.ovrAvatarRenderPart_GetType(renderPart);
		ulong num;
		switch (ovrAvatarRenderPartType2)
		{
		case ovrAvatarRenderPartType.SkinnedMeshRender:
			num = CAPI.ovrAvatarSkinnedMeshRender_GetDirtyJoints(renderPart);
			break;
		case ovrAvatarRenderPartType.SkinnedMeshRenderPBS:
			num = CAPI.ovrAvatarSkinnedMeshRenderPBS_GetDirtyJoints(renderPart);
			break;
		case ovrAvatarRenderPartType.SkinnedMeshRenderPBS_V2:
			num = CAPI.ovrAvatarSkinnedMeshRenderPBSV2_GetDirtyJoints(renderPart);
			break;
		default:
			throw new Exception("Unhandled render part type: " + ovrAvatarRenderPartType2);
		}
		for (uint num2 = 0u; num2 < 64; num2++)
		{
			ulong num3 = (ulong)(1L << (int)num2);
			if ((firstSkinnedUpdate && num2 < bones.Length) || (num3 & num) != 0)
			{
				Transform target = bones[num2];
				ovrAvatarTransform ovrAvatarTransform2;
				switch (ovrAvatarRenderPartType2)
				{
				case ovrAvatarRenderPartType.SkinnedMeshRender:
					ovrAvatarTransform2 = CAPI.ovrAvatarSkinnedMeshRender_GetJointTransform(renderPart, num2);
					break;
				case ovrAvatarRenderPartType.SkinnedMeshRenderPBS:
					ovrAvatarTransform2 = CAPI.ovrAvatarSkinnedMeshRenderPBS_GetJointTransform(renderPart, num2);
					break;
				case ovrAvatarRenderPartType.SkinnedMeshRenderPBS_V2:
					ovrAvatarTransform2 = CAPI.ovrAvatarSkinnedMeshRenderPBSV2_GetJointTransform(renderPart, num2);
					break;
				default:
					throw new Exception("Unhandled render part type: " + ovrAvatarRenderPartType2);
				}
				OvrAvatar.ConvertTransform(ovrAvatarTransform2, target);
			}
		}
		firstSkinnedUpdate = false;
	}

	protected Material CreateAvatarMaterial(string name, Shader shader)
	{
		if (shader == null)
		{
			throw new Exception("No shader provided for avatar material.");
		}
		Material material = new Material(shader);
		material.name = name;
		return material;
	}
}
