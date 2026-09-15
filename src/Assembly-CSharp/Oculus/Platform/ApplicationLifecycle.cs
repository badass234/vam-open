using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public static class ApplicationLifecycle
	{
		public static LaunchDetails GetLaunchDetails()
		{
			return new LaunchDetails(CAPI.ovr_ApplicationLifecycle_GetLaunchDetails());
		}
	}
}
