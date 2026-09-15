using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentCapsuleCollider2D : PersistentCollider2D
	{
		public Vector2 size;

		public uint direction;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			CapsuleCollider2D capsuleCollider2D = (CapsuleCollider2D)obj;
			capsuleCollider2D.size = size;
			capsuleCollider2D.direction = (CapsuleDirection2D)direction;
			return capsuleCollider2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				CapsuleCollider2D capsuleCollider2D = (CapsuleCollider2D)obj;
				size = capsuleCollider2D.size;
				direction = (uint)capsuleCollider2D.direction;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
