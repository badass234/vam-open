using UnityEngine;

namespace Leap.Unity.RuntimeGizmos
{
	public class RuntimeColliderGizmos : MonoBehaviour, IRuntimeGizmoComponent
	{
		public Color color = Color.white;

		public bool useWireframe = true;

		public bool traverseHierarchy = true;

		public bool drawTriggers;

		private void Start()
		{
		}

		public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer)
		{
			if (base.gameObject.activeInHierarchy && base.enabled)
			{
				drawer.color = color;
				drawer.DrawColliders(base.gameObject, useWireframe, traverseHierarchy, drawTriggers);
			}
		}
	}
}
