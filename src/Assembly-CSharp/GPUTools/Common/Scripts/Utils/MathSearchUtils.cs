using System.Collections.Generic;
using UnityEngine;

namespace GPUTools.Common.Scripts.Utils
{
	public class MathSearchUtils
	{
		public static List<Vector3> FindCloseVertices(Vector3[] vertices, Vector3 testVertex, int count)
		{
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < count; i++)
			{
				Vector3 item = FindCloseVertex(vertices, testVertex, list);
				list.Add(item);
			}
			return list;
		}

		public static Vector3 FindCloseVertex(Vector3[] vertices, Vector3 testVertex, List<Vector3> ignoreList = null)
		{
			float num = float.PositiveInfinity;
			Vector3 result = vertices[0];
			foreach (Vector3 vector in vertices)
			{
				if (ignoreList == null || !ignoreList.Contains(vector))
				{
					float sqrMagnitude = (vector - testVertex).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						result = vector;
					}
				}
			}
			return result;
		}

		public static Vector3 FindCloseVertex(List<Vector3> vertices, Vector3 testVertex, List<Vector3> ignoreList = null)
		{
			float num = float.PositiveInfinity;
			Vector3 result = vertices[0];
			foreach (Vector3 vertex in vertices)
			{
				if (ignoreList == null || !ignoreList.Contains(vertex))
				{
					float sqrMagnitude = (vertex - testVertex).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						result = vertex;
					}
				}
			}
			return result;
		}
	}
}
