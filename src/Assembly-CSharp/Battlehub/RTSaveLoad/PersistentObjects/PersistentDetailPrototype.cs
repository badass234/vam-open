using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentDetailPrototype : PersistentData
	{
		public long prototype;

		public long prototypeTexture;

		public float minWidth;

		public float maxWidth;

		public float minHeight;

		public float maxHeight;

		public float noiseSpread;

		public float bendFactor;

		public Color healthyColor;

		public Color dryColor;

		public uint renderMode;

		public bool usePrototypeMesh;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			DetailPrototype detailPrototype = (DetailPrototype)obj;
			detailPrototype.prototype = (GameObject)objects.Get(prototype);
			detailPrototype.prototypeTexture = (Texture2D)objects.Get(prototypeTexture);
			detailPrototype.minWidth = minWidth;
			detailPrototype.maxWidth = maxWidth;
			detailPrototype.minHeight = minHeight;
			detailPrototype.maxHeight = maxHeight;
			detailPrototype.noiseSpread = noiseSpread;
			detailPrototype.bendFactor = bendFactor;
			detailPrototype.healthyColor = healthyColor;
			detailPrototype.dryColor = dryColor;
			detailPrototype.renderMode = (DetailRenderMode)renderMode;
			detailPrototype.usePrototypeMesh = usePrototypeMesh;
			return detailPrototype;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				DetailPrototype detailPrototype = (DetailPrototype)obj;
				prototype = detailPrototype.prototype.GetMappedInstanceID();
				prototypeTexture = detailPrototype.prototypeTexture.GetMappedInstanceID();
				minWidth = detailPrototype.minWidth;
				maxWidth = detailPrototype.maxWidth;
				minHeight = detailPrototype.minHeight;
				maxHeight = detailPrototype.maxHeight;
				noiseSpread = detailPrototype.noiseSpread;
				bendFactor = detailPrototype.bendFactor;
				healthyColor = detailPrototype.healthyColor;
				dryColor = detailPrototype.dryColor;
				renderMode = (uint)detailPrototype.renderMode;
				usePrototypeMesh = detailPrototype.usePrototypeMesh;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(prototype, dependencies, objects, allowNulls);
			AddDependency(prototypeTexture, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				DetailPrototype detailPrototype = (DetailPrototype)obj;
				AddDependency(detailPrototype.prototype, dependencies);
				AddDependency(detailPrototype.prototypeTexture, dependencies);
			}
		}
	}
}
