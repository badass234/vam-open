using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentColorOverLifetimeModule : PersistentData
	{
		public bool enabled;

		public PersistentMinMaxGradient color;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule = (ParticleSystem.ColorOverLifetimeModule)obj;
			colorOverLifetimeModule.enabled = enabled;
			colorOverLifetimeModule.color = Write(colorOverLifetimeModule.color, color, objects);
			return colorOverLifetimeModule;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule = (ParticleSystem.ColorOverLifetimeModule)obj;
				enabled = colorOverLifetimeModule.enabled;
				color = Read(color, colorOverLifetimeModule.color);
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			FindDependencies(color, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule = (ParticleSystem.ColorOverLifetimeModule)obj;
				GetDependencies(color, colorOverLifetimeModule.color, dependencies);
			}
		}
	}
}
