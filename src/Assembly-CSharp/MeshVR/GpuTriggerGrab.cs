using UnityEngine;

namespace MeshVR
{
	public class GpuTriggerGrab : MonoBehaviour
	{
		public enum Side
		{
			Left,
			Right
		}

		public Side side;

		protected GpuGrabSphere grabSphere;

		private void Start()
		{
			grabSphere = GetComponent<GpuGrabSphere>();
		}

		private void Update()
		{
			if (!(SuperController.singleton != null))
			{
				return;
			}
			if (side == Side.Left)
			{
				if (SuperController.singleton.GetLeftGrab())
				{
					grabSphere.enabled = true;
				}
				if (SuperController.singleton.GetLeftGrabRelease())
				{
					grabSphere.enabled = false;
				}
			}
			else
			{
				if (SuperController.singleton.GetRightGrab())
				{
					grabSphere.enabled = true;
				}
				if (SuperController.singleton.GetRightGrabRelease())
				{
					grabSphere.enabled = false;
				}
			}
		}
	}
}
