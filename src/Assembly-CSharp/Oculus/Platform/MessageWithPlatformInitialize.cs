using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithPlatformInitialize : Message<PlatformInitialize>
	{
		public MessageWithPlatformInitialize(IntPtr c_message)
			: base(c_message)
		{
		}

		public override PlatformInitialize GetPlatformInitialize()
		{
			return base.Data;
		}

		protected override PlatformInitialize GetDataFromMessage(IntPtr c_message)
		{
			IntPtr obj = CAPI.ovr_Message_GetNativeMessage(c_message);
			IntPtr o = CAPI.ovr_Message_GetPlatformInitialize(obj);
			return new PlatformInitialize(o);
		}
	}
}
