using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentLensFlare : PersistentBehaviour
	{
		public long flare;

		public float brightness;

		public float fadeSpeed;

		public Color color;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			LensFlare lensFlare = (LensFlare)obj;
			lensFlare.flare = (Flare)objects.Get(flare);
			lensFlare.brightness = brightness;
			lensFlare.fadeSpeed = fadeSpeed;
			lensFlare.color = color;
			return lensFlare;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				LensFlare lensFlare = (LensFlare)obj;
				flare = lensFlare.flare.GetMappedInstanceID();
				brightness = lensFlare.brightness;
				fadeSpeed = lensFlare.fadeSpeed;
				color = lensFlare.color;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(flare, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				LensFlare lensFlare = (LensFlare)obj;
				AddDependency(lensFlare.flare, dependencies);
			}
		}
	}
}
