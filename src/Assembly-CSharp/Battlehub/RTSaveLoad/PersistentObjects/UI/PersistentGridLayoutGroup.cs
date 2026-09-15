using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad.PersistentObjects.UI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentGridLayoutGroup : PersistentLayoutGroup
	{
		public uint startCorner;

		public uint startAxis;

		public Vector2 cellSize;

		public Vector2 spacing;

		public uint constraint;

		public int constraintCount;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			GridLayoutGroup gridLayoutGroup = (GridLayoutGroup)obj;
			gridLayoutGroup.startCorner = (GridLayoutGroup.Corner)startCorner;
			gridLayoutGroup.startAxis = (GridLayoutGroup.Axis)startAxis;
			gridLayoutGroup.cellSize = cellSize;
			gridLayoutGroup.spacing = spacing;
			gridLayoutGroup.constraint = (GridLayoutGroup.Constraint)constraint;
			gridLayoutGroup.constraintCount = constraintCount;
			return gridLayoutGroup;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				GridLayoutGroup gridLayoutGroup = (GridLayoutGroup)obj;
				startCorner = (uint)gridLayoutGroup.startCorner;
				startAxis = (uint)gridLayoutGroup.startAxis;
				cellSize = gridLayoutGroup.cellSize;
				spacing = gridLayoutGroup.spacing;
				constraint = (uint)gridLayoutGroup.constraint;
				constraintCount = gridLayoutGroup.constraintCount;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
