using UnityEngine;

namespace GPUTools.Hair.Scripts.Geometry.Constrains
{
	public class HairJointArea : MonoBehaviour
	{
		[SerializeField]
		private float radius;

		public float Radius => radius;

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(base.transform.position, radius);
		}
	}
}
