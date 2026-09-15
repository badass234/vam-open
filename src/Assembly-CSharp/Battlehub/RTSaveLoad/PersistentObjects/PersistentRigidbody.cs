using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentRigidbody : PersistentComponent
	{
		public Vector3 velocity;

		public Vector3 angularVelocity;

		public float drag;

		public float angularDrag;

		public float mass;

		public bool useGravity;

		public float maxDepenetrationVelocity;

		public bool isKinematic;

		public bool freezeRotation;

		public uint constraints;

		public uint collisionDetectionMode;

		public Vector3 centerOfMass;

		public bool detectCollisions;

		public Vector3 position;

		public Quaternion rotation;

		public uint interpolation;

		public int solverIterations;

		public int solverVelocityIterations;

		public float sleepThreshold;

		public float maxAngularVelocity;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Rigidbody rigidbody = (Rigidbody)obj;
			rigidbody.velocity = velocity;
			rigidbody.angularVelocity = angularVelocity;
			rigidbody.drag = drag;
			rigidbody.angularDrag = angularDrag;
			rigidbody.mass = mass;
			rigidbody.useGravity = useGravity;
			rigidbody.maxDepenetrationVelocity = maxDepenetrationVelocity;
			rigidbody.isKinematic = isKinematic;
			rigidbody.freezeRotation = freezeRotation;
			rigidbody.constraints = (RigidbodyConstraints)constraints;
			rigidbody.collisionDetectionMode = (CollisionDetectionMode)collisionDetectionMode;
			rigidbody.centerOfMass = centerOfMass;
			rigidbody.detectCollisions = detectCollisions;
			rigidbody.position = position;
			rigidbody.rotation = rotation;
			rigidbody.interpolation = (RigidbodyInterpolation)interpolation;
			rigidbody.solverIterations = solverIterations;
			rigidbody.solverVelocityIterations = solverVelocityIterations;
			rigidbody.sleepThreshold = sleepThreshold;
			rigidbody.maxAngularVelocity = maxAngularVelocity;
			return rigidbody;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Rigidbody rigidbody = (Rigidbody)obj;
				velocity = rigidbody.velocity;
				angularVelocity = rigidbody.angularVelocity;
				drag = rigidbody.drag;
				angularDrag = rigidbody.angularDrag;
				mass = rigidbody.mass;
				useGravity = rigidbody.useGravity;
				maxDepenetrationVelocity = rigidbody.maxDepenetrationVelocity;
				isKinematic = rigidbody.isKinematic;
				freezeRotation = rigidbody.freezeRotation;
				constraints = (uint)rigidbody.constraints;
				collisionDetectionMode = (uint)rigidbody.collisionDetectionMode;
				centerOfMass = rigidbody.centerOfMass;
				detectCollisions = rigidbody.detectCollisions;
				position = rigidbody.position;
				rotation = rigidbody.rotation;
				interpolation = (uint)rigidbody.interpolation;
				solverIterations = rigidbody.solverIterations;
				solverVelocityIterations = rigidbody.solverVelocityIterations;
				sleepThreshold = rigidbody.sleepThreshold;
				maxAngularVelocity = rigidbody.maxAngularVelocity;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
