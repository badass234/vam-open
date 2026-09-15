using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad.PersistentObjects.UI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentSlider : PersistentSelectable
	{
		public long fillRect;

		public long handleRect;

		public Slider.Direction direction;

		public float minValue;

		public float maxValue;

		public bool wholeNumbers;

		public float value;

		public float normalizedValue;

		public PersistentUnityEventBase onValueChanged;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Slider slider = (Slider)obj;
			slider.fillRect = (RectTransform)objects.Get(fillRect);
			slider.handleRect = (RectTransform)objects.Get(handleRect);
			slider.direction = direction;
			slider.minValue = minValue;
			slider.maxValue = maxValue;
			slider.wholeNumbers = wholeNumbers;
			slider.value = value;
			slider.normalizedValue = normalizedValue;
			Write(slider.onValueChanged, onValueChanged, objects);
			return slider;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Slider slider = (Slider)obj;
				fillRect = slider.fillRect.GetMappedInstanceID();
				handleRect = slider.handleRect.GetMappedInstanceID();
				direction = slider.direction;
				minValue = slider.minValue;
				maxValue = slider.maxValue;
				wholeNumbers = slider.wholeNumbers;
				value = slider.value;
				normalizedValue = slider.normalizedValue;
				Read(onValueChanged, slider.onValueChanged);
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(fillRect, dependencies, objects, allowNulls);
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
				Slider slider = (Slider)obj;
				AddDependency(slider.fillRect, dependencies);
				AddDependency(slider.handleRect, dependencies);
				PersistentUnityEventBase persistentUnityEventBase = new PersistentUnityEventBase();
				persistentUnityEventBase.GetDependencies(slider.onValueChanged, dependencies);
			}
		}
	}
}
