using System;
using System.Runtime.InteropServices;
using Oculus.Platform.Models;
using UnityEngine;

namespace Oculus.Platform
{
	public sealed class StandalonePlatform
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void UnityLogDelegate(IntPtr tag, IntPtr msg);

		public Request<PlatformInitialize> InitializeInEditor()
		{
			if (string.IsNullOrEmpty(PlatformSettings.AppID))
			{
				throw new UnityException("Update your App ID by selecting 'Oculus Platform' -> 'Edit Settings'");
			}
			string appID = PlatformSettings.AppID;
			if (string.IsNullOrEmpty(StandalonePlatformSettings.OculusPlatformTestUserEmail))
			{
				throw new UnityException("Update your standalone email address by selecting 'Oculus Platform' -> 'Edit Settings'");
			}
			if (string.IsNullOrEmpty(StandalonePlatformSettings.OculusPlatformTestUserPassword))
			{
				throw new UnityException("Update your standalone user password by selecting 'Oculus Platform' -> 'Edit Settings'");
			}
			CAPI.ovr_UnityResetTestPlatform();
			CAPI.ovr_UnityInitGlobals(IntPtr.Zero);
			CAPI.OculusInitParams init = default(CAPI.OculusInitParams);
			init.sType = 1;
			init.appId = ulong.Parse(appID);
			init.email = StandalonePlatformSettings.OculusPlatformTestUserEmail;
			init.password = StandalonePlatformSettings.OculusPlatformTestUserPassword;
			return new Request<PlatformInitialize>(CAPI.ovr_Platform_InitializeStandaloneOculus(ref init));
		}
	}
}
