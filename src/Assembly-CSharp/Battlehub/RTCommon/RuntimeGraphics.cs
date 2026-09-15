using System;
using UnityEngine;

namespace Battlehub.RTCommon
{
	public static class RuntimeGraphics
	{
		public const int RuntimeHandlesLayer = 24;

		private static Mesh m_quadMesh;

		static RuntimeGraphics()
		{
			m_quadMesh = CreateQuadMesh();
		}

		public static float GetScreenScale(Vector3 position, Camera camera)
		{
			float num = camera.pixelHeight;
			if (camera.orthographic)
			{
				return camera.orthographicSize * 2f / num * 90f;
			}
			Transform transform = camera.transform;
			float num2 = Vector3.Dot(position - transform.position, transform.forward);
			float num3 = 2f * num2 * Mathf.Tan(camera.fieldOfView * 0.5f * ((float)Math.PI / 180f));
			return num3 / num * 90f;
		}

		public static Mesh CreateCubeMesh(Color color, Vector3 center, float scale, float cubeLength = 1f, float cubeWidth = 1f, float cubeHeight = 1f)
		{
			cubeHeight *= scale;
			cubeWidth *= scale;
			cubeLength *= scale;
			Vector3 vector = center + new Vector3((0f - cubeLength) * 0.5f, (0f - cubeWidth) * 0.5f, cubeHeight * 0.5f);
			Vector3 vector2 = center + new Vector3(cubeLength * 0.5f, (0f - cubeWidth) * 0.5f, cubeHeight * 0.5f);
			Vector3 vector3 = center + new Vector3(cubeLength * 0.5f, (0f - cubeWidth) * 0.5f, (0f - cubeHeight) * 0.5f);
			Vector3 vector4 = center + new Vector3((0f - cubeLength) * 0.5f, (0f - cubeWidth) * 0.5f, (0f - cubeHeight) * 0.5f);
			Vector3 vector5 = center + new Vector3((0f - cubeLength) * 0.5f, cubeWidth * 0.5f, cubeHeight * 0.5f);
			Vector3 vector6 = center + new Vector3(cubeLength * 0.5f, cubeWidth * 0.5f, cubeHeight * 0.5f);
			Vector3 vector7 = center + new Vector3(cubeLength * 0.5f, cubeWidth * 0.5f, (0f - cubeHeight) * 0.5f);
			Vector3 vector8 = center + new Vector3((0f - cubeLength) * 0.5f, cubeWidth * 0.5f, (0f - cubeHeight) * 0.5f);
			Vector3[] array = new Vector3[24]
			{
				vector, vector2, vector3, vector4, vector8, vector5, vector, vector4, vector5, vector6,
				vector2, vector, vector7, vector8, vector4, vector3, vector6, vector7, vector3, vector2,
				vector8, vector7, vector6, vector5
			};
			int[] triangles = new int[36]
			{
				3, 1, 0, 3, 2, 1, 7, 5, 4, 7,
				6, 5, 11, 9, 8, 11, 10, 9, 15, 13,
				12, 15, 14, 13, 19, 17, 16, 19, 18, 17,
				23, 21, 20, 23, 22, 21
			};
			Color[] array2 = new Color[array.Length];
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = color;
			}
			Mesh mesh = new Mesh();
			mesh.name = "cube";
			mesh.vertices = array;
			mesh.triangles = triangles;
			mesh.colors = array2;
			mesh.RecalculateNormals();
			return mesh;
		}

		public static Mesh CreateQuadMesh(float quadWidth = 1f, float quadHeight = 1f)
		{
			Vector3 vector = new Vector3((0f - quadWidth) * 0.5f, (0f - quadHeight) * 0.5f, 0f);
			Vector3 vector2 = new Vector3(quadWidth * 0.5f, (0f - quadHeight) * 0.5f, 0f);
			Vector3 vector3 = new Vector3((0f - quadWidth) * 0.5f, quadHeight * 0.5f, 0f);
			Vector3 vector4 = new Vector3(quadWidth * 0.5f, quadHeight * 0.5f, 0f);
			Vector3[] vertices = new Vector3[4] { vector3, vector4, vector2, vector };
			int[] triangles = new int[6] { 3, 1, 0, 3, 2, 1 };
			Vector2[] uv = new Vector2[4]
			{
				new Vector2(1f, 0f),
				new Vector2(0f, 0f),
				new Vector2(0f, 1f),
				new Vector2(1f, 1f)
			};
			Mesh mesh = new Mesh();
			mesh.name = "quad";
			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.uv = uv;
			mesh.RecalculateNormals();
			return mesh;
		}

		public static void DrawQuad(Matrix4x4 transform)
		{
			Graphics.DrawMeshNow(m_quadMesh, transform);
		}

		public static void DrawCircleGL(Matrix4x4 transform, float radius = 1f, int pointsCount = 64)
		{
			DrawArcGL(transform, Vector3.zero, radius, pointsCount);
		}

		public static void DrawArcGL(Matrix4x4 transform, Vector3 offset, float radius = 1f, int pointsCount = 64, float fromAngle = 0f, float toAngle = (float)Math.PI * 2f)
		{
			float num = fromAngle;
			float num2 = toAngle - fromAngle;
			float z = 0f;
			float x = radius * Mathf.Cos(num);
			float y = radius * Mathf.Sin(num);
			Vector3 v = transform.MultiplyPoint(new Vector3(x, y, z) + offset);
			for (int i = 0; i < pointsCount; i++)
			{
				GL.Vertex(v);
				num += num2 / (float)pointsCount;
				x = radius * Mathf.Cos(num);
				y = radius * Mathf.Sin(num);
				Vector3 vector = transform.MultiplyPoint(new Vector3(x, y, z) + offset);
				GL.Vertex(vector);
				v = vector;
			}
		}

		public static void DrawWireConeGL(Matrix4x4 transform, Vector3 offset, float radius = 1f, float length = 2f, int pointsCount = 64, float fromAngle = 0f, float toAngle = (float)Math.PI * 2f)
		{
			float num = fromAngle;
			float num2 = toAngle - fromAngle;
			float z = 0f;
			for (int i = 0; i < pointsCount; i++)
			{
				float x = radius * Mathf.Cos(num);
				float y = radius * Mathf.Sin(num);
				Vector3 v = transform.MultiplyPoint(new Vector3(x, y, z) + offset);
				Vector3 v2 = transform.MultiplyPoint(new Vector3(x, y, z) + offset + Vector3.forward * length);
				GL.Vertex(v);
				GL.Vertex(v2);
				num += num2 / (float)pointsCount;
			}
		}

		public static void DrawCapsule2DGL(Matrix4x4 transform, float radius = 1f, float height = 1f, int pointsCount = 64)
		{
			DrawArcGL(transform, Vector3.up * height / 2f, radius, pointsCount / 2, 0f, (float)Math.PI);
			DrawArcGL(transform, Vector3.down * height / 2f, radius, pointsCount / 2, (float)Math.PI);
			GL.Vertex(transform.MultiplyPoint(new Vector3(radius, height / 2f, 0f)));
			GL.Vertex(transform.MultiplyPoint(new Vector3(radius, (0f - height) / 2f, 0f)));
			GL.Vertex(transform.MultiplyPoint(new Vector3(0f - radius, height / 2f, 0f)));
			GL.Vertex(transform.MultiplyPoint(new Vector3(0f - radius, (0f - height) / 2f, 0f)));
		}
	}
}
