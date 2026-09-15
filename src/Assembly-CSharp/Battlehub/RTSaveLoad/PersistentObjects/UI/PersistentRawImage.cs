using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad.PersistentObjects.UI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentRawImage : PersistentMaskableGraphic
	{
		public long texture;

		public Rect uvRect;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			RawImage rawImage = (RawImage)obj;
			rawImage.texture = (Texture)objects.Get(texture);
			rawImage.uvRect = uvRect;
			return rawImage;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				RawImage rawImage = (RawImage)obj;
				texture = rawImage.texture.GetMappedInstanceID();
				uvRect = rawImage.uvRect;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(texture, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				RawImage rawImage = (RawImage)obj;
				AddDependency(rawImage.texture, dependencies);
			}
		}
	}
}
