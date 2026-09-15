using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentHingeJoint : PersistentJoint
	{
		public JointMotor motor;

		public JointLimits limits;

		public JointSpring spring;

		public bool useMotor;

		public bool useLimits;

		public bool useSpring;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			HingeJoint hingeJoint = (HingeJoint)obj;
			hingeJoint.motor = motor;
			hingeJoint.limits = limits;
			hingeJoint.spring = spring;
			hingeJoint.useMotor = useMotor;
			hingeJoint.useLimits = useLimits;
			hingeJoint.useSpring = useSpring;
			return hingeJoint;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				HingeJoint hingeJoint = (HingeJoint)obj;
				motor = hingeJoint.motor;
				limits = hingeJoint.limits;
				spring = hingeJoint.spring;
				useMotor = hingeJoint.useMotor;
				useLimits = hingeJoint.useLimits;
				useSpring = hingeJoint.useSpring;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
