using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentBoxCollider2D : PersistentCollider2D
	{
		public Vector2 size;

		public float edgeRadius;

		public bool autoTiling;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			BoxCollider2D boxCollider2D = (BoxCollider2D)obj;
			boxCollider2D.size = size;
			boxCollider2D.edgeRadius = edgeRadius;
			boxCollider2D.autoTiling = autoTiling;
			return boxCollider2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				BoxCollider2D boxCollider2D = (BoxCollider2D)obj;
				size = boxCollider2D.size;
				edgeRadius = boxCollider2D.edgeRadius;
				autoTiling = boxCollider2D.autoTiling;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
