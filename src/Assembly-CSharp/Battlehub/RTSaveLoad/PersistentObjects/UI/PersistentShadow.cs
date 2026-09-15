using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad.PersistentObjects.UI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1136, typeof(PersistentOutline))]
	public class PersistentShadow : PersistentBaseMeshEffect
	{
		public Color effectColor;

		public Vector2 effectDistance;

		public bool useGraphicAlpha;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Shadow shadow = (Shadow)obj;
			shadow.effectColor = effectColor;
			shadow.effectDistance = effectDistance;
			shadow.useGraphicAlpha = useGraphicAlpha;
			return shadow;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Shadow shadow = (Shadow)obj;
				effectColor = shadow.effectColor;
				effectDistance = shadow.effectDistance;
				useGraphicAlpha = shadow.useGraphicAlpha;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
