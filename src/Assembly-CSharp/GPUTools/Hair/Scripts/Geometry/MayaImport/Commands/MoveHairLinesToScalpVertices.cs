using System.Collections.Generic;
using GPUTools.Common.Scripts.Tools.Commands;
using GPUTools.Hair.Scripts.Geometry.MayaImport.Data;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Geometry.MayaImport.Commands
{
	public class MoveHairLinesToScalpVertices : ICacheCommand
	{
		private readonly MayaHairGeometryImporter importer;

		private readonly MayaHairData data;

		public MoveHairLinesToScalpVertices(MayaHairGeometryImporter importer)
		{
			this.importer = importer;
			data = importer.Data;
		}

		public void Cache()
		{
			int[] indices = importer.ScalpProvider.Mesh.GetIndices(0);
			Vector3[] vertices = importer.ScalpProvider.Mesh.vertices;
			List<Vector3> list = new List<Vector3>();
			List<int> list2 = new List<int>();
			for (int i = 0; i < indices.Length; i += 3)
			{
				Vector3 vector = data.TringlesCenters[i / 3];
				int num = FindStandIndex(data.Lines, vector, data.Segments);
				if (num == -1)
				{
					continue;
				}
				for (int j = 0; j < 3; j++)
				{
					Vector3 vector2 = vertices[indices[i + j]];
					Vector3 offset = vector2 - vector;
					int num2 = FindStandIndex(list, vector2, data.Segments);
					if (num2 == -1 || !CompareStands(list, num2, data.Lines, num, data.Segments))
					{
						list2.Add(list.Count / data.Segments);
						list.AddRange(CreateStandWithOffsetForRegion(data.Lines, offset, num, data.Segments));
					}
					else
					{
						list2.Add(num2 / data.Segments);
					}
				}
			}
			data.Vertices = list;
			data.Indices = list2.ToArray();
		}

		private bool CompareStands(List<Vector3> hairStands1, int stand1, List<Vector3> hairStands2, int stand2, int segments)
		{
			float num = importer.RegionThresholdDistance * importer.RegionThresholdDistance;
			for (int i = 0; i < segments; i++)
			{
				Vector3 vector = hairStands1[stand1 + i];
				Vector3 vector2 = hairStands2[stand2 + i];
				if ((vector - vector2).sqrMagnitude > num)
				{
					return false;
				}
			}
			return true;
		}

		private int FindStandIndex(List<Vector3> hairVertices, Vector3 vertex, int segments)
		{
			for (int i = 0; i < hairVertices.Count; i += segments)
			{
				if (hairVertices[i] == vertex)
				{
					return i;
				}
			}
			return -1;
		}

		private List<Vector3> CreateStandWithOffsetForRegion(List<Vector3> vertices, Vector3 offset, int start, int segments)
		{
			List<Vector3> list = new List<Vector3>();
			int num = start + segments;
			for (int i = start; i < num; i++)
			{
				list.Add(vertices[i] + offset);
			}
			return list;
		}
	}
}
