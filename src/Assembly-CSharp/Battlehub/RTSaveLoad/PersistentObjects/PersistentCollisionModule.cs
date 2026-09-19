using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentCollisionModule : PersistentData
	{
		public bool enabled;

		public ParticleSystemCollisionType type;

		public ParticleSystemCollisionMode mode;

		public long[] planes;

		public PersistentMinMaxCurve dampen;

		public PersistentMinMaxCurve bounce;

		public PersistentMinMaxCurve lifetimeLoss;

		public float dampenMultiplier;

		public float bounceMultiplier;

		public float lifetimeLossMultiplier;

		public float minKillSpeed;

		public float maxKillSpeed;

		public LayerMask collidesWith;

		public bool enableDynamicColliders;

		public bool enableInteriorCollisions;

		public int maxCollisionShapes;

		public ParticleSystemCollisionQuality quality;

		public float voxelSize;

		public float radiusScale;

		public bool sendCollisionMessages;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.CollisionModule collisionModule = (ParticleSystem.CollisionModule)obj;
			collisionModule.enabled = enabled;
			collisionModule.type = type;
			collisionModule.mode = mode;
			collisionModule.dampen = Write(collisionModule.dampen, dampen, objects);
			collisionModule.bounce = Write(collisionModule.bounce, bounce, objects);
			collisionModule.lifetimeLoss = Write(collisionModule.lifetimeLoss, lifetimeLoss, objects);
			collisionModule.dampenMultiplier = dampenMultiplier;
			collisionModule.bounceMultiplier = bounceMultiplier;
			collisionModule.lifetimeLossMultiplier = lifetimeLossMultiplier;
			collisionModule.minKillSpeed = minKillSpeed;
			collisionModule.maxKillSpeed = maxKillSpeed;
			collisionModule.collidesWith = collidesWith;
			collisionModule.enableDynamicColliders = enableDynamicColliders;
			collisionModule.maxCollisionShapes = maxCollisionShapes;
			collisionModule.quality = quality;
			collisionModule.voxelSize = voxelSize;
			collisionModule.radiusScale = radiusScale;
			collisionModule.sendCollisionMessages = sendCollisionMessages;
			if (planes == null)
			{
				for (int i = 0; i < collisionModule.planeCount; i++)
				{
					collisionModule.SetPlane(i, null);
				}
			}
			else
			{
				for (int j = 0; j < Mathf.Min(collisionModule.planeCount, planes.Length); j++)
				{
					collisionModule.SetPlane(j, (Transform)objects.Get(planes[j]));
				}
			}
			return collisionModule;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.CollisionModule collisionModule = (ParticleSystem.CollisionModule)obj;
				enabled = collisionModule.enabled;
				type = collisionModule.type;
				mode = collisionModule.mode;
				dampen = Read(dampen, collisionModule.dampen);
				bounce = Read(bounce, collisionModule.bounce);
				lifetimeLoss = Read(lifetimeLoss, collisionModule.lifetimeLoss);
				dampenMultiplier = collisionModule.dampenMultiplier;
				bounceMultiplier = collisionModule.bounceMultiplier;
				lifetimeLossMultiplier = collisionModule.lifetimeLossMultiplier;
				minKillSpeed = collisionModule.minKillSpeed;
				maxKillSpeed = collisionModule.maxKillSpeed;
				collidesWith = collisionModule.collidesWith;
				enableDynamicColliders = collisionModule.enableDynamicColliders;
				maxCollisionShapes = collisionModule.maxCollisionShapes;
				quality = collisionModule.quality;
				voxelSize = collisionModule.voxelSize;
				radiusScale = collisionModule.radiusScale;
				sendCollisionMessages = collisionModule.sendCollisionMessages;
				if (collisionModule.planeCount > 20)
				{
					Debug.LogWarning("maxPlaneCount is expected to be 6 or at least <= 20");
				}
				planes = new long[collisionModule.planeCount];
				for (int i = 0; i < collisionModule.planeCount; i++)
				{
					planes[i] = collisionModule.GetPlane(i).GetMappedInstanceID();
				}
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependencies(planes, dependencies, objects, allowNulls);
			FindDependencies(dampen, dependencies, objects, allowNulls);
			FindDependencies(bounce, dependencies, objects, allowNulls);
			FindDependencies(lifetimeLoss, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				ParticleSystem.CollisionModule collisionModule = (ParticleSystem.CollisionModule)obj;
				UnityEngine.Object[] array = new UnityEngine.Object[collisionModule.planeCount];
				for (int i = 0; i < collisionModule.planeCount; i++)
				{
					array[i] = collisionModule.GetPlane(i);
				}
				AddDependencies(array, dependencies);
				GetDependencies(dampen, collisionModule.dampen, dependencies);
				GetDependencies(bounce, collisionModule.bounce, dependencies);
				GetDependencies(lifetimeLoss, collisionModule.lifetimeLoss, dependencies);
			}
		}
	}
}
