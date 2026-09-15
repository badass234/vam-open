using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentLight : PersistentBehaviour
	{
		public uint type;

		public Color color;

		public float colorTemperature;

		public float intensity;

		public float bounceIntensity;

		public uint shadows;

		public float shadowStrength;

		public uint shadowResolution;

		public int shadowCustomResolution;

		public float shadowBias;

		public float shadowNormalBias;

		public float shadowNearPlane;

		public float range;

		public float spotAngle;

		public float cookieSize;

		public long cookie;

		public long flare;

		public uint renderMode;

		public bool alreadyLightmapped;

		public int cullingMask;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Light light = (Light)obj;
			light.type = (LightType)type;
			light.color = color;
			light.colorTemperature = colorTemperature;
			light.intensity = intensity;
			light.bounceIntensity = bounceIntensity;
			light.shadows = (LightShadows)shadows;
			light.shadowStrength = shadowStrength;
			light.shadowResolution = (LightShadowResolution)shadowResolution;
			light.shadowCustomResolution = shadowCustomResolution;
			light.shadowBias = shadowBias;
			light.shadowNormalBias = shadowNormalBias;
			light.shadowNearPlane = shadowNearPlane;
			light.range = range;
			light.spotAngle = spotAngle;
			light.cookieSize = cookieSize;
			light.cookie = (Texture)objects.Get(cookie);
			light.flare = (Flare)objects.Get(flare);
			light.renderMode = (LightRenderMode)renderMode;
			light.cullingMask = cullingMask;
			return light;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Light light = (Light)obj;
				type = (uint)light.type;
				color = light.color;
				colorTemperature = light.colorTemperature;
				intensity = light.intensity;
				bounceIntensity = light.bounceIntensity;
				shadows = (uint)light.shadows;
				shadowStrength = light.shadowStrength;
				shadowResolution = (uint)light.shadowResolution;
				shadowCustomResolution = light.shadowCustomResolution;
				shadowBias = light.shadowBias;
				shadowNormalBias = light.shadowNormalBias;
				shadowNearPlane = light.shadowNearPlane;
				range = light.range;
				spotAngle = light.spotAngle;
				cookieSize = light.cookieSize;
				cookie = light.cookie.GetMappedInstanceID();
				flare = light.flare.GetMappedInstanceID();
				renderMode = (uint)light.renderMode;
				cullingMask = light.cullingMask;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(cookie, dependencies, objects, allowNulls);
			AddDependency(flare, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				Light light = (Light)obj;
				AddDependency(light.cookie, dependencies);
				AddDependency(light.flare, dependencies);
			}
		}
	}
}
