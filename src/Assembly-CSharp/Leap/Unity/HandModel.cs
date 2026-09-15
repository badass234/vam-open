using UnityEngine;

namespace Leap.Unity
{
	public abstract class HandModel : HandModelBase
	{
		[SerializeField]
		private Chirality handedness;

		private ModelType handModelType;

		public const int NUM_FINGERS = 5;

		public float handModelPalmWidth = 0.085f;

		public FingerModel[] fingers = new FingerModel[5];

		public Transform palm;

		public Transform forearm;

		public Transform wristJoint;

		public Transform elbowJoint;

		protected Hand hand_;

		public override Chirality Handedness
		{
			get
			{
				return handedness;
			}
			set
			{
				handedness = value;
			}
		}

		public abstract override ModelType HandModelType { get; }

		public Vector3 GetPalmPosition()
		{
			return hand_.PalmPosition.ToVector3();
		}

		public Quaternion GetPalmRotation()
		{
			if (hand_ != null)
			{
				return hand_.Basis.CalculateRotation();
			}
			if ((bool)palm)
			{
				return palm.rotation;
			}
			return Quaternion.identity;
		}

		public Vector3 GetPalmDirection()
		{
			if (hand_ != null)
			{
				return hand_.Direction.ToVector3();
			}
			if ((bool)palm)
			{
				return palm.forward;
			}
			return Vector3.forward;
		}

		public Vector3 GetPalmNormal()
		{
			if (hand_ != null)
			{
				return hand_.PalmNormal.ToVector3();
			}
			if ((bool)palm)
			{
				return -palm.up;
			}
			return -Vector3.up;
		}

		public Vector3 GetArmDirection()
		{
			if (hand_ != null)
			{
				return hand_.Arm.Direction.ToVector3();
			}
			if ((bool)forearm)
			{
				return forearm.forward;
			}
			return Vector3.forward;
		}

		public Vector3 GetArmCenter()
		{
			if (hand_ != null)
			{
				Vector vector = 0.5f * (hand_.Arm.WristPosition + hand_.Arm.ElbowPosition);
				return vector.ToVector3();
			}
			if ((bool)forearm)
			{
				return forearm.position;
			}
			return Vector3.zero;
		}

		public float GetArmLength()
		{
			return (hand_.Arm.WristPosition - hand_.Arm.ElbowPosition).Magnitude;
		}

		public float GetArmWidth()
		{
			return hand_.Arm.Width;
		}

		public Vector3 GetElbowPosition()
		{
			if (hand_ != null)
			{
				return hand_.Arm.ElbowPosition.ToVector3();
			}
			if ((bool)elbowJoint)
			{
				return elbowJoint.position;
			}
			return Vector3.zero;
		}

		public Vector3 GetWristPosition()
		{
			if (hand_ != null)
			{
				return hand_.Arm.WristPosition.ToVector3();
			}
			if ((bool)wristJoint)
			{
				return wristJoint.position;
			}
			return Vector3.zero;
		}

		public Quaternion GetArmRotation()
		{
			if (hand_ != null)
			{
				return hand_.Arm.Rotation.ToQuaternion();
			}
			if ((bool)forearm)
			{
				return forearm.rotation;
			}
			return Quaternion.identity;
		}

		public override Hand GetLeapHand()
		{
			return hand_;
		}

		public override void SetLeapHand(Hand hand)
		{
			hand_ = hand;
			for (int i = 0; i < fingers.Length; i++)
			{
				if (fingers[i] != null)
				{
					fingers[i].SetLeapHand(hand_);
				}
			}
		}

		public override void InitHand()
		{
			for (int i = 0; i < fingers.Length; i++)
			{
				if (fingers[i] != null)
				{
					fingers[i].fingerType = (Finger.FingerType)i;
					fingers[i].InitFinger();
				}
			}
		}

		public int LeapID()
		{
			if (hand_ != null)
			{
				return hand_.Id;
			}
			return -1;
		}

		public abstract override void UpdateHand();
	}
}
