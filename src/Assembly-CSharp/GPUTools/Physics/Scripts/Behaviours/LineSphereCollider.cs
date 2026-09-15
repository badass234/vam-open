using System;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Behaviours
{
	public class LineSphereCollider : MonoBehaviour
	{
		[SerializeField]
		public Vector3 A = Vector3.zero;

		[SerializeField]
		public Vector3 B = new Vector3(0f, -0.2f, 0f);

		[SerializeField]
		public float RadiusA = 0.1f;

		[SerializeField]
		public float RadiusB = 0.1f;

		public Vector3 WorldA
		{
			get
			{
				return base.transform.TransformPoint(A);
			}
			set
			{
				A = base.transform.InverseTransformPoint(value);
			}
		}

		public Vector3 WorldB
		{
			get
			{
				return base.transform.TransformPoint(B);
			}
			set
			{
				B = base.transform.InverseTransformPoint(value);
			}
		}

		public float WorldRadiusA
		{
			get
			{
				return RadiusA * base.transform.lossyScale.x;
			}
			set
			{
				RadiusA = value / Scale;
			}
		}

		public float WorldRadiusB
		{
			get
			{
				return RadiusB * base.transform.lossyScale.x;
			}
			set
			{
				RadiusB = value / Scale;
			}
		}

		private float Scale => Mathf.Max(Mathf.Max(base.transform.lossyScale.x, base.transform.lossyScale.y), base.transform.lossyScale.z);

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(WorldA, WorldRadiusA);
			Gizmos.DrawWireSphere(WorldB, WorldRadiusB);
			if (!(WorldA != WorldB))
			{
				return;
			}
			Vector3 lhs = Vector3.Normalize(WorldA - WorldB);
			Vector3 normalized = Vector3.Cross(lhs, new Vector3(lhs.z, lhs.y, 0f - lhs.x)).normalized;
			float f = (float)Math.PI / 10f;
			float num = Mathf.Cos(f);
			float w = Mathf.Sin(f);
			Quaternion quaternion = new Quaternion(num * lhs.x, num * lhs.y, num * lhs.z, w);
			if (!(quaternion == Quaternion.identity))
			{
				Quaternion identity = Quaternion.identity;
				for (int i = 0; i < 5; i++)
				{
					identity *= quaternion;
					Matrix4x4 matrix4x = Matrix4x4.TRS(WorldA, identity, Vector3.one * WorldRadiusA);
					Matrix4x4 matrix4x2 = Matrix4x4.TRS(WorldB, identity, Vector3.one * WorldRadiusB);
					Vector3 from = matrix4x.MultiplyPoint3x4(normalized);
					Vector3 to = matrix4x2.MultiplyPoint3x4(normalized);
					Gizmos.DrawLine(from, to);
				}
			}
		}
	}
}
