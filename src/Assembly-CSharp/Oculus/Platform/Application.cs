using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public static class Application
	{
		public static Request<ApplicationVersion> GetVersion()
		{
			if (Core.IsInitialized())
			{
				return new Request<ApplicationVersion>(CAPI.ovr_Application_GetVersion());
			}
			return null;
		}

		public static Request<string> LaunchOtherApp(ulong appID, ApplicationOptions deeplink_options = null)
		{
			if (Core.IsInitialized())
			{
				return new Request<string>(CAPI.ovr_Application_LaunchOtherApp(appID, (IntPtr)deeplink_options));
			}
			return null;
		}
	}
}
