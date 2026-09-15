using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentRotationBySpeedModule : PersistentData
	{
		public bool enabled;

		public PersistentMinMaxCurve x;

		public float xMultiplier;

		public PersistentMinMaxCurve y;

		public float yMultiplier;

		public PersistentMinMaxCurve z;

		public float zMultiplier;

		public bool separateAxes;

		public Vector2 range;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.RotationBySpeedModule rotationBySpeedModule = (ParticleSystem.RotationBySpeedModule)obj;
			rotationBySpeedModule.enabled = enabled;
			rotationBySpeedModule.x = Write(rotationBySpeedModule.x, x, objects);
			rotationBySpeedModule.xMultiplier = xMultiplier;
			rotationBySpeedModule.y = Write(rotationBySpeedModule.y, y, objects);
			rotationBySpeedModule.yMultiplier = yMultiplier;
			rotationBySpeedModule.z = Write(rotationBySpeedModule.z, z, objects);
			rotationBySpeedModule.zMultiplier = zMultiplier;
			rotationBySpeedModule.separateAxes = separateAxes;
			rotationBySpeedModule.range = range;
			return rotationBySpeedModule;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.RotationBySpeedModule rotationBySpeedModule = (ParticleSystem.RotationBySpeedModule)obj;
				enabled = rotationBySpeedModule.enabled;
				x = Read(x, rotationBySpeedModule.x);
				xMultiplier = rotationBySpeedModule.xMultiplier;
				y = Read(y, rotationBySpeedModule.y);
				yMultiplier = rotationBySpeedModule.yMultiplier;
				z = Read(z, rotationBySpeedModule.z);
				zMultiplier = rotationBySpeedModule.zMultiplier;
				separateAxes = rotationBySpeedModule.separateAxes;
				range = rotationBySpeedModule.range;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			FindDependencies(x, dependencies, objects, allowNulls);
			FindDependencies(y, dependencies, objects, allowNulls);
			FindDependencies(z, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				ParticleSystem.RotationBySpeedModule rotationBySpeedModule = (ParticleSystem.RotationBySpeedModule)obj;
				GetDependencies(x, rotationBySpeedModule.x, dependencies);
				GetDependencies(y, rotationBySpeedModule.y, dependencies);
				GetDependencies(z, rotationBySpeedModule.z, dependencies);
			}
		}
	}
}
