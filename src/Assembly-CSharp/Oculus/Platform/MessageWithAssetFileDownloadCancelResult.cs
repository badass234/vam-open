using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithAssetFileDownloadCancelResult : Message<AssetFileDownloadCancelResult>
	{
		public MessageWithAssetFileDownloadCancelResult(IntPtr c_message)
			: base(c_message)
		{
		}

		public override AssetFileDownloadCancelResult GetAssetFileDownloadCancelResult()
		{
			return base.Data;
		}

		protected override AssetFileDownloadCancelResult GetDataFromMessage(IntPtr c_message)
		{
			IntPtr obj = CAPI.ovr_Message_GetNativeMessage(c_message);
			IntPtr o = CAPI.ovr_Message_GetAssetFileDownloadCancelResult(obj);
			return new AssetFileDownloadCancelResult(o);
		}
	}
}
