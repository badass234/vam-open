using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentRotationOverLifetimeModule : PersistentData
	{
		public bool enabled;

		public PersistentMinMaxCurve x;

		public float xMultiplier;

		public PersistentMinMaxCurve y;

		public float yMultiplier;

		public PersistentMinMaxCurve z;

		public float zMultiplier;

		public bool separateAxes;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.RotationOverLifetimeModule rotationOverLifetimeModule = (ParticleSystem.RotationOverLifetimeModule)obj;
			rotationOverLifetimeModule.enabled = enabled;
			rotationOverLifetimeModule.x = Write(rotationOverLifetimeModule.x, x, objects);
			rotationOverLifetimeModule.xMultiplier = xMultiplier;
			rotationOverLifetimeModule.y = Write(rotationOverLifetimeModule.y, y, objects);
			rotationOverLifetimeModule.yMultiplier = yMultiplier;
			rotationOverLifetimeModule.z = Write(rotationOverLifetimeModule.z, z, objects);
			rotationOverLifetimeModule.zMultiplier = zMultiplier;
			rotationOverLifetimeModule.separateAxes = separateAxes;
			return rotationOverLifetimeModule;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.RotationOverLifetimeModule rotationOverLifetimeModule = (ParticleSystem.RotationOverLifetimeModule)obj;
				enabled = rotationOverLifetimeModule.enabled;
				x = Read(x, rotationOverLifetimeModule.x);
				xMultiplier = rotationOverLifetimeModule.xMultiplier;
				y = Read(y, rotationOverLifetimeModule.y);
				yMultiplier = rotationOverLifetimeModule.yMultiplier;
				z = Read(z, rotationOverLifetimeModule.z);
				zMultiplier = rotationOverLifetimeModule.zMultiplier;
				separateAxes = rotationOverLifetimeModule.separateAxes;
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
				ParticleSystem.RotationOverLifetimeModule rotationOverLifetimeModule = (ParticleSystem.RotationOverLifetimeModule)obj;
				GetDependencies(x, rotationOverLifetimeModule.x, dependencies);
				GetDependencies(y, rotationOverLifetimeModule.y, dependencies);
				GetDependencies(z, rotationOverLifetimeModule.z, dependencies);
			}
		}
	}
}
