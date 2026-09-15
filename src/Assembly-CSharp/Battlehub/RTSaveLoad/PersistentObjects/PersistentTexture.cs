using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1074, typeof(PersistentProceduralTexture))]
	[ProtoInclude(1075, typeof(PersistentTexture2D))]
	[ProtoInclude(1076, typeof(PersistentCubemap))]
	[ProtoInclude(1077, typeof(PersistentTexture3D))]
	[ProtoInclude(1078, typeof(PersistentTexture2DArray))]
	[ProtoInclude(1079, typeof(PersistentCubemapArray))]
	[ProtoInclude(1080, typeof(PersistentSparseTexture))]
	[ProtoInclude(1081, typeof(PersistentRenderTexture))]
	[ProtoInclude(1082, typeof(PersistentMovieTexture))]
	[ProtoInclude(1083, typeof(PersistentWebCamTexture))]
	public class PersistentTexture : PersistentObject
	{
		public int width;

		public int height;

		public TextureDimension dimension;

		public FilterMode filterMode;

		public int anisoLevel;

		public TextureWrapMode wrapMode;

		public float mipMapBias;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Texture texture = (Texture)obj;
			texture.filterMode = filterMode;
			texture.anisoLevel = anisoLevel;
			texture.wrapMode = wrapMode;
			texture.mipMapBias = mipMapBias;
			return texture;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Texture texture = (Texture)obj;
				filterMode = texture.filterMode;
				anisoLevel = texture.anisoLevel;
				wrapMode = texture.wrapMode;
				mipMapBias = texture.mipMapBias;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
