namespace Leap.Unity
{
	public class SkeletalFinger : FingerModel
	{
		public override void InitFinger()
		{
			SetPositions();
		}

		public override void UpdateFinger()
		{
			SetPositions();
		}

		protected void SetPositions()
		{
			for (int i = 0; i < bones.Length; i++)
			{
				if (bones[i] != null)
				{
					bones[i].transform.position = GetBoneCenter(i);
					bones[i].transform.rotation = GetBoneRotation(i);
				}
			}
			for (int j = 0; j < joints.Length; j++)
			{
				if (joints[j] != null)
				{
					joints[j].transform.position = GetJointPosition(j + 1);
					joints[j].transform.rotation = GetBoneRotation(j + 1);
				}
			}
		}
	}
}
