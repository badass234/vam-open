using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public static class AssetFile
	{
		public static Request<AssetFileDeleteResult> Delete(ulong assetFileID)
		{
			if (Core.IsInitialized())
			{
				return new Request<AssetFileDeleteResult>(CAPI.ovr_AssetFile_Delete(assetFileID));
			}
			return null;
		}

		public static Request<AssetFileDownloadResult> Download(ulong assetFileID)
		{
			if (Core.IsInitialized())
			{
				return new Request<AssetFileDownloadResult>(CAPI.ovr_AssetFile_Download(assetFileID));
			}
			return null;
		}

		public static Request<AssetFileDownloadCancelResult> DownloadCancel(ulong assetFileID)
		{
			if (Core.IsInitialized())
			{
				return new Request<AssetFileDownloadCancelResult>(CAPI.ovr_AssetFile_DownloadCancel(assetFileID));
			}
			return null;
		}
	}
}
