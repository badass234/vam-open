using UnityEngine;

namespace Battlehub.RTCommon
{
	public class GameCamera : MonoBehaviour
	{
		public static event GameCameraEvent Awaked;

		public static event GameCameraEvent Destroyed;

		public static event GameCameraEvent Enabled;

		public static event GameCameraEvent Disabled;

		private void Awake()
		{
			if (GameCamera.Awaked != null)
			{
				GameCamera.Awaked();
			}
		}

		private void OnDestroy()
		{
			if (GameCamera.Destroyed != null)
			{
				GameCamera.Destroyed();
			}
		}

		private void OnEnable()
		{
			if (GameCamera.Enabled != null)
			{
				GameCamera.Enabled();
			}
		}

		private void OnDisable()
		{
			if (GameCamera.Disabled != null)
			{
				GameCamera.Disabled();
			}
		}
	}
}
