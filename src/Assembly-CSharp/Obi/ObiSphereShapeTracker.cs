using UnityEngine;

namespace Obi
{
	public class ObiSphereShapeTracker : ObiShapeTracker
	{
		private float radius;

		private Vector3 center;

		public ObiSphereShapeTracker(SphereCollider collider)
		{
			base.collider = collider;
			adaptor.is2D = false;
			oniShape = Oni.CreateShape(Oni.ShapeType.Sphere);
		}

		public override void UpdateIfNeeded()
		{
			SphereCollider sphereCollider = collider as SphereCollider;
			if (sphereCollider != null && (sphereCollider.radius != radius || sphereCollider.center != center))
			{
				radius = sphereCollider.radius;
				center = sphereCollider.center;
				adaptor.Set(center, radius);
				Oni.UpdateShape(oniShape, ref adaptor);
			}
		}
	}
}
