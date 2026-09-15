using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentRigidbody2D : PersistentComponent
	{
		public Vector2 position;

		public float rotation;

		public Vector2 velocity;

		public float angularVelocity;

		public bool useAutoMass;

		public float mass;

		public long sharedMaterial;

		public Vector2 centerOfMass;

		public float inertia;

		public float drag;

		public float angularDrag;

		public float gravityScale;

		public uint bodyType;

		public bool useFullKinematicContacts;

		public bool isKinematic;

		public bool freezeRotation;

		public uint constraints;

		public bool simulated;

		public uint interpolation;

		public uint sleepMode;

		public uint collisionDetectionMode;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Rigidbody2D rigidbody2D = (Rigidbody2D)obj;
			rigidbody2D.position = position;
			rigidbody2D.rotation = rotation;
			rigidbody2D.velocity = velocity;
			rigidbody2D.angularVelocity = angularVelocity;
			rigidbody2D.useAutoMass = useAutoMass;
			rigidbody2D.mass = mass;
			rigidbody2D.sharedMaterial = (PhysicsMaterial2D)objects.Get(sharedMaterial);
			rigidbody2D.centerOfMass = centerOfMass;
			rigidbody2D.inertia = inertia;
			rigidbody2D.drag = drag;
			rigidbody2D.angularDrag = angularDrag;
			rigidbody2D.gravityScale = gravityScale;
			rigidbody2D.bodyType = (RigidbodyType2D)bodyType;
			rigidbody2D.useFullKinematicContacts = useFullKinematicContacts;
			rigidbody2D.isKinematic = isKinematic;
			rigidbody2D.freezeRotation = freezeRotation;
			rigidbody2D.constraints = (RigidbodyConstraints2D)constraints;
			rigidbody2D.simulated = simulated;
			rigidbody2D.interpolation = (RigidbodyInterpolation2D)interpolation;
			rigidbody2D.sleepMode = (RigidbodySleepMode2D)sleepMode;
			rigidbody2D.collisionDetectionMode = (CollisionDetectionMode2D)collisionDetectionMode;
			return rigidbody2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Rigidbody2D rigidbody2D = (Rigidbody2D)obj;
				position = rigidbody2D.position;
				rotation = rigidbody2D.rotation;
				velocity = rigidbody2D.velocity;
				angularVelocity = rigidbody2D.angularVelocity;
				useAutoMass = rigidbody2D.useAutoMass;
				mass = rigidbody2D.mass;
				sharedMaterial = rigidbody2D.sharedMaterial.GetMappedInstanceID();
				centerOfMass = rigidbody2D.centerOfMass;
				inertia = rigidbody2D.inertia;
				drag = rigidbody2D.drag;
				angularDrag = rigidbody2D.angularDrag;
				gravityScale = rigidbody2D.gravityScale;
				bodyType = (uint)rigidbody2D.bodyType;
				useFullKinematicContacts = rigidbody2D.useFullKinematicContacts;
				isKinematic = rigidbody2D.isKinematic;
				freezeRotation = rigidbody2D.freezeRotation;
				constraints = (uint)rigidbody2D.constraints;
				simulated = rigidbody2D.simulated;
				interpolation = (uint)rigidbody2D.interpolation;
				sleepMode = (uint)rigidbody2D.sleepMode;
				collisionDetectionMode = (uint)rigidbody2D.collisionDetectionMode;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(sharedMaterial, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				Rigidbody2D rigidbody2D = (Rigidbody2D)obj;
				AddDependency(rigidbody2D.sharedMaterial, dependencies);
			}
		}
	}
}
