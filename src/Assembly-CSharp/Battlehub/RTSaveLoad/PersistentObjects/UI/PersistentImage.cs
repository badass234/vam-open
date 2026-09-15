using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad.PersistentObjects.UI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentImage : PersistentMaskableGraphic
	{
		public long sprite;

		public long overrideSprite;

		public uint type;

		public bool preserveAspect;

		public bool fillCenter;

		public uint fillMethod;

		public float fillAmount;

		public bool fillClockwise;

		public int fillOrigin;

		public float alphaHitTestMinimumThreshold;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Image image = (Image)obj;
			image.sprite = (Sprite)objects.Get(sprite);
			image.overrideSprite = (Sprite)objects.Get(overrideSprite);
			image.type = (Image.Type)type;
			image.preserveAspect = preserveAspect;
			image.fillCenter = fillCenter;
			image.fillMethod = (Image.FillMethod)fillMethod;
			image.fillAmount = fillAmount;
			image.fillClockwise = fillClockwise;
			image.fillOrigin = fillOrigin;
			image.alphaHitTestMinimumThreshold = alphaHitTestMinimumThreshold;
			return image;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Image image = (Image)obj;
				sprite = image.sprite.GetMappedInstanceID();
				overrideSprite = image.overrideSprite.GetMappedInstanceID();
				type = (uint)image.type;
				preserveAspect = image.preserveAspect;
				fillCenter = image.fillCenter;
				fillMethod = (uint)image.fillMethod;
				fillAmount = image.fillAmount;
				fillClockwise = image.fillClockwise;
				fillOrigin = image.fillOrigin;
				alphaHitTestMinimumThreshold = image.alphaHitTestMinimumThreshold;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(sprite, dependencies, objects, allowNulls);
			AddDependency(overrideSprite, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				Image image = (Image)obj;
				AddDependency(image.sprite, dependencies);
				AddDependency(image.overrideSprite, dependencies);
			}
		}
	}
}
