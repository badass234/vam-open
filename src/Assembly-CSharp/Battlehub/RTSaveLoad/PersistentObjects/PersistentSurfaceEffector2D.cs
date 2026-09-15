using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentSurfaceEffector2D : PersistentEffector2D
	{
		public float speed;

		public float speedVariation;

		public float forceScale;

		public bool useContactForce;

		public bool useFriction;

		public bool useBounce;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			SurfaceEffector2D surfaceEffector2D = (SurfaceEffector2D)obj;
			surfaceEffector2D.speed = speed;
			surfaceEffector2D.speedVariation = speedVariation;
			surfaceEffector2D.forceScale = forceScale;
			surfaceEffector2D.useContactForce = useContactForce;
			surfaceEffector2D.useFriction = useFriction;
			surfaceEffector2D.useBounce = useBounce;
			return surfaceEffector2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				SurfaceEffector2D surfaceEffector2D = (SurfaceEffector2D)obj;
				speed = surfaceEffector2D.speed;
				speedVariation = surfaceEffector2D.speedVariation;
				forceScale = surfaceEffector2D.forceScale;
				useContactForce = surfaceEffector2D.useContactForce;
				useFriction = surfaceEffector2D.useFriction;
				useBounce = surfaceEffector2D.useBounce;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
