using UnityEngine;

namespace GPUTools.Physics.Scripts.Behaviours
{
	[ExecuteInEditMode]
	public class GpuEditCapsule : LineSphereCollider
	{
		public CapsuleCollider capsuleCollider;

		public float oversizeRadius;

		public float oversizeHeight;

		public float strength = 1f;

		public virtual void UpdateData()
		{
			if (capsuleCollider != null)
			{
				float num = capsuleCollider.radius + oversizeRadius;
				float num2 = (capsuleCollider.height + oversizeHeight) * 0.5f;
				if (num2 < num)
				{
					num2 = num;
				}
				RadiusA = num;
				RadiusB = num;
				Vector3 center = capsuleCollider.center;
				float num3 = num2 - num;
				switch (capsuleCollider.direction)
				{
				case 0:
					A.x = center.x + num3;
					A.y = center.y;
					A.z = center.z;
					B.x = center.x - num3;
					B.y = center.y;
					B.z = center.z;
					break;
				case 1:
					A.x = center.x;
					A.y = center.y + num3;
					A.z = center.z;
					B.x = center.x;
					B.y = center.y - num3;
					B.z = center.z;
					break;
				case 2:
					A.x = center.x;
					A.y = center.y;
					A.z = center.z + num3;
					B.x = center.x;
					B.y = center.y;
					B.z = center.z - num3;
					break;
				}
			}
		}
	}
}
