using System;

namespace Oculus.Platform
{
	public class ApplicationOptions
	{
		private IntPtr Handle;

		public ApplicationOptions()
		{
			Handle = CAPI.ovr_ApplicationOptions_Create();
		}

		public void SetDeeplinkMessage(string value)
		{
			CAPI.ovr_ApplicationOptions_SetDeeplinkMessage(Handle, value);
		}

		public static explicit operator IntPtr(ApplicationOptions options)
		{
			return options?.Handle ?? IntPtr.Zero;
		}

		~ApplicationOptions()
		{
			CAPI.ovr_ApplicationOptions_Destroy(Handle);
		}
	}
}
