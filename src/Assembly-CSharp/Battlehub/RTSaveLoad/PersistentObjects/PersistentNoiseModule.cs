using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentNoiseModule : PersistentData
	{
		public bool enabled;

		public bool separateAxes;

		public PersistentMinMaxCurve strength;

		public float strengthMultiplier;

		public PersistentMinMaxCurve strengthX;

		public float strengthXMultiplier;

		public PersistentMinMaxCurve strengthY;

		public float strengthYMultiplier;

		public PersistentMinMaxCurve strengthZ;

		public float strengthZMultiplier;

		public float frequency;

		public bool damping;

		public int octaveCount;

		public float octaveMultiplier;

		public float octaveScale;

		public uint quality;

		public PersistentMinMaxCurve scrollSpeed;

		public float scrollSpeedMultiplier;

		public bool remapEnabled;

		public PersistentMinMaxCurve remap;

		public float remapMultiplier;

		public PersistentMinMaxCurve remapX;

		public float remapXMultiplier;

		public PersistentMinMaxCurve remapY;

		public float remapYMultiplier;

		public PersistentMinMaxCurve remapZ;

		public float remapZMultiplier;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.NoiseModule noiseModule = (ParticleSystem.NoiseModule)obj;
			noiseModule.enabled = enabled;
			noiseModule.separateAxes = separateAxes;
			noiseModule.strength = Write(noiseModule.strength, strength, objects);
			noiseModule.strengthMultiplier = strengthMultiplier;
			noiseModule.strengthX = Write(noiseModule.strengthX, strengthX, objects);
			noiseModule.strengthXMultiplier = strengthXMultiplier;
			noiseModule.strengthY = Write(noiseModule.strengthY, strengthY, objects);
			noiseModule.strengthYMultiplier = strengthYMultiplier;
			noiseModule.strengthZ = Write(noiseModule.strengthZ, strengthZ, objects);
			noiseModule.strengthZMultiplier = strengthZMultiplier;
			noiseModule.frequency = frequency;
			noiseModule.damping = damping;
			noiseModule.octaveCount = octaveCount;
			noiseModule.octaveMultiplier = octaveMultiplier;
			noiseModule.octaveScale = octaveScale;
			noiseModule.quality = (ParticleSystemNoiseQuality)quality;
			noiseModule.scrollSpeed = Write(noiseModule.scrollSpeed, scrollSpeed, objects);
			noiseModule.scrollSpeedMultiplier = scrollSpeedMultiplier;
			noiseModule.remapEnabled = remapEnabled;
			noiseModule.remap = Write(noiseModule.remap, remap, objects);
			noiseModule.remapMultiplier = remapMultiplier;
			noiseModule.remapX = Write(noiseModule.remapX, remapX, objects);
			noiseModule.remapXMultiplier = remapXMultiplier;
			noiseModule.remapY = Write(noiseModule.remapY, remapY, objects);
			noiseModule.remapYMultiplier = remapYMultiplier;
			noiseModule.remapZ = Write(noiseModule.remapZ, remapZ, objects);
			noiseModule.remapZMultiplier = remapZMultiplier;
			return noiseModule;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.NoiseModule noiseModule = (ParticleSystem.NoiseModule)obj;
				enabled = noiseModule.enabled;
				separateAxes = noiseModule.separateAxes;
				strength = Read(strength, noiseModule.strength);
				strengthMultiplier = noiseModule.strengthMultiplier;
				strengthX = Read(strengthX, noiseModule.strengthX);
				strengthXMultiplier = noiseModule.strengthXMultiplier;
				strengthY = Read(strengthY, noiseModule.strengthY);
				strengthYMultiplier = noiseModule.strengthYMultiplier;
				strengthZ = Read(strengthZ, noiseModule.strengthZ);
				strengthZMultiplier = noiseModule.strengthZMultiplier;
				frequency = noiseModule.frequency;
				damping = noiseModule.damping;
				octaveCount = noiseModule.octaveCount;
				octaveMultiplier = noiseModule.octaveMultiplier;
				octaveScale = noiseModule.octaveScale;
				quality = (uint)noiseModule.quality;
				scrollSpeed = Read(scrollSpeed, noiseModule.scrollSpeed);
				scrollSpeedMultiplier = noiseModule.scrollSpeedMultiplier;
				remapEnabled = noiseModule.remapEnabled;
				remap = Read(remap, noiseModule.remap);
				remapMultiplier = noiseModule.remapMultiplier;
				remapX = Read(remapX, noiseModule.remapX);
				remapXMultiplier = noiseModule.remapXMultiplier;
				remapY = Read(remapY, noiseModule.remapY);
				remapYMultiplier = noiseModule.remapYMultiplier;
				remapZ = Read(remapZ, noiseModule.remapZ);
				remapZMultiplier = noiseModule.remapZMultiplier;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			FindDependencies(strength, dependencies, objects, allowNulls);
			FindDependencies(strengthX, dependencies, objects, allowNulls);
			FindDependencies(strengthY, dependencies, objects, allowNulls);
			FindDependencies(strengthZ, dependencies, objects, allowNulls);
			FindDependencies(scrollSpeed, dependencies, objects, allowNulls);
			FindDependencies(remap, dependencies, objects, allowNulls);
			FindDependencies(remapX, dependencies, objects, allowNulls);
			FindDependencies(remapY, dependencies, objects, allowNulls);
			FindDependencies(remapZ, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				ParticleSystem.NoiseModule noiseModule = (ParticleSystem.NoiseModule)obj;
				GetDependencies(strength, noiseModule.strength, dependencies);
				GetDependencies(strengthX, noiseModule.strengthX, dependencies);
				GetDependencies(strengthY, noiseModule.strengthY, dependencies);
				GetDependencies(strengthZ, noiseModule.strengthZ, dependencies);
				GetDependencies(scrollSpeed, noiseModule.scrollSpeed, dependencies);
				GetDependencies(remap, noiseModule.remap, dependencies);
				GetDependencies(remapX, noiseModule.remapX, dependencies);
				GetDependencies(remapY, noiseModule.remapY, dependencies);
				GetDependencies(remapZ, noiseModule.remapZ, dependencies);
			}
		}
	}
}
