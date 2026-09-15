using UnityEngine;

public struct ovrAvatarPBSMaterialState
{
	public Vector4 baseColor;

	public ulong albedoTextureID;

	public Vector4 albedoMultiplier;

	public ulong metallicnessTextureID;

	public float glossinessScale;

	public ulong normalTextureID;

	public ulong heightTextureID;

	public ulong occlusionTextureID;

	public ulong emissionTextureID;

	public Vector4 emissionMultiplier;

	public ulong detailMaskTextureID;

	public ulong detailAlbedoTextureID;

	public ulong detailNormalTextureID;

	private static bool VectorEquals(Vector4 a, Vector4 b)
	{
		return a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ovrAvatarPBSMaterialState))
		{
			return false;
		}
		ovrAvatarPBSMaterialState ovrAvatarPBSMaterialState2 = (ovrAvatarPBSMaterialState)obj;
		if (!VectorEquals(baseColor, ovrAvatarPBSMaterialState2.baseColor))
		{
			return false;
		}
		if (albedoTextureID != ovrAvatarPBSMaterialState2.albedoTextureID)
		{
			return false;
		}
		if (!VectorEquals(albedoMultiplier, ovrAvatarPBSMaterialState2.albedoMultiplier))
		{
			return false;
		}
		if (metallicnessTextureID != ovrAvatarPBSMaterialState2.metallicnessTextureID)
		{
			return false;
		}
		if (glossinessScale != ovrAvatarPBSMaterialState2.glossinessScale)
		{
			return false;
		}
		if (normalTextureID != ovrAvatarPBSMaterialState2.normalTextureID)
		{
			return false;
		}
		if (heightTextureID != ovrAvatarPBSMaterialState2.heightTextureID)
		{
			return false;
		}
		if (occlusionTextureID != ovrAvatarPBSMaterialState2.occlusionTextureID)
		{
			return false;
		}
		if (emissionTextureID != ovrAvatarPBSMaterialState2.emissionTextureID)
		{
			return false;
		}
		if (!VectorEquals(emissionMultiplier, ovrAvatarPBSMaterialState2.emissionMultiplier))
		{
			return false;
		}
		if (detailMaskTextureID != ovrAvatarPBSMaterialState2.detailMaskTextureID)
		{
			return false;
		}
		if (detailAlbedoTextureID != ovrAvatarPBSMaterialState2.detailAlbedoTextureID)
		{
			return false;
		}
		if (detailNormalTextureID != ovrAvatarPBSMaterialState2.detailNormalTextureID)
		{
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return baseColor.GetHashCode() ^ albedoTextureID.GetHashCode() ^ albedoMultiplier.GetHashCode() ^ metallicnessTextureID.GetHashCode() ^ glossinessScale.GetHashCode() ^ normalTextureID.GetHashCode() ^ heightTextureID.GetHashCode() ^ occlusionTextureID.GetHashCode() ^ emissionTextureID.GetHashCode() ^ emissionMultiplier.GetHashCode() ^ detailMaskTextureID.GetHashCode() ^ detailAlbedoTextureID.GetHashCode() ^ detailNormalTextureID.GetHashCode();
	}
}
