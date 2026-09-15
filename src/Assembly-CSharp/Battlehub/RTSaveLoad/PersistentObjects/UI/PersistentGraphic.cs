using System;
using System.Collections.Generic;
using Battlehub.RTSaveLoad.PersistentObjects.EventSystems;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad.PersistentObjects.UI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1129, typeof(PersistentMaskableGraphic))]
	public class PersistentGraphic : PersistentUIBehaviour
	{
		public Color color;

		public bool raycastTarget;

		public long material;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Graphic graphic = (Graphic)obj;
			graphic.color = color;
			graphic.raycastTarget = raycastTarget;
			graphic.material = (Material)objects.Get(material);
			return graphic;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Graphic graphic = (Graphic)obj;
				color = graphic.color;
				raycastTarget = graphic.raycastTarget;
				material = graphic.material.GetMappedInstanceID();
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
				Graphic graphic = (Graphic)obj;
				AddDependency(graphic.material, dependencies);
			}
		}
	}
}
