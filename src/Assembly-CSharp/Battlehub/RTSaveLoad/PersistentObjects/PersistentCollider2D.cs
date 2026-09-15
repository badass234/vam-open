using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1087, typeof(PersistentCircleCollider2D))]
	[ProtoInclude(1088, typeof(PersistentBoxCollider2D))]
	[ProtoInclude(1089, typeof(PersistentEdgeCollider2D))]
	[ProtoInclude(1090, typeof(PersistentCapsuleCollider2D))]
	[ProtoInclude(1091, typeof(PersistentCompositeCollider2D))]
	[ProtoInclude(1092, typeof(PersistentPolygonCollider2D))]
	public class PersistentCollider2D : PersistentBehaviour
	{
		public float density;

		public bool isTrigger;

		public bool usedByEffector;

		public bool usedByComposite;

		public Vector2 offset;

		public long sharedMaterial;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Collider2D collider2D = (Collider2D)obj;
			collider2D.density = density;
			collider2D.isTrigger = isTrigger;
			collider2D.usedByEffector = usedByEffector;
			collider2D.usedByComposite = usedByComposite;
			collider2D.offset = offset;
			collider2D.sharedMaterial = (PhysicsMaterial2D)objects.Get(sharedMaterial);
			return collider2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Collider2D collider2D = (Collider2D)obj;
				density = collider2D.density;
				isTrigger = collider2D.isTrigger;
				usedByEffector = collider2D.usedByEffector;
				usedByComposite = collider2D.usedByComposite;
				offset = collider2D.offset;
				sharedMaterial = collider2D.sharedMaterial.GetMappedInstanceID();
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
				Collider2D collider2D = (Collider2D)obj;
				AddDependency(collider2D.sharedMaterial, dependencies);
			}
		}
	}
}
