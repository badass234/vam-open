using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentWindZone : PersistentComponent
	{
		public uint mode;

		public float radius;

		public float windMain;

		public float windTurbulence;

		public float windPulseMagnitude;

		public float windPulseFrequency;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			WindZone windZone = (WindZone)obj;
			windZone.mode = (WindZoneMode)mode;
			windZone.radius = radius;
			windZone.windMain = windMain;
			windZone.windTurbulence = windTurbulence;
			windZone.windPulseMagnitude = windPulseMagnitude;
			windZone.windPulseFrequency = windPulseFrequency;
			return windZone;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				WindZone windZone = (WindZone)obj;
				mode = (uint)windZone.mode;
				radius = windZone.radius;
				windMain = windZone.windMain;
				windTurbulence = windZone.windTurbulence;
				windPulseMagnitude = windZone.windPulseMagnitude;
				windPulseFrequency = windZone.windPulseFrequency;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
