using System.Collections.Generic;
using UnityEngine;

namespace GPUTools.Common.Scripts.Tools.SpatialSearch
{
	public class SpatialSearcher
	{
		private readonly List<SearchVoxel> voxels;

		private readonly List<Vector3> vertices;

		private readonly Bounds bounds;

		private FixedList<int> fixedList;

		public SpatialSearcher(List<Vector3> vertices, Bounds bounds, int splitX, int splitY, int splitZ)
		{
			this.vertices = vertices;
			this.bounds = bounds;
			voxels = CreateVoxels(splitX, splitY, splitZ);
			fixedList = new FixedList<int>(vertices.Count);
		}

		private List<SearchVoxel> CreateVoxels(int splitX, int splitY, int splitZ)
		{
			Vector3 size = new Vector3(bounds.size.x / (float)splitX, bounds.size.y / (float)splitY, bounds.size.z / (float)splitZ);
			List<SearchVoxel> list = new List<SearchVoxel>();
			for (int i = 0; i <= splitX; i++)
			{
				for (int j = 0; j <= splitY; j++)
				{
					for (int k = 0; k <= splitZ; k++)
					{
						Vector3 center = bounds.center + new Vector3(size.x * (float)i, size.y * (float)j, size.z * (float)k) - bounds.size * 0.5f;
						SearchVoxel searchVoxel = new SearchVoxel(bounds: new Bounds(center, size), vertices: vertices);
						if (searchVoxel.TotalVertices > 0)
						{
							list.Add(searchVoxel);
						}
					}
				}
			}
			return list;
		}

		public FixedList<int> SearchInSphereSlow(Vector3 center, float radius)
		{
			fixedList.Reset();
			for (int i = 0; i < vertices.Count; i++)
			{
				Vector3 vector = vertices[i];
				if ((center - vector).sqrMagnitude < radius * radius)
				{
					fixedList.Add(i);
				}
			}
			return fixedList;
		}

		public List<int> SearchInSphereSlow(Ray ray, float radius)
		{
			List<int> list = new List<int>();
			float num = radius * radius;
			for (int i = 0; i < vertices.Count; i++)
			{
				Vector3 vector = vertices[i];
				float sqrMagnitude = Vector3.Cross(ray.direction, vector - ray.origin).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					list.Add(i);
				}
			}
			return list;
		}

		public List<int> SearchInSphere(Vector3 center, float radius)
		{
			List<SearchVoxel> list = SearchVoxelsInSphere(center, radius);
			List<int> list2 = new List<int>();
			foreach (SearchVoxel item in list)
			{
				list2.AddRange(item.SearchInSphere(center, radius));
			}
			return list2;
		}

		public List<int> SearchInSphere(Ray ray, float radius)
		{
			List<SearchVoxel> list = SearchVoxelsInSphere(ray, radius);
			List<int> list2 = new List<int>();
			foreach (SearchVoxel item in list)
			{
				list2.AddRange(item.SearchInSphere(ray, radius));
			}
			return list2;
		}

		private List<SearchVoxel> SearchVoxelsInSphere(Vector3 center, float radius)
		{
			List<SearchVoxel> list = new List<SearchVoxel>();
			foreach (SearchVoxel voxel in voxels)
			{
				Vector3 vector = voxel.Bounds.ClosestPoint(center);
				if ((vector - center).sqrMagnitude < radius * radius)
				{
					list.Add(voxel);
				}
			}
			return list;
		}

		private List<SearchVoxel> SearchVoxelsInSphere(Ray ray, float radius)
		{
			List<SearchVoxel> list = new List<SearchVoxel>();
			foreach (SearchVoxel voxel in voxels)
			{
				if (voxel.Bounds.IntersectRay(ray))
				{
					list.Add(voxel);
				}
			}
			return list;
		}

		public void DebugDraw(Transform transform)
		{
			foreach (SearchVoxel voxel in voxels)
			{
				voxel.DebugDraw(transform);
			}
		}
	}
}
