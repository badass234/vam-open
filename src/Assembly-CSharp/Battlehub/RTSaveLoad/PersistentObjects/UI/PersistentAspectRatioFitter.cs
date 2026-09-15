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
	public class PersistentAspectRatioFitter : PersistentUIBehaviour
	{
		public uint aspectMode;

		public float aspectRatio;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			AspectRatioFitter aspectRatioFitter = (AspectRatioFitter)obj;
			aspectRatioFitter.aspectMode = (AspectRatioFitter.AspectMode)aspectMode;
			aspectRatioFitter.aspectRatio = aspectRatio;
			return aspectRatioFitter;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				AspectRatioFitter aspectRatioFitter = (AspectRatioFitter)obj;
				aspectMode = (uint)aspectRatioFitter.aspectMode;
				aspectRatio = aspectRatioFitter.aspectRatio;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
