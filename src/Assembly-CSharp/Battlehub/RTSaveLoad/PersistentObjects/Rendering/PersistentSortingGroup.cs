using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTSaveLoad.PersistentObjects.Rendering
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentSortingGroup : PersistentBehaviour
	{
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
			SortingGroup sortingGroup = (SortingGroup)obj;
			sortingGroup.sortingLayerName = sortingLayerName;
			sortingGroup.sortingLayerID = sortingLayerID;
			sortingGroup.sortingOrder = sortingOrder;
			return sortingGroup;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				SortingGroup sortingGroup = (SortingGroup)obj;
				sortingLayerName = sortingGroup.sortingLayerName;
				sortingLayerID = sortingGroup.sortingLayerID;
				sortingOrder = sortingGroup.sortingOrder;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
