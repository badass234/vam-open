using UnityEngine;

namespace Obi
{
	public class ObiCapsuleShapeTracker : ObiShapeTracker
	{
		private int direction;

		private float radius;

		private float height;

		private Vector3 center;

		public ObiCapsuleShapeTracker(CapsuleCollider collider)
		{
			base.collider = collider;
			adaptor.is2D = false;
			oniShape = Oni.CreateShape(Oni.ShapeType.Capsule);
		}

		public ObiCapsuleShapeTracker(CharacterController collider)
		{
			base.collider = collider;
			adaptor.is2D = false;
			oniShape = Oni.CreateShape(Oni.ShapeType.Capsule);
		}

		public override void UpdateIfNeeded()
		{
			CapsuleCollider capsuleCollider = collider as CapsuleCollider;
			if (capsuleCollider != null && (capsuleCollider.radius != radius || capsuleCollider.height != height || capsuleCollider.direction != direction || capsuleCollider.center != center))
			{
				radius = capsuleCollider.radius;
				height = capsuleCollider.height;
				direction = capsuleCollider.direction;
				center = capsuleCollider.center;
				adaptor.Set(center, radius, height, direction);
				Oni.UpdateShape(oniShape, ref adaptor);
			}
			CharacterController characterController = collider as CharacterController;
			if (characterController != null && (characterController.radius != radius || characterController.height != height || characterController.center != center))
			{
				radius = characterController.radius;
				height = characterController.height;
				center = characterController.center;
				adaptor.Set(center, radius, height, 1);
				Oni.UpdateShape(oniShape, ref adaptor);
			}
		}
	}
}
