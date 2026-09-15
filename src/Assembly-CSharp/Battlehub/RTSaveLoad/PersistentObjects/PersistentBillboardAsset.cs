using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentBillboardAsset : PersistentObject
	{
		public float width;

		public float height;

		public float bottom;

		public long material;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			BillboardAsset billboardAsset = (BillboardAsset)obj;
			billboardAsset.width = width;
			billboardAsset.height = height;
			billboardAsset.bottom = bottom;
			billboardAsset.material = (Material)objects.Get(material);
			return billboardAsset;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				BillboardAsset billboardAsset = (BillboardAsset)obj;
				width = billboardAsset.width;
				height = billboardAsset.height;
				bottom = billboardAsset.bottom;
				material = billboardAsset.material.GetMappedInstanceID();
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(material, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				BillboardAsset billboardAsset = (BillboardAsset)obj;
				AddDependency(billboardAsset.material, dependencies);
			}
		}
	}
}
