using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentVelocityOverLifetimeModule : PersistentData
	{
		public bool enabled;

		public PersistentMinMaxCurve x;

		public PersistentMinMaxCurve y;

		public PersistentMinMaxCurve z;

		public float xMultiplier;

		public float yMultiplier;

		public float zMultiplier;

		public uint space;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule = (ParticleSystem.VelocityOverLifetimeModule)obj;
			velocityOverLifetimeModule.enabled = enabled;
			velocityOverLifetimeModule.x = Write(velocityOverLifetimeModule.x, x, objects);
			velocityOverLifetimeModule.y = Write(velocityOverLifetimeModule.y, y, objects);
			velocityOverLifetimeModule.z = Write(velocityOverLifetimeModule.z, z, objects);
			velocityOverLifetimeModule.xMultiplier = xMultiplier;
			velocityOverLifetimeModule.yMultiplier = yMultiplier;
			velocityOverLifetimeModule.zMultiplier = zMultiplier;
			velocityOverLifetimeModule.space = (ParticleSystemSimulationSpace)space;
			return velocityOverLifetimeModule;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule = (ParticleSystem.VelocityOverLifetimeModule)obj;
				enabled = velocityOverLifetimeModule.enabled;
				x = Read(x, velocityOverLifetimeModule.x);
				y = Read(y, velocityOverLifetimeModule.y);
				z = Read(z, velocityOverLifetimeModule.z);
				xMultiplier = velocityOverLifetimeModule.xMultiplier;
				yMultiplier = velocityOverLifetimeModule.yMultiplier;
				zMultiplier = velocityOverLifetimeModule.zMultiplier;
				space = (uint)velocityOverLifetimeModule.space;
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
				ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule = (ParticleSystem.VelocityOverLifetimeModule)obj;
				GetDependencies(x, velocityOverLifetimeModule.x, dependencies);
				GetDependencies(y, velocityOverLifetimeModule.y, dependencies);
				GetDependencies(z, velocityOverLifetimeModule.z, dependencies);
			}
		}
	}
}
