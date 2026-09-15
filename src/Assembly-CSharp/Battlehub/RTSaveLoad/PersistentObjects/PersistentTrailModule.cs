using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentTrailModule : PersistentData
	{
		public bool enabled;

		public float ratio;

		public PersistentMinMaxCurve lifetime;

		public float lifetimeMultiplier;

		public float minVertexDistance;

		public uint textureMode;

		public bool worldSpace;

		public bool dieWithParticles;

		public bool sizeAffectsWidth;

		public bool sizeAffectsLifetime;

		public bool inheritParticleColor;

		public PersistentMinMaxGradient colorOverLifetime;

		public PersistentMinMaxCurve widthOverTrail;

		public float widthOverTrailMultiplier;

		public PersistentMinMaxGradient colorOverTrail;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.TrailModule trailModule = (ParticleSystem.TrailModule)obj;
			trailModule.enabled = enabled;
			trailModule.ratio = ratio;
			trailModule.lifetime = Write(trailModule.lifetime, lifetime, objects);
			trailModule.lifetimeMultiplier = lifetimeMultiplier;
			trailModule.minVertexDistance = minVertexDistance;
			trailModule.textureMode = (ParticleSystemTrailTextureMode)textureMode;
			trailModule.worldSpace = worldSpace;
			trailModule.dieWithParticles = dieWithParticles;
			trailModule.sizeAffectsWidth = sizeAffectsWidth;
			trailModule.sizeAffectsLifetime = sizeAffectsLifetime;
			trailModule.inheritParticleColor = inheritParticleColor;
			trailModule.colorOverLifetime = Write(trailModule.colorOverLifetime, colorOverLifetime, objects);
			trailModule.widthOverTrail = Write(trailModule.widthOverTrail, widthOverTrail, objects);
			trailModule.widthOverTrailMultiplier = widthOverTrailMultiplier;
			trailModule.colorOverTrail = Write(trailModule.colorOverTrail, colorOverTrail, objects);
			return trailModule;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.TrailModule trailModule = (ParticleSystem.TrailModule)obj;
				enabled = trailModule.enabled;
				ratio = trailModule.ratio;
				lifetime = Read(lifetime, trailModule.lifetime);
				lifetimeMultiplier = trailModule.lifetimeMultiplier;
				minVertexDistance = trailModule.minVertexDistance;
				textureMode = (uint)trailModule.textureMode;
				worldSpace = trailModule.worldSpace;
				dieWithParticles = trailModule.dieWithParticles;
				sizeAffectsWidth = trailModule.sizeAffectsWidth;
				sizeAffectsLifetime = trailModule.sizeAffectsLifetime;
				inheritParticleColor = trailModule.inheritParticleColor;
				colorOverLifetime = Read(colorOverLifetime, trailModule.colorOverLifetime);
				widthOverTrail = Read(widthOverTrail, trailModule.widthOverTrail);
				widthOverTrailMultiplier = trailModule.widthOverTrailMultiplier;
				colorOverTrail = Read(colorOverTrail, trailModule.colorOverTrail);
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			FindDependencies(lifetime, dependencies, objects, allowNulls);
			FindDependencies(colorOverLifetime, dependencies, objects, allowNulls);
			FindDependencies(widthOverTrail, dependencies, objects, allowNulls);
			FindDependencies(colorOverTrail, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				ParticleSystem.TrailModule trailModule = (ParticleSystem.TrailModule)obj;
				GetDependencies(lifetime, trailModule.lifetime, dependencies);
				GetDependencies(colorOverLifetime, trailModule.colorOverLifetime, dependencies);
				GetDependencies(widthOverTrail, trailModule.widthOverTrail, dependencies);
				GetDependencies(colorOverTrail, trailModule.colorOverTrail, dependencies);
			}
		}
	}
}
