using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentSplatPrototype : PersistentData
	{
		public long texture;

		public long normalMap;

		public Vector2 tileSize;

		public Vector2 tileOffset;

		public Color specular;

		public float metallic;

		public float smoothness;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			SplatPrototype splatPrototype = (SplatPrototype)obj;
			splatPrototype.texture = (Texture2D)objects.Get(texture);
			splatPrototype.normalMap = (Texture2D)objects.Get(normalMap);
			splatPrototype.tileSize = tileSize;
			splatPrototype.tileOffset = tileOffset;
			splatPrototype.specular = specular;
			splatPrototype.metallic = metallic;
			splatPrototype.smoothness = smoothness;
			return splatPrototype;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				SplatPrototype splatPrototype = (SplatPrototype)obj;
				texture = splatPrototype.texture.GetMappedInstanceID();
				normalMap = splatPrototype.normalMap.GetMappedInstanceID();
				tileSize = splatPrototype.tileSize;
				tileOffset = splatPrototype.tileOffset;
				specular = splatPrototype.specular;
				metallic = splatPrototype.metallic;
				smoothness = splatPrototype.smoothness;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(texture, dependencies, objects, allowNulls);
			AddDependency(normalMap, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				SplatPrototype splatPrototype = (SplatPrototype)obj;
				AddDependency(splatPrototype.texture, dependencies);
				AddDependency(splatPrototype.normalMap, dependencies);
			}
		}
	}
}
