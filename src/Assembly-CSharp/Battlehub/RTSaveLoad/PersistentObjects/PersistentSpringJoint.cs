using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentSpringJoint : PersistentJoint
	{
		public float spring;

		public float damper;

		public float minDistance;

		public float maxDistance;

		public float tolerance;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			SpringJoint springJoint = (SpringJoint)obj;
			springJoint.spring = spring;
			springJoint.damper = damper;
			springJoint.minDistance = minDistance;
			springJoint.maxDistance = maxDistance;
			springJoint.tolerance = tolerance;
			return springJoint;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				SpringJoint springJoint = (SpringJoint)obj;
				spring = springJoint.spring;
				damper = springJoint.damper;
				minDistance = springJoint.minDistance;
				maxDistance = springJoint.maxDistance;
				tolerance = springJoint.tolerance;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
