using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentRenderTexture : PersistentTexture
	{
		public int depth;

		public bool isPowerOfTwo;

		public uint format;

		public bool useMipMap;

		public bool autoGenerateMips;

		public int volumeDepth;

		public int antiAliasing;

		public bool enableRandomWrite;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			RenderTexture renderTexture = (RenderTexture)obj;
			renderTexture.depth = depth;
			renderTexture.isPowerOfTwo = isPowerOfTwo;
			renderTexture.format = (RenderTextureFormat)format;
			renderTexture.useMipMap = useMipMap;
			renderTexture.autoGenerateMips = autoGenerateMips;
			renderTexture.volumeDepth = volumeDepth;
			renderTexture.antiAliasing = antiAliasing;
			renderTexture.enableRandomWrite = enableRandomWrite;
			return renderTexture;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				RenderTexture renderTexture = (RenderTexture)obj;
				depth = renderTexture.depth;
				isPowerOfTwo = renderTexture.isPowerOfTwo;
				format = (uint)renderTexture.format;
				useMipMap = renderTexture.useMipMap;
				autoGenerateMips = renderTexture.autoGenerateMips;
				volumeDepth = renderTexture.volumeDepth;
				antiAliasing = renderTexture.antiAliasing;
				enableRandomWrite = renderTexture.enableRandomWrite;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
