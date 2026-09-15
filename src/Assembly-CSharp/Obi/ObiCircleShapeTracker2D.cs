using UnityEngine;

namespace Obi
{
	public class ObiCircleShapeTracker2D : ObiShapeTracker
	{
		private float radius;

		private Vector2 center;

		public ObiCircleShapeTracker2D(CircleCollider2D collider)
		{
			base.collider = collider;
			adaptor.is2D = true;
			oniShape = Oni.CreateShape(Oni.ShapeType.Sphere);
		}

		public override void UpdateIfNeeded()
		{
			CircleCollider2D circleCollider2D = collider as CircleCollider2D;
			if (circleCollider2D != null && (circleCollider2D.radius != radius || circleCollider2D.offset != center))
			{
				radius = circleCollider2D.radius;
				center = circleCollider2D.offset;
				adaptor.Set(center, radius);
				Oni.UpdateShape(oniShape, ref adaptor);
			}
		}
	}
}
