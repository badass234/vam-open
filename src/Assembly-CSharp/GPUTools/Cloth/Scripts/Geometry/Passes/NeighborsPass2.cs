using System.Collections.Generic;
using System.Linq;
using GPUTools.Cloth.Scripts.Geometry.Data;
using GPUTools.Common.Scripts.Tools.Commands;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Geometry.Passes
{
	public class NeighborsPass2 : ICacheCommand
	{
		private readonly ClothGeometryData data;

		private readonly Mesh mesh;

		public NeighborsPass2(ClothSettings settings)
		{
			mesh = settings.MeshProvider.MeshForImport;
			data = settings.GeometryData;
		}

		public void Cache()
		{
			List<int>[] list = CreateAllTrianglesList(data.AllTringles, data.MeshToPhysicsVerticesMap);
			SortList(list);
			data.ParticleToNeiborCounts = CreateConts(list);
			data.ParticleToNeibor = ConvertTo1DArray(list);
		}

		private List<int>[] CreateAllTrianglesList(int[] triangles, int[] meshToPhysicsVerticesMap)
		{
			List<int>[] array = new List<int>[data.MeshToPhysicsVerticesMap.Length];
			for (int i = 0; i < triangles.Length; i += 3)
			{
				int num = triangles[i];
				int num2 = triangles[i + 1];
				int num3 = triangles[i + 2];
				int num4 = meshToPhysicsVerticesMap[num];
				int num5 = meshToPhysicsVerticesMap[num2];
				int num6 = meshToPhysicsVerticesMap[num3];
				Add(array, num, num5, num6);
				Add(array, num2, num6, num4);
				Add(array, num3, num4, num5);
			}
			return array;
		}

		private void Add(List<int>[] physList, int p, int p1, int p2)
		{
			if (physList[p] == null)
			{
				physList[p] = new List<int>();
			}
			if (!physList[p].Contains(p1))
			{
				physList[p].Add(p1);
			}
			if (!physList[p].Contains(p2))
			{
				physList[p].Add(p2);
			}
		}

		private int[] CreateConts(List<int>[] list)
		{
			int[] array = new int[list.Length + 1];
			int num = 0;
			for (int i = 1; i < list.Length + 1; i++)
			{
				num = (array[i] = num + list[i - 1].Count);
			}
			return array;
		}

		private void SortList(List<int>[] list)
		{
			Vector3[] vertices = mesh.vertices;
			Vector3[] normals = mesh.normals;
			for (int j = 0; j < list.Length; j++)
			{
				List<int> list2 = list[j];
				Vector3 normal = normals[j];
				Vector3 vertex = vertices[j];
				list2.Sort(delegate(int i1, int i2)
				{
					Vector3 lhs = data.Particles[i1] - vertex;
					Vector3 rhs = data.Particles[i2] - vertex;
					Vector3 rhs2 = Vector3.Cross(lhs, rhs);
					return (int)Mathf.Sign(Vector3.Dot(normal, rhs2));
				});
			}
		}

		private int[] ConvertTo1DArray(List<int>[] list)
		{
			return list.SelectMany((List<int> neibors) => neibors).ToArray();
		}
	}
}
