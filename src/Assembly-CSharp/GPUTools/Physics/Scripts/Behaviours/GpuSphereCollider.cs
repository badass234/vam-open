using UnityEngine;

namespace GPUTools.Physics.Scripts.Behaviours
{
	[ExecuteInEditMode]
	public class GpuSphereCollider : MonoBehaviour
	{
		public SphereCollider sphereCollider;

		public float oversizeRadius;

		public float friction = 1f;

		public Vector3 center
		{
			get
			{
				if (sphereCollider != null)
				{
					return sphereCollider.center;
				}
				return Vector3.zero;
			}
			set
			{
				if (sphereCollider != null && sphereCollider.center != value)
				{
					sphereCollider.center = value;
				}
			}
		}

		public Vector3 worldCenter
		{
			get
			{
				return base.transform.TransformPoint(center);
			}
			set
			{
				center = base.transform.InverseTransformPoint(value);
			}
		}

		public float radius
		{
			get
			{
				if (sphereCollider != null)
				{
					return sphereCollider.radius + oversizeRadius;
				}
				return 0f;
			}
			set
			{
				if (sphereCollider != null && sphereCollider.radius != value)
				{
					sphereCollider.radius = value;
				}
			}
		}

		public float worldRadius => radius * base.transform.lossyScale.x;

		private float Scale => Mathf.Max(Mathf.Max(base.transform.lossyScale.x, base.transform.lossyScale.y), base.transform.lossyScale.z);

		private void OnEnable()
		{
			if (sphereCollider == null)
			{
				sphereCollider = GetComponent<SphereCollider>();
			}
			if (Application.isPlaying)
			{
				GPUCollidersManager.RegisterSphereCollider(this);
			}
		}

		private void OnDisable()
		{
			if (Application.isPlaying)
			{
				GPUCollidersManager.DeregisterSphereCollider(this);
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(worldCenter, worldRadius);
		}
	}
}
