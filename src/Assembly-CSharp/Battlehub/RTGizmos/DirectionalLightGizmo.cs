using UnityEngine;

namespace Battlehub.RTGizmos
{
	public class DirectionalLightGizmo : BaseGizmo
	{
		protected override Matrix4x4 HandlesTransform => Target.localToWorldMatrix;

		protected override void DrawOverride()
		{
			base.DrawOverride();
			if (!(Target == null))
			{
				RuntimeGizmos.DrawDirectionalLight(Target.position, Target.rotation, Vector3.one, LineColor);
			}
		}

		private void Reset()
		{
			LineColor = new Color(1f, 1f, 0.5f, 0.5f);
			HandlesColor = new Color(1f, 1f, 0.35f, 0.95f);
			SelectionColor = new Color(1f, 1f, 0f, 1f);
		}
	}
}
