using Valve.VR;

namespace MeshVR.Hands
{
	public class SteamVRHandInput : HandInput
	{
		protected SteamVR_Behaviour_Skeleton skeleton;

		public float thumbProximalBend;

		public float thumbProximalSpread;

		public float thumbProximalTwist;

		public float thumbMiddleBend;

		public float thumbDistalBend;

		public float indexProximalBend;

		public float indexProximalSpread;

		public float indexProximalTwist;

		public float indexMiddleBend;

		public float indexDistalBend;

		public float middleProximalBend;

		public float middleProximalSpread;

		public float middleProximalTwist;

		public float middleMiddleBend;

		public float middleDistalBend;

		public float ringProximalBend;

		public float ringProximalSpread;

		public float ringProximalTwist;

		public float ringMiddleBend;

		public float ringDistalBend;

		public float pinkyProximalBend;

		public float pinkyProximalSpread;

		public float pinkyProximalTwist;

		public float pinkyMiddleBend;

		public float pinkyDistalBend;

		public float currentThumbValue;

		public float currentFingerValue;

		protected override void Update()
		{
			if (skeleton != null && skeleton.skeletonAvailable)
			{
				base.Update();
			}
			else if (hand == Hand.Left)
			{
				float num = (currentThumbValue = SuperController.singleton.GetLeftGrabVal() * thumbInputFactor);
				HandInput.leftThumbProximalBend = thumbProximalBend * num + thumbBendOffset;
				HandInput.leftThumbProximalSpread = thumbProximalSpread * num + thumbSpreadOffset;
				HandInput.leftThumbProximalTwist = thumbProximalTwist * num;
				HandInput.leftThumbMiddleBend = thumbMiddleBend * num + thumbBendOffset;
				HandInput.leftThumbDistalBend = thumbDistalBend * num + thumbBendOffset;
				num = (currentFingerValue = SuperController.singleton.GetLeftGrabVal() * fingerInputFactor);
				HandInput.leftIndexProximalBend = indexProximalBend * num + fingerBendOffset;
				HandInput.leftIndexProximalSpread = indexProximalSpread * num + fingerSpreadOffset;
				HandInput.leftIndexProximalTwist = indexProximalTwist * num;
				HandInput.leftIndexMiddleBend = indexMiddleBend * num + fingerBendOffset;
				HandInput.leftIndexDistalBend = indexDistalBend * num + fingerBendOffset;
				HandInput.leftMiddleProximalBend = middleProximalBend * num + fingerBendOffset;
				HandInput.leftMiddleProximalSpread = middleProximalSpread * num + fingerSpreadOffset;
				HandInput.leftMiddleProximalTwist = middleProximalTwist * num;
				HandInput.leftMiddleMiddleBend = middleMiddleBend * num + fingerBendOffset;
				HandInput.leftMiddleDistalBend = middleDistalBend * num + fingerBendOffset;
				HandInput.leftRingProximalBend = ringProximalBend * num + fingerBendOffset;
				HandInput.leftRingProximalSpread = ringProximalSpread * num + fingerSpreadOffset;
				HandInput.leftRingProximalTwist = ringProximalTwist * num;
				HandInput.leftRingMiddleBend = ringMiddleBend * num + fingerBendOffset;
				HandInput.leftRingDistalBend = ringDistalBend * num + fingerBendOffset;
				HandInput.leftPinkyProximalBend = pinkyProximalBend * num + fingerBendOffset;
				HandInput.leftPinkyProximalSpread = pinkyProximalSpread * num + fingerSpreadOffset;
				HandInput.leftPinkyProximalTwist = pinkyProximalTwist * num;
				HandInput.leftPinkyMiddleBend = pinkyMiddleBend * num + fingerBendOffset;
				HandInput.leftPinkyDistalBend = pinkyDistalBend * num + fingerBendOffset;
			}
			else
			{
				float num2 = (currentThumbValue = SuperController.singleton.GetRightGrabVal() * thumbInputFactor);
				HandInput.rightThumbProximalBend = thumbProximalBend * num2 + thumbBendOffset;
				HandInput.rightThumbProximalSpread = thumbProximalSpread * num2 + thumbSpreadOffset;
				HandInput.rightThumbProximalTwist = thumbProximalTwist * num2;
				HandInput.rightThumbMiddleBend = thumbMiddleBend * num2 + thumbBendOffset;
				HandInput.rightThumbDistalBend = thumbDistalBend * num2 + thumbBendOffset;
				num2 = (currentFingerValue = SuperController.singleton.GetRightGrabVal() * fingerInputFactor);
				HandInput.rightIndexProximalBend = indexProximalBend * num2 + fingerBendOffset;
				HandInput.rightIndexProximalSpread = indexProximalSpread * num2 + fingerSpreadOffset;
				HandInput.rightIndexProximalTwist = indexProximalTwist * num2;
				HandInput.rightIndexMiddleBend = indexMiddleBend * num2 + fingerBendOffset;
				HandInput.rightIndexDistalBend = indexDistalBend * num2 + fingerBendOffset;
				HandInput.rightMiddleProximalBend = middleProximalBend * num2 + fingerBendOffset;
				HandInput.rightMiddleProximalSpread = middleProximalSpread * num2 + fingerSpreadOffset;
				HandInput.rightMiddleProximalTwist = middleProximalTwist * num2;
				HandInput.rightMiddleMiddleBend = middleMiddleBend * num2 + fingerBendOffset;
				HandInput.rightMiddleDistalBend = middleDistalBend * num2 + fingerBendOffset;
				HandInput.rightRingProximalBend = ringProximalBend * num2 + fingerBendOffset;
				HandInput.rightRingProximalSpread = ringProximalSpread * num2 + fingerSpreadOffset;
				HandInput.rightRingProximalTwist = ringProximalTwist * num2;
				HandInput.rightRingMiddleBend = ringMiddleBend * num2 + fingerBendOffset;
				HandInput.rightRingDistalBend = ringDistalBend * num2 + fingerBendOffset;
				HandInput.rightPinkyProximalBend = pinkyProximalBend * num2 + fingerBendOffset;
				HandInput.rightPinkyProximalSpread = pinkyProximalSpread * num2 + fingerSpreadOffset;
				HandInput.rightPinkyProximalTwist = pinkyProximalTwist * num2;
				HandInput.rightPinkyMiddleBend = pinkyMiddleBend * num2 + fingerBendOffset;
				HandInput.rightPinkyDistalBend = pinkyDistalBend * num2 + fingerBendOffset;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			skeleton = GetComponent<SteamVR_Behaviour_Skeleton>();
		}
	}
}
