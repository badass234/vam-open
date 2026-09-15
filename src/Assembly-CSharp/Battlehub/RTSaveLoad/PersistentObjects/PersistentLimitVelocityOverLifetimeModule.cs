using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentLimitVelocityOverLifetimeModule : PersistentData
	{
		public bool enabled;

		public PersistentMinMaxCurve limitX;

		public float limitXMultiplier;

		public PersistentMinMaxCurve limitY;

		public float limitYMultiplier;

		public PersistentMinMaxCurve limitZ;

		public float limitZMultiplier;

		public PersistentMinMaxCurve limit;

		public float limitMultiplier;

		public float dampen;

		public bool separateAxes;

		public uint space;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityOverLifetimeModule = (ParticleSystem.LimitVelocityOverLifetimeModule)obj;
			limitVelocityOverLifetimeModule.enabled = enabled;
			limitVelocityOverLifetimeModule.limitX = Write(limitVelocityOverLifetimeModule.limitX, limitX, objects);
			limitVelocityOverLifetimeModule.limitXMultiplier = limitXMultiplier;
			limitVelocityOverLifetimeModule.limitY = Write(limitVelocityOverLifetimeModule.limitY, limitY, objects);
			limitVelocityOverLifetimeModule.limitYMultiplier = limitYMultiplier;
			limitVelocityOverLifetimeModule.limitZ = Write(limitVelocityOverLifetimeModule.limitZ, limitZ, objects);
			limitVelocityOverLifetimeModule.limitZMultiplier = limitZMultiplier;
			limitVelocityOverLifetimeModule.limit = Write(limitVelocityOverLifetimeModule.limit, limit, objects);
			limitVelocityOverLifetimeModule.limitMultiplier = limitMultiplier;
			limitVelocityOverLifetimeModule.dampen = dampen;
			limitVelocityOverLifetimeModule.separateAxes = separateAxes;
			limitVelocityOverLifetimeModule.space = (ParticleSystemSimulationSpace)space;
			return limitVelocityOverLifetimeModule;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityOverLifetimeModule = (ParticleSystem.LimitVelocityOverLifetimeModule)obj;
				enabled = limitVelocityOverLifetimeModule.enabled;
				limitX = Read(limitX, limitVelocityOverLifetimeModule.limitX);
				limitXMultiplier = limitVelocityOverLifetimeModule.limitXMultiplier;
				limitY = Read(limitY, limitVelocityOverLifetimeModule.limitY);
				limitYMultiplier = limitVelocityOverLifetimeModule.limitYMultiplier;
				limitZ = Read(limitZ, limitVelocityOverLifetimeModule.limitZ);
				limitZMultiplier = limitVelocityOverLifetimeModule.limitZMultiplier;
				limit = Read(limit, limitVelocityOverLifetimeModule.limit);
				limitMultiplier = limitVelocityOverLifetimeModule.limitMultiplier;
				dampen = limitVelocityOverLifetimeModule.dampen;
				separateAxes = limitVelocityOverLifetimeModule.separateAxes;
				space = (uint)limitVelocityOverLifetimeModule.space;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			FindDependencies(limitX, dependencies, objects, allowNulls);
			FindDependencies(limitY, dependencies, objects, allowNulls);
			FindDependencies(limitZ, dependencies, objects, allowNulls);
			FindDependencies(limit, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityOverLifetimeModule = (ParticleSystem.LimitVelocityOverLifetimeModule)obj;
				GetDependencies(limitX, limitVelocityOverLifetimeModule.limitX, dependencies);
				GetDependencies(limitY, limitVelocityOverLifetimeModule.limitY, dependencies);
				GetDependencies(limitZ, limitVelocityOverLifetimeModule.limitZ, dependencies);
				GetDependencies(limit, limitVelocityOverLifetimeModule.limit, dependencies);
			}
		}
	}
}
