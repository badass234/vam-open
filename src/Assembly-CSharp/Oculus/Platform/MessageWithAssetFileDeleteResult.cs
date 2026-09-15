using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithAssetFileDeleteResult : Message<AssetFileDeleteResult>
	{
		public MessageWithAssetFileDeleteResult(IntPtr c_message)
			: base(c_message)
		{
		}

		public override AssetFileDeleteResult GetAssetFileDeleteResult()
		{
			return base.Data;
		}

		protected override AssetFileDeleteResult GetDataFromMessage(IntPtr c_message)
		{
			IntPtr obj = CAPI.ovr_Message_GetNativeMessage(c_message);
			IntPtr o = CAPI.ovr_Message_GetAssetFileDeleteResult(obj);
			return new AssetFileDeleteResult(o);
		}
	}
}
