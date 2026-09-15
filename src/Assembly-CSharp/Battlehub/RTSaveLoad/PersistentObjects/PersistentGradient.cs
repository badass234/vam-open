using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentGradient : PersistentData
	{
		public GradientColorKey[] colorKeys;

		public GradientAlphaKey[] alphaKeys;

		public uint mode;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Gradient gradient = (Gradient)obj;
			gradient.colorKeys = colorKeys;
			gradient.alphaKeys = alphaKeys;
			gradient.mode = (GradientMode)mode;
			return gradient;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Gradient gradient = (Gradient)obj;
				colorKeys = gradient.colorKeys;
				alphaKeys = gradient.alphaKeys;
				mode = (uint)gradient.mode;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
