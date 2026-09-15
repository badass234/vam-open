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
	public class PersistentLayoutElement : PersistentUIBehaviour
	{
		public bool ignoreLayout;

		public float minWidth;

		public float minHeight;

		public float preferredWidth;

		public float preferredHeight;

		public float flexibleWidth;

		public float flexibleHeight;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			LayoutElement layoutElement = (LayoutElement)obj;
			layoutElement.ignoreLayout = ignoreLayout;
			layoutElement.minWidth = minWidth;
			layoutElement.minHeight = minHeight;
			layoutElement.preferredWidth = preferredWidth;
			layoutElement.preferredHeight = preferredHeight;
			layoutElement.flexibleWidth = flexibleWidth;
			layoutElement.flexibleHeight = flexibleHeight;
			return layoutElement;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				LayoutElement layoutElement = (LayoutElement)obj;
				ignoreLayout = layoutElement.ignoreLayout;
				minWidth = layoutElement.minWidth;
				minHeight = layoutElement.minHeight;
				preferredWidth = layoutElement.preferredWidth;
				preferredHeight = layoutElement.preferredHeight;
				flexibleWidth = layoutElement.flexibleWidth;
				flexibleHeight = layoutElement.flexibleHeight;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
