using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentDistanceJoint2D : PersistentAnchoredJoint2D
	{
		public bool autoConfigureDistance;

		public float distance;

		public bool maxDistanceOnly;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			DistanceJoint2D distanceJoint2D = (DistanceJoint2D)obj;
			distanceJoint2D.autoConfigureDistance = autoConfigureDistance;
			distanceJoint2D.distance = distance;
			distanceJoint2D.maxDistanceOnly = maxDistanceOnly;
			return distanceJoint2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				DistanceJoint2D distanceJoint2D = (DistanceJoint2D)obj;
				autoConfigureDistance = distanceJoint2D.autoConfigureDistance;
				distance = distanceJoint2D.distance;
				maxDistanceOnly = distanceJoint2D.maxDistanceOnly;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
