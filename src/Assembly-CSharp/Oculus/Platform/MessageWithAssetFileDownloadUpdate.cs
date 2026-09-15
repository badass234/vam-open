using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithAssetFileDownloadUpdate : Message<AssetFileDownloadUpdate>
	{
		public MessageWithAssetFileDownloadUpdate(IntPtr c_message)
			: base(c_message)
		{
		}

		public override AssetFileDownloadUpdate GetAssetFileDownloadUpdate()
		{
			return base.Data;
		}

		protected override AssetFileDownloadUpdate GetDataFromMessage(IntPtr c_message)
		{
			IntPtr obj = CAPI.ovr_Message_GetNativeMessage(c_message);
			IntPtr o = CAPI.ovr_Message_GetAssetFileDownloadUpdate(obj);
			return new AssetFileDownloadUpdate(o);
		}
	}
}
