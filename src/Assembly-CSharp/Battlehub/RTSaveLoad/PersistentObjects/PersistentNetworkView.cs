using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentNetworkView : PersistentBehaviour
	{
		public long observed;

		public uint stateSynchronization;

		public int group;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			return base.WriteTo(obj, objects);
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(observed, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
		}
	}
}
