using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentFixedJoint2D : PersistentAnchoredJoint2D
	{
		public float dampingRatio;

		public float frequency;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			FixedJoint2D fixedJoint2D = (FixedJoint2D)obj;
			fixedJoint2D.dampingRatio = dampingRatio;
			fixedJoint2D.frequency = frequency;
			return fixedJoint2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				FixedJoint2D fixedJoint2D = (FixedJoint2D)obj;
				dampingRatio = fixedJoint2D.dampingRatio;
				frequency = fixedJoint2D.frequency;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
