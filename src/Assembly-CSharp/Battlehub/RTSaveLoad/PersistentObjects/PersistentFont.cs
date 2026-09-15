using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentFont : PersistentObject
	{
		public long material;

		public string[] fontNames;

		public CharacterInfo[] characterInfo;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Font font = (Font)obj;
			font.material = (Material)objects.Get(material);
			font.fontNames = fontNames;
			font.characterInfo = characterInfo;
			return font;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Font font = (Font)obj;
				material = font.material.GetMappedInstanceID();
				fontNames = font.fontNames;
				characterInfo = font.characterInfo;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(material, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				Font font = (Font)obj;
				AddDependency(font.material, dependencies);
			}
		}
	}
}
