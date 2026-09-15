using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentSpringJoint2D : PersistentAnchoredJoint2D
	{
		public bool autoConfigureDistance;

		public float distance;

		public float dampingRatio;

		public float frequency;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			SpringJoint2D springJoint2D = (SpringJoint2D)obj;
			springJoint2D.autoConfigureDistance = autoConfigureDistance;
			springJoint2D.distance = distance;
			springJoint2D.dampingRatio = dampingRatio;
			springJoint2D.frequency = frequency;
			return springJoint2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				SpringJoint2D springJoint2D = (SpringJoint2D)obj;
				autoConfigureDistance = springJoint2D.autoConfigureDistance;
				distance = springJoint2D.distance;
				dampingRatio = springJoint2D.dampingRatio;
				frequency = springJoint2D.frequency;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
