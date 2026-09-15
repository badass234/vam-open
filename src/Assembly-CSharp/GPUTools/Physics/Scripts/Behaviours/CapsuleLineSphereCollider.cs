using UnityEngine;

namespace GPUTools.Physics.Scripts.Behaviours
{
	[ExecuteInEditMode]
	public class CapsuleLineSphereCollider : LineSphereCollider
	{
		public CapsuleCollider capsuleCollider;

		public float oversizeRadius;

		public float oversizeHeight;

		public float friction = 1f;

		public void UpdateData(float radius, float height, int direction, Vector3 center)
		{
			float num = radius + oversizeRadius;
			float num2 = (height + oversizeHeight) * 0.5f;
			if (num2 < num)
			{
				num2 = num;
			}
			RadiusA = num;
			RadiusB = num;
			float num3 = num2 - num;
			switch (direction)
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

		public void UpdateData()
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

		private void OnEnable()
		{
			if (capsuleCollider == null)
			{
				capsuleCollider = GetComponent<CapsuleCollider>();
			}
			if (Application.isPlaying)
			{
				GPUCollidersManager.RegisterLineSphereCollider(this);
			}
			UpdateData();
		}

		private void OnDisable()
		{
			if (Application.isPlaying)
			{
				GPUCollidersManager.DeregisterLineSphereCollider(this);
			}
		}
	}
}
