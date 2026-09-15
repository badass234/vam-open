using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentPhysicsMaterial2D : PersistentObject
	{
		public float bounciness;

		public float friction;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			PhysicsMaterial2D physicsMaterial2D = (PhysicsMaterial2D)obj;
			physicsMaterial2D.bounciness = bounciness;
			physicsMaterial2D.friction = friction;
			return physicsMaterial2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				PhysicsMaterial2D physicsMaterial2D = (PhysicsMaterial2D)obj;
				bounciness = physicsMaterial2D.bounciness;
				friction = physicsMaterial2D.friction;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
