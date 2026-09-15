using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentProjector : PersistentBehaviour
	{
		public float nearClipPlane;

		public float farClipPlane;

		public float fieldOfView;

		public float aspectRatio;

		public bool orthographic;

		public float orthographicSize;

		public int ignoreLayers;

		public long material;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Projector projector = (Projector)obj;
			projector.nearClipPlane = nearClipPlane;
			projector.farClipPlane = farClipPlane;
			projector.fieldOfView = fieldOfView;
			projector.aspectRatio = aspectRatio;
			projector.orthographic = orthographic;
			projector.orthographicSize = orthographicSize;
			projector.ignoreLayers = ignoreLayers;
			projector.material = (Material)objects.Get(material);
			return projector;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Projector projector = (Projector)obj;
				nearClipPlane = projector.nearClipPlane;
				farClipPlane = projector.farClipPlane;
				fieldOfView = projector.fieldOfView;
				aspectRatio = projector.aspectRatio;
				orthographic = projector.orthographic;
				orthographicSize = projector.orthographicSize;
				ignoreLayers = projector.ignoreLayers;
				material = projector.material.GetMappedInstanceID();
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(material, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				Projector projector = (Projector)obj;
				AddDependency(projector.material, dependencies);
			}
		}
	}
}
