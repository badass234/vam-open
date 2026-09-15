using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithHttpTransferUpdate : Message<HttpTransferUpdate>
	{
		public MessageWithHttpTransferUpdate(IntPtr c_message)
			: base(c_message)
		{
		}

		public override HttpTransferUpdate GetHttpTransferUpdate()
		{
			return base.Data;
		}

		protected override HttpTransferUpdate GetDataFromMessage(IntPtr c_message)
		{
			IntPtr obj = CAPI.ovr_Message_GetNativeMessage(c_message);
			IntPtr o = CAPI.ovr_Message_GetHttpTransferUpdate(obj);
			return new HttpTransferUpdate(o);
		}
	}
}
