using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad.PersistentObjects.UI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentScrollbar : PersistentSelectable
	{
		public long handleRect;

		public Scrollbar.Direction direction;

		public float value;

		public float size;

		public int numberOfSteps;

		public PersistentUnityEventBase onValueChanged;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Scrollbar scrollbar = (Scrollbar)obj;
			scrollbar.handleRect = (RectTransform)objects.Get(handleRect);
			scrollbar.direction = direction;
			scrollbar.value = value;
			scrollbar.size = size;
			scrollbar.numberOfSteps = numberOfSteps;
			Write(scrollbar.onValueChanged, onValueChanged, objects);
			return scrollbar;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Scrollbar scrollbar = (Scrollbar)obj;
				handleRect = scrollbar.handleRect.GetMappedInstanceID();
				direction = scrollbar.direction;
				value = scrollbar.value;
				size = scrollbar.size;
				numberOfSteps = scrollbar.numberOfSteps;
				Read(onValueChanged, scrollbar.onValueChanged);
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(handleRect, dependencies, objects, allowNulls);
			if (onValueChanged != null)
			{
				onValueChanged.FindDependencies(dependencies, objects, allowNulls);
			}
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				Scrollbar scrollbar = (Scrollbar)obj;
				AddDependency(scrollbar.handleRect, dependencies);
				PersistentUnityEventBase persistentUnityEventBase = new PersistentUnityEventBase();
				persistentUnityEventBase.GetDependencies(scrollbar.onValueChanged, dependencies);
			}
		}
	}
}
