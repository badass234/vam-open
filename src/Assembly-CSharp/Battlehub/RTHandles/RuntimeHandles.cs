using System;
using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTHandles
{
	public static class RuntimeHandles
	{
		public const float HandleScale = 1f;

		private static readonly Mesh Arrows;

		private static readonly Mesh ArrowY;

		private static readonly Mesh ArrowX;

		private static readonly Mesh ArrowZ;

		private static readonly Mesh SelectionArrowY;

		private static readonly Mesh SelectionArrowX;

		private static readonly Mesh SelectionArrowZ;

		private static readonly Mesh DisabledArrowY;

		private static readonly Mesh DisabledArrowX;

		private static readonly Mesh DisabledArrowZ;

		private static readonly Mesh SelectionCube;

		private static readonly Mesh DisabledCube;

		private static readonly Mesh CubeX;

		private static readonly Mesh CubeY;

		private static readonly Mesh CubeZ;

		private static readonly Mesh CubeUniform;

		private static readonly Mesh SceneGizmoSelectedAxis;

		private static readonly Mesh SceneGizmoXAxis;

		private static readonly Mesh SceneGizmoYAxis;

		private static readonly Mesh SceneGizmoZAxis;

		private static readonly Mesh SceneGizmoCube;

		private static readonly Mesh SceneGizmoSelectedCube;

		private static readonly Mesh SceneGizmoQuad;

		private static readonly Material ShapesMaterialZTest;

		private static readonly Material ShapesMaterialZTest2;

		private static readonly Material ShapesMaterialZTest3;

		private static readonly Material ShapesMaterialZTest4;

		private static readonly Material ShapesMaterialZTestOffset;

		private static readonly Material ShapesMaterial;

		private static readonly Material LinesMaterial;

		private static readonly Material LinesMaterialZTest;

		private static readonly Material LinesClipMaterial;

		private static readonly Material LinesBillboardMaterial;

		private static readonly Material XMaterial;

		private static readonly Material YMaterial;

		private static readonly Material ZMaterial;

		private static readonly Material GridMaterial;

		static RuntimeHandles()
		{
			LinesMaterial = new Material(Shader.Find("Battlehub/RTHandles/VertexColor"));
			LinesMaterial.color = Color.white;
			LinesMaterial.enableInstancing = true;
			LinesMaterialZTest = new Material(Shader.Find("Battlehub/RTHandles/VertexColor"));
			LinesMaterialZTest.color = Color.white;
			LinesMaterialZTest.SetFloat("_ZTest", 4f);
			LinesMaterialZTest.enableInstancing = true;
			LinesClipMaterial = new Material(Shader.Find("Battlehub/RTHandles/VertexColorClip"));
			LinesClipMaterial.color = Color.white;
			LinesClipMaterial.enableInstancing = true;
			LinesBillboardMaterial = new Material(Shader.Find("Battlehub/RTHandles/VertexColorBillboard"));
			LinesBillboardMaterial.color = Color.white;
			LinesBillboardMaterial.enableInstancing = true;
			ShapesMaterial = new Material(Shader.Find("Battlehub/RTHandles/Shape"));
			ShapesMaterial.color = Color.white;
			ShapesMaterial.enableInstancing = true;
			ShapesMaterialZTest = new Material(Shader.Find("Battlehub/RTHandles/Shape"));
			ShapesMaterialZTest.color = new Color(1f, 1f, 1f, 0f);
			ShapesMaterialZTest.SetFloat("_ZTest", 4f);
			ShapesMaterialZTest.SetFloat("_ZWrite", 1f);
			ShapesMaterialZTest.enableInstancing = true;
			ShapesMaterialZTestOffset = new Material(Shader.Find("Battlehub/RTHandles/Shape"));
			ShapesMaterialZTestOffset.color = new Color(1f, 1f, 1f, 1f);
			ShapesMaterialZTestOffset.SetFloat("_ZTest", 4f);
			ShapesMaterialZTestOffset.SetFloat("_ZWrite", 1f);
			ShapesMaterialZTestOffset.SetFloat("_OFactors", -1f);
			ShapesMaterialZTestOffset.SetFloat("_OUnits", -1f);
			ShapesMaterialZTestOffset.enableInstancing = true;
			ShapesMaterialZTest2 = new Material(Shader.Find("Battlehub/RTHandles/Shape"));
			ShapesMaterialZTest2.color = new Color(1f, 1f, 1f, 0f);
			ShapesMaterialZTest2.SetFloat("_ZTest", 4f);
			ShapesMaterialZTest2.SetFloat("_ZWrite", 1f);
			ShapesMaterialZTest2.enableInstancing = true;
			ShapesMaterialZTest3 = new Material(Shader.Find("Battlehub/RTHandles/Shape"));
			ShapesMaterialZTest3.color = new Color(1f, 1f, 1f, 0f);
			ShapesMaterialZTest3.SetFloat("_ZTest", 4f);
			ShapesMaterialZTest3.SetFloat("_ZWrite", 1f);
			ShapesMaterialZTest3.enableInstancing = true;
			ShapesMaterialZTest4 = new Material(Shader.Find("Battlehub/RTHandles/Shape"));
			ShapesMaterialZTest4.color = new Color(1f, 1f, 1f, 0f);
			ShapesMaterialZTest4.SetFloat("_ZTest", 4f);
			ShapesMaterialZTest4.SetFloat("_ZWrite", 1f);
			ShapesMaterialZTest4.enableInstancing = true;
			XMaterial = new Material(Shader.Find("Battlehub/RTCommon/Billboard"));
			XMaterial.color = Color.white;
			XMaterial.mainTexture = Resources.Load<Texture>("Battlehub.RuntimeHandles.x");
			XMaterial.enableInstancing = true;
			YMaterial = new Material(Shader.Find("Battlehub/RTCommon/Billboard"));
			YMaterial.color = Color.white;
			YMaterial.mainTexture = Resources.Load<Texture>("Battlehub.RuntimeHandles.y");
			YMaterial.enableInstancing = true;
			ZMaterial = new Material(Shader.Find("Battlehub/RTCommon/Billboard"));
			ZMaterial.color = Color.white;
			ZMaterial.mainTexture = Resources.Load<Texture>("Battlehub.RuntimeHandles.z");
			ZMaterial.enableInstancing = true;
			GridMaterial = new Material(Shader.Find("Battlehub/RTHandles/Grid"));
			GridMaterial.color = Color.white;
			GridMaterial.SetFloat("_ZTest", 1f);
			GridMaterial.enableInstancing = true;
			Mesh mesh = CreateConeMesh(RTHColors.SelectionColor, 1f);
			Mesh mesh2 = CreateConeMesh(RTHColors.DisabledColor, 1f);
			CombineInstance combineInstance = new CombineInstance
			{
				mesh = mesh,
				transform = Matrix4x4.TRS(Vector3.up * 1f, Quaternion.identity, Vector3.one)
			};
			SelectionArrowY = new Mesh();
			SelectionArrowY.CombineMeshes(new CombineInstance[1] { combineInstance }, true);
			SelectionArrowY.RecalculateNormals();
			combineInstance.mesh = mesh2;
			combineInstance.transform = Matrix4x4.TRS(Vector3.up * 1f, Quaternion.identity, Vector3.one);
			DisabledArrowY = new Mesh();
			DisabledArrowY.CombineMeshes(new CombineInstance[1] { combineInstance }, true);
			DisabledArrowY.RecalculateNormals();
			combineInstance.mesh = CreateConeMesh(RTHColors.YColor, 1f);
			combineInstance.transform = Matrix4x4.TRS(Vector3.up * 1f, Quaternion.identity, Vector3.one);
			ArrowY = new Mesh();
			ArrowY.CombineMeshes(new CombineInstance[1] { combineInstance }, true);
			ArrowY.RecalculateNormals();
			CombineInstance combineInstance2 = new CombineInstance
			{
				mesh = mesh,
				transform = Matrix4x4.TRS(Vector3.right * 1f, Quaternion.AngleAxis(-90f, Vector3.forward), Vector3.one)
			};
			SelectionArrowX = new Mesh();
			SelectionArrowX.CombineMeshes(new CombineInstance[1] { combineInstance2 }, true);
			SelectionArrowX.RecalculateNormals();
			combineInstance2.mesh = mesh2;
			combineInstance2.transform = Matrix4x4.TRS(Vector3.right * 1f, Quaternion.AngleAxis(-90f, Vector3.forward), Vector3.one);
			DisabledArrowX = new Mesh();
			DisabledArrowX.CombineMeshes(new CombineInstance[1] { combineInstance2 }, true);
			DisabledArrowX.RecalculateNormals();
			combineInstance2.mesh = CreateConeMesh(RTHColors.XColor, 1f);
			combineInstance2.transform = Matrix4x4.TRS(Vector3.right * 1f, Quaternion.AngleAxis(-90f, Vector3.forward), Vector3.one);
			ArrowX = new Mesh();
			ArrowX.CombineMeshes(new CombineInstance[1] { combineInstance2 }, true);
			ArrowX.RecalculateNormals();
			CombineInstance combineInstance3 = new CombineInstance
			{
				mesh = mesh,
				transform = Matrix4x4.TRS(Vector3.forward * 1f, Quaternion.AngleAxis(90f, Vector3.right), Vector3.one)
			};
			SelectionArrowZ = new Mesh();
			SelectionArrowZ.CombineMeshes(new CombineInstance[1] { combineInstance3 }, true);
			SelectionArrowZ.RecalculateNormals();
			combineInstance3.mesh = mesh2;
			combineInstance3.transform = Matrix4x4.TRS(Vector3.forward * 1f, Quaternion.AngleAxis(90f, Vector3.right), Vector3.one);
			DisabledArrowZ = new Mesh();
			DisabledArrowZ.CombineMeshes(new CombineInstance[1] { combineInstance3 }, true);
			DisabledArrowZ.RecalculateNormals();
			combineInstance3.mesh = CreateConeMesh(RTHColors.ZColor, 1f);
			combineInstance3.transform = Matrix4x4.TRS(Vector3.forward * 1f, Quaternion.AngleAxis(90f, Vector3.right), Vector3.one);
			ArrowZ = new Mesh();
			ArrowZ.CombineMeshes(new CombineInstance[1] { combineInstance3 }, true);
			ArrowZ.RecalculateNormals();
			combineInstance.mesh = CreateConeMesh(RTHColors.YColor, 1f);
			combineInstance2.mesh = CreateConeMesh(RTHColors.XColor, 1f);
			combineInstance3.mesh = CreateConeMesh(RTHColors.ZColor, 1f);
			Arrows = new Mesh();
			Arrows.CombineMeshes(new CombineInstance[3] { combineInstance, combineInstance2, combineInstance3 }, true);
			Arrows.RecalculateNormals();
			SelectionCube = RuntimeGraphics.CreateCubeMesh(RTHColors.SelectionColor, Vector3.zero, 1f, 0.1f, 0.1f, 0.1f);
			DisabledCube = RuntimeGraphics.CreateCubeMesh(RTHColors.DisabledColor, Vector3.zero, 1f, 0.1f, 0.1f, 0.1f);
			CubeX = RuntimeGraphics.CreateCubeMesh(RTHColors.XColor, Vector3.zero, 1f, 0.1f, 0.1f, 0.1f);
			CubeY = RuntimeGraphics.CreateCubeMesh(RTHColors.YColor, Vector3.zero, 1f, 0.1f, 0.1f, 0.1f);
			CubeZ = RuntimeGraphics.CreateCubeMesh(RTHColors.ZColor, Vector3.zero, 1f, 0.1f, 0.1f, 0.1f);
			CubeUniform = RuntimeGraphics.CreateCubeMesh(RTHColors.AltColor, Vector3.zero, 1f, 0.1f, 0.1f, 0.1f);
			SceneGizmoSelectedAxis = CreateSceneGizmoHalfAxis(RTHColors.SelectionColor, Quaternion.AngleAxis(90f, Vector3.right));
			SceneGizmoXAxis = CreateSceneGizmoAxis(RTHColors.XColor, RTHColors.AltColor, Quaternion.AngleAxis(-90f, Vector3.forward));
			SceneGizmoYAxis = CreateSceneGizmoAxis(RTHColors.YColor, RTHColors.AltColor, Quaternion.identity);
			SceneGizmoZAxis = CreateSceneGizmoAxis(RTHColors.ZColor, RTHColors.AltColor, Quaternion.AngleAxis(90f, Vector3.right));
			SceneGizmoCube = RuntimeGraphics.CreateCubeMesh(RTHColors.AltColor, Vector3.zero, 1f);
			SceneGizmoSelectedCube = RuntimeGraphics.CreateCubeMesh(RTHColors.SelectionColor, Vector3.zero, 1f);
			SceneGizmoQuad = RuntimeGraphics.CreateQuadMesh();
		}

		private static Mesh CreateConeMesh(Color color, float scale)
		{
			int num = 12;
			float num2 = 0.2f;
			num2 *= scale;
			Vector3[] array = new Vector3[num * 3 + 1];
			int[] array2 = new int[num * 6];
			Color[] array3 = new Color[array.Length];
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i] = color;
			}
			float num3 = num2 / 2.6f;
			float num4 = num2;
			float num5 = (float)Math.PI * 2f / (float)num;
			float y = 0f - num4;
			array[array.Length - 1] = new Vector3(0f, 0f - num4, 0f);
			for (int j = 0; j < num; j++)
			{
				float f = (float)j * num5;
				float x = Mathf.Cos(f) * num3;
				float z = Mathf.Sin(f) * num3;
				array[j] = new Vector3(x, y, z);
				array[num + j] = new Vector3(0f, 0.01f, 0f);
				array[2 * num + j] = array[j];
			}
			for (int k = 0; k < num; k++)
			{
				array2[k * 6] = k;
				array2[k * 6 + 1] = num + k;
				array2[k * 6 + 2] = (k + 1) % num;
				array2[k * 6 + 3] = array.Length - 1;
				array2[k * 6 + 4] = 2 * num + k;
				array2[k * 6 + 5] = 2 * num + (k + 1) % num;
			}
			Mesh mesh = new Mesh();
			mesh.name = "Cone";
			mesh.vertices = array;
			mesh.triangles = array2;
			mesh.colors = array3;
			return mesh;
		}

		private static Mesh CreateSceneGizmoHalfAxis(Color color, Quaternion rotation)
		{
			Mesh mesh = CreateConeMesh(color, 1f);
			CombineInstance combineInstance = default(CombineInstance);
			combineInstance.mesh = mesh;
			combineInstance.transform = Matrix4x4.TRS(Vector3.up * 0.1f, Quaternion.AngleAxis(180f, Vector3.right), Vector3.one);
			Mesh mesh2 = new Mesh();
			mesh2.CombineMeshes(new CombineInstance[1] { combineInstance }, true);
			CombineInstance combineInstance2 = default(CombineInstance);
			combineInstance2.mesh = mesh2;
			combineInstance2.transform = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
			mesh2 = new Mesh();
			mesh2.CombineMeshes(new CombineInstance[1] { combineInstance2 }, true);
			mesh2.RecalculateNormals();
			return mesh2;
		}

		private static Mesh CreateSceneGizmoAxis(Color axisColor, Color altColor, Quaternion rotation)
		{
			Mesh mesh = CreateConeMesh(axisColor, 1f);
			Mesh mesh2 = CreateConeMesh(altColor, 1f);
			CombineInstance combineInstance = default(CombineInstance);
			combineInstance.mesh = mesh;
			combineInstance.transform = Matrix4x4.TRS(Vector3.up * 0.1f, Quaternion.AngleAxis(180f, Vector3.right), Vector3.one);
			CombineInstance combineInstance2 = default(CombineInstance);
			combineInstance2.mesh = mesh2;
			combineInstance2.transform = Matrix4x4.TRS(Vector3.down * 0.1f, Quaternion.identity, Vector3.one);
			Mesh mesh3 = new Mesh();
			mesh3.CombineMeshes(new CombineInstance[2] { combineInstance, combineInstance2 }, true);
			CombineInstance combineInstance3 = default(CombineInstance);
			combineInstance3.mesh = mesh3;
			combineInstance3.transform = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
			mesh3 = new Mesh();
			mesh3.CombineMeshes(new CombineInstance[1] { combineInstance3 }, true);
			mesh3.RecalculateNormals();
			return mesh3;
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

		private static void DoAxes(Vector3 position, Matrix4x4 transform, RuntimeHandleAxis selectedAxis, bool xLocked, bool yLocked, bool zLocked)
		{
			Vector3 vector = Vector3.right * 1f;
			Vector3 vector2 = Vector3.up * 1f;
			Vector3 vector3 = Vector3.forward * 1f;
			vector = transform.MultiplyVector(vector);
			vector2 = transform.MultiplyVector(vector2);
			vector3 = transform.MultiplyVector(vector3);
			if (xLocked)
			{
				GL.Color(RTHColors.DisabledColor);
			}
			else
			{
				GL.Color(((selectedAxis & RuntimeHandleAxis.X) != 0) ? RTHColors.SelectionColor : RTHColors.XColor);
			}
			GL.Vertex(position);
			GL.Vertex(position + vector);
			if (yLocked)
			{
				GL.Color(RTHColors.DisabledColor);
			}
			else
			{
				GL.Color(((selectedAxis & RuntimeHandleAxis.Y) != 0) ? RTHColors.SelectionColor : RTHColors.YColor);
			}
			GL.Vertex(position);
			GL.Vertex(position + vector2);
			if (zLocked)
			{
				GL.Color(RTHColors.DisabledColor);
			}
			else
			{
				GL.Color(((selectedAxis & RuntimeHandleAxis.Z) != 0) ? RTHColors.SelectionColor : RTHColors.ZColor);
			}
			GL.Vertex(position);
			GL.Vertex(position + vector3);
		}

		public static void DoPositionHandle(Vector3 position, Quaternion rotation, RuntimeHandleAxis selectedAxis = RuntimeHandleAxis.None, bool snapMode = false, LockObject lockObject = null)
		{
			float screenScale = GetScreenScale(position, Camera.current);
			Matrix4x4 matrix4x = Matrix4x4.TRS(position, rotation, new Vector3(screenScale, screenScale, screenScale));
			LinesMaterial.SetPass(0);
			GL.Begin(1);
			bool flag = lockObject?.PositionX ?? false;
			bool flag2 = lockObject?.PositionY ?? false;
			bool flag3 = lockObject?.PositionZ ?? false;
			DoAxes(position, matrix4x, selectedAxis, flag, flag2, flag3);
			Vector3 vector = Vector3.right * 0.2f;
			Vector3 vector2 = Vector3.up * 0.2f;
			Vector3 vector3 = Vector3.forward * 0.2f;
			if (snapMode)
			{
				GL.End();
				LinesBillboardMaterial.SetPass(0);
				GL.PushMatrix();
				GL.MultMatrix(matrix4x);
				GL.Begin(1);
				if (selectedAxis == RuntimeHandleAxis.Snap)
				{
					GL.Color(RTHColors.SelectionColor);
				}
				else
				{
					GL.Color(RTHColors.AltColor);
				}
				float num = 0.1f;
				Vector3 v = new Vector3(num, num, 0f);
				Vector3 v2 = new Vector3(num, 0f - num, 0f);
				Vector3 v3 = new Vector3(0f - num, 0f - num, 0f);
				Vector3 v4 = new Vector3(0f - num, num, 0f);
				GL.Vertex(v);
				GL.Vertex(v2);
				GL.Vertex(v2);
				GL.Vertex(v3);
				GL.Vertex(v3);
				GL.Vertex(v4);
				GL.Vertex(v4);
				GL.Vertex(v);
				GL.End();
				GL.PopMatrix();
			}
			else
			{
				Camera current = Camera.current;
				Vector3 lhs = matrix4x.inverse.MultiplyVector(current.transform.position - position);
				float num2 = Mathf.Sign(Vector3.Dot(lhs, vector)) * 1f;
				float num3 = Mathf.Sign(Vector3.Dot(lhs, vector2)) * 1f;
				float num4 = Mathf.Sign(Vector3.Dot(lhs, vector3)) * 1f;
				vector.x *= num2;
				vector2.y *= num3;
				vector3.z *= num4;
				Vector3 point = vector + vector2;
				Vector3 point2 = vector + vector3;
				Vector3 point3 = vector2 + vector3;
				vector = matrix4x.MultiplyPoint(vector);
				vector2 = matrix4x.MultiplyPoint(vector2);
				vector3 = matrix4x.MultiplyPoint(vector3);
				point = matrix4x.MultiplyPoint(point);
				point2 = matrix4x.MultiplyPoint(point2);
				point3 = matrix4x.MultiplyPoint(point3);
				if (!flag && !flag3)
				{
					GL.Color((selectedAxis == RuntimeHandleAxis.XZ) ? RTHColors.SelectionColor : RTHColors.YColor);
					GL.Vertex(position);
					GL.Vertex(vector3);
					GL.Vertex(vector3);
					GL.Vertex(point2);
					GL.Vertex(point2);
					GL.Vertex(vector);
					GL.Vertex(vector);
					GL.Vertex(position);
				}
				if (!flag && !flag2)
				{
					GL.Color((selectedAxis == RuntimeHandleAxis.XY) ? RTHColors.SelectionColor : RTHColors.ZColor);
					GL.Vertex(position);
					GL.Vertex(vector2);
					GL.Vertex(vector2);
					GL.Vertex(point);
					GL.Vertex(point);
					GL.Vertex(vector);
					GL.Vertex(vector);
					GL.Vertex(position);
				}
				if (!flag2 && !flag3)
				{
					GL.Color((selectedAxis == RuntimeHandleAxis.YZ) ? RTHColors.SelectionColor : RTHColors.XColor);
					GL.Vertex(position);
					GL.Vertex(vector2);
					GL.Vertex(vector2);
					GL.Vertex(point3);
					GL.Vertex(point3);
					GL.Vertex(vector3);
					GL.Vertex(vector3);
					GL.Vertex(position);
				}
				GL.End();
				GL.Begin(7);
				if (!flag && !flag3)
				{
					GL.Color(RTHColors.YColorTransparent);
					GL.Vertex(position);
					GL.Vertex(vector3);
					GL.Vertex(point2);
					GL.Vertex(vector);
				}
				if (!flag && !flag2)
				{
					GL.Color(RTHColors.ZColorTransparent);
					GL.Vertex(position);
					GL.Vertex(vector2);
					GL.Vertex(point);
					GL.Vertex(vector);
				}
				if (!flag2 && !flag3)
				{
					GL.Color(RTHColors.XColorTransparent);
					GL.Vertex(position);
					GL.Vertex(vector2);
					GL.Vertex(point3);
					GL.Vertex(vector3);
				}
				GL.End();
			}
			ShapesMaterial.SetPass(0);
			if (!flag && !flag2 && !flag3)
			{
				Graphics.DrawMeshNow(Arrows, matrix4x);
				if ((selectedAxis & RuntimeHandleAxis.X) != 0)
				{
					Graphics.DrawMeshNow(SelectionArrowX, matrix4x);
				}
				if ((selectedAxis & RuntimeHandleAxis.Y) != 0)
				{
					Graphics.DrawMeshNow(SelectionArrowY, matrix4x);
				}
				if ((selectedAxis & RuntimeHandleAxis.Z) != 0)
				{
					Graphics.DrawMeshNow(SelectionArrowZ, matrix4x);
				}
				return;
			}
			if (flag)
			{
				Graphics.DrawMeshNow(DisabledArrowX, matrix4x);
			}
			else if ((selectedAxis & RuntimeHandleAxis.X) != 0)
			{
				Graphics.DrawMeshNow(SelectionArrowX, matrix4x);
			}
			else
			{
				Graphics.DrawMeshNow(ArrowX, matrix4x);
			}
			if (flag2)
			{
				Graphics.DrawMeshNow(DisabledArrowY, matrix4x);
			}
			else if ((selectedAxis & RuntimeHandleAxis.Y) != 0)
			{
				Graphics.DrawMeshNow(SelectionArrowY, matrix4x);
			}
			else
			{
				Graphics.DrawMeshNow(ArrowY, matrix4x);
			}
			if (flag3)
			{
				Graphics.DrawMeshNow(DisabledArrowZ, matrix4x);
			}
			else if ((selectedAxis & RuntimeHandleAxis.Z) != 0)
			{
				Graphics.DrawMeshNow(SelectionArrowZ, matrix4x);
			}
			else
			{
				Graphics.DrawMeshNow(ArrowZ, matrix4x);
			}
		}

		public static void DoRotationHandle(Quaternion rotation, Vector3 position, RuntimeHandleAxis selectedAxis = RuntimeHandleAxis.None, LockObject lockObject = null)
		{
			float screenScale = GetScreenScale(position, Camera.current);
			float num = 1f;
			Vector3 s = new Vector3(screenScale, screenScale, screenScale);
			Matrix4x4 transform = Matrix4x4.TRS(Vector3.zero, rotation * Quaternion.AngleAxis(-90f, Vector3.up), Vector3.one);
			Matrix4x4 transform2 = Matrix4x4.TRS(Vector3.zero, rotation * Quaternion.AngleAxis(-90f, Vector3.right), Vector3.one);
			Matrix4x4 transform3 = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
			Matrix4x4 m = Matrix4x4.TRS(position, Quaternion.identity, s);
			bool flag = lockObject?.RotationX ?? false;
			bool flag2 = lockObject?.RotationY ?? false;
			bool flag3 = lockObject?.RotationZ ?? false;
			bool flag4 = lockObject?.RotationScreen ?? false;
			LinesClipMaterial.SetPass(0);
			GL.PushMatrix();
			GL.MultMatrix(m);
			GL.Begin(1);
			if (flag)
			{
				GL.Color(RTHColors.DisabledColor);
			}
			else
			{
				GL.Color((selectedAxis == RuntimeHandleAxis.X) ? RTHColors.SelectionColor : RTHColors.XColor);
			}
			RuntimeGraphics.DrawCircleGL(transform, num);
			if (flag2)
			{
				GL.Color(RTHColors.DisabledColor);
			}
			else
			{
				GL.Color((selectedAxis == RuntimeHandleAxis.Y) ? RTHColors.SelectionColor : RTHColors.YColor);
			}
			RuntimeGraphics.DrawCircleGL(transform2, num);
			if (flag3)
			{
				GL.Color(RTHColors.DisabledColor);
			}
			else
			{
				GL.Color((selectedAxis == RuntimeHandleAxis.Z) ? RTHColors.SelectionColor : RTHColors.ZColor);
			}
			RuntimeGraphics.DrawCircleGL(transform3, num);
			GL.End();
			GL.PopMatrix();
			LinesBillboardMaterial.SetPass(0);
			GL.PushMatrix();
			GL.MultMatrix(m);
			GL.Begin(1);
			if (flag && flag2 && flag3)
			{
				GL.Color(RTHColors.DisabledColor);
			}
			else
			{
				GL.Color((selectedAxis == RuntimeHandleAxis.Free) ? RTHColors.SelectionColor : RTHColors.AltColor);
			}
			RuntimeGraphics.DrawCircleGL(Matrix4x4.identity, num);
			if (flag4)
			{
				GL.Color(RTHColors.DisabledColor);
			}
			else
			{
				GL.Color((selectedAxis == RuntimeHandleAxis.Screen) ? RTHColors.SelectionColor : RTHColors.AltColor);
			}
			RuntimeGraphics.DrawCircleGL(Matrix4x4.identity, num * 1.1f);
			GL.End();
			GL.PopMatrix();
		}

		public static void DoScaleHandle(Vector3 scale, Vector3 position, Quaternion rotation, RuntimeHandleAxis selectedAxis = RuntimeHandleAxis.None, LockObject lockObject = null)
		{
			float screenScale = GetScreenScale(position, Camera.current);
			Matrix4x4 transform = Matrix4x4.TRS(position, rotation, scale * screenScale);
			LinesMaterial.SetPass(0);
			bool flag = lockObject?.ScaleX ?? false;
			bool flag2 = lockObject?.ScaleY ?? false;
			bool flag3 = lockObject?.ScaleZ ?? false;
			GL.Begin(1);
			DoAxes(position, transform, selectedAxis, flag, flag2, flag3);
			GL.End();
			Matrix4x4 matrix4x = Matrix4x4.TRS(Vector3.zero, rotation, scale);
			ShapesMaterial.SetPass(0);
			Vector3 vector = new Vector3(screenScale, screenScale, screenScale);
			Vector3 vector2 = matrix4x.MultiplyVector(Vector3.right) * screenScale * 1f;
			Vector3 vector3 = matrix4x.MultiplyVector(Vector3.up) * screenScale * 1f;
			Vector3 vector4 = matrix4x.MultiplyVector(Vector3.forward) * screenScale * 1f;
			switch (selectedAxis)
			{
			case RuntimeHandleAxis.X:
				Graphics.DrawMeshNow((!flag) ? SelectionCube : DisabledCube, Matrix4x4.TRS(position + vector2, rotation, vector));
				Graphics.DrawMeshNow((!flag2) ? CubeY : DisabledCube, Matrix4x4.TRS(position + vector3, rotation, vector));
				Graphics.DrawMeshNow((!flag3) ? CubeZ : DisabledCube, Matrix4x4.TRS(position + vector4, rotation, vector));
				Graphics.DrawMeshNow((!flag || !flag2 || !flag3) ? CubeUniform : DisabledCube, Matrix4x4.TRS(position, rotation, vector * 1.35f));
				break;
			case RuntimeHandleAxis.Y:
				Graphics.DrawMeshNow((!flag) ? CubeX : DisabledCube, Matrix4x4.TRS(position + vector2, rotation, vector));
				Graphics.DrawMeshNow((!flag2) ? SelectionCube : DisabledCube, Matrix4x4.TRS(position + vector3, rotation, vector));
				Graphics.DrawMeshNow((!flag3) ? CubeZ : DisabledCube, Matrix4x4.TRS(position + vector4, rotation, vector));
				Graphics.DrawMeshNow((!flag || !flag2 || !flag3) ? CubeUniform : DisabledCube, Matrix4x4.TRS(position, rotation, vector * 1.35f));
				break;
			case RuntimeHandleAxis.Z:
				Graphics.DrawMeshNow((!flag) ? CubeX : DisabledCube, Matrix4x4.TRS(position + vector2, rotation, vector));
				Graphics.DrawMeshNow((!flag2) ? CubeY : DisabledCube, Matrix4x4.TRS(position + vector3, rotation, vector));
				Graphics.DrawMeshNow((!flag3) ? SelectionCube : DisabledCube, Matrix4x4.TRS(position + vector4, rotation, vector));
				Graphics.DrawMeshNow((!flag || !flag2 || !flag3) ? CubeUniform : DisabledCube, Matrix4x4.TRS(position, rotation, vector * 1.35f));
				break;
			case RuntimeHandleAxis.Free:
				Graphics.DrawMeshNow((!flag) ? CubeX : DisabledCube, Matrix4x4.TRS(position + vector2, rotation, vector));
				Graphics.DrawMeshNow((!flag2) ? CubeY : DisabledCube, Matrix4x4.TRS(position + vector3, rotation, vector));
				Graphics.DrawMeshNow((!flag3) ? CubeZ : DisabledCube, Matrix4x4.TRS(position + vector4, rotation, vector));
				Graphics.DrawMeshNow((!flag || !flag2 || !flag3) ? SelectionCube : DisabledCube, Matrix4x4.TRS(position, rotation, vector * 1.35f));
				break;
			default:
				Graphics.DrawMeshNow((!flag) ? CubeX : DisabledCube, Matrix4x4.TRS(position + vector2, rotation, vector));
				Graphics.DrawMeshNow((!flag2) ? CubeY : DisabledCube, Matrix4x4.TRS(position + vector3, rotation, vector));
				Graphics.DrawMeshNow((!flag3) ? CubeZ : DisabledCube, Matrix4x4.TRS(position + vector4, rotation, vector));
				Graphics.DrawMeshNow((!flag || !flag2 || !flag3) ? CubeUniform : DisabledCube, Matrix4x4.TRS(position, rotation, vector * 1.35f));
				break;
			}
		}

		public static void DoSceneGizmo(Vector3 position, Quaternion rotation, Vector3 selection, float gizmoScale, float xAlpha = 1f, float yAlpha = 1f, float zAlpha = 1f)
		{
			float num = GetScreenScale(position, Camera.current) * gizmoScale;
			Vector3 vector = new Vector3(num, num, num);
			float billboardOffset = 0.4f;
			if (Camera.current.orthographic)
			{
				billboardOffset = 0.42f;
			}
			if (selection != Vector3.zero)
			{
				if (selection == Vector3.one)
				{
					ShapesMaterialZTestOffset.SetPass(0);
					Graphics.DrawMeshNow(SceneGizmoSelectedCube, Matrix4x4.TRS(position, rotation, vector * 0.15f));
				}
				else if ((xAlpha == 1f || xAlpha == 0f) && (yAlpha == 1f || yAlpha == 0f) && (zAlpha == 1f || zAlpha == 0f))
				{
					ShapesMaterialZTestOffset.SetPass(0);
					Graphics.DrawMeshNow(SceneGizmoSelectedAxis, Matrix4x4.TRS(position, rotation * Quaternion.LookRotation(selection, Vector3.up), vector));
				}
			}
			ShapesMaterialZTest.SetPass(0);
			ShapesMaterialZTest.color = Color.white;
			Graphics.DrawMeshNow(SceneGizmoCube, Matrix4x4.TRS(position, rotation, vector * 0.15f));
			if (xAlpha == 1f && yAlpha == 1f && zAlpha == 1f)
			{
				Graphics.DrawMeshNow(SceneGizmoXAxis, Matrix4x4.TRS(position, rotation, vector));
				Graphics.DrawMeshNow(SceneGizmoYAxis, Matrix4x4.TRS(position, rotation, vector));
				Graphics.DrawMeshNow(SceneGizmoZAxis, Matrix4x4.TRS(position, rotation, vector));
			}
			else if (xAlpha < 1f)
			{
				ShapesMaterialZTest3.SetPass(0);
				ShapesMaterialZTest3.color = new Color(1f, 1f, 1f, yAlpha);
				Graphics.DrawMeshNow(SceneGizmoYAxis, Matrix4x4.TRS(position, rotation, vector));
				ShapesMaterialZTest4.SetPass(0);
				ShapesMaterialZTest4.color = new Color(1f, 1f, 1f, zAlpha);
				Graphics.DrawMeshNow(SceneGizmoZAxis, Matrix4x4.TRS(position, rotation, vector));
				ShapesMaterialZTest2.SetPass(0);
				ShapesMaterialZTest2.color = new Color(1f, 1f, 1f, xAlpha);
				Graphics.DrawMeshNow(SceneGizmoXAxis, Matrix4x4.TRS(position, rotation, vector));
				XMaterial.SetPass(0);
			}
			else if (yAlpha < 1f)
			{
				ShapesMaterialZTest4.SetPass(0);
				ShapesMaterialZTest4.color = new Color(1f, 1f, 1f, zAlpha);
				Graphics.DrawMeshNow(SceneGizmoZAxis, Matrix4x4.TRS(position, rotation, vector));
				ShapesMaterialZTest2.SetPass(0);
				ShapesMaterialZTest2.color = new Color(1f, 1f, 1f, xAlpha);
				Graphics.DrawMeshNow(SceneGizmoXAxis, Matrix4x4.TRS(position, rotation, vector));
				ShapesMaterialZTest3.SetPass(0);
				ShapesMaterialZTest3.color = new Color(1f, 1f, 1f, yAlpha);
				Graphics.DrawMeshNow(SceneGizmoYAxis, Matrix4x4.TRS(position, rotation, vector));
			}
			else
			{
				ShapesMaterialZTest2.SetPass(0);
				ShapesMaterialZTest2.color = new Color(1f, 1f, 1f, xAlpha);
				Graphics.DrawMeshNow(SceneGizmoXAxis, Matrix4x4.TRS(position, rotation, vector));
				ShapesMaterialZTest3.SetPass(0);
				ShapesMaterialZTest3.color = new Color(1f, 1f, 1f, yAlpha);
				Graphics.DrawMeshNow(SceneGizmoYAxis, Matrix4x4.TRS(position, rotation, vector));
				ShapesMaterialZTest4.SetPass(0);
				ShapesMaterialZTest4.color = new Color(1f, 1f, 1f, zAlpha);
				Graphics.DrawMeshNow(SceneGizmoZAxis, Matrix4x4.TRS(position, rotation, vector));
			}
			XMaterial.SetPass(0);
			XMaterial.color = new Color(1f, 1f, 1f, xAlpha);
			DragSceneGizmoAxis(position, rotation, Vector3.right, gizmoScale, 0.125f, billboardOffset, num);
			YMaterial.SetPass(0);
			YMaterial.color = new Color(1f, 1f, 1f, yAlpha);
			DragSceneGizmoAxis(position, rotation, Vector3.up, gizmoScale, 0.125f, billboardOffset, num);
			ZMaterial.SetPass(0);
			ZMaterial.color = new Color(1f, 1f, 1f, zAlpha);
			DragSceneGizmoAxis(position, rotation, Vector3.forward, gizmoScale, 0.125f, billboardOffset, num);
		}

		private static void DragSceneGizmoAxis(Vector3 position, Quaternion rotation, Vector3 axis, float gizmoScale, float billboardScale, float billboardOffset, float sScale)
		{
			Vector3 vector = Vector3.Reflect(Camera.current.transform.forward, axis) * 0.1f;
			float num = Vector3.Dot(Camera.current.transform.forward, axis);
			if (num > 0f)
			{
				if (Camera.current.orthographic)
				{
					vector += axis * num * 0.4f;
				}
				else
				{
					vector = axis * num * 0.7f;
				}
			}
			else if (Camera.current.orthographic)
			{
				vector -= axis * num * 0.1f;
			}
			else
			{
				vector = Vector3.zero;
			}
			Vector3 vector2 = position + (axis + vector) * billboardOffset * sScale;
			float num2 = GetScreenScale(vector2, Camera.current) * gizmoScale;
			Graphics.DrawMeshNow(matrix: Matrix4x4.TRS(vector2, rotation, new Vector3(num2, num2, num2) * billboardScale), mesh: SceneGizmoQuad);
		}

		public static float GetGridFarPlane()
		{
			float y = Camera.current.transform.position.y;
			float num = CountOfDigits(y);
			float num2 = Mathf.Pow(10f, num - 1f);
			return num2 * 150f;
		}

		public static void DrawGrid(Vector3 gridOffset, float camOffset = 0f)
		{
			float f = camOffset;
			f = Mathf.Abs(f);
			f = Mathf.Max(1f, f);
			float num = CountOfDigits(f);
			float num2 = Mathf.Pow(10f, num - 1f);
			float num3 = Mathf.Pow(10f, num);
			float num4 = Mathf.Pow(10f, num + 1f);
			float alpha = 1f - (f - num2) / (num3 - num2);
			float alpha2 = (f * 10f - num3) / (num4 - num3);
			Vector3 position = Camera.current.transform.position;
			DrawGrid(position, gridOffset, num2, alpha, f * 20f);
			DrawGrid(position, gridOffset, num3, alpha2, f * 20f);
		}

		private static void DrawGrid(Vector3 cameraPosition, Vector3 gridOffset, float spacing, float alpha, float fadeDisance)
		{
			cameraPosition.y = gridOffset.y;
			gridOffset.y = 0f;
			GridMaterial.SetFloat("_FadeDistance", fadeDisance);
			GridMaterial.SetPass(0);
			GL.Begin(1);
			GL.Color(new Color(1f, 1f, 1f, 0.1f * alpha));
			cameraPosition.x = Mathf.Floor(cameraPosition.x / spacing) * spacing;
			cameraPosition.z = Mathf.Floor(cameraPosition.z / spacing) * spacing;
			for (int i = -150; i < 150; i++)
			{
				GL.Vertex(gridOffset + cameraPosition + new Vector3((float)i * spacing, 0f, -150f * spacing));
				GL.Vertex(gridOffset + cameraPosition + new Vector3((float)i * spacing, 0f, 150f * spacing));
				GL.Vertex(gridOffset + cameraPosition + new Vector3(-150f * spacing, 0f, (float)i * spacing));
				GL.Vertex(gridOffset + cameraPosition + new Vector3(150f * spacing, 0f, (float)i * spacing));
			}
			GL.End();
		}

		public static void DrawBoundRay(ref Bounds bounds, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			LinesMaterialZTest.SetPass(0);
			Matrix4x4 m = Matrix4x4.TRS(position, rotation, scale);
			Vector3 position2 = m.MultiplyPoint(bounds.center);
			float screenScale = GetScreenScale(position2, Camera.current);
			float num = 10f * screenScale;
			Vector3 center = bounds.center;
			Vector3 vector = bounds.center + new Vector3(0f, 0f - num, 0f);
			GL.PushMatrix();
			GL.MultMatrix(m);
			GL.Begin(1);
			GL.Color(RTHColors.RaysColor);
			int num2 = 100;
			Vector3 vector2 = vector - center;
			vector2 /= (float)num2;
			for (int i = 0; i < num2; i++)
			{
				center += vector2;
				GL.Vertex(center);
				GL.Vertex(center + vector2);
				center += vector2;
			}
			GL.End();
			GL.PopMatrix();
		}

		public static void DrawBounds(ref Bounds bounds, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			LinesMaterialZTest.SetPass(0);
			Matrix4x4 m = Matrix4x4.TRS(position, rotation, scale);
			GL.PushMatrix();
			GL.MultMatrix(m);
			GL.Begin(1);
			GL.Color(RTHColors.BoundsColor);
			for (int i = -1; i <= 1; i += 2)
			{
				for (int j = -1; j <= 1; j += 2)
				{
					for (int k = -1; k <= 1; k += 2)
					{
						Vector3 vector = bounds.center + new Vector3(bounds.extents.x * (float)i, bounds.extents.y * (float)j, bounds.extents.z * (float)k);
						Vector3 position2 = m.MultiplyPoint(vector);
						float num = Mathf.Max(GetScreenScale(position2, Camera.current), 0.1f);
						Vector3 vector2 = Vector3.one * 0.2f * num;
						Vector3 sizeX = new Vector3(Mathf.Min(vector2.x / Mathf.Abs(scale.x), bounds.extents.x), 0f, 0f);
						Vector3 sizeY = new Vector3(0f, Mathf.Min(vector2.y / Mathf.Abs(scale.y), bounds.extents.y), 0f);
						Vector3 sizeZ = new Vector3(0f, 0f, Mathf.Min(vector2.z / Mathf.Abs(scale.z), bounds.extents.z));
						DrawCorner(vector, sizeX, sizeY, sizeZ, new Vector3(-1 * i, -1 * j, -1 * k));
					}
				}
			}
			GL.End();
			GL.PopMatrix();
		}

		private static void DrawCorner(Vector3 p, Vector3 sizeX, Vector3 sizeY, Vector3 sizeZ, Vector3 s)
		{
			GL.Vertex(p);
			GL.Vertex(p + sizeX * s.x);
			GL.Vertex(p);
			GL.Vertex(p + sizeY * s.y);
			GL.Vertex(p);
			GL.Vertex(p + sizeZ * s.z);
			GL.Vertex(p);
			GL.Vertex(p + sizeX * s.x);
			GL.Vertex(p);
			GL.Vertex(p + sizeY * s.y);
			GL.Vertex(p);
			GL.Vertex(p + sizeZ * s.z);
		}

		public static float CountOfDigits(float number)
		{
			return (number != 0f) ? Mathf.Ceil(Mathf.Log10(Mathf.Abs(number) + 0.5f)) : 1f;
		}
	}
}
