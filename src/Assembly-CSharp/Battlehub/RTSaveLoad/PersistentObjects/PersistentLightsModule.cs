using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentLightsModule : PersistentData
	{
		public bool enabled;

		public float ratio;

		public bool useRandomDistribution;

		public long light;

		public bool useParticleColor;

		public bool sizeAffectsRange;

		public bool alphaAffectsIntensity;

		public PersistentMinMaxCurve range;

		public float rangeMultiplier;

		public PersistentMinMaxCurve intensity;

		public float intensityMultiplier;

		public int maxLights;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.LightsModule lightsModule = (ParticleSystem.LightsModule)obj;
			lightsModule.enabled = enabled;
			lightsModule.ratio = ratio;
			lightsModule.useRandomDistribution = useRandomDistribution;
			lightsModule.light = (Light)objects.Get(light);
			lightsModule.useParticleColor = useParticleColor;
			lightsModule.sizeAffectsRange = sizeAffectsRange;
			lightsModule.alphaAffectsIntensity = alphaAffectsIntensity;
			lightsModule.range = Write(lightsModule.range, range, objects);
			lightsModule.rangeMultiplier = rangeMultiplier;
			lightsModule.intensity = Write(lightsModule.intensity, intensity, objects);
			lightsModule.intensityMultiplier = intensityMultiplier;
			lightsModule.maxLights = maxLights;
			return lightsModule;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.LightsModule lightsModule = (ParticleSystem.LightsModule)obj;
				enabled = lightsModule.enabled;
				ratio = lightsModule.ratio;
				useRandomDistribution = lightsModule.useRandomDistribution;
				light = lightsModule.light.GetMappedInstanceID();
				useParticleColor = lightsModule.useParticleColor;
				sizeAffectsRange = lightsModule.sizeAffectsRange;
				alphaAffectsIntensity = lightsModule.alphaAffectsIntensity;
				range = Read(range, lightsModule.range);
				rangeMultiplier = lightsModule.rangeMultiplier;
				intensity = Read(intensity, lightsModule.intensity);
				intensityMultiplier = lightsModule.intensityMultiplier;
				maxLights = lightsModule.maxLights;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(light, dependencies, objects, allowNulls);
			FindDependencies(range, dependencies, objects, allowNulls);
			FindDependencies(intensity, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				ParticleSystem.LightsModule lightsModule = (ParticleSystem.LightsModule)obj;
				AddDependency(lightsModule.light, dependencies);
				GetDependencies(range, lightsModule.range, dependencies);
				GetDependencies(intensity, lightsModule.intensity, dependencies);
			}
		}
	}
}
