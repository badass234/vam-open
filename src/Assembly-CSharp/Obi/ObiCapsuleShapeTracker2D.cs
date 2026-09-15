using UnityEngine;

namespace Obi
{
	public class ObiCapsuleShapeTracker2D : ObiShapeTracker
	{
		private CapsuleDirection2D direction;

		private Vector2 size;

		private Vector2 center;

		public ObiCapsuleShapeTracker2D(CapsuleCollider2D collider)
		{
			base.collider = collider;
			adaptor.is2D = true;
			oniShape = Oni.CreateShape(Oni.ShapeType.Capsule);
		}

		public override void UpdateIfNeeded()
		{
			CapsuleCollider2D capsuleCollider2D = collider as CapsuleCollider2D;
			if (capsuleCollider2D != null && (capsuleCollider2D.size != size || capsuleCollider2D.direction != direction || capsuleCollider2D.offset != center))
			{
				size = capsuleCollider2D.size;
				direction = capsuleCollider2D.direction;
				center = capsuleCollider2D.offset;
				adaptor.Set(center, ((capsuleCollider2D.direction != CapsuleDirection2D.Horizontal) ? size.x : size.y) * 0.5f, Mathf.Max(size.x, size.y), (capsuleCollider2D.direction != CapsuleDirection2D.Horizontal) ? 1 : 0);
				Oni.UpdateShape(oniShape, ref adaptor);
			}
		}
	}
}
