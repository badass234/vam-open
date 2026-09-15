using UnityEngine;

namespace Leap.Unity
{
	public class RiggedFinger : FingerModel
	{
		[HideInInspector]
		public bool deformPosition;

		[HideInInspector]
		public bool scaleLastFingerBone;

		public Vector3 modelFingerPointing = Vector3.forward;

		public Vector3 modelPalmFacing = -Vector3.up;

		private static float[] s_standardFingertipLengths;

		static RiggedFinger()
		{
			s_standardFingertipLengths = new float[5];
			Hand hand = TestHandFactory.MakeTestHand(true, 0, 0, TestHandFactory.UnitType.UnityUnits);
			for (int i = 0; i < 5; i++)
			{
				Bone bone = hand.Fingers[i].bones[3];
				s_standardFingertipLengths[i] = bone.Length;
			}
		}

		public Quaternion Reorientation()
		{
			return Quaternion.Inverse(Quaternion.LookRotation(modelFingerPointing, -modelPalmFacing));
		}

		public override void UpdateFinger()
		{
			for (int i = 0; i < bones.Length; i++)
			{
				if (!(bones[i] != null))
				{
					continue;
				}
				bones[i].rotation = GetBoneRotation(i) * Reorientation();
				if (deformPosition)
				{
					Vector3 jointPosition = GetJointPosition(i);
					bones[i].position = jointPosition;
					if (i == 3 && scaleLastFingerBone)
					{
						Vector3 jointPosition2 = GetJointPosition(i + 1);
						float magnitude = (jointPosition2 - jointPosition).magnitude;
						float num = s_standardFingertipLengths[(int)fingerType];
						Vector3 localScale = bones[i].transform.localScale;
						int largestComponentIndex = getLargestComponentIndex(modelFingerPointing);
						localScale[largestComponentIndex] = magnitude / num;
						bones[i].transform.localScale = localScale;
					}
				}
			}
		}

		private int getLargestComponentIndex(Vector3 pointingVector)
		{
			float num = 0f;
			int result = 0;
			for (int i = 0; i < 3; i++)
			{
				float f = pointingVector[i];
				if (Mathf.Abs(f) > num)
				{
					result = i;
					num = Mathf.Abs(f);
				}
			}
			return result;
		}

		public void SetupRiggedFinger(bool useMetaCarpals)
		{
			findBoneTransforms(useMetaCarpals);
			modelFingerPointing = calulateModelFingerPointing();
		}

		private void findBoneTransforms(bool useMetaCarpals)
		{
			if (!useMetaCarpals || fingerType == Finger.FingerType.TYPE_THUMB)
			{
				bones[1] = base.transform;
				bones[2] = base.transform.GetChild(0).transform;
				bones[3] = base.transform.GetChild(0).transform.GetChild(0).transform;
			}
			else
			{
				bones[0] = base.transform;
				bones[1] = base.transform.GetChild(0).transform;
				bones[2] = base.transform.GetChild(0).transform.GetChild(0).transform;
				bones[3] = base.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).transform;
			}
		}

		private Vector3 calulateModelFingerPointing()
		{
			Vector3 vectorToZero = base.transform.InverseTransformPoint(base.transform.position) - base.transform.InverseTransformPoint(base.transform.GetChild(0).transform.position);
			return RiggedHand.CalculateZeroedVector(vectorToZero);
		}
	}
}
