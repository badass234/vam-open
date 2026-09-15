using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentColorBySpeedModule : PersistentData
	{
		public bool enabled;

		public PersistentMinMaxGradient color;

		public Vector2 range;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.ColorBySpeedModule colorBySpeedModule = (ParticleSystem.ColorBySpeedModule)obj;
			colorBySpeedModule.enabled = enabled;
			colorBySpeedModule.color = Write(colorBySpeedModule.color, color, objects);
			colorBySpeedModule.range = range;
			return colorBySpeedModule;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.ColorBySpeedModule colorBySpeedModule = (ParticleSystem.ColorBySpeedModule)obj;
				enabled = colorBySpeedModule.enabled;
				color = Read(color, colorBySpeedModule.color);
				range = colorBySpeedModule.range;
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
				ParticleSystem.ColorBySpeedModule colorBySpeedModule = (ParticleSystem.ColorBySpeedModule)obj;
				GetDependencies(color, colorBySpeedModule.color, dependencies);
			}
		}
	}
}
