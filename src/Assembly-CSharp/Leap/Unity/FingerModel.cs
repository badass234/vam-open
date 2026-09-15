using UnityEngine;

namespace Leap.Unity
{
	public abstract class FingerModel : MonoBehaviour
	{
		public const int NUM_BONES = 4;

		public const int NUM_JOINTS = 3;

		public Finger.FingerType fingerType = Finger.FingerType.TYPE_INDEX;

		public Transform[] bones = new Transform[4];

		public Transform[] joints = new Transform[3];

		protected Hand hand_;

		protected Finger finger_;

		public void SetLeapHand(Hand hand)
		{
			hand_ = hand;
			if (hand_ != null)
			{
				finger_ = hand.Fingers[(int)fingerType];
			}
		}

		public Hand GetLeapHand()
		{
			return hand_;
		}

		public Finger GetLeapFinger()
		{
			return finger_;
		}

		public virtual void InitFinger()
		{
			UpdateFinger();
		}

		public abstract void UpdateFinger();

		public Vector3 GetTipPosition()
		{
			if (finger_ != null)
			{
				return finger_.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3();
			}
			if ((bool)bones[3] && (bool)joints[1])
			{
				return 2f * bones[3].position - joints[1].position;
			}
			return Vector3.zero;
		}

		public Vector3 GetJointPosition(int joint)
		{
			if (joint >= 4)
			{
				return GetTipPosition();
			}
			if (finger_ != null)
			{
				return finger_.Bone((Bone.BoneType)joint).PrevJoint.ToVector3();
			}
			if ((bool)joints[joint])
			{
				return joints[joint].position;
			}
			return Vector3.zero;
		}

		public Ray GetRay()
		{
			return new Ray(GetTipPosition(), GetBoneDirection(3));
		}

		public Vector3 GetBoneCenter(int bone_type)
		{
			if (finger_ != null)
			{
				Bone bone = finger_.Bone((Bone.BoneType)bone_type);
				return bone.Center.ToVector3();
			}
			if ((bool)bones[bone_type])
			{
				return bones[bone_type].position;
			}
			return Vector3.zero;
		}

		public Vector3 GetBoneDirection(int bone_type)
		{
			if (finger_ != null)
			{
				return (GetJointPosition(bone_type + 1) - GetJointPosition(bone_type)).normalized;
			}
			if ((bool)bones[bone_type])
			{
				return bones[bone_type].forward;
			}
			return Vector3.forward;
		}

		public Quaternion GetBoneRotation(int bone_type)
		{
			if (finger_ != null)
			{
				return finger_.Bone((Bone.BoneType)bone_type).Rotation.ToQuaternion();
			}
			if ((bool)bones[bone_type])
			{
				return bones[bone_type].rotation;
			}
			return Quaternion.identity;
		}

		public float GetBoneLength(int bone_type)
		{
			return finger_.Bone((Bone.BoneType)bone_type).Length;
		}

		public float GetBoneWidth(int bone_type)
		{
			return finger_.Bone((Bone.BoneType)bone_type).Width;
		}

		public float GetFingerJointStretchMecanim(int joint_type)
		{
			Quaternion quaternion = Quaternion.identity;
			if (finger_ != null)
			{
				quaternion = Quaternion.Inverse(finger_.Bone((Bone.BoneType)joint_type).Rotation.ToQuaternion()) * finger_.Bone((Bone.BoneType)(joint_type + 1)).Rotation.ToQuaternion();
			}
			else if ((bool)bones[joint_type] && (bool)bones[joint_type + 1])
			{
				quaternion = Quaternion.Inverse(GetBoneRotation(joint_type)) * GetBoneRotation(joint_type + 1);
			}
			float num = 0f - quaternion.eulerAngles.x;
			if (num <= -180f)
			{
				num += 360f;
			}
			return num;
		}

		public float GetFingerJointSpreadMecanim()
		{
			Quaternion quaternion = Quaternion.identity;
			if (finger_ != null)
			{
				quaternion = Quaternion.Inverse(finger_.Bone(Bone.BoneType.TYPE_METACARPAL).Rotation.ToQuaternion()) * finger_.Bone(Bone.BoneType.TYPE_PROXIMAL).Rotation.ToQuaternion();
			}
			else if ((bool)bones[0] && (bool)bones[1])
			{
				quaternion = Quaternion.Inverse(GetBoneRotation(0)) * GetBoneRotation(1);
			}
			float num = 0f;
			Finger.FingerType fingerType = this.fingerType;
			if (finger_ != null)
			{
				this.fingerType = finger_.Type;
			}
			if (fingerType == Finger.FingerType.TYPE_INDEX || fingerType == Finger.FingerType.TYPE_MIDDLE)
			{
				num = quaternion.eulerAngles.y;
				if (num > 180f)
				{
					num -= 360f;
				}
			}
			if (fingerType == Finger.FingerType.TYPE_THUMB || fingerType == Finger.FingerType.TYPE_RING || fingerType == Finger.FingerType.TYPE_PINKY)
			{
				num = 0f - quaternion.eulerAngles.y;
				if (num <= -180f)
				{
					num += 360f;
				}
			}
			return num;
		}
	}
}
