using System;

namespace Oculus.Platform.Models
{
	public class AssetFileDeleteResult
	{
		public readonly ulong AssetFileId;

		public readonly bool Success;

		public AssetFileDeleteResult(IntPtr o)
		{
			AssetFileId = CAPI.ovr_AssetFileDeleteResult_GetAssetFileId(o);
			Success = CAPI.ovr_AssetFileDeleteResult_GetSuccess(o);
		}
	}
}
