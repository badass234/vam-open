using UnityEngine;

namespace MeshVR.Hands
{
	public class HandInput : MonoBehaviour
	{
		public enum Hand
		{
			Left,
			Right
		}

		public static float rightThumbProximalBend;

		public static float rightThumbProximalSpread;

		public static float rightThumbProximalTwist;

		public static float rightThumbMiddleBend;

		public static float rightThumbDistalBend;

		public static float rightIndexProximalBend;

		public static float rightIndexProximalSpread;

		public static float rightIndexProximalTwist;

		public static float rightIndexMiddleBend;

		public static float rightIndexDistalBend;

		public static float rightMiddleProximalBend;

		public static float rightMiddleProximalSpread;

		public static float rightMiddleProximalTwist;

		public static float rightMiddleMiddleBend;

		public static float rightMiddleDistalBend;

		public static float rightRingProximalBend;

		public static float rightRingProximalSpread;

		public static float rightRingProximalTwist;

		public static float rightRingMiddleBend;

		public static float rightRingDistalBend;

		public static float rightPinkyProximalBend;

		public static float rightPinkyProximalSpread;

		public static float rightPinkyProximalTwist;

		public static float rightPinkyMiddleBend;

		public static float rightPinkyDistalBend;

		public static float leftThumbProximalBend;

		public static float leftThumbProximalSpread;

		public static float leftThumbProximalTwist;

		public static float leftThumbMiddleBend;

		public static float leftThumbDistalBend;

		public static float leftIndexProximalBend;

		public static float leftIndexProximalSpread;

		public static float leftIndexProximalTwist;

		public static float leftIndexMiddleBend;

		public static float leftIndexDistalBend;

		public static float leftMiddleProximalBend;

		public static float leftMiddleProximalSpread;

		public static float leftMiddleProximalTwist;

		public static float leftMiddleMiddleBend;

		public static float leftMiddleDistalBend;

		public static float leftRingProximalBend;

		public static float leftRingProximalSpread;

		public static float leftRingProximalTwist;

		public static float leftRingMiddleBend;

		public static float leftRingDistalBend;

		public static float leftPinkyProximalBend;

		public static float leftPinkyProximalSpread;

		public static float leftPinkyProximalTwist;

		public static float leftPinkyMiddleBend;

		public static float leftPinkyDistalBend;

		public Hand hand;

		public bool ignoreInputFingerSpread;

		public float fingerInputFactor = 1f;

		public float thumbInputFactor = 1f;

		public float fingerSpreadOffset;

		public float fingerBendOffset;

		public float thumbSpreadOffset;

		public float thumbBendOffset;

		public FingerInput thumbProximal;

		public FingerInput thumbMiddle;

		public FingerInput thumbDistal;

		public FingerInput indexProximal;

		public FingerInput indexMiddle;

		public FingerInput indexDistal;

		public FingerInput middleProximal;

		public FingerInput middleMiddle;

		public FingerInput middleDistal;

		public FingerInput ringProximal;

		public FingerInput ringMiddle;

		public FingerInput ringDistal;

		public FingerInput pinkyProximal;

		public FingerInput pinkyMiddle;

		public FingerInput pinkyDistal;

