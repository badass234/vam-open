using System.Collections.Generic;
using UnityEngine;

namespace GPUTools.Common.Scripts.Tools.SpatialSearch
{
	public class SearchVoxel
	{
		private readonly List<Vector3> vertices;

		private readonly List<int> mineIndices;

		private readonly Bounds bounds;

		public Bounds Bounds => bounds;

		public int TotalVertices => mineIndices.Count;

		public SearchVoxel(List<Vector3> vertices, Bounds bounds)
		{
			this.bounds = bounds;
			this.vertices = vertices;
			mineIndices = SearchMineIndices();
		}

		private List<int> SearchMineIndices()
		{
			List<int> list = new List<int>();
			for (int i = 0; i < vertices.Count; i++)
			{
				Vector3 point = vertices[i];
				if (bounds.Contains(point))
				{
					list.Add(i);
				}
			}
			return list;
		}

		public List<int> SearchInSphere(Vector3 center, float radius)
		{
			List<int> list = new List<int>();
			foreach (int mineIndex in mineIndices)
			{
				Vector3 vector = vertices[mineIndex];
				if ((vector - center).sqrMagnitude < radius * radius)
				{
					list.Add(mineIndex);
				}
			}
			return list;
		}

		public List<int> SearchInSphere(Ray ray, float radius)
		{
			List<int> list = new List<int>();
			foreach (int mineIndex in mineIndices)
			{
				Vector3 vector = vertices[mineIndex];
				float sqrMagnitude = Vector3.Cross(ray.direction, vector - ray.origin).sqrMagnitude;
				if (sqrMagnitude < radius * radius)
				{
					list.Add(mineIndex);
				}
			}
			return list;
		}

		public void DebugDraw(Transform transforms)
		{
		}
	}
}
