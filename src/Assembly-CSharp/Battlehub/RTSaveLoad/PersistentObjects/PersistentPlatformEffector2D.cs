using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentPlatformEffector2D : PersistentEffector2D
	{
		public bool useOneWay;

		public bool useOneWayGrouping;

		public bool useSideFriction;

		public bool useSideBounce;

		public float surfaceArc;

		public float sideArc;

		public float rotationalOffset;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			PlatformEffector2D platformEffector2D = (PlatformEffector2D)obj;
			platformEffector2D.useOneWay = useOneWay;
			platformEffector2D.useOneWayGrouping = useOneWayGrouping;
			platformEffector2D.useSideFriction = useSideFriction;
			platformEffector2D.useSideBounce = useSideBounce;
			platformEffector2D.surfaceArc = surfaceArc;
			platformEffector2D.sideArc = sideArc;
			platformEffector2D.rotationalOffset = rotationalOffset;
			return platformEffector2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				PlatformEffector2D platformEffector2D = (PlatformEffector2D)obj;
				useOneWay = platformEffector2D.useOneWay;
				useOneWayGrouping = platformEffector2D.useOneWayGrouping;
				useSideFriction = platformEffector2D.useSideFriction;
				useSideBounce = platformEffector2D.useSideBounce;
				surfaceArc = platformEffector2D.surfaceArc;
				sideArc = platformEffector2D.sideArc;
				rotationalOffset = platformEffector2D.rotationalOffset;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
