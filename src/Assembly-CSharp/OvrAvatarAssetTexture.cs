using System;
using Oculus.Avatar;
using UnityEngine;

public class OvrAvatarAssetTexture : OvrAvatarAsset
{
	public Texture2D texture;

	private const int ASTCHeaderSize = 16;

	public OvrAvatarAssetTexture(ulong _assetId, IntPtr asset)
	{
		assetID = _assetId;
		ovrAvatarTextureAssetData ovrAvatarTextureAssetData2 = CAPI.ovrAvatarAsset_GetTextureData(asset);
		IntPtr data = ovrAvatarTextureAssetData2.textureData;
		int num = (int)ovrAvatarTextureAssetData2.textureDataSize;
		TextureFormat format;
		switch (ovrAvatarTextureAssetData2.format)
		{
		case ovrAvatarTextureFormat.RGB24:
			format = TextureFormat.RGB24;
			break;
		case ovrAvatarTextureFormat.DXT1:
			format = TextureFormat.DXT1;
			break;
		case ovrAvatarTextureFormat.DXT5:
			format = TextureFormat.DXT5;
			break;
		case ovrAvatarTextureFormat.ASTC_RGB_6x6:
			format = TextureFormat.ASTC_6x6;
			data = new IntPtr(data.ToInt64() + 16);
			num -= 16;
			break;
		case ovrAvatarTextureFormat.ASTC_RGB_6x6_MIPMAPS:
			format = TextureFormat.ASTC_6x6;
			break;
		default:
			throw new NotImplementedException($"Unsupported texture format {ovrAvatarTextureAssetData2.format.ToString()}");
		}
		texture = new Texture2D((int)ovrAvatarTextureAssetData2.sizeX, (int)ovrAvatarTextureAssetData2.sizeY, format, ovrAvatarTextureAssetData2.mipCount > 1, false);
		texture.LoadRawTextureData(data, num);
		texture.Apply(true, false);
	}
}
