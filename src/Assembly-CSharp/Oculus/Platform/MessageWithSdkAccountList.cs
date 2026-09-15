using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithSdkAccountList : Message<SdkAccountList>
	{
		public MessageWithSdkAccountList(IntPtr c_message)
			: base(c_message)
		{
		}

		public override SdkAccountList GetSdkAccountList()
		{
			return base.Data;
		}

		protected override SdkAccountList GetDataFromMessage(IntPtr c_message)
		{
			IntPtr obj = CAPI.ovr_Message_GetNativeMessage(c_message);
			IntPtr a = CAPI.ovr_Message_GetSdkAccountArray(obj);
			return new SdkAccountList(a);
		}
	}
}
