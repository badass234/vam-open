using System;
using UnityEngine;

namespace Obi
{
	public class ObiCollisionMaterial : ScriptableObject
	{
		private IntPtr oniCollisionMaterial = IntPtr.Zero;

		private Oni.CollisionMaterial adaptor = default(Oni.CollisionMaterial);

		public float friction;

		public float stickiness;

		public float stickDistance;

		public Oni.MaterialCombineMode frictionCombine;

		public Oni.MaterialCombineMode stickinessCombine;

		public IntPtr OniCollisionMaterial => oniCollisionMaterial;

		public void OnEnable()
		{
			oniCollisionMaterial = Oni.CreateCollisionMaterial();
			OnValidate();
		}

		public void OnDisable()
		{
			Oni.DestroyCollisionMaterial(oniCollisionMaterial);
			oniCollisionMaterial = IntPtr.Zero;
		}

		public void OnValidate()
		{
			adaptor.friction = friction;
			adaptor.stickiness = stickiness;
			adaptor.stickDistance = stickDistance;
			adaptor.frictionCombine = frictionCombine;
			adaptor.stickinessCombine = stickinessCombine;
			Oni.UpdateCollisionMaterial(oniCollisionMaterial, ref adaptor);
		}
	}
}
