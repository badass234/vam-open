using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentForceOverLifetimeModule : PersistentData
	{
		public bool enabled;

		public PersistentMinMaxCurve x;

		public PersistentMinMaxCurve y;

		public PersistentMinMaxCurve z;

		public float xMultiplier;

		public float yMultiplier;

		public float zMultiplier;

		public uint space;

		public bool randomized;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.ForceOverLifetimeModule forceOverLifetimeModule = (ParticleSystem.ForceOverLifetimeModule)obj;
			forceOverLifetimeModule.enabled = enabled;
			forceOverLifetimeModule.x = Write(forceOverLifetimeModule.x, x, objects);
			forceOverLifetimeModule.y = Write(forceOverLifetimeModule.y, y, objects);
			forceOverLifetimeModule.z = Write(forceOverLifetimeModule.z, z, objects);
			forceOverLifetimeModule.xMultiplier = xMultiplier;
			forceOverLifetimeModule.yMultiplier = yMultiplier;
			forceOverLifetimeModule.zMultiplier = zMultiplier;
			forceOverLifetimeModule.space = (ParticleSystemSimulationSpace)space;
			forceOverLifetimeModule.randomized = randomized;
			return forceOverLifetimeModule;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.ForceOverLifetimeModule forceOverLifetimeModule = (ParticleSystem.ForceOverLifetimeModule)obj;
				enabled = forceOverLifetimeModule.enabled;
				x = Read(x, forceOverLifetimeModule.x);
				y = Read(y, forceOverLifetimeModule.y);
				z = Read(z, forceOverLifetimeModule.z);
				xMultiplier = forceOverLifetimeModule.xMultiplier;
				yMultiplier = forceOverLifetimeModule.yMultiplier;
				zMultiplier = forceOverLifetimeModule.zMultiplier;
				space = (uint)forceOverLifetimeModule.space;
				randomized = forceOverLifetimeModule.randomized;
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
				ParticleSystem.ForceOverLifetimeModule forceOverLifetimeModule = (ParticleSystem.ForceOverLifetimeModule)obj;
				GetDependencies(x, forceOverLifetimeModule.x, dependencies);
				GetDependencies(y, forceOverLifetimeModule.y, dependencies);
				GetDependencies(z, forceOverLifetimeModule.z, dependencies);
			}
		}
	}
}
