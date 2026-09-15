using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentCapsuleCollider : PersistentCollider
	{
		public Vector3 center;

		public float radius;

		public float height;

		public int direction;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			CapsuleCollider capsuleCollider = (CapsuleCollider)obj;
			capsuleCollider.center = center;
			capsuleCollider.radius = radius;
			capsuleCollider.height = height;
			capsuleCollider.direction = direction;
			return capsuleCollider;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				CapsuleCollider capsuleCollider = (CapsuleCollider)obj;
				center = capsuleCollider.center;
				radius = capsuleCollider.radius;
				height = capsuleCollider.height;
				direction = capsuleCollider.direction;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
