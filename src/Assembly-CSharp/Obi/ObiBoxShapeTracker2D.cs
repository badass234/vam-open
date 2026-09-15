using UnityEngine;

namespace Obi
{
	public class ObiBoxShapeTracker2D : ObiShapeTracker
	{
		private Vector2 size;

		private Vector2 center;

		public ObiBoxShapeTracker2D(BoxCollider2D collider)
		{
			base.collider = collider;
			adaptor.is2D = true;
			oniShape = Oni.CreateShape(Oni.ShapeType.Box);
		}

		public override void UpdateIfNeeded()
		{
			BoxCollider2D boxCollider2D = collider as BoxCollider2D;
			if (boxCollider2D != null && (boxCollider2D.size != size || boxCollider2D.offset != center))
			{
				size = boxCollider2D.size;
				center = boxCollider2D.offset;
				adaptor.Set(center, size);
				Oni.UpdateShape(oniShape, ref adaptor);
			}
		}
	}
}
