using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentTreePrototype : PersistentData
	{
		public long prefab;

		public float bendFactor;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			TreePrototype treePrototype = (TreePrototype)obj;
			treePrototype.prefab = (GameObject)objects.Get(prefab);
			treePrototype.bendFactor = bendFactor;
			return treePrototype;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				TreePrototype treePrototype = (TreePrototype)obj;
				prefab = treePrototype.prefab.GetMappedInstanceID();
				bendFactor = treePrototype.bendFactor;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(prefab, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				TreePrototype treePrototype = (TreePrototype)obj;
				AddDependency(treePrototype.prefab, dependencies);
			}
		}
	}
}
