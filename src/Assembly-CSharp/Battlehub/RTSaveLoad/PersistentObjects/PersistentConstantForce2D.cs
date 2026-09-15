using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentConstantForce2D : PersistentPhysicsUpdateBehaviour2D
	{
		public Vector2 force;

		public Vector2 relativeForce;

		public float torque;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ConstantForce2D constantForce2D = (ConstantForce2D)obj;
			constantForce2D.force = force;
			constantForce2D.relativeForce = relativeForce;
			constantForce2D.torque = torque;
			return constantForce2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ConstantForce2D constantForce2D = (ConstantForce2D)obj;
				force = constantForce2D.force;
				relativeForce = constantForce2D.relativeForce;
				torque = constantForce2D.torque;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
