using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentHingeJoint2D : PersistentAnchoredJoint2D
	{
		public bool useMotor;

		public bool useLimits;

		public JointMotor2D motor;

		public JointAngleLimits2D limits;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			HingeJoint2D hingeJoint2D = (HingeJoint2D)obj;
			hingeJoint2D.useMotor = useMotor;
			hingeJoint2D.useLimits = useLimits;
			hingeJoint2D.motor = motor;
			hingeJoint2D.limits = limits;
			return hingeJoint2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				HingeJoint2D hingeJoint2D = (HingeJoint2D)obj;
				useMotor = hingeJoint2D.useMotor;
				useLimits = hingeJoint2D.useLimits;
				motor = hingeJoint2D.motor;
				limits = hingeJoint2D.limits;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
