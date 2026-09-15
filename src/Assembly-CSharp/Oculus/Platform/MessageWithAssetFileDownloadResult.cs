using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithAssetFileDownloadResult : Message<AssetFileDownloadResult>
	{
		public MessageWithAssetFileDownloadResult(IntPtr c_message)
			: base(c_message)
		{
		}

		public override AssetFileDownloadResult GetAssetFileDownloadResult()
		{
			return base.Data;
		}

		protected override AssetFileDownloadResult GetDataFromMessage(IntPtr c_message)
		{
			IntPtr obj = CAPI.ovr_Message_GetNativeMessage(c_message);
			IntPtr o = CAPI.ovr_Message_GetAssetFileDownloadResult(obj);
			return new AssetFileDownloadResult(o);
		}
	}
}
