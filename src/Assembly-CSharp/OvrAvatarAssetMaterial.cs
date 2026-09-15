using System;
using Oculus.Avatar;

public class OvrAvatarAssetMaterial : OvrAvatarAsset
{
	public ovrAvatarMaterialState material;

	public OvrAvatarAssetMaterial(ulong id, IntPtr mat)
	{
		assetID = id;
		material = CAPI.ovrAvatarAsset_GetMaterialState(mat);
	}
}
