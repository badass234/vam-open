using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentAreaEffector2D : PersistentEffector2D
	{
		public float forceAngle;

		public bool useGlobalAngle;

		public float forceMagnitude;

		public float forceVariation;

		public float drag;

		public float angularDrag;

		public uint forceTarget;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			AreaEffector2D areaEffector2D = (AreaEffector2D)obj;
			areaEffector2D.forceAngle = forceAngle;
			areaEffector2D.useGlobalAngle = useGlobalAngle;
			areaEffector2D.forceMagnitude = forceMagnitude;
			areaEffector2D.forceVariation = forceVariation;
			areaEffector2D.drag = drag;
			areaEffector2D.angularDrag = angularDrag;
			areaEffector2D.forceTarget = (EffectorSelection2D)forceTarget;
			return areaEffector2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				AreaEffector2D areaEffector2D = (AreaEffector2D)obj;
				forceAngle = areaEffector2D.forceAngle;
				useGlobalAngle = areaEffector2D.useGlobalAngle;
				forceMagnitude = areaEffector2D.forceMagnitude;
				forceVariation = areaEffector2D.forceVariation;
				drag = areaEffector2D.drag;
				angularDrag = areaEffector2D.angularDrag;
				forceTarget = (uint)areaEffector2D.forceTarget;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
