using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad.PersistentObjects.UI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentDropdown : PersistentSelectable
	{
		public long template;

		public long captionText;

		public long captionImage;

		public long itemText;

		public long itemImage;

		public PersistentUnityEventBase onValueChanged;

		public PersistentOptionData[] options;

		public int value;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Dropdown dropdown = (Dropdown)obj;
			dropdown.template = (RectTransform)objects.Get(template);
			dropdown.captionText = (Text)objects.Get(captionText);
			dropdown.captionImage = (Image)objects.Get(captionImage);
			dropdown.itemText = (Text)objects.Get(itemText);
			dropdown.itemImage = (Image)objects.Get(itemImage);
			dropdown.onValueChanged = Write(dropdown.onValueChanged, onValueChanged, objects);
			dropdown.value = value;
			if (options != null)
			{
				List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
				for (int i = 0; i < options.Length; i++)
				{
					PersistentOptionData persistentOptionData = options[i];
					Dropdown.OptionData optionData = new Dropdown.OptionData();
					persistentOptionData.WriteTo(optionData, objects);
					list.Add(optionData);
				}
				dropdown.options = list;
			}
			else
			{
				dropdown.options = null;
			}
			return dropdown;
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(template, dependencies, objects, allowNulls);
			AddDependency(captionText, dependencies, objects, allowNulls);
			AddDependency(captionImage, dependencies, objects, allowNulls);
			AddDependency(itemText, dependencies, objects, allowNulls);
			AddDependency(itemImage, dependencies, objects, allowNulls);
			if (onValueChanged != null)
			{
				onValueChanged.FindDependencies(dependencies, objects, allowNulls);
			}
			if (options != null)
			{
				for (int i = 0; i < options.Length; i++)
				{
					PersistentOptionData persistentOptionData = options[i];
					persistentOptionData.FindDependencies(dependencies, objects, allowNulls);
				}
			}
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj == null)
			{
				return;
			}
			Dropdown dropdown = (Dropdown)obj;
			template = dropdown.template.GetMappedInstanceID();
			captionText = dropdown.captionText.GetMappedInstanceID();
			captionImage = dropdown.captionImage.GetMappedInstanceID();
			itemText = dropdown.itemText.GetMappedInstanceID();
			itemImage = dropdown.itemImage.GetMappedInstanceID();
			onValueChanged = Read(onValueChanged, dropdown.onValueChanged);
			if (dropdown.options != null)
			{
				options = new PersistentOptionData[dropdown.options.Count];
				for (int i = 0; i < dropdown.options.Count; i++)
				{
					PersistentOptionData persistentOptionData = new PersistentOptionData();
					persistentOptionData.ReadFrom(dropdown.options[i]);
					options[i] = persistentOptionData;
				}
			}
			else
			{
				options = null;
			}
			value = dropdown.value;
		}
	}
}
