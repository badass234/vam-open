using System;
using System.Collections.Generic;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Geometry.Create
{
	[Serializable]
	public class GeometryGroupData
	{
		public float Length = 2f;

		public int Segments;

		public List<Vector3> GuideVertices;

		public List<float> Distances;

		public List<Vector3> Vertices;

		public List<Color> Colors;

		[SerializeField]
		private GeometryGroupHistory history = new GeometryGroupHistory();

		public bool IsUndo => history.IsUndo;

		public bool IsRedo => history.IsRedo;

		public void Generate(Mesh mesh, int segments)
		{
			Vertices = new List<Vector3>();
			GuideVertices = new List<Vector3>();
			Distances = new List<float>();
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < mesh.vertices.Length; i++)
			{
				Vector3 vector = mesh.vertices[i];
				Vector3 normal = mesh.normals[i];
				if (!list.Contains(vector))
				{
					List<Vector3> collection = CreateStand(vector, normal, segments);
					Vertices.AddRange(collection);
					Distances.Add(Length / (float)segments);
					GuideVertices.AddRange(collection);
					list.Add(vector);
				}
			}
			Colors = new List<Color>();
			for (int j = 0; j < Vertices.Count; j++)
			{
				Colors.Add(new Color(1f, 1f, 1f));
			}
			Debug.Log("Total nodes:" + Vertices.Count);
		}

		public void Fix()
		{
		}

		public void Reset()
		{
			Vertices.Clear();
			Vertices = null;
		}

		private List<Vector3> CreateStand(Vector3 start, Vector3 normal, int segments)
		{
			List<Vector3> list = new List<Vector3>();
			float num = Length / (float)segments;
			for (int i = 0; i < segments; i++)
			{
				list.Add(start + normal * (num * (float)i));
			}
			return list;
		}

		public void Record()
		{
			history.Record(Vertices);
		}

		public void Undo()
		{
			if (history.IsUndo)
			{
				Vertices = history.Undo();
			}
		}

		public void Redo()
		{
			if (history.IsRedo)
			{
				Vertices = history.Redo();
			}
		}

		public void Clear()
		{
			history.Clear();
		}

		public void OnDrawGizmos(int segments, bool isSelected, Matrix4x4 toWorld)
		{
			Segments = segments;
			if (Vertices == null)
			{
				return;
			}
			if (Colors == null || Colors.Count != Vertices.Count)
			{
				FillColors();
			}
			for (int i = 1; i < Vertices.Count; i++)
			{
				if (i % segments != 0)
				{
					Color color = Colors[i];
					Gizmos.color = ((!isSelected) ? Color.grey : color);
					Vector3 from = toWorld.MultiplyPoint3x4(Vertices[i - 1]);
					Vector3 to = toWorld.MultiplyPoint3x4(Vertices[i]);
					Gizmos.DrawLine(from, to);
				}
			}
		}

		private void FillColors()
		{
			Colors = new List<Color>();
			for (int i = 0; i < Vertices.Count; i++)
			{
				Colors.Add(Color.white);
			}
		}
	}
}
