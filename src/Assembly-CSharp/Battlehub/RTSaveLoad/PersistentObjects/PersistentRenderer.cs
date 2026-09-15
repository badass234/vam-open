using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1060, typeof(PersistentBillboardRenderer))]
	[ProtoInclude(1061, typeof(PersistentSkinnedMeshRenderer))]
	[ProtoInclude(1062, typeof(PersistentTrailRenderer))]
	[ProtoInclude(1063, typeof(PersistentLineRenderer))]
	[ProtoInclude(1064, typeof(PersistentMeshRenderer))]
	[ProtoInclude(1065, typeof(PersistentSpriteRenderer))]
	[ProtoInclude(1066, typeof(PersistentParticleSystemRenderer))]
	public class PersistentRenderer : PersistentComponent
	{
		public bool enabled;

		public uint shadowCastingMode;

		public bool receiveShadows;

		public long sharedMaterial;

		public long[] sharedMaterials;

		public int lightmapIndex;

		public int realtimeLightmapIndex;

		public Vector4 lightmapScaleOffset;

		public uint motionVectorGenerationMode;

		public Vector4 realtimeLightmapScaleOffset;

		public uint lightProbeUsage;

		public long lightProbeProxyVolumeOverride;

		public long probeAnchor;

		public uint reflectionProbeUsage;

		public string sortingLayerName;

		public int sortingLayerID;

		public int sortingOrder;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Renderer renderer = (Renderer)obj;
			renderer.enabled = enabled;
			renderer.shadowCastingMode = (ShadowCastingMode)shadowCastingMode;
			renderer.receiveShadows = receiveShadows;
			renderer.sharedMaterial = (Material)objects.Get(sharedMaterial);
			renderer.sharedMaterials = Resolve<Material, UnityEngine.Object>(sharedMaterials, objects);
			renderer.lightmapIndex = lightmapIndex;
			renderer.realtimeLightmapIndex = realtimeLightmapIndex;
			renderer.lightmapScaleOffset = lightmapScaleOffset;
			renderer.motionVectorGenerationMode = (MotionVectorGenerationMode)motionVectorGenerationMode;
			renderer.realtimeLightmapScaleOffset = realtimeLightmapScaleOffset;
			renderer.lightProbeUsage = (LightProbeUsage)lightProbeUsage;
			renderer.lightProbeProxyVolumeOverride = (GameObject)objects.Get(lightProbeProxyVolumeOverride);
			renderer.probeAnchor = (Transform)objects.Get(probeAnchor);
			renderer.reflectionProbeUsage = (ReflectionProbeUsage)reflectionProbeUsage;
			renderer.sortingLayerName = sortingLayerName;
			renderer.sortingLayerID = sortingLayerID;
			renderer.sortingOrder = sortingOrder;
			return renderer;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Renderer renderer = (Renderer)obj;
				enabled = renderer.enabled;
				shadowCastingMode = (uint)renderer.shadowCastingMode;
				receiveShadows = renderer.receiveShadows;
				sharedMaterial = renderer.sharedMaterial.GetMappedInstanceID();
				sharedMaterials = renderer.sharedMaterials.GetMappedInstanceID();
				lightmapIndex = renderer.lightmapIndex;
				realtimeLightmapIndex = renderer.realtimeLightmapIndex;
				lightmapScaleOffset = renderer.lightmapScaleOffset;
				motionVectorGenerationMode = (uint)renderer.motionVectorGenerationMode;
				realtimeLightmapScaleOffset = renderer.realtimeLightmapScaleOffset;
				lightProbeUsage = (uint)renderer.lightProbeUsage;
				lightProbeProxyVolumeOverride = renderer.lightProbeProxyVolumeOverride.GetMappedInstanceID();
				probeAnchor = renderer.probeAnchor.GetMappedInstanceID();
				reflectionProbeUsage = (uint)renderer.reflectionProbeUsage;
				sortingLayerName = renderer.sortingLayerName;
				sortingLayerID = renderer.sortingLayerID;
				sortingOrder = renderer.sortingOrder;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(sharedMaterial, dependencies, objects, allowNulls);
			AddDependencies(sharedMaterials, dependencies, objects, allowNulls);
			AddDependency(lightProbeProxyVolumeOverride, dependencies, objects, allowNulls);
			AddDependency(probeAnchor, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				Renderer renderer = (Renderer)obj;
				AddDependency(renderer.sharedMaterial, dependencies);
				AddDependencies(renderer.sharedMaterials, dependencies);
				AddDependency(renderer.lightProbeProxyVolumeOverride, dependencies);
				AddDependency(renderer.probeAnchor, dependencies);
			}
		}
	}
}
