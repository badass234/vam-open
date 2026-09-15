using UnityEngine;

namespace Obi
{
	public class ObiBoxShapeTracker : ObiShapeTracker
	{
		private Vector3 size;

		private Vector3 center;

		public ObiBoxShapeTracker(BoxCollider collider)
		{
			base.collider = collider;
			adaptor.is2D = false;
			oniShape = Oni.CreateShape(Oni.ShapeType.Box);
		}

		public override void UpdateIfNeeded()
		{
			BoxCollider boxCollider = collider as BoxCollider;
			if (boxCollider != null && (boxCollider.size != size || boxCollider.center != center))
			{
				size = boxCollider.size;
				center = boxCollider.center;
				adaptor.Set(center, size);
				Oni.UpdateShape(oniShape, ref adaptor);
			}
		}
	}
}
