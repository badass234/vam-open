using System.Collections.Generic;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Geometry.Tools
{
	public class MeshUtils
	{
		public static List<Vector3> GetWorldVertices(MeshFilter fiter)
		{
			List<Vector3> list = new List<Vector3>();
			Vector3[] vertices = fiter.sharedMesh.vertices;
			Vector3[] array = vertices;
			foreach (Vector3 position in array)
			{
				list.Add(fiter.transform.TransformPoint(position));
			}
			return list;
		}

		public static List<Vector3> GetVerticesInSpace(Mesh mesh, Matrix4x4 toWorld, Matrix4x4 toTransform)
		{
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < mesh.vertexCount; i++)
			{
				Vector3 point = mesh.vertices[i];
				Vector3 point2 = toWorld.MultiplyPoint3x4(point);
				Vector3 item = toTransform.MultiplyPoint3x4(point2);
				list.Add(item);
			}
			return list;
		}
	}
}
