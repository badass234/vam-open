using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1084, typeof(PersistentRectTransform))]
	public class PersistentTransform : PersistentComponent
	{
		public Vector3 position;

		public Quaternion rotation;

		public Vector3 localScale;

		public long parent;

		public int hierarchyCapacity;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			Transform transform = (Transform)obj;
			transform.SetParent((Transform)objects.Get(parent), false);
			transform.position = position;
			transform.rotation = rotation;
			transform.localScale = localScale;
			return transform;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			Transform transform = (Transform)obj;
			position = transform.position;
			rotation = transform.rotation;
			localScale = transform.localScale;
			parent = transform.parent.GetMappedInstanceID();
		}
	}
}
