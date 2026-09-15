using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentTerrainCollider : PersistentCollider
	{
		public long terrainData;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			TerrainCollider terrainCollider = (TerrainCollider)obj;
			terrainCollider.terrainData = (TerrainData)objects.Get(terrainData);
			return terrainCollider;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				TerrainCollider terrainCollider = (TerrainCollider)obj;
				terrainData = terrainCollider.terrainData.GetMappedInstanceID();
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(terrainData, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				TerrainCollider terrainCollider = (TerrainCollider)obj;
				AddDependency(terrainCollider.terrainData, dependencies);
			}
		}
	}
}
