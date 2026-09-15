using System.Collections.Generic;
using GPUTools.Common.Scripts.Tools.Commands;
using GPUTools.Common.Scripts.Utils;
using GPUTools.Hair.Scripts.Geometry.MayaImport.Data;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Geometry.MayaImport.Commands
{
	public class AssignHairLinesToTriangles : ICacheCommand
	{
		private readonly MayaHairGeometryImporter importer;

		private readonly MayaHairData data;

		public AssignHairLinesToTriangles(MayaHairGeometryImporter importer)
		{
			this.importer = importer;
			data = importer.Data;
		}

		public void Cache()
		{
			data.TringlesCenters = ComputeTringlesCenters();
			data.Lines = Assign(data.TringlesCenters);
		}

		private List<Vector3> Assign(List<Vector3> scalpTringlesCenters)
		{
			List<Vector3> lines = data.Lines;
			List<Vector3> list = new List<Vector3>();
			List<Vector3> list2 = new List<Vector3>();
			for (int i = 0; i < lines.Count; i += data.Segments)
			{
				Vector3 vector = lines[i];
				Vector3 vector2 = MathSearchUtils.FindCloseVertex(scalpTringlesCenters, vector);
				Vector3 offset = vector2 - vector;
				if (!list2.Contains(vector2))
				{
					List<Vector3> collection = CreateStandWithOffsetForRegion(lines, offset, i, i + data.Segments);
					list.AddRange(collection);
					list2.Add(vector2);
				}
			}
			return list;
		}

		private List<Vector3> ComputeTringlesCenters()
		{
			int[] indices = importer.ScalpProvider.Mesh.GetIndices(0);
			Vector3[] vertices = importer.ScalpProvider.Mesh.vertices;
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < indices.Length; i += 3)
			{
				Vector3 vector = vertices[indices[i]];
				Vector3 vector2 = vertices[indices[i + 1]];
				Vector3 vector3 = vertices[indices[i + 2]];
				list.Add((vector + vector2 + vector3) / 3f);
			}
			return list;
		}

		private List<Vector3> CreateStandWithOffsetForRegion(List<Vector3> vertices, Vector3 offset, int start, int end)
		{
			List<Vector3> list = new List<Vector3>();
			for (int i = start; i < end; i++)
			{
				list.Add(vertices[i] + offset);
			}
			return list;
		}
	}
}
