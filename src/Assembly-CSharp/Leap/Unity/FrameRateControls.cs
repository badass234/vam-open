using UnityEngine;

namespace Leap.Unity
{
	public class FrameRateControls : MonoBehaviour
	{
		public int targetRenderRate = 60;

		public int targetRenderRateStep = 1;

		public int fixedPhysicsRate = 50;

		public int fixedPhysicsRateStep = 1;

		public KeyCode unlockRender = KeyCode.RightShift;

		public KeyCode unlockPhysics = KeyCode.LeftShift;

		public KeyCode decrease = KeyCode.DownArrow;

		public KeyCode increase = KeyCode.UpArrow;

		public KeyCode resetRate = KeyCode.Backspace;

		private void Awake()
		{
			if (QualitySettings.vSyncCount != 0)
			{
				Debug.LogWarning("vSync will override target frame rate. vSyncCount = " + QualitySettings.vSyncCount);
			}
			Application.targetFrameRate = targetRenderRate;
			Time.fixedDeltaTime = 1f / (float)fixedPhysicsRate;
		}

		private void Update()
		{
			if (Input.GetKey(unlockRender))
			{
				if (Input.GetKeyDown(decrease) && targetRenderRate > targetRenderRateStep)
				{
					targetRenderRate -= targetRenderRateStep;
					Application.targetFrameRate = targetRenderRate;
				}
				if (Input.GetKeyDown(increase))
				{
					targetRenderRate += targetRenderRateStep;
					Application.targetFrameRate = targetRenderRate;
				}
				if (Input.GetKeyDown(resetRate))
				{
					ResetRender();
				}
			}
			if (Input.GetKey(unlockPhysics))
			{
				if (Input.GetKeyDown(decrease) && fixedPhysicsRate > fixedPhysicsRateStep)
				{
					fixedPhysicsRate -= fixedPhysicsRateStep;
					Time.fixedDeltaTime = 1f / (float)fixedPhysicsRate;
				}
				if (Input.GetKeyDown(increase))
				{
					fixedPhysicsRate += fixedPhysicsRateStep;
					Time.fixedDeltaTime = 1f / (float)fixedPhysicsRate;
				}
				if (Input.GetKeyDown(resetRate))
				{
					ResetPhysics();
				}
			}
		}

		public void ResetRender()
		{
			targetRenderRate = 60;
			Application.targetFrameRate = -1;
		}

		public void ResetPhysics()
		{
			fixedPhysicsRate = 50;
			Time.fixedDeltaTime = 0.02f;
		}

		public void ResetAll()
		{
			ResetRender();
			ResetPhysics();
		}
	}
}
