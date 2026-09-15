using UnityEngine;

namespace MeshVR
{
	public class PerfMonCamera : MonoBehaviour
	{
		public static float renderStartTime;

		public static bool wasSet { get; protected set; }

		private void LateUpdate()
		{
			wasSet = false;
		}

		private void OnDisable()
		{
			wasSet = false;
		}

		private void OnPreCull()
		{
			if (!wasSet)
			{
				wasSet = true;
				renderStartTime = GlobalStopwatch.GetElapsedMilliseconds();
			}
		}
	}
}
