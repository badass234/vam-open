using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentConstantForce : PersistentBehaviour
	{
		public Vector3 force;

		public Vector3 relativeForce;

		public Vector3 torque;

		public Vector3 relativeTorque;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ConstantForce constantForce = (ConstantForce)obj;
			constantForce.force = force;
			constantForce.relativeForce = relativeForce;
			constantForce.torque = torque;
			constantForce.relativeTorque = relativeTorque;
			return constantForce;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ConstantForce constantForce = (ConstantForce)obj;
				force = constantForce.force;
				relativeForce = constantForce.relativeForce;
				torque = constantForce.torque;
				relativeTorque = constantForce.relativeTorque;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
