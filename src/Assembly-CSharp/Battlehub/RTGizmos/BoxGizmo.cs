using UnityEngine;

namespace Battlehub.RTGizmos
{
	public abstract class BoxGizmo : BaseGizmo
	{
		protected abstract Bounds Bounds { get; set; }

		protected override Matrix4x4 HandlesTransform => Matrix4x4.TRS(Target.TransformPoint(Bounds.center), Target.rotation, Vector3.Scale(Bounds.extents, Target.localScale));

		protected override bool OnDrag(int index, Vector3 offset)
		{
			Bounds bounds = Bounds;
			bounds.center += offset / 2f;
			bounds.extents += Vector3.Scale(offset / 2f, HandlesPositions[index]);
			Bounds = bounds;
			return true;
		}

		protected override void DrawOverride()
		{
			base.DrawOverride();
			Bounds bounds = Bounds;
			Vector3 vector = Vector3.Scale(bounds.extents, Target.localScale);
			RuntimeGizmos.DrawCubeHandles(Target.TransformPoint(bounds.center), Target.rotation, vector, HandlesColor);
			RuntimeGizmos.DrawWireCubeGL(ref bounds, Target.TransformPoint(bounds.center), Target.rotation, Target.localScale, LineColor);
			if (base.IsDragging)
			{
				RuntimeGizmos.DrawSelection(Target.TransformPoint(bounds.center + Vector3.Scale(HandlesPositions[base.DragIndex], vector)), Target.rotation, vector, SelectionColor);
			}
		}
	}
}
