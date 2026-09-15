using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentWebCamTexture : PersistentTexture
	{
		public string deviceName;

		public float requestedFPS;

		public int requestedWidth;

		public int requestedHeight;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			WebCamTexture webCamTexture = (WebCamTexture)obj;
			webCamTexture.deviceName = deviceName;
			webCamTexture.requestedFPS = requestedFPS;
			webCamTexture.requestedWidth = requestedWidth;
			webCamTexture.requestedHeight = requestedHeight;
			return webCamTexture;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				WebCamTexture webCamTexture = (WebCamTexture)obj;
				deviceName = webCamTexture.deviceName;
				requestedFPS = webCamTexture.requestedFPS;
				requestedWidth = webCamTexture.requestedWidth;
				requestedHeight = webCamTexture.requestedHeight;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
