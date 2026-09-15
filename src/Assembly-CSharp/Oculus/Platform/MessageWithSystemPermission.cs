using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithSystemPermission : Message<SystemPermission>
	{
		public MessageWithSystemPermission(IntPtr c_message)
			: base(c_message)
		{
		}

		public override SystemPermission GetSystemPermission()
		{
			return base.Data;
		}

		protected override SystemPermission GetDataFromMessage(IntPtr c_message)
		{
			IntPtr obj = CAPI.ovr_Message_GetNativeMessage(c_message);
			IntPtr o = CAPI.ovr_Message_GetSystemPermission(obj);
			return new SystemPermission(o);
		}
	}
}
