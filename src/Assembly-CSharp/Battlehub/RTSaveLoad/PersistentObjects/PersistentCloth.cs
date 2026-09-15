using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentCloth : PersistentComponent
	{
		public float sleepThreshold;

		public float bendingStiffness;

		public float stretchingStiffness;

		public float damping;

		public Vector3 externalAcceleration;

		public Vector3 randomAcceleration;

		public bool useGravity;

		public bool enabled;

		public float friction;

		public float collisionMassScale;

		public float useContinuousCollision;

		public float useVirtualParticles;

		public ClothSkinningCoefficient[] coefficients;

		public float worldVelocityScale;

		public float worldAccelerationScale;

		public bool solverFrequency;

		public long[] capsuleColliders;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Cloth cloth = (Cloth)obj;
			cloth.sleepThreshold = sleepThreshold;
			cloth.bendingStiffness = bendingStiffness;
			cloth.stretchingStiffness = stretchingStiffness;
			cloth.damping = damping;
			cloth.externalAcceleration = externalAcceleration;
			cloth.randomAcceleration = randomAcceleration;
			cloth.useGravity = useGravity;
			cloth.enabled = enabled;
			cloth.friction = friction;
			cloth.collisionMassScale = collisionMassScale;
			cloth.useVirtualParticles = useVirtualParticles;
			cloth.coefficients = coefficients;
			cloth.worldVelocityScale = worldVelocityScale;
			cloth.worldAccelerationScale = worldAccelerationScale;
			cloth.capsuleColliders = Resolve<CapsuleCollider, UnityEngine.Object>(capsuleColliders, objects);
			return cloth;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Cloth cloth = (Cloth)obj;
				sleepThreshold = cloth.sleepThreshold;
				bendingStiffness = cloth.bendingStiffness;
				stretchingStiffness = cloth.stretchingStiffness;
				damping = cloth.damping;
				externalAcceleration = cloth.externalAcceleration;
				randomAcceleration = cloth.randomAcceleration;
				useGravity = cloth.useGravity;
				enabled = cloth.enabled;
				friction = cloth.friction;
				collisionMassScale = cloth.collisionMassScale;
				useVirtualParticles = cloth.useVirtualParticles;
				coefficients = cloth.coefficients;
				worldVelocityScale = cloth.worldVelocityScale;
				worldAccelerationScale = cloth.worldAccelerationScale;
				capsuleColliders = cloth.capsuleColliders.GetMappedInstanceID();
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependencies(capsuleColliders, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				Cloth cloth = (Cloth)obj;
				AddDependencies(cloth.capsuleColliders, dependencies);
			}
		}
	}
}
