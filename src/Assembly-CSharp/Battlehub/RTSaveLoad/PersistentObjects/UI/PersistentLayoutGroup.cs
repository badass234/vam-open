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
	[ProtoInclude(1132, typeof(PersistentGridLayoutGroup))]
	[ProtoInclude(1133, typeof(PersistentHorizontalOrVerticalLayoutGroup))]
	public class PersistentLayoutGroup : PersistentUIBehaviour
	{
		public RectOffset padding;

		public uint childAlignment;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			LayoutGroup layoutGroup = (LayoutGroup)obj;
			layoutGroup.padding = padding;
			layoutGroup.childAlignment = (TextAnchor)childAlignment;
			return layoutGroup;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				LayoutGroup layoutGroup = (LayoutGroup)obj;
				padding = layoutGroup.padding;
				childAlignment = (uint)layoutGroup.childAlignment;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
