using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1101, typeof(PersistentAreaEffector2D))]
	[ProtoInclude(1102, typeof(PersistentPlatformEffector2D))]
	[ProtoInclude(1103, typeof(PersistentBuoyancyEffector2D))]
	[ProtoInclude(1104, typeof(PersistentPointEffector2D))]
	[ProtoInclude(1105, typeof(PersistentSurfaceEffector2D))]
	public class PersistentEffector2D : PersistentBehaviour
	{
		public bool useColliderMask;

		public int colliderMask;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Effector2D effector2D = (Effector2D)obj;
			effector2D.useColliderMask = useColliderMask;
			effector2D.colliderMask = colliderMask;
			return effector2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Effector2D effector2D = (Effector2D)obj;
				useColliderMask = effector2D.useColliderMask;
				colliderMask = effector2D.colliderMask;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
