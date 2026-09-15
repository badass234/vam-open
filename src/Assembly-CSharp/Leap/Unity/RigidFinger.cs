using UnityEngine;

namespace Leap.Unity
{
	public class RigidFinger : SkeletalFinger
	{
		public float filtering = 0.5f;

		private void Start()
		{
			for (int i = 0; i < bones.Length; i++)
			{
				if (bones[i] != null)
				{
					bones[i].GetComponent<Rigidbody>().maxAngularVelocity = float.PositiveInfinity;
				}
			}
		}

		public override void UpdateFinger()
		{
			for (int i = 0; i < bones.Length; i++)
			{
				if (bones[i] != null)
				{
					CapsuleCollider component = bones[i].GetComponent<CapsuleCollider>();
					if (component != null)
					{
						component.direction = 2;
						bones[i].localScale = new Vector3(1f / base.transform.lossyScale.x, 1f / base.transform.lossyScale.y, 1f / base.transform.lossyScale.z);
						component.radius = GetBoneWidth(i) / 2f;
						component.height = GetBoneLength(i) + GetBoneWidth(i);
					}
					Rigidbody component2 = bones[i].GetComponent<Rigidbody>();
					if ((bool)component2)
					{
						component2.MovePosition(GetBoneCenter(i));
						component2.MoveRotation(GetBoneRotation(i));
					}
					else
					{
						bones[i].position = GetBoneCenter(i);
						bones[i].rotation = GetBoneRotation(i);
					}
				}
			}
		}
	}
}
