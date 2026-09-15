using UnityEngine;

namespace MeshVR
{
	public class GpuProximityGrab : MonoBehaviour
	{
		public Transform measureTo;

		public float squareDistance = 0.0001f;

		public float currentSquareDistance;

		protected GpuGrabSphere grabSphere;

		private void Start()
		{
			grabSphere = GetComponent<GpuGrabSphere>();
		}

		private void Update()
		{
			if (measureTo != null)
			{
				currentSquareDistance = (measureTo.position - base.transform.position).sqrMagnitude;
				if (currentSquareDistance < squareDistance)
				{
					grabSphere.enabled = true;
				}
				else
				{
					grabSphere.enabled = false;
				}
			}
		}
	}
}
