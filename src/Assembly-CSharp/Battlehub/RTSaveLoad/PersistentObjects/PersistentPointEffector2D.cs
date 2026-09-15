using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentPointEffector2D : PersistentEffector2D
	{
		public float forceMagnitude;

		public float forceVariation;

		public float distanceScale;

		public float drag;

		public float angularDrag;

		public uint forceSource;

		public uint forceTarget;

		public uint forceMode;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			PointEffector2D pointEffector2D = (PointEffector2D)obj;
			pointEffector2D.forceMagnitude = forceMagnitude;
			pointEffector2D.forceVariation = forceVariation;
			pointEffector2D.distanceScale = distanceScale;
			pointEffector2D.drag = drag;
			pointEffector2D.angularDrag = angularDrag;
			pointEffector2D.forceSource = (EffectorSelection2D)forceSource;
			pointEffector2D.forceTarget = (EffectorSelection2D)forceTarget;
			pointEffector2D.forceMode = (EffectorForceMode2D)forceMode;
			return pointEffector2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				PointEffector2D pointEffector2D = (PointEffector2D)obj;
				forceMagnitude = pointEffector2D.forceMagnitude;
				forceVariation = pointEffector2D.forceVariation;
				distanceScale = pointEffector2D.distanceScale;
				drag = pointEffector2D.drag;
				angularDrag = pointEffector2D.angularDrag;
				forceSource = (uint)pointEffector2D.forceSource;
				forceTarget = (uint)pointEffector2D.forceTarget;
				forceMode = (uint)pointEffector2D.forceMode;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
