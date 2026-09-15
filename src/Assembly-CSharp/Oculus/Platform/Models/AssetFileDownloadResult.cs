using System;

namespace Oculus.Platform.Models
{
	public class AssetFileDownloadResult
	{
		public readonly string Filepath;

		public AssetFileDownloadResult(IntPtr o)
		{
			Filepath = CAPI.ovr_AssetFileDownloadResult_GetFilepath(o);
		}
	}
}
