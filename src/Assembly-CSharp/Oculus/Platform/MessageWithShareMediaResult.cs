using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithShareMediaResult : Message<ShareMediaResult>
	{
		public MessageWithShareMediaResult(IntPtr c_message)
			: base(c_message)
		{
		}

		public override ShareMediaResult GetShareMediaResult()
		{
			return base.Data;
		}

		protected override ShareMediaResult GetDataFromMessage(IntPtr c_message)
		{
			IntPtr obj = CAPI.ovr_Message_GetNativeMessage(c_message);
			IntPtr o = CAPI.ovr_Message_GetShareMediaResult(obj);
			return new ShareMediaResult(o);
		}
	}
}
