using System.Collections.Generic;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Geometry.Tools
{
	public class ScalpProcessingTools
	{
		public const float Accuracy = 1E-05f;

		public static List<int> HairRootToScalpIndices(List<Vector3> scalpVertices, List<Vector3> hairVertices, int segments, float accuracy = 1E-05f)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < hairVertices.Count; i += segments)
			{
				for (int j = 0; j < scalpVertices.Count; j++)
				{
					if ((hairVertices[i] - scalpVertices[j]).sqrMagnitude < accuracy * accuracy)
					{
						list.Add(j);
						break;
					}
				}
			}
			return list;
		}

		public static List<int> ProcessIndices(List<int> scalpIndices, List<Vector3> scalpVertices, List<List<Vector3>> hairVerticesGroups, int segments, float accuracy = 1E-05f)
		{
			List<int> list = new List<int>();
			int num = 0;
			foreach (List<Vector3> hairVerticesGroup in hairVerticesGroups)
			{
				List<int> collection = ProcessIndicesForMesh(num, scalpVertices, scalpIndices, hairVerticesGroup, segments, accuracy);
				list.AddRange(collection);
				num += hairVerticesGroup.Count;
			}
			for (int i = 0; i < list.Count; i++)
			{
				list[i] /= segments;
			}
			return list;
		}

		private static List<int> ProcessIndicesForMesh(int startIndex, List<Vector3> scalpVertices, List<int> scalpIndices, List<Vector3> hairVertices, int segments, float accuracy = 1E-05f)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < scalpIndices.Count; i++)
			{
				int index = scalpIndices[i];
				Vector3 vector = scalpVertices[index];
				if (i % 3 == 0)
				{
					FixNotCompletedPolygon(list);
				}
				for (int j = 0; j < hairVertices.Count; j += segments)
				{
					Vector3 vector2 = hairVertices[j];
					if ((vector2 - vector).sqrMagnitude < accuracy * accuracy)
					{
						list.Add(startIndex + j);
						break;
					}
				}
			}
			FixNotCompletedPolygon(list);
			return list;
		}

		private static void FixNotCompletedPolygon(List<int> hairIndices)
		{
			int num = hairIndices.Count % 3;
			if (num > 0)
			{
				hairIndices.RemoveRange(hairIndices.Count - num, num);
			}
		}

		public static float MiddleDistanceBetweenPoints(Mesh mesh)
		{
			Vector3[] vertices = mesh.vertices;
			int[] indices = mesh.GetIndices(0);
			float num = 0f;
			int num2 = 0;
			for (int i = 0; i < Mathf.Min(500, indices.Length); i += 3)
			{
				Vector3 a = vertices[indices[i]];
				Vector3 b = vertices[indices[i + 1]];
				num += Vector3.Distance(a, b);
				num2++;
			}
			return num / (float)num2;
		}

		public static List<Vector3> ShiftToScalpRoot(List<Vector3> scalpVertices, List<Vector3> hairVertices, int segments)
		{
			for (int i = 0; i < hairVertices.Count; i += segments)
			{
				int index = 0;
				float num = float.MaxValue;
				for (int j = 0; j < scalpVertices.Count; j++)
				{
					float sqrMagnitude = (hairVertices[i] - scalpVertices[j]).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						index = j;
						num = sqrMagnitude;
					}
				}
				hairVertices[i] = scalpVertices[index];
			}
			return hairVertices;
		}
	}
}
