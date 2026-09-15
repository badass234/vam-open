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
	public class PersistentMask : PersistentUIBehaviour
	{
		public bool showMaskGraphic;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Mask mask = (Mask)obj;
			mask.showMaskGraphic = showMaskGraphic;
			return mask;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Mask mask = (Mask)obj;
				showMaskGraphic = mask.showMaskGraphic;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
