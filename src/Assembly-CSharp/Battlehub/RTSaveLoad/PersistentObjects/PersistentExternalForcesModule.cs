using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentExternalForcesModule : PersistentData
	{
		public bool enabled;

		public float multiplier;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.ExternalForcesModule externalForcesModule = (ParticleSystem.ExternalForcesModule)obj;
			externalForcesModule.enabled = enabled;
			externalForcesModule.multiplier = multiplier;
			return externalForcesModule;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.ExternalForcesModule externalForcesModule = (ParticleSystem.ExternalForcesModule)obj;
				enabled = externalForcesModule.enabled;
				multiplier = externalForcesModule.multiplier;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
