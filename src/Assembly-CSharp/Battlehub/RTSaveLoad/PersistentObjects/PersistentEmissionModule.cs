using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentEmissionModule : PersistentData
	{
		public PersistentBurst[] bursts;

		public bool enabled;

		public PersistentMinMaxCurve rateOverTime;

		public PersistentMinMaxCurve rateOverDistance;

		public float rateOverTimeMultiplier;

		public float rateOverDistanceMultiplier;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.EmissionModule emissionModule = (ParticleSystem.EmissionModule)obj;
			emissionModule.enabled = enabled;
			emissionModule.rateOverTime = Write(emissionModule.rateOverTime, rateOverTime, objects);
			emissionModule.rateOverDistance = Write(emissionModule.rateOverDistance, rateOverDistance, objects);
			if (bursts == null)
			{
				emissionModule.SetBursts(new ParticleSystem.Burst[0]);
			}
			else
			{
				ParticleSystem.Burst[] array = new ParticleSystem.Burst[bursts.Length];
				for (int i = 0; i < bursts.Length; i++)
				{
					ParticleSystem.Burst dst = default(ParticleSystem.Burst);
					array[i] = Write(dst, bursts[i], objects);
				}
				emissionModule.SetBursts(array);
			}
			emissionModule.rateOverTimeMultiplier = rateOverTimeMultiplier;
			emissionModule.rateOverDistanceMultiplier = rateOverDistanceMultiplier;
			return emissionModule;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.EmissionModule emissionModule = (ParticleSystem.EmissionModule)obj;
				enabled = emissionModule.enabled;
				rateOverTime = Read(rateOverTime, emissionModule.rateOverTime);
				rateOverDistance = Read(rateOverDistance, emissionModule.rateOverDistance);
				ParticleSystem.Burst[] array = new ParticleSystem.Burst[emissionModule.burstCount];
				bursts = new PersistentBurst[emissionModule.burstCount];
				emissionModule.GetBursts(array);
				for (int i = 0; i < bursts.Length; i++)
				{
					PersistentBurst persistentBurst = new PersistentBurst();
					persistentBurst.ReadFrom(bursts[i]);
					bursts[i] = persistentBurst;
				}
				rateOverTimeMultiplier = emissionModule.rateOverTimeMultiplier;
				rateOverDistanceMultiplier = emissionModule.rateOverDistanceMultiplier;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			FindDependencies(rateOverTime, dependencies, objects, allowNulls);
			FindDependencies(rateOverDistance, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				ParticleSystem.EmissionModule emissionModule = (ParticleSystem.EmissionModule)obj;
				GetDependencies(rateOverTime, emissionModule.rateOverTime, dependencies);
				GetDependencies(rateOverDistance, emissionModule.rateOverDistance, dependencies);
			}
		}
	}
}
