using UnityEngine;

namespace Leap.Unity
{
	public class SkeletalHand : HandModel
	{
		protected const float PALM_CENTER_OFFSET = 0.015f;

		public override ModelType HandModelType => ModelType.Graphics;

		private void Start()
		{
			Utils.IgnoreCollisions(base.gameObject, base.gameObject);
			for (int i = 0; i < fingers.Length; i++)
			{
				if (fingers[i] != null)
				{
					fingers[i].fingerType = (Finger.FingerType)i;
				}
			}
		}

		public override void UpdateHand()
		{
			SetPositions();
		}

		protected Vector3 GetPalmCenter()
		{
			Vector3 vector = 0.015f * Vector3.Scale(GetPalmDirection(), base.transform.lossyScale);
			return GetPalmPosition() - vector;
		}

		protected void SetPositions()
		{
			for (int i = 0; i < fingers.Length; i++)
			{
				if (fingers[i] != null)
				{
					fingers[i].UpdateFinger();
				}
			}
			if (palm != null)
			{
				palm.position = GetPalmCenter();
				palm.rotation = GetPalmRotation();
			}
			if (wristJoint != null)
			{
				wristJoint.position = GetWristPosition();
				wristJoint.rotation = GetPalmRotation();
			}
			if (forearm != null)
			{
				forearm.position = GetArmCenter();
				forearm.rotation = GetArmRotation();
			}
		}
	}
}
