using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentBillboardRenderer : PersistentRenderer
	{
		public long billboard;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			BillboardRenderer billboardRenderer = (BillboardRenderer)obj;
			billboardRenderer.billboard = (BillboardAsset)objects.Get(billboard);
			return billboardRenderer;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				BillboardRenderer billboardRenderer = (BillboardRenderer)obj;
				billboard = billboardRenderer.billboard.GetMappedInstanceID();
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(billboard, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				BillboardRenderer billboardRenderer = (BillboardRenderer)obj;
				AddDependency(billboardRenderer.billboard, dependencies);
			}
		}
	}
}
