using System;

namespace Oculus.Platform.Models
{
	public class AssetFileDownloadCancelResult
	{
		public readonly ulong AssetFileId;

		public readonly bool Success;

		public AssetFileDownloadCancelResult(IntPtr o)
		{
			AssetFileId = CAPI.ovr_AssetFileDownloadCancelResult_GetAssetFileId(o);
			Success = CAPI.ovr_AssetFileDownloadCancelResult_GetSuccess(o);
		}
	}
}
