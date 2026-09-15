using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad.PersistentObjects.UI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentOptionData : PersistentData
	{
		public string text;

		public long image;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Dropdown.OptionData optionData = (Dropdown.OptionData)obj;
			optionData.text = text;
			optionData.image = (Sprite)objects.Get(image);
			return optionData;
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(image, dependencies, objects, allowNulls);
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Dropdown.OptionData optionData = (Dropdown.OptionData)obj;
				text = optionData.text;
				image = optionData.image.GetMappedInstanceID();
			}
		}
	}
}
