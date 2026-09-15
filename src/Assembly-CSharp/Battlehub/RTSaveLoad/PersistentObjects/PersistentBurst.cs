using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentBurst : PersistentData
	{
		public float time;

		public short minCount;

		public short maxCount;

		public int cycleCount;

		public float repeatInterval;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.Burst burst = (ParticleSystem.Burst)obj;
			burst.time = time;
			burst.minCount = minCount;
			burst.maxCount = maxCount;
			burst.cycleCount = cycleCount;
			burst.repeatInterval = repeatInterval;
			return burst;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.Burst burst = (ParticleSystem.Burst)obj;
				time = burst.time;
				minCount = burst.minCount;
				maxCount = burst.maxCount;
				cycleCount = burst.cycleCount;
				repeatInterval = burst.repeatInterval;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
