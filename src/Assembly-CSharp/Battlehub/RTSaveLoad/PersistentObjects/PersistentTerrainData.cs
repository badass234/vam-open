using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentTerrainData : PersistentObject
	{
		public int heightmapResolution;

		public Vector3 size;

		public float thickness;

		public float wavingGrassStrength;

		public float wavingGrassAmount;

		public float wavingGrassSpeed;

		public Color wavingGrassTint;

		public PersistentDetailPrototype[] detailPrototypes;

		public TreeInstance[] treeInstances;

		public PersistentTreePrototype[] treePrototypes;

		public int alphamapResolution;

		public int baseMapResolution;

		public PersistentSplatPrototype[] splatPrototypes;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			TerrainData terrainData = (TerrainData)obj;
			terrainData.heightmapResolution = heightmapResolution;
			terrainData.size = size;
			terrainData.thickness = thickness;
			terrainData.wavingGrassStrength = wavingGrassStrength;
			terrainData.wavingGrassAmount = wavingGrassAmount;
			terrainData.wavingGrassSpeed = wavingGrassSpeed;
			terrainData.wavingGrassTint = wavingGrassTint;
			terrainData.detailPrototypes = Write(terrainData.detailPrototypes, detailPrototypes, objects);
			terrainData.treeInstances = treeInstances;
			terrainData.treePrototypes = Write(terrainData.treePrototypes, treePrototypes, objects);
			terrainData.alphamapResolution = alphamapResolution;
			terrainData.baseMapResolution = baseMapResolution;
			terrainData.splatPrototypes = Write(terrainData.splatPrototypes, splatPrototypes, objects);
			return terrainData;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				TerrainData terrainData = (TerrainData)obj;
				heightmapResolution = terrainData.heightmapResolution;
				size = terrainData.size;
				thickness = terrainData.thickness;
				wavingGrassStrength = terrainData.wavingGrassStrength;
				wavingGrassAmount = terrainData.wavingGrassAmount;
				wavingGrassSpeed = terrainData.wavingGrassSpeed;
				wavingGrassTint = terrainData.wavingGrassTint;
				detailPrototypes = Read(detailPrototypes, terrainData.detailPrototypes);
				treeInstances = terrainData.treeInstances;
				treePrototypes = Read(treePrototypes, terrainData.treePrototypes);
				alphamapResolution = terrainData.alphamapResolution;
				baseMapResolution = terrainData.baseMapResolution;
				splatPrototypes = Read(splatPrototypes, terrainData.splatPrototypes);
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			FindDependencies(detailPrototypes, dependencies, objects, allowNulls);
			FindDependencies(treePrototypes, dependencies, objects, allowNulls);
			FindDependencies(splatPrototypes, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				TerrainData terrainData = (TerrainData)obj;
				GetDependencies(detailPrototypes, terrainData.detailPrototypes, dependencies);
				GetDependencies(treePrototypes, terrainData.treePrototypes, dependencies);
				GetDependencies(splatPrototypes, terrainData.splatPrototypes, dependencies);
			}
		}
	}
}
