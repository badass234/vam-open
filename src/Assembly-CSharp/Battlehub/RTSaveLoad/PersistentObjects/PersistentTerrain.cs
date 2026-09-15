using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentTerrain : PersistentBehaviour
	{
		public long terrainData;

		public float treeDistance;

		public float treeBillboardDistance;

		public float treeCrossFadeLength;

		public int treeMaximumFullLODCount;

		public float detailObjectDistance;

		public float detailObjectDensity;

		public float heightmapPixelError;

		public int heightmapMaximumLOD;

		public float basemapDistance;

		public int lightmapIndex;

		public int realtimeLightmapIndex;

		public Vector4 lightmapScaleOffset;

		public Vector4 realtimeLightmapScaleOffset;

		public bool castShadows;

		public uint reflectionProbeUsage;

		public uint materialType;

		public long materialTemplate;

		public Color legacySpecular;

		public float legacyShininess;

		public bool drawHeightmap;

		public bool drawTreesAndFoliage;

		public float treeLODBiasMultiplier;

		public bool collectDetailPatches;

		public uint editorRenderFlags;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Terrain terrain = (Terrain)obj;
			terrain.terrainData = (TerrainData)objects.Get(terrainData);
			terrain.treeDistance = treeDistance;
			terrain.treeBillboardDistance = treeBillboardDistance;
			terrain.treeCrossFadeLength = treeCrossFadeLength;
			terrain.treeMaximumFullLODCount = treeMaximumFullLODCount;
			terrain.detailObjectDistance = detailObjectDistance;
			terrain.detailObjectDensity = detailObjectDensity;
			terrain.heightmapPixelError = heightmapPixelError;
			terrain.heightmapMaximumLOD = heightmapMaximumLOD;
			terrain.basemapDistance = basemapDistance;
			terrain.lightmapIndex = lightmapIndex;
			terrain.realtimeLightmapIndex = realtimeLightmapIndex;
			terrain.lightmapScaleOffset = lightmapScaleOffset;
			terrain.realtimeLightmapScaleOffset = realtimeLightmapScaleOffset;
			terrain.castShadows = castShadows;
			terrain.reflectionProbeUsage = (ReflectionProbeUsage)reflectionProbeUsage;
			terrain.materialType = (Terrain.MaterialType)materialType;
			terrain.materialTemplate = (Material)objects.Get(materialTemplate);
			terrain.legacySpecular = legacySpecular;
			terrain.legacyShininess = legacyShininess;
			terrain.drawHeightmap = drawHeightmap;
			terrain.drawTreesAndFoliage = drawTreesAndFoliage;
			terrain.treeLODBiasMultiplier = treeLODBiasMultiplier;
			terrain.collectDetailPatches = collectDetailPatches;
			terrain.editorRenderFlags = (TerrainRenderFlags)editorRenderFlags;
			return terrain;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Terrain terrain = (Terrain)obj;
				terrainData = terrain.terrainData.GetMappedInstanceID();
				treeDistance = terrain.treeDistance;
				treeBillboardDistance = terrain.treeBillboardDistance;
				treeCrossFadeLength = terrain.treeCrossFadeLength;
				treeMaximumFullLODCount = terrain.treeMaximumFullLODCount;
				detailObjectDistance = terrain.detailObjectDistance;
				detailObjectDensity = terrain.detailObjectDensity;
				heightmapPixelError = terrain.heightmapPixelError;
				heightmapMaximumLOD = terrain.heightmapMaximumLOD;
				basemapDistance = terrain.basemapDistance;
				lightmapIndex = terrain.lightmapIndex;
				realtimeLightmapIndex = terrain.realtimeLightmapIndex;
				lightmapScaleOffset = terrain.lightmapScaleOffset;
				realtimeLightmapScaleOffset = terrain.realtimeLightmapScaleOffset;
				castShadows = terrain.castShadows;
				reflectionProbeUsage = (uint)terrain.reflectionProbeUsage;
				materialType = (uint)terrain.materialType;
				materialTemplate = terrain.materialTemplate.GetMappedInstanceID();
				legacySpecular = terrain.legacySpecular;
				legacyShininess = terrain.legacyShininess;
				drawHeightmap = terrain.drawHeightmap;
				drawTreesAndFoliage = terrain.drawTreesAndFoliage;
				treeLODBiasMultiplier = terrain.treeLODBiasMultiplier;
				collectDetailPatches = terrain.collectDetailPatches;
				editorRenderFlags = (uint)terrain.editorRenderFlags;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(terrainData, dependencies, objects, allowNulls);
			AddDependency(materialTemplate, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				Terrain terrain = (Terrain)obj;
				AddDependency(terrain.terrainData, dependencies);
				AddDependency(terrain.materialTemplate, dependencies);
			}
		}
	}
}
