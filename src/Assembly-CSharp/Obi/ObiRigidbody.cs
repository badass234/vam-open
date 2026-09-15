using System;
using UnityEngine;

namespace Obi
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Rigidbody))]
	public class ObiRigidbody : MonoBehaviour
	{
		public bool kinematicForParticles;

		private IntPtr oniRigidbody = IntPtr.Zero;

		private Rigidbody unityRigidbody;

		private bool dirty = true;

		private Oni.Rigidbody adaptor = default(Oni.Rigidbody);

		private Oni.RigidbodyVelocities oniVelocities = default(Oni.RigidbodyVelocities);

		private Vector3 velocity;

		private Vector3 angularVelocity;

		public IntPtr OniRigidbody => oniRigidbody;

		public void Awake()
		{
			unityRigidbody = GetComponent<Rigidbody>();
			oniRigidbody = Oni.CreateRigidbody();
			UpdateIfNeeded();
		}

		public void OnDestroy()
		{
			Oni.DestroyRigidbody(oniRigidbody);
			oniRigidbody = IntPtr.Zero;
		}

		public void UpdateIfNeeded()
		{
			if (dirty)
			{
				velocity = unityRigidbody.velocity;
				angularVelocity = unityRigidbody.angularVelocity;
				adaptor.Set(unityRigidbody, kinematicForParticles);
				Oni.UpdateRigidbody(oniRigidbody, ref adaptor);
				dirty = false;
			}
		}

		public void UpdateVelocities()
		{
			if (!dirty)
			{
				if (unityRigidbody.isKinematic || !kinematicForParticles)
				{
					Oni.GetRigidbodyVelocity(oniRigidbody, ref oniVelocities);
					unityRigidbody.velocity += oniVelocities.linearVelocity - velocity;
					unityRigidbody.angularVelocity += oniVelocities.angularVelocity - angularVelocity;
				}
				dirty = true;
			}
		}
	}
}
