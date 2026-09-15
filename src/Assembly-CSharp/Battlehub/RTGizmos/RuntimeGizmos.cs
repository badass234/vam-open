using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTGizmos
{
	public static class RuntimeGizmos
	{
		private static readonly Material LinesMaterial;

		private static readonly Material HandlesMaterial;

		private static readonly Material SelectionMaterial;

		private static Mesh CubeHandles;

		private static Mesh ConeHandles;

		private static Mesh Selection;

		public const float HandleScale = 2f;

		static RuntimeGizmos()
		{
			HandlesMaterial = new Material(Shader.Find("Battlehub/RTGizmos/Handles"));
			HandlesMaterial.enableInstancing = true;
			LinesMaterial = new Material(Shader.Find("Battlehub/RTHandles/VertexColor"));
			LinesMaterial.enableInstancing = true;
			SelectionMaterial = new Material(Shader.Find("Battlehub/RTGizmos/Handles"));
			SelectionMaterial.SetFloat("_Offset", 1f);
			SelectionMaterial.SetFloat("_MinAlpha", 1f);
			SelectionMaterial.enableInstancing = true;
			CubeHandles = CreateCubeHandles(2f);
			ConeHandles = CreateConeHandles(2f);
			Selection = CreateHandlesMesh(2f, new Vector3[1] { Vector3.zero }, new Vector3[1] { Vector3.back });
		}

		public static Vector3[] GetHandlesPositions()
		{
			return new Vector3[6]
			{
				Vector3.up,
				Vector3.down,
				Vector3.right,
				Vector3.left,
				Vector3.forward,
				Vector3.back
			};
		}

		public static Vector3[] GetHandlesNormals()
		{
			return new Vector3[6]
			{
				Vector3.up,
				Vector3.down,
				Vector3.right,
				Vector3.left,
				Vector3.forward,
				Vector3.back
			};
		}

		public static Vector3[] GetConeHandlesPositions()
		{
			return new Vector3[5]
			{
				Vector3.zero,
				new Vector3(1f, 1f, 0f).normalized,
				new Vector3(-1f, 1f, 0f).normalized,
				new Vector3(-1f, -1f, 0f).normalized,
				new Vector3(1f, -1f, 0f).normalized
			};
		}

		public static Vector3[] GetConeHandlesNormals()
		{
			return new Vector3[5]
			{
				Vector3.forward,
				new Vector3(1f, 1f, 0f).normalized,
				new Vector3(-1f, 1f, 0f).normalized,
				new Vector3(-1f, -1f, 0f).normalized,
				new Vector3(1f, -1f, 0f).normalized
			};
		}

		private static Mesh CreateConeHandles(float size)
		{
			Vector3[] coneHandlesPositions = GetConeHandlesPositions();
			Vector3[] coneHandlesNormals = GetConeHandlesNormals();
			return CreateHandlesMesh(size, coneHandlesPositions, coneHandlesNormals);
		}

		private static Mesh CreateCubeHandles(float size)
		{
			Vector3[] handlesPositions = GetHandlesPositions();
			Vector3[] handlesNormals = GetHandlesNormals();
			return CreateHandlesMesh(size, handlesPositions, handlesNormals);
		}

		private static Mesh CreateHandlesMesh(float size, Vector3[] vertices, Vector3[] normals)
		{
			Vector2[] array = new Vector2[vertices.Length * 4];
			Vector3[] array2 = new Vector3[vertices.Length * 4];
			Vector3[] array3 = new Vector3[normals.Length * 4];
			for (int i = 0; i < vertices.Length; i++)
			{
				Vector3 vert = vertices[i];
				Vector3 vert2 = normals[i];
				SetVertex(i, array2, vert);
				SetVertex(i, array3, vert2);
				SetOffset(i, array, size);
			}
			int[] array4 = new int[array2.Length + array2.Length / 2];
			int num = 0;
			for (int j = 0; j < array4.Length; j += 6)
			{
				array4[j] = num;
				array4[j + 1] = num + 1;
				array4[j + 2] = num + 2;
				array4[j + 3] = num;
				array4[j + 4] = num + 2;
				array4[j + 5] = num + 3;
				num += 4;
			}
			Mesh mesh = new Mesh();
			mesh.vertices = array2;
			mesh.triangles = array4;
			mesh.normals = array3;
			mesh.uv = array;
			return mesh;
		}

		private static void SetVertex(int index, Vector3[] vertices, Vector3 vert)
		{
			for (int i = 0; i < 4; i++)
			{
				vertices[index * 4 + i] = vert;
			}
		}

		private static void SetOffset(int index, Vector2[] offsets, float size)
		{
			float num = size / 2f;
			offsets[index * 4] = new Vector2(0f - num, 0f - num);
			offsets[index * 4 + 1] = new Vector2(0f - num, num);
			offsets[index * 4 + 2] = new Vector2(num, num);
			offsets[index * 4 + 3] = new Vector2(num, 0f - num);
		}

		public static void DrawSelection(Vector3 position, Quaternion rotation, Vector3 scale, Color color)
		{
			Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scale);
			SelectionMaterial.color = color;
			SelectionMaterial.SetPass(0);
			Graphics.DrawMeshNow(Selection, matrix);
		}

		public static void DrawCubeHandles(Vector3 position, Quaternion rotation, Vector3 scale, Color color)
		{
			Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scale);
			HandlesMaterial.color = color;
			HandlesMaterial.SetPass(0);
			Graphics.DrawMeshNow(CubeHandles, matrix);
		}

		public static void DrawConeHandles(Vector3 position, Quaternion rotation, Vector3 scale, Color color)
		{
			Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scale);
			HandlesMaterial.color = color;
			HandlesMaterial.SetPass(0);
			Graphics.DrawMeshNow(ConeHandles, matrix);
		}

		public static void DrawWireConeGL(float height, float radius, Vector3 position, Quaternion rotation, Vector3 scale, Color color)
		{
			Matrix4x4 transform = Matrix4x4.TRS(height * Vector3.forward, Quaternion.identity, Vector3.one);
			Matrix4x4 m = Matrix4x4.TRS(position, rotation, scale);
			LinesMaterial.SetPass(0);
			GL.PushMatrix();
			GL.MultMatrix(m);
			GL.Begin(1);
			GL.Color(color);
			RuntimeGraphics.DrawCircleGL(transform, radius);
			GL.Vertex(Vector3.zero);
			GL.Vertex(Vector3.forward * height + new Vector3(1f, 1f, 0f).normalized * radius);
			GL.Vertex(Vector3.zero);
			GL.Vertex(Vector3.forward * height + new Vector3(-1f, 1f, 0f).normalized * radius);
			GL.Vertex(Vector3.zero);
			GL.Vertex(Vector3.forward * height + new Vector3(-1f, -1f, 0f).normalized * radius);
			GL.Vertex(Vector3.zero);
			GL.Vertex(Vector3.forward * height + new Vector3(1f, -1f, 0f).normalized * radius);
			GL.End();
			GL.PopMatrix();
		}

		public static void DrawWireCapsuleGL(int axis, float height, float radius, Vector3 position, Quaternion rotation, Vector3 scale, Color color)
		{
			radius = Mathf.Abs(radius);
			height = ((!(Mathf.Abs(height) < 2f * radius)) ? (Mathf.Abs(height) - 2f * radius) : 0f);
			Matrix4x4 transform;
			Matrix4x4 transform2;
			Matrix4x4 transform3;
			Matrix4x4 transform4;
			switch (axis)
			{
			case 1:
				transform = Matrix4x4.TRS(Vector3.up * height / 2f, Quaternion.AngleAxis(-90f, Vector3.right), Vector3.one);
				transform2 = Matrix4x4.TRS(Vector3.down * height / 2f, Quaternion.AngleAxis(-90f, Vector3.right), Vector3.one);
				transform3 = Matrix4x4.identity;
				transform4 = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(-90f, Vector3.up), Vector3.one);
				break;
			case 0:
				transform = Matrix4x4.TRS(Vector3.right * height / 2f, Quaternion.AngleAxis(-90f, Vector3.up), Vector3.one);
				transform2 = Matrix4x4.TRS(Vector3.left * height / 2f, Quaternion.AngleAxis(-90f, Vector3.up), Vector3.one);
				transform3 = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(-90f, Vector3.forward), Vector3.one);
				transform4 = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(-90f, Vector3.forward) * Quaternion.AngleAxis(-90f, Vector3.up), Vector3.one);
				break;
			default:
				transform = Matrix4x4.TRS(Vector3.forward * height / 2f, Quaternion.identity, Vector3.one);
				transform2 = Matrix4x4.TRS(Vector3.back * height / 2f, Quaternion.identity, Vector3.one);
				transform3 = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(-90f, Vector3.right), Vector3.one);
				transform4 = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(-90f, Vector3.right) * Quaternion.AngleAxis(-90f, Vector3.up), Vector3.one);
				break;
			}
			Matrix4x4 m = Matrix4x4.TRS(position, rotation, scale);
			LinesMaterial.SetPass(0);
			GL.PushMatrix();
			GL.MultMatrix(m);
			GL.Begin(1);
			GL.Color(color);
			RuntimeGraphics.DrawCircleGL(transform, radius);
			RuntimeGraphics.DrawCircleGL(transform2, radius);
			RuntimeGraphics.DrawCapsule2DGL(transform3, radius, height);
			RuntimeGraphics.DrawCapsule2DGL(transform4, radius, height);
			GL.End();
			GL.PopMatrix();
		}

		public static void DrawDirectionalLight(Vector3 position, Quaternion rotation, Vector3 scale, Color color)
		{
			float screenScale = RuntimeGraphics.GetScreenScale(position, Camera.current);
			Matrix4x4 transform = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
			Matrix4x4 m = Matrix4x4.TRS(position, Quaternion.identity, scale * screenScale);
			LinesMaterial.SetPass(0);
			GL.PushMatrix();
			GL.MultMatrix(m);
			GL.Begin(1);
			GL.Color(color);
			float radius = 0.25f;
			float num = 1.25f;
			RuntimeGraphics.DrawCircleGL(transform, radius);
			RuntimeGraphics.DrawWireConeGL(transform, Vector3.zero, radius, num, 8);
			Vector3 v = transform.MultiplyPoint(Vector3.zero);
			Vector3 v2 = transform.MultiplyPoint(Vector3.forward * num);
			GL.Vertex(v);
			GL.Vertex(v2);
			GL.End();
			GL.PopMatrix();
		}

		public static void DrawWireDisc(Vector3 position, Quaternion rotation, Vector3 scale, Color color)
		{
			Matrix4x4 transform = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
			Matrix4x4 m = Matrix4x4.TRS(position, Quaternion.identity, scale);
			LinesMaterial.SetPass(0);
			GL.PushMatrix();
			GL.MultMatrix(m);
			GL.Begin(1);
			GL.Color(color);
			RuntimeGraphics.DrawCircleGL(transform);
			GL.End();
			GL.PopMatrix();
		}

		public static void DrawWireSphereGL(Vector3 position, Quaternion rotation, Vector3 scale, Color color)
		{
			Matrix4x4 transform = Matrix4x4.TRS(Vector3.zero, rotation * Quaternion.AngleAxis(-90f, Vector3.up), Vector3.one);
			Matrix4x4 transform2 = Matrix4x4.TRS(Vector3.zero, rotation * Quaternion.AngleAxis(-90f, Vector3.right), Vector3.one);
			Matrix4x4 transform3 = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
			Matrix4x4 m = Matrix4x4.TRS(position, Quaternion.identity, scale);
			LinesMaterial.SetPass(0);
			GL.PushMatrix();
			GL.MultMatrix(m);
			GL.Begin(1);
			GL.Color(color);
			RuntimeGraphics.DrawCircleGL(transform);
			RuntimeGraphics.DrawCircleGL(transform2);
			RuntimeGraphics.DrawCircleGL(transform3);
			if (Camera.current.orthographic)
			{
				Matrix4x4 transform4 = Matrix4x4.TRS(Vector3.zero, Camera.current.transform.rotation, Vector3.one);
				RuntimeGraphics.DrawCircleGL(transform4);
			}
			else
			{
				Vector3 vector = Camera.current.transform.position - position;
				Vector3 normalized = vector.normalized;
				if (Vector3.Dot(normalized, Camera.current.transform.forward) < 0f)
				{
					float magnitude = vector.magnitude;
					Matrix4x4 transform5 = Matrix4x4.TRS(normalized * 0.56f * scale.x / magnitude, Quaternion.LookRotation(normalized, Camera.current.transform.up), Vector3.one);
					RuntimeGraphics.DrawCircleGL(transform5);
				}
			}
			GL.End();
			GL.PopMatrix();
		}

		public static void DrawWireCubeGL(ref Bounds bounds, Vector3 position, Quaternion rotation, Vector3 scale, Color color)
		{
			LinesMaterial.SetPass(0);
			Matrix4x4 m = Matrix4x4.TRS(position, rotation, scale);
			Vector3 v = new Vector3(0f - bounds.extents.x, 0f - bounds.extents.y, 0f - bounds.extents.z);
			Vector3 v2 = new Vector3(0f - bounds.extents.x, 0f - bounds.extents.y, bounds.extents.z);
			Vector3 v3 = new Vector3(0f - bounds.extents.x, bounds.extents.y, 0f - bounds.extents.z);
			Vector3 v4 = new Vector3(0f - bounds.extents.x, bounds.extents.y, bounds.extents.z);
			Vector3 v5 = new Vector3(bounds.extents.x, 0f - bounds.extents.y, 0f - bounds.extents.z);
			Vector3 v6 = new Vector3(bounds.extents.x, 0f - bounds.extents.y, bounds.extents.z);
			Vector3 v7 = new Vector3(bounds.extents.x, bounds.extents.y, 0f - bounds.extents.z);
			Vector3 v8 = new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z);
			GL.PushMatrix();
			GL.MultMatrix(m);
			GL.Begin(1);
			GL.Color(color);
			GL.Vertex(v);
			GL.Vertex(v2);
			GL.Vertex(v3);
			GL.Vertex(v4);
			GL.Vertex(v5);
			GL.Vertex(v6);
			GL.Vertex(v7);
			GL.Vertex(v8);
			GL.Vertex(v);
			GL.Vertex(v3);
			GL.Vertex(v2);
			GL.Vertex(v4);
			GL.Vertex(v5);
			GL.Vertex(v7);
			GL.Vertex(v6);
			GL.Vertex(v8);
			GL.Vertex(v);
			GL.Vertex(v5);
			GL.Vertex(v2);
			GL.Vertex(v6);
			GL.Vertex(v3);
			GL.Vertex(v7);
			GL.Vertex(v4);
			GL.Vertex(v8);
			GL.End();
			GL.PopMatrix();
		}
	}
}
