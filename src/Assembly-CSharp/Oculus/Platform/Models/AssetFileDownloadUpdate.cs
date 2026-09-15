using System;

namespace Oculus.Platform.Models
{
	public class AssetFileDownloadUpdate
	{
		public readonly ulong AssetFileId;

		public readonly uint BytesTotal;

		public readonly uint BytesTransferred;

		public readonly bool Completed;

		public AssetFileDownloadUpdate(IntPtr o)
		{
			AssetFileId = CAPI.ovr_AssetFileDownloadUpdate_GetAssetFileId(o);
			BytesTotal = CAPI.ovr_AssetFileDownloadUpdate_GetBytesTotal(o);
			BytesTransferred = CAPI.ovr_AssetFileDownloadUpdate_GetBytesTransferred(o);
			Completed = CAPI.ovr_AssetFileDownloadUpdate_GetCompleted(o);
		}
	}
}
