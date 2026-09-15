using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentRectTransform : PersistentTransform
	{
		public Vector2 anchorMin;

		public Vector2 anchorMax;

		public Vector3 anchoredPosition3D;

		public Vector2 anchoredPosition;

		public Vector2 sizeDelta;

		public Vector2 pivot;

		public Vector2 offsetMin;

		public Vector2 offsetMax;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			RectTransform rectTransform = (RectTransform)obj;
			rectTransform.anchorMin = anchorMin;
			rectTransform.anchorMax = anchorMax;
			rectTransform.anchoredPosition3D = anchoredPosition3D;
			rectTransform.anchoredPosition = anchoredPosition;
			rectTransform.sizeDelta = sizeDelta;
			rectTransform.pivot = pivot;
			rectTransform.offsetMin = offsetMin;
			rectTransform.offsetMax = offsetMax;
			return rectTransform;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				RectTransform rectTransform = (RectTransform)obj;
				anchorMin = rectTransform.anchorMin;
				anchorMax = rectTransform.anchorMax;
				anchoredPosition3D = rectTransform.anchoredPosition3D;
				anchoredPosition = rectTransform.anchoredPosition;
				sizeDelta = rectTransform.sizeDelta;
				pivot = rectTransform.pivot;
				offsetMin = rectTransform.offsetMin;
				offsetMax = rectTransform.offsetMax;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
