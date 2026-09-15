using System;
using Oculus.Platform.Models;
using UnityEngine;

namespace Oculus.Platform
{
	public sealed class Core
	{
		private static bool IsPlatformInitialized;

		public static bool LogMessages;

		public static bool IsInitialized()
		{
			return IsPlatformInitialized;
		}

		internal static void ForceInitialized()
		{
			IsPlatformInitialized = true;
		}

		private static string getAppID(string appId = null)
		{
			string appIDFromConfig = GetAppIDFromConfig();
			if (string.IsNullOrEmpty(appId))
			{
				if (string.IsNullOrEmpty(appIDFromConfig))
				{
					throw new UnityException("Update your app id by selecting 'Oculus Platform' -> 'Edit Settings'");
				}
				appId = appIDFromConfig;
			}
			else if (!string.IsNullOrEmpty(appIDFromConfig))
			{
				Debug.LogWarningFormat("The 'Oculus App Id ({0})' field in 'Oculus Platform/Edit Settings' is clobbering appId ({1}) that you passed in to Platform.Core.Init.  You should only specify this in one place.  We recommend the menu location.", appIDFromConfig, appId);
			}
			return appId;
		}

		public static Request<PlatformInitialize> AsyncInitialize(string appId = null)
		{
			appId = getAppID(appId);
			Request<PlatformInitialize> request;
			if (UnityEngine.Application.isEditor && PlatformSettings.UseStandalonePlatform)
			{
				StandalonePlatform standalonePlatform = new StandalonePlatform();
				request = standalonePlatform.InitializeInEditor();
			}
			else if (UnityEngine.Application.platform == RuntimePlatform.WindowsEditor || UnityEngine.Application.platform == RuntimePlatform.WindowsPlayer)
			{
				WindowsPlatform windowsPlatform = new WindowsPlatform();
				request = windowsPlatform.AsyncInitialize(appId);
			}
			else
			{
				if (UnityEngine.Application.platform != RuntimePlatform.Android)
				{
					throw new NotImplementedException("Oculus platform is not implemented on this platform yet.");
				}
				AndroidPlatform androidPlatform = new AndroidPlatform();
				request = androidPlatform.AsyncInitialize(appId);
			}
			IsPlatformInitialized = request != null;
			if (!IsPlatformInitialized)
			{
				throw new UnityException("Oculus Platform failed to initialize.");
			}
			if (LogMessages)
			{
				Debug.LogWarning("Oculus.Platform.Core.LogMessages is set to true. This will cause extra heap allocations, and should not be used outside of testing and debugging.");
			}
			new GameObject("Oculus.Platform.CallbackRunner").AddComponent<CallbackRunner>();
			return request;
		}

		public static void Initialize(string appId = null)
		{
			appId = getAppID(appId);
			if (UnityEngine.Application.isEditor && PlatformSettings.UseStandalonePlatform)
			{
				StandalonePlatform standalonePlatform = new StandalonePlatform();
				IsPlatformInitialized = standalonePlatform.InitializeInEditor() != null;
			}
			else if (UnityEngine.Application.platform == RuntimePlatform.WindowsEditor || UnityEngine.Application.platform == RuntimePlatform.WindowsPlayer)
			{
				WindowsPlatform windowsPlatform = new WindowsPlatform();
				IsPlatformInitialized = windowsPlatform.Initialize(appId);
			}
			else
			{
				if (UnityEngine.Application.platform != RuntimePlatform.Android)
				{
					throw new NotImplementedException("Oculus platform is not implemented on this platform yet.");
				}
				AndroidPlatform androidPlatform = new AndroidPlatform();
				IsPlatformInitialized = androidPlatform.Initialize(appId);
			}
			if (!IsPlatformInitialized)
			{
				throw new UnityException("Oculus Platform failed to initialize.");
			}
			if (LogMessages)
			{
				Debug.LogWarning("Oculus.Platform.Core.LogMessages is set to true. This will cause extra heap allocations, and should not be used outside of testing and debugging.");
			}
			new GameObject("Oculus.Platform.CallbackRunner").AddComponent<CallbackRunner>();
		}

		private static string GetAppIDFromConfig()
		{
			if (UnityEngine.Application.platform == RuntimePlatform.Android)
			{
				return PlatformSettings.MobileAppID;
			}
			return PlatformSettings.AppID;
		}
	}
}