		protected virtual void Update()
		{
			float num = fingerSpreadOffset * 0.25f;
			if (hand == Hand.Left)
			{
				if (thumbProximal != null)
				{
					thumbProximal.bendInputFactor = thumbInputFactor;
					thumbProximal.UpdateInput();
					leftThumbProximalBend = thumbProximal.currentBend + thumbBendOffset;
					leftThumbProximalSpread = thumbProximal.currentSpread + thumbSpreadOffset;
					leftThumbProximalTwist = thumbProximal.currentTwist;
				}
				if (thumbMiddle != null)
				{
					thumbMiddle.bendInputFactor = thumbInputFactor;
					thumbMiddle.UpdateInput();
					leftThumbMiddleBend = thumbMiddle.currentBend + thumbBendOffset;
				}
				if (thumbDistal != null)
				{
					thumbDistal.bendInputFactor = thumbInputFactor;
					thumbDistal.UpdateInput();
					leftThumbDistalBend = thumbDistal.currentBend + thumbBendOffset;
				}
				if (indexProximal != null)
				{
					indexProximal.bendInputFactor = fingerInputFactor;
					indexProximal.UpdateInput();
					leftIndexProximalBend = indexProximal.currentBend + fingerBendOffset;
					if (ignoreInputFingerSpread)
					{
						leftIndexProximalSpread = 0f - fingerSpreadOffset;
					}
					else
					{
						leftIndexProximalSpread = indexProximal.currentSpread - fingerSpreadOffset;
					}
					leftIndexProximalTwist = indexProximal.currentTwist;
				}
				if (indexMiddle != null)
				{
					indexMiddle.bendInputFactor = fingerInputFactor;
					indexMiddle.UpdateInput();
					leftIndexMiddleBend = indexMiddle.currentBend + fingerBendOffset;
				}
				if (indexDistal != null)
				{
					indexDistal.bendInputFactor = fingerInputFactor;
					indexDistal.UpdateInput();
					leftIndexDistalBend = indexDistal.currentBend + fingerBendOffset;
				}
				if (middleProximal != null)
				{
					middleProximal.bendInputFactor = fingerInputFactor;
					middleProximal.UpdateInput();
					leftMiddleProximalBend = middleProximal.currentBend + fingerBendOffset;
					if (ignoreInputFingerSpread)
					{
						leftMiddleProximalSpread = 0f - num;
					}
					else
					{
						leftMiddleProximalSpread = middleProximal.currentSpread - num;
					}
					leftMiddleProximalTwist = middleProximal.currentTwist;
				}
				if (middleMiddle != null)
				{
					middleMiddle.bendInputFactor = fingerInputFactor;
					middleMiddle.UpdateInput();
					leftMiddleMiddleBend = middleMiddle.currentBend + fingerBendOffset;
				}
				if (middleDistal != null)
				{
					middleDistal.bendInputFactor = fingerInputFactor;
					middleDistal.UpdateInput();
					leftMiddleDistalBend = middleDistal.currentBend + fingerBendOffset;
				}
				if (ringProximal != null)
				{
					ringProximal.bendInputFactor = fingerInputFactor;
					ringProximal.UpdateInput();
					leftRingProximalBend = ringProximal.currentBend + fingerBendOffset;
					if (ignoreInputFingerSpread)
					{
						leftRingProximalSpread = num;
					}
					else
					{
						leftRingProximalSpread = ringProximal.currentSpread + num;
					}
					leftRingProximalTwist = ringProximal.currentTwist;
				}
				if (ringMiddle != null)
				{
					ringMiddle.bendInputFactor = fingerInputFactor;
					ringMiddle.UpdateInput();
					leftRingMiddleBend = ringMiddle.currentBend + fingerBendOffset;
				}
				if (ringDistal != null)
				{
					ringDistal.bendInputFactor = fingerInputFactor;
					ringDistal.UpdateInput();
					leftRingDistalBend = ringDistal.currentBend + fingerBendOffset;
				}
				if (pinkyProximal != null)
				{
					pinkyProximal.bendInputFactor = fingerInputFactor;
					pinkyProximal.UpdateInput();
					leftPinkyProximalBend = pinkyProximal.currentBend + fingerBendOffset;
					if (ignoreInputFingerSpread)
					{
						leftPinkyProximalSpread = fingerSpreadOffset;
					}
					else
					{
						leftPinkyProximalSpread = pinkyProximal.currentSpread + fingerSpreadOffset;
					}
					leftPinkyProximalTwist = pinkyProximal.currentTwist;
				}
				if (pinkyMiddle != null)
				{
					pinkyMiddle.bendInputFactor = fingerInputFactor;
					pinkyMiddle.UpdateInput();
					leftPinkyMiddleBend = pinkyMiddle.currentBend + fingerBendOffset;
				}
				if (pinkyDistal != null)
				{
					pinkyDistal.bendInputFactor = fingerInputFactor;
					pinkyDistal.UpdateInput();
					leftPinkyDistalBend = pinkyDistal.currentBend + fingerBendOffset;
				}
				return;
			}
			if (thumbProximal != null)
			{
				thumbProximal.bendInputFactor = thumbInputFactor;
				thumbProximal.UpdateInput();
				rightThumbProximalBend = thumbProximal.currentBend + thumbBendOffset;
				rightThumbProximalSpread = thumbProximal.currentSpread + thumbSpreadOffset;
				rightThumbProximalTwist = thumbProximal.currentTwist;
			}
			if (thumbMiddle != null)
			{
				thumbMiddle.bendInputFactor = thumbInputFactor;
				thumbMiddle.UpdateInput();
				rightThumbMiddleBend = thumbMiddle.currentBend + thumbBendOffset;
			}
			if (thumbDistal != null)
			{
				thumbDistal.bendInputFactor = thumbInputFactor;
				thumbDistal.UpdateInput();
				rightThumbDistalBend = thumbDistal.currentBend + thumbBendOffset;
			}
			if (indexProximal != null)
			{
				indexProximal.bendInputFactor = fingerInputFactor;
				indexProximal.UpdateInput();
				rightIndexProximalBend = indexProximal.currentBend + fingerBendOffset;
				if (ignoreInputFingerSpread)
				{
					rightIndexProximalSpread = 0f - fingerSpreadOffset;
				}
				else
				{
					rightIndexProximalSpread = indexProximal.currentSpread - fingerSpreadOffset;
				}
				rightIndexProximalTwist = indexProximal.currentTwist;
			}
			if (indexMiddle != null)
			{
				indexMiddle.bendInputFactor = fingerInputFactor;
				indexMiddle.UpdateInput();
				rightIndexMiddleBend = indexMiddle.currentBend + fingerBendOffset;
			}
			if (indexDistal != null)
			{
				indexDistal.bendInputFactor = fingerInputFactor;
				indexDistal.UpdateInput();
				rightIndexDistalBend = indexDistal.currentBend + fingerBendOffset;
			}
			if (middleProximal != null)
			{
				middleProximal.bendInputFactor = fingerInputFactor;
				middleProximal.UpdateInput();
				rightMiddleProximalBend = middleProximal.currentBend + fingerBendOffset;
				if (ignoreInputFingerSpread)
				{
					rightMiddleProximalSpread = 0f - num;
				}
				else
				{
					rightMiddleProximalSpread = middleProximal.currentSpread - num;
				}
				rightMiddleProximalTwist = middleProximal.currentTwist;
			}
			if (middleMiddle != null)
			{
				middleMiddle.bendInputFactor = fingerInputFactor;
				middleMiddle.UpdateInput();
				rightMiddleMiddleBend = middleMiddle.currentBend + fingerBendOffset;
			}
			if (middleDistal != null)
			{
				middleDistal.bendInputFactor = fingerInputFactor;
				middleDistal.UpdateInput();
				rightMiddleDistalBend = middleDistal.currentBend + fingerBendOffset;
			}
			if (ringProximal != null)
			{
				ringProximal.bendInputFactor = fingerInputFactor;
				ringProximal.UpdateInput();
				rightRingProximalBend = ringProximal.currentBend + fingerBendOffset;
				if (ignoreInputFingerSpread)
				{
					rightRingProximalSpread = num;
				}
				else
				{
					rightRingProximalSpread = ringProximal.currentSpread + num;
				}
				rightRingProximalTwist = ringProximal.currentTwist;
			}
			if (ringMiddle != null)
			{
				ringMiddle.bendInputFactor = fingerInputFactor;
				ringMiddle.UpdateInput();
				rightRingMiddleBend = ringMiddle.currentBend + fingerBendOffset;
			}
			if (ringDistal != null)
			{
				ringDistal.bendInputFactor = fingerInputFactor;
				ringDistal.UpdateInput();
				rightRingDistalBend = ringDistal.currentBend + fingerBendOffset;
			}
			if (pinkyProximal != null)
			{
				pinkyProximal.bendInputFactor = fingerInputFactor;
				pinkyProximal.UpdateInput();
				rightPinkyProximalBend = pinkyProximal.currentBend + fingerBendOffset;
				if (ignoreInputFingerSpread)
				{
					rightPinkyProximalSpread = fingerSpreadOffset;
				}
				else
				{
					rightPinkyProximalSpread = pinkyProximal.currentSpread + fingerSpreadOffset;
				}
				rightPinkyProximalTwist = pinkyProximal.currentTwist;
			}
			if (pinkyMiddle != null)
			{
				pinkyMiddle.bendInputFactor = fingerInputFactor;
				pinkyMiddle.UpdateInput();
				rightPinkyMiddleBend = pinkyMiddle.currentBend + fingerBendOffset;
			}
			if (pinkyDistal != null)
			{
				pinkyDistal.bendInputFactor = fingerInputFactor;
				pinkyDistal.UpdateInput();
				rightPinkyDistalBend = pinkyDistal.currentBend + fingerBendOffset;
			}
		}

		public void Init()
		{
			if (thumbProximal != null)
			{
				thumbProximal.Init();
			}
			if (thumbMiddle != null)
			{
				thumbMiddle.Init();
			}
			if (thumbDistal != null)
			{
				thumbDistal.Init();
			}
			if (indexProximal != null)
			{
				indexProximal.Init();
			}
			if (indexMiddle != null)
			{
				indexMiddle.Init();
			}
			if (indexDistal != null)
			{
				indexDistal.Init();
			}
			if (middleProximal != null)
			{
				middleProximal.Init();
			}
			if (middleMiddle != null)
			{
				middleMiddle.Init();
			}
			if (middleDistal != null)
			{
				middleDistal.Init();
			}
			if (ringProximal != null)
			{
				ringProximal.Init();
			}
			if (ringMiddle != null)
			{
				ringMiddle.Init();
			}
			if (ringDistal != null)
			{
				ringDistal.Init();
			}
			if (pinkyProximal != null)
			{
				pinkyProximal.Init();
			}
			if (pinkyMiddle != null)
			{
				pinkyMiddle.Init();
			}
			if (pinkyDistal != null)
			{
				pinkyDistal.Init();
			}
		}

		protected virtual void Awake()
		{
			Init();
		}
	}
}
