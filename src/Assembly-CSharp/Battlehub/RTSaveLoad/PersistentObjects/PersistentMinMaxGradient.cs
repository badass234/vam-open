using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentMinMaxGradient : PersistentData
	{
		public uint mode;

		public PersistentGradient gradientMax;

		public PersistentGradient gradientMin;

		public Color colorMax;

		public Color colorMin;

		public Color color;

		public PersistentGradient gradient;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.MinMaxGradient minMaxGradient = (ParticleSystem.MinMaxGradient)obj;
			minMaxGradient.mode = (ParticleSystemGradientMode)mode;
			minMaxGradient.gradientMax = Write(minMaxGradient.gradientMax, gradientMax, objects);
			minMaxGradient.gradientMin = Write(minMaxGradient.gradientMin, gradientMin, objects);
			minMaxGradient.colorMax = colorMax;
			minMaxGradient.colorMin = colorMin;
			minMaxGradient.color = color;
			minMaxGradient.gradient = Write(minMaxGradient.gradient, gradient, objects);
			return minMaxGradient;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.MinMaxGradient minMaxGradient = (ParticleSystem.MinMaxGradient)obj;
				mode = (uint)minMaxGradient.mode;
				gradientMax = Read(gradientMax, minMaxGradient.gradientMax);
				gradientMin = Read(gradientMin, minMaxGradient.gradientMin);
				colorMax = minMaxGradient.colorMax;
				colorMin = minMaxGradient.colorMin;
				color = minMaxGradient.color;
				gradient = Read(gradient, minMaxGradient.gradient);
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			FindDependencies(gradientMax, dependencies, objects, allowNulls);
			FindDependencies(gradientMin, dependencies, objects, allowNulls);
			FindDependencies(gradient, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				ParticleSystem.MinMaxGradient minMaxGradient = (ParticleSystem.MinMaxGradient)obj;
				GetDependencies(gradientMax, minMaxGradient.gradientMax, dependencies);
				GetDependencies(gradientMin, minMaxGradient.gradientMin, dependencies);
				GetDependencies(gradient, minMaxGradient.gradient, dependencies);
			}
		}
	}
}
