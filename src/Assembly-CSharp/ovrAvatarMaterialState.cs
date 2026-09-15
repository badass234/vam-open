using System.Runtime.InteropServices;
using UnityEngine;

public struct ovrAvatarMaterialState
{
	public Vector4 baseColor;

	public ovrAvatarMaterialMaskType baseMaskType;

	public Vector4 baseMaskParameters;

	public Vector4 baseMaskAxis;

	public ovrAvatarMaterialLayerSampleMode sampleMode;

	public ulong alphaMaskTextureID;

	public Vector4 alphaMaskScaleOffset;

	public ulong normalMapTextureID;

	public Vector4 normalMapScaleOffset;

	public ulong parallaxMapTextureID;

	public Vector4 parallaxMapScaleOffset;

	public ulong roughnessMapTextureID;

	public Vector4 roughnessMapScaleOffset;

	public uint layerCount;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
	public ovrAvatarMaterialLayerState[] layers;

	private static bool VectorEquals(Vector4 a, Vector4 b)
	{
		return a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ovrAvatarMaterialState))
		{
			return false;
		}
		ovrAvatarMaterialState ovrAvatarMaterialState2 = (ovrAvatarMaterialState)obj;
		if (!VectorEquals(baseColor, ovrAvatarMaterialState2.baseColor))
		{
			return false;
		}
		if (baseMaskType != ovrAvatarMaterialState2.baseMaskType)
		{
			return false;
		}
		if (!VectorEquals(baseMaskParameters, ovrAvatarMaterialState2.baseMaskParameters))
		{
			return false;
		}
		if (!VectorEquals(baseMaskAxis, ovrAvatarMaterialState2.baseMaskAxis))
		{
			return false;
		}
		if (sampleMode != ovrAvatarMaterialState2.sampleMode)
		{
			return false;
		}
		if (alphaMaskTextureID != ovrAvatarMaterialState2.alphaMaskTextureID)
		{
			return false;
		}
		if (!VectorEquals(alphaMaskScaleOffset, ovrAvatarMaterialState2.alphaMaskScaleOffset))
		{
			return false;
		}
		if (normalMapTextureID != ovrAvatarMaterialState2.normalMapTextureID)
		{
			return false;
		}
		if (!VectorEquals(normalMapScaleOffset, ovrAvatarMaterialState2.normalMapScaleOffset))
		{
			return false;
		}
		if (parallaxMapTextureID != ovrAvatarMaterialState2.parallaxMapTextureID)
		{
			return false;
		}
		if (!VectorEquals(parallaxMapScaleOffset, ovrAvatarMaterialState2.parallaxMapScaleOffset))
		{
			return false;
		}
		if (roughnessMapTextureID != ovrAvatarMaterialState2.roughnessMapTextureID)
		{
			return false;
		}
		if (!VectorEquals(roughnessMapScaleOffset, ovrAvatarMaterialState2.roughnessMapScaleOffset))
		{
			return false;
		}
		if (layerCount != ovrAvatarMaterialState2.layerCount)
		{
			return false;
		}
		for (int i = 0; i < layerCount; i++)
		{
			if (!layers[i].Equals(ovrAvatarMaterialState2.layers[i]))
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		int num = 0;
		num ^= baseColor.GetHashCode();
		num ^= baseMaskType.GetHashCode();
		num ^= baseMaskParameters.GetHashCode();
		num ^= baseMaskAxis.GetHashCode();
		num ^= sampleMode.GetHashCode();
		num ^= alphaMaskTextureID.GetHashCode();
		num ^= alphaMaskScaleOffset.GetHashCode();
		num ^= normalMapTextureID.GetHashCode();
		num ^= normalMapScaleOffset.GetHashCode();
		num ^= parallaxMapTextureID.GetHashCode();
		num ^= parallaxMapScaleOffset.GetHashCode();
		num ^= roughnessMapTextureID.GetHashCode();
		num ^= roughnessMapScaleOffset.GetHashCode();
		num ^= layerCount.GetHashCode();
		for (int i = 0; i < layerCount; i++)
		{
			num ^= layers[i].GetHashCode();
		}
		return num;
	}
}
