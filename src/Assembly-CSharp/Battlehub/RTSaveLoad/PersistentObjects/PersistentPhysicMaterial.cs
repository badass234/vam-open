using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentPhysicMaterial : PersistentObject
	{
		public float dynamicFriction;

		public float staticFriction;

		public float bounciness;

		public uint frictionCombine;

		public uint bounceCombine;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			PhysicMaterial physicMaterial = (PhysicMaterial)obj;
			physicMaterial.dynamicFriction = dynamicFriction;
			physicMaterial.staticFriction = staticFriction;
			physicMaterial.bounciness = bounciness;
			physicMaterial.frictionCombine = (PhysicMaterialCombine)frictionCombine;
			physicMaterial.bounceCombine = (PhysicMaterialCombine)bounceCombine;
			return physicMaterial;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				PhysicMaterial physicMaterial = (PhysicMaterial)obj;
				dynamicFriction = physicMaterial.dynamicFriction;
				staticFriction = physicMaterial.staticFriction;
				bounciness = physicMaterial.bounciness;
				frictionCombine = (uint)physicMaterial.frictionCombine;
				bounceCombine = (uint)physicMaterial.bounceCombine;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
