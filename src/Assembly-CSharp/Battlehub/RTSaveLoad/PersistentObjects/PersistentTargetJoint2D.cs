using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentTargetJoint2D : PersistentJoint2D
	{
		public Vector2 anchor;

		public Vector2 target;

		public bool autoConfigureTarget;

		public float maxForce;

		public float dampingRatio;

		public float frequency;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			TargetJoint2D targetJoint2D = (TargetJoint2D)obj;
			targetJoint2D.anchor = anchor;
			targetJoint2D.target = target;
			targetJoint2D.autoConfigureTarget = autoConfigureTarget;
			targetJoint2D.maxForce = maxForce;
			targetJoint2D.dampingRatio = dampingRatio;
			targetJoint2D.frequency = frequency;
			return targetJoint2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				TargetJoint2D targetJoint2D = (TargetJoint2D)obj;
				anchor = targetJoint2D.anchor;
				target = targetJoint2D.target;
				autoConfigureTarget = targetJoint2D.autoConfigureTarget;
				maxForce = targetJoint2D.maxForce;
				dampingRatio = targetJoint2D.dampingRatio;
				frequency = targetJoint2D.frequency;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
