using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentPolygonCollider2D : PersistentCollider2D
	{
		public Vector2[] points;

		public int pathCount;

		public bool autoTiling;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			PolygonCollider2D polygonCollider2D = (PolygonCollider2D)obj;
			polygonCollider2D.points = points;
			polygonCollider2D.pathCount = pathCount;
			polygonCollider2D.autoTiling = autoTiling;
			return polygonCollider2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				PolygonCollider2D polygonCollider2D = (PolygonCollider2D)obj;
				points = polygonCollider2D.points;
				pathCount = polygonCollider2D.pathCount;
				autoTiling = polygonCollider2D.autoTiling;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
