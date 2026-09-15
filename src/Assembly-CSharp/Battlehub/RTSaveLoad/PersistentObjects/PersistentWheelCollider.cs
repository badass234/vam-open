using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentWheelCollider : PersistentCollider
	{
		public Vector3 center;

		public float radius;

		public float suspensionDistance;

		public JointSpring suspensionSpring;

		public float forceAppPointDistance;

		public float mass;

		public float wheelDampingRate;

		public WheelFrictionCurve forwardFriction;

		public WheelFrictionCurve sidewaysFriction;

		public float motorTorque;

		public float brakeTorque;

		public float steerAngle;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			WheelCollider wheelCollider = (WheelCollider)obj;
			wheelCollider.center = center;
			wheelCollider.radius = radius;
			wheelCollider.suspensionDistance = suspensionDistance;
			wheelCollider.suspensionSpring = suspensionSpring;
			wheelCollider.forceAppPointDistance = forceAppPointDistance;
			wheelCollider.mass = mass;
			wheelCollider.wheelDampingRate = wheelDampingRate;
			wheelCollider.forwardFriction = forwardFriction;
			wheelCollider.sidewaysFriction = sidewaysFriction;
			wheelCollider.motorTorque = motorTorque;
			wheelCollider.brakeTorque = brakeTorque;
			wheelCollider.steerAngle = steerAngle;
			return wheelCollider;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				WheelCollider wheelCollider = (WheelCollider)obj;
				center = wheelCollider.center;
				radius = wheelCollider.radius;
				suspensionDistance = wheelCollider.suspensionDistance;
				suspensionSpring = wheelCollider.suspensionSpring;
				forceAppPointDistance = wheelCollider.forceAppPointDistance;
				mass = wheelCollider.mass;
				wheelDampingRate = wheelCollider.wheelDampingRate;
				forwardFriction = wheelCollider.forwardFriction;
				sidewaysFriction = wheelCollider.sidewaysFriction;
				motorTorque = wheelCollider.motorTorque;
				brakeTorque = wheelCollider.brakeTorque;
				steerAngle = wheelCollider.steerAngle;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
