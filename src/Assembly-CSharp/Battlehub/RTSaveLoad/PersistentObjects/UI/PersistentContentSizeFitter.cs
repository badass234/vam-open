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
	public class PersistentContentSizeFitter : PersistentUIBehaviour
	{
		public uint horizontalFit;

		public uint verticalFit;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ContentSizeFitter contentSizeFitter = (ContentSizeFitter)obj;
			contentSizeFitter.horizontalFit = (ContentSizeFitter.FitMode)horizontalFit;
			contentSizeFitter.verticalFit = (ContentSizeFitter.FitMode)verticalFit;
			return contentSizeFitter;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ContentSizeFitter contentSizeFitter = (ContentSizeFitter)obj;
				horizontalFit = (uint)contentSizeFitter.horizontalFit;
				verticalFit = (uint)contentSizeFitter.verticalFit;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
