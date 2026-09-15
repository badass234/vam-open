using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentCompositeCollider2D : PersistentCollider2D
	{
		public uint geometryType;

		public uint generationType;

		public float vertexDistance;

		public float edgeRadius;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			CompositeCollider2D compositeCollider2D = (CompositeCollider2D)obj;
			compositeCollider2D.geometryType = (CompositeCollider2D.GeometryType)geometryType;
			compositeCollider2D.generationType = (CompositeCollider2D.GenerationType)generationType;
			compositeCollider2D.vertexDistance = vertexDistance;
			compositeCollider2D.edgeRadius = edgeRadius;
			return compositeCollider2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				CompositeCollider2D compositeCollider2D = (CompositeCollider2D)obj;
				geometryType = (uint)compositeCollider2D.geometryType;
				generationType = (uint)compositeCollider2D.generationType;
				vertexDistance = compositeCollider2D.vertexDistance;
				edgeRadius = compositeCollider2D.edgeRadius;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
