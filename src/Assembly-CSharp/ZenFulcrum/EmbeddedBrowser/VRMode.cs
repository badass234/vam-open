using UnityEngine;
using UnityEngine.XR;

namespace ZenFulcrum.EmbeddedBrowser
{
	public class VRMode : MonoBehaviour
	{
		public bool enableVR;

		private bool oldState;

		public void OnEnable()
		{
			oldState = XRSettings.enabled;
			XRSettings.enabled = enableVR;
			if (XRSettings.enabled)
			{
				XRDevice.SetTrackingSpaceType(TrackingSpaceType.RoomScale);
			}
		}

		public void OnDisable()
		{
			XRSettings.enabled = oldState;
		}
	}
}
