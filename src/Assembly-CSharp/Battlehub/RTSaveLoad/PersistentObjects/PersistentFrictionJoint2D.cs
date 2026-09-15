using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentFrictionJoint2D : PersistentAnchoredJoint2D
	{
		public float maxForce;

		public float maxTorque;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			FrictionJoint2D frictionJoint2D = (FrictionJoint2D)obj;
			frictionJoint2D.maxForce = maxForce;
			frictionJoint2D.maxTorque = maxTorque;
			return frictionJoint2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				FrictionJoint2D frictionJoint2D = (FrictionJoint2D)obj;
				maxForce = frictionJoint2D.maxForce;
				maxTorque = frictionJoint2D.maxTorque;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
