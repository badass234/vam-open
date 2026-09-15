using System;
using UnityEngine;

namespace Battlehub.RTGizmos
{
	public abstract class SphereGizmo : BaseGizmo
	{
		protected abstract Vector3 Center { get; set; }

		protected abstract float Radius { get; set; }

		protected override Matrix4x4 HandlesTransform => Matrix4x4.TRS(Target.TransformPoint(Center), Target.rotation, Target.localScale * Radius);

		protected override bool OnDrag(int index, Vector3 offset)
		{
			Radius += offset.magnitude * (float)Math.Sign(Vector3.Dot(offset, HandlesNormals[index]));
			if (Radius < 0f)
			{
				Radius = 0f;
				return false;
			}
			return true;
		}

		protected override void DrawOverride()
		{
			base.DrawOverride();
			if (!(Target == null))
			{
				Vector3 vector = Target.localScale * Radius;
				RuntimeGizmos.DrawCubeHandles(Target.TransformPoint(Center), Target.rotation, vector, HandlesColor);
				RuntimeGizmos.DrawWireSphereGL(Target.TransformPoint(Center), Target.rotation, vector, LineColor);
				if (base.IsDragging)
				{
					RuntimeGizmos.DrawSelection(Target.TransformPoint(Center + Vector3.Scale(HandlesPositions[base.DragIndex], vector)), Target.rotation, vector, SelectionColor);
				}
			}
		}
	}
}
