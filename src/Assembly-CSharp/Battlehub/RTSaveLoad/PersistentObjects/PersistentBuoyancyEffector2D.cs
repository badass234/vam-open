using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentBuoyancyEffector2D : PersistentEffector2D
	{
		public float surfaceLevel;

		public float density;

		public float linearDrag;

		public float angularDrag;

		public float flowAngle;

		public float flowMagnitude;

		public float flowVariation;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			BuoyancyEffector2D buoyancyEffector2D = (BuoyancyEffector2D)obj;
			buoyancyEffector2D.surfaceLevel = surfaceLevel;
			buoyancyEffector2D.density = density;
			buoyancyEffector2D.linearDrag = linearDrag;
			buoyancyEffector2D.angularDrag = angularDrag;
			buoyancyEffector2D.flowAngle = flowAngle;
			buoyancyEffector2D.flowMagnitude = flowMagnitude;
			buoyancyEffector2D.flowVariation = flowVariation;
			return buoyancyEffector2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				BuoyancyEffector2D buoyancyEffector2D = (BuoyancyEffector2D)obj;
				surfaceLevel = buoyancyEffector2D.surfaceLevel;
				density = buoyancyEffector2D.density;
				linearDrag = buoyancyEffector2D.linearDrag;
				angularDrag = buoyancyEffector2D.angularDrag;
				flowAngle = buoyancyEffector2D.flowAngle;
				flowMagnitude = buoyancyEffector2D.flowMagnitude;
				flowVariation = buoyancyEffector2D.flowVariation;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
