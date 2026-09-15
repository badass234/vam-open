using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1077, typeof(PersistentBoxCollider))]
	[ProtoInclude(1078, typeof(PersistentSphereCollider))]
	[ProtoInclude(1079, typeof(PersistentMeshCollider))]
	[ProtoInclude(1080, typeof(PersistentCapsuleCollider))]
	[ProtoInclude(1081, typeof(PersistentCharacterController))]
	[ProtoInclude(1082, typeof(PersistentWheelCollider))]
	[ProtoInclude(1083, typeof(PersistentTerrainCollider))]
	public class PersistentCollider : PersistentComponent
	{
		public bool enabled;

		public bool isTrigger;

		public float contactOffset;

		public long sharedMaterial;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Collider collider = (Collider)obj;
			collider.enabled = enabled;
			collider.isTrigger = isTrigger;
			collider.contactOffset = contactOffset;
			collider.sharedMaterial = (PhysicMaterial)objects.Get(sharedMaterial);
			return collider;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Collider collider = (Collider)obj;
				enabled = collider.enabled;
				isTrigger = collider.isTrigger;
				contactOffset = collider.contactOffset;
				sharedMaterial = collider.sharedMaterial.GetMappedInstanceID();
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
				Collider collider = (Collider)obj;
				AddDependency(collider.sharedMaterial, dependencies);
			}
		}
	}
}
