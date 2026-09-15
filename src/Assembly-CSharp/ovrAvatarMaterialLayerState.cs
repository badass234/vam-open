using UnityEngine;

public struct ovrAvatarMaterialLayerState
{
	public ovrAvatarMaterialLayerBlendMode blendMode;

	public ovrAvatarMaterialLayerSampleMode sampleMode;

	public ovrAvatarMaterialMaskType maskType;

	public Vector4 layerColor;

	public Vector4 sampleParameters;

	public ulong sampleTexture;

	public Vector4 sampleScaleOffset;

	public Vector4 maskParameters;

	public Vector4 maskAxis;

	private static bool VectorEquals(Vector4 a, Vector4 b)
	{
		return a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ovrAvatarMaterialLayerState))
		{
			return false;
		}
		ovrAvatarMaterialLayerState ovrAvatarMaterialLayerState2 = (ovrAvatarMaterialLayerState)obj;
		if (blendMode != ovrAvatarMaterialLayerState2.blendMode)
		{
			return false;
		}
		if (sampleMode != ovrAvatarMaterialLayerState2.sampleMode)
		{
			return false;
		}
		if (maskType != ovrAvatarMaterialLayerState2.maskType)
		{
			return false;
		}
		if (!VectorEquals(layerColor, ovrAvatarMaterialLayerState2.layerColor))
		{
			return false;
		}
		if (!VectorEquals(sampleParameters, ovrAvatarMaterialLayerState2.sampleParameters))
		{
			return false;
		}
		if (sampleTexture != ovrAvatarMaterialLayerState2.sampleTexture)
		{
			return false;
		}
		if (!VectorEquals(sampleScaleOffset, ovrAvatarMaterialLayerState2.sampleScaleOffset))
		{
			return false;
		}
		if (!VectorEquals(maskParameters, ovrAvatarMaterialLayerState2.maskParameters))
		{
			return false;
		}
		if (!VectorEquals(maskAxis, ovrAvatarMaterialLayerState2.maskAxis))
		{
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return blendMode.GetHashCode() ^ sampleMode.GetHashCode() ^ maskType.GetHashCode() ^ layerColor.GetHashCode() ^ sampleParameters.GetHashCode() ^ sampleTexture.GetHashCode() ^ sampleScaleOffset.GetHashCode() ^ maskParameters.GetHashCode() ^ maskAxis.GetHashCode();
	}
}
