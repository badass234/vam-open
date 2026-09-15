using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentWheelJoint2D : PersistentAnchoredJoint2D
	{
		public JointSuspension2D suspension;

		public bool useMotor;

		public JointMotor2D motor;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			WheelJoint2D wheelJoint2D = (WheelJoint2D)obj;
			wheelJoint2D.suspension = suspension;
			wheelJoint2D.useMotor = useMotor;
			wheelJoint2D.motor = motor;
			return wheelJoint2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				WheelJoint2D wheelJoint2D = (WheelJoint2D)obj;
				suspension = wheelJoint2D.suspension;
				useMotor = wheelJoint2D.useMotor;
				motor = wheelJoint2D.motor;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
