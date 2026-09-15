using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentReflectionProbe : PersistentBehaviour
	{
		public bool hdr;

		public Vector3 size;

		public Vector3 center;

		public float nearClipPlane;

		public float farClipPlane;

		public float shadowDistance;

		public int resolution;

		public int cullingMask;

		public uint clearFlags;

		public Color backgroundColor;

		public float intensity;

		public float blendDistance;

		public bool boxProjection;

		public uint mode;

		public int importance;

		public uint refreshMode;

		public uint timeSlicingMode;

		public long bakedTexture;

		public long customBakedTexture;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ReflectionProbe reflectionProbe = (ReflectionProbe)obj;
			reflectionProbe.hdr = hdr;
			reflectionProbe.size = size;
			reflectionProbe.center = center;
			reflectionProbe.nearClipPlane = nearClipPlane;
			reflectionProbe.farClipPlane = farClipPlane;
			reflectionProbe.shadowDistance = shadowDistance;
			reflectionProbe.resolution = resolution;
			reflectionProbe.cullingMask = cullingMask;
			reflectionProbe.clearFlags = (ReflectionProbeClearFlags)clearFlags;
			reflectionProbe.backgroundColor = backgroundColor;
			reflectionProbe.intensity = intensity;
			reflectionProbe.blendDistance = blendDistance;
			reflectionProbe.boxProjection = boxProjection;
			reflectionProbe.mode = (ReflectionProbeMode)mode;
			reflectionProbe.importance = importance;
			reflectionProbe.refreshMode = (ReflectionProbeRefreshMode)refreshMode;
			reflectionProbe.timeSlicingMode = (ReflectionProbeTimeSlicingMode)timeSlicingMode;
			reflectionProbe.bakedTexture = (Texture)objects.Get(bakedTexture);
			reflectionProbe.customBakedTexture = (Texture)objects.Get(customBakedTexture);
			return reflectionProbe;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ReflectionProbe reflectionProbe = (ReflectionProbe)obj;
				hdr = reflectionProbe.hdr;
				size = reflectionProbe.size;
				center = reflectionProbe.center;
				nearClipPlane = reflectionProbe.nearClipPlane;
				farClipPlane = reflectionProbe.farClipPlane;
				shadowDistance = reflectionProbe.shadowDistance;
				resolution = reflectionProbe.resolution;
				cullingMask = reflectionProbe.cullingMask;
				clearFlags = (uint)reflectionProbe.clearFlags;
				backgroundColor = reflectionProbe.backgroundColor;
				intensity = reflectionProbe.intensity;
				blendDistance = reflectionProbe.blendDistance;
				boxProjection = reflectionProbe.boxProjection;
				mode = (uint)reflectionProbe.mode;
				importance = reflectionProbe.importance;
				refreshMode = (uint)reflectionProbe.refreshMode;
				timeSlicingMode = (uint)reflectionProbe.timeSlicingMode;
				bakedTexture = reflectionProbe.bakedTexture.GetMappedInstanceID();
				customBakedTexture = reflectionProbe.customBakedTexture.GetMappedInstanceID();
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(bakedTexture, dependencies, objects, allowNulls);
			AddDependency(customBakedTexture, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				ReflectionProbe reflectionProbe = (ReflectionProbe)obj;
				AddDependency(reflectionProbe.bakedTexture, dependencies);
				AddDependency(reflectionProbe.customBakedTexture, dependencies);
			}
		}
	}
}
