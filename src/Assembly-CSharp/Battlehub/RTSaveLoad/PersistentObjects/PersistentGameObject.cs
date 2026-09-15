using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentGameObject : PersistentObject
	{
		public int layer;

		public bool isStatic;

		public string tag;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			GameObject gameObject = (GameObject)obj;
			gameObject.layer = layer;
			gameObject.isStatic = isStatic;
			try
			{
				gameObject.tag = tag;
			}
			catch (UnityException ex)
			{
				Debug.LogWarning(ex.Message);
			}
			return gameObject;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				GameObject gameObject = (GameObject)obj;
				layer = gameObject.layer;
				isStatic = gameObject.isStatic;
				tag = gameObject.tag;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
