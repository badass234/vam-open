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
	public class PersistentGraphicRaycaster : PersistentBaseRaycaster
	{
		public bool ignoreReversedGraphics;

		public uint blockingObjects;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			GraphicRaycaster graphicRaycaster = (GraphicRaycaster)obj;
			graphicRaycaster.ignoreReversedGraphics = ignoreReversedGraphics;
			graphicRaycaster.blockingObjects = (GraphicRaycaster.BlockingObjects)blockingObjects;
			return graphicRaycaster;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				GraphicRaycaster graphicRaycaster = (GraphicRaycaster)obj;
				ignoreReversedGraphics = graphicRaycaster.ignoreReversedGraphics;
				blockingObjects = (uint)graphicRaycaster.blockingObjects;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
