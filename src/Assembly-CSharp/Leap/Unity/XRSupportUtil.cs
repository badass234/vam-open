using UnityEngine;
using UnityEngine.XR;

namespace Leap.Unity
{
	public static class XRSupportUtil
	{
		private static bool outputPresenceWarning;

		public static bool IsXREnabled()
		{
			return XRSettings.enabled;
		}

		public static bool IsXRDevicePresent()
		{
			// XRDevice.isPresent is an obsolete-error from 2020.1; XRSettings.isDeviceActive is the surviving equivalent.
			return XRSettings.isDeviceActive;
		}

		public static bool IsUserPresent(bool defaultPresence = true)
		{
			// UserPresenceState and XRDevice.userPresence were removed in 2020.1; CommonUsages.userPresence is the surviving source.
			bool present = false;
			InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.Head);
			bool supported = device.isValid && device.TryGetFeatureValue(CommonUsages.userPresence, out present);
			if (supported && present)
			{
				return true;
			}
			if (!outputPresenceWarning && !supported)
			{
				Debug.LogWarning("XR UserPresenceState unsupported (XR support is probably disabled).");
				outputPresenceWarning = true;
			}
			return defaultPresence;
		}

		public static Vector3 GetXRNodeCenterEyeLocalPosition()
		{
			return InputTracking.GetLocalPosition(XRNode.CenterEye);
		}

		public static Quaternion GetXRNodeCenterEyeLocalRotation()
		{
			return InputTracking.GetLocalRotation(XRNode.CenterEye);
		}

		public static Vector3 GetXRNodeHeadLocalPosition()
		{
			return InputTracking.GetLocalPosition(XRNode.Head);
		}

		public static Quaternion GetXRNodeHeadLocalRotation()
		{
			return InputTracking.GetLocalRotation(XRNode.Head);
		}

		public static void Recenter()
		{
			InputTracking.Recenter();
		}

		public static string GetLoadedDeviceName()
		{
			return XRSettings.loadedDeviceName;
		}

		public static bool IsRoomScale()
		{
			return XRDevice.GetTrackingSpaceType() == TrackingSpaceType.RoomScale;
		}

		public static float GetGPUTime()
		{
			float gpuTimeLastFrame = 0f;
			XRStats.TryGetGPUTimeLastFrame(out gpuTimeLastFrame);
			return gpuTimeLastFrame;
		}
	}
}
