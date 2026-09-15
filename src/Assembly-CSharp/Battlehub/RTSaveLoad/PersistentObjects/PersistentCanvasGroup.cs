using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentCanvasGroup : PersistentComponent
	{
		public float alpha;

		public bool interactable;

		public bool blocksRaycasts;

		public bool ignoreParentGroups;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			CanvasGroup canvasGroup = (CanvasGroup)obj;
			canvasGroup.alpha = alpha;
			canvasGroup.interactable = interactable;
			canvasGroup.blocksRaycasts = blocksRaycasts;
			canvasGroup.ignoreParentGroups = ignoreParentGroups;
			return canvasGroup;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				CanvasGroup canvasGroup = (CanvasGroup)obj;
				alpha = canvasGroup.alpha;
				interactable = canvasGroup.interactable;
				blocksRaycasts = canvasGroup.blocksRaycasts;
				ignoreParentGroups = canvasGroup.ignoreParentGroups;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
