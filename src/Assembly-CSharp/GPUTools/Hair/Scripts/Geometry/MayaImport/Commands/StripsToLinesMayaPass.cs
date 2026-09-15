using System.Collections.Generic;
using GPUTools.Common.Scripts.Tools.Commands;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Geometry.MayaImport.Commands
{
	public class StripsToLinesMayaPass : ICacheCommand
	{
		private readonly MayaHairGeometryImporter importer;

		public StripsToLinesMayaPass(MayaHairGeometryImporter importer)
		{
			this.importer = importer;
		}

		public void Cache()
		{
			MeshFilter[] componentsInChildren = importer.HairContainer.GetComponentsInChildren<MeshFilter>();
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				List<Vector3> list2 = CacheStand(componentsInChildren[i]);
				list.AddRange(list2);
				importer.Data.Segments = list2.Count;
			}
			importer.Data.Lines = list;
		}

		private List<Vector3> CacheStand(MeshFilter meshFilter)
		{
			Vector3[] vertices = meshFilter.sharedMesh.vertices;
			int[] triangles = meshFilter.sharedMesh.triangles;
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < triangles.Length; i += 6)
			{
				int num = triangles[i + 2];
				Vector3 item = ToScalpSpace(meshFilter, vertices[num]);
				list.Add(item);
			}
			return list;
		}

		private Vector3 ToScalpSpace(MeshFilter filter, Vector3 point)
		{
			Matrix4x4 inverse = importer.ScalpProvider.ToWorldMatrix.inverse;
			Vector3 point2 = filter.transform.TransformPoint(point);
			return inverse.MultiplyPoint3x4(point2);
		}
	}
}
