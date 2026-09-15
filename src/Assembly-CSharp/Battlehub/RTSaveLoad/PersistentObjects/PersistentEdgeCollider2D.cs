using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentEdgeCollider2D : PersistentCollider2D
	{
		public float edgeRadius;

		public Vector2[] points;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			EdgeCollider2D edgeCollider2D = (EdgeCollider2D)obj;
			edgeCollider2D.edgeRadius = edgeRadius;
			edgeCollider2D.points = points;
			return edgeCollider2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				EdgeCollider2D edgeCollider2D = (EdgeCollider2D)obj;
				edgeRadius = edgeCollider2D.edgeRadius;
				points = edgeCollider2D.points;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
