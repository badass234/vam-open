using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad.PersistentObjects.UI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1131, typeof(PersistentImage))]
	[ProtoInclude(1132, typeof(PersistentRawImage))]
	[ProtoInclude(1133, typeof(PersistentText))]
	public class PersistentMaskableGraphic : PersistentGraphic
	{
		public PersistentUnityEventBase onCullStateChanged;

		public bool maskable;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			MaskableGraphic maskableGraphic = (MaskableGraphic)obj;
			onCullStateChanged.WriteTo(maskableGraphic.onCullStateChanged, objects);
			maskableGraphic.maskable = maskable;
			return maskableGraphic;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				MaskableGraphic maskableGraphic = (MaskableGraphic)obj;
				onCullStateChanged = new PersistentUnityEventBase();
				onCullStateChanged.ReadFrom(maskableGraphic.onCullStateChanged);
				maskable = maskableGraphic.maskable;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			onCullStateChanged.FindDependencies(dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			MaskableGraphic maskableGraphic = (MaskableGraphic)obj;
			if (!(maskableGraphic == null))
			{
				PersistentUnityEventBase persistentUnityEventBase = new PersistentUnityEventBase();
				persistentUnityEventBase.GetDependencies(maskableGraphic.onCullStateChanged, dependencies);
			}
		}
	}
}
