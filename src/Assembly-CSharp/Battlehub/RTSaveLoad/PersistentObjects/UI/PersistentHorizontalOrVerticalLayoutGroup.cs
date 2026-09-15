using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad.PersistentObjects.UI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1130, typeof(PersistentHorizontalLayoutGroup))]
	[ProtoInclude(1131, typeof(PersistentVerticalLayoutGroup))]
	public class PersistentHorizontalOrVerticalLayoutGroup : PersistentLayoutGroup
	{
		public float spacing;

		public bool childForceExpandWidth;

		public bool childForceExpandHeight;

		public bool childControlWidth;

		public bool childControlHeight;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			HorizontalOrVerticalLayoutGroup horizontalOrVerticalLayoutGroup = (HorizontalOrVerticalLayoutGroup)obj;
			horizontalOrVerticalLayoutGroup.spacing = spacing;
			horizontalOrVerticalLayoutGroup.childForceExpandWidth = childForceExpandWidth;
			horizontalOrVerticalLayoutGroup.childForceExpandHeight = childForceExpandHeight;
			horizontalOrVerticalLayoutGroup.childControlWidth = childControlWidth;
			horizontalOrVerticalLayoutGroup.childControlHeight = childControlHeight;
			return horizontalOrVerticalLayoutGroup;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				HorizontalOrVerticalLayoutGroup horizontalOrVerticalLayoutGroup = (HorizontalOrVerticalLayoutGroup)obj;
				spacing = horizontalOrVerticalLayoutGroup.spacing;
				childForceExpandWidth = horizontalOrVerticalLayoutGroup.childForceExpandWidth;
				childForceExpandHeight = horizontalOrVerticalLayoutGroup.childForceExpandHeight;
				childControlWidth = horizontalOrVerticalLayoutGroup.childControlWidth;
				childControlHeight = horizontalOrVerticalLayoutGroup.childControlHeight;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
