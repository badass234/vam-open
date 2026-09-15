using System;
using System.Collections.Generic;
using Leap.Unity.Infix;
using UnityEngine;

namespace Leap.Unity.RuntimeGizmos
{
	public class RuntimeGizmoDrawer
	{
		private enum OperationType
		{
			SetMatrix,
			ToggleWireframe,
			SetColor,
			DrawLine,
			DrawWireSphere,
			DrawMesh
		}

		private struct Line
		{
			public Vector3 a;

			public Vector3 b;

			public Line(Vector3 a, Vector3 b)
			{
				this.a = a;
				this.b = b;
			}
		}

		private struct WireSphere
		{
			public Pose pose;

			public float radius;

			public int numSegments;
		}

		public const int UNLIT_SOLID_PASS = 0;

		public const int UNLIT_TRANSPARENT_PASS = 1;

		public const int SHADED_SOLID_PASS = 2;

		public const int SHADED_TRANSPARENT_PASS = 3;

		private List<OperationType> _operations = new List<OperationType>();

		private List<Matrix4x4> _matrices = new List<Matrix4x4>();

		private List<Color> _colors = new List<Color>();

		private List<Line> _lines = new List<Line>();

		private List<WireSphere> _wireSpheres = new List<WireSphere>();

		private List<Mesh> _meshes = new List<Mesh>();

		private Color _currColor = Color.white;

		private Matrix4x4 _currMatrix = Matrix4x4.identity;

		private Stack<Matrix4x4> _matrixStack = new Stack<Matrix4x4>();

		private bool _isInWireMode;

		private Material _gizmoMaterial;

		private int _operationCountOnGuard = -1;

		public Mesh cubeMesh;

		public Mesh wireCubeMesh;

		public Mesh sphereMesh;

		public Mesh wireSphereMesh;

		private List<Collider> _colliderList = new List<Collider>();

		public Shader gizmoShader
		{
			get
			{
				if (_gizmoMaterial == null)
				{
					return null;
				}
				return _gizmoMaterial.shader;
			}
			set
			{
				if (_gizmoMaterial == null)
				{
					_gizmoMaterial = new Material(value);
					_gizmoMaterial.name = "Runtime Gizmo Material";
					_gizmoMaterial.hideFlags = HideFlags.HideAndDontSave;
				}
				else
				{
					_gizmoMaterial.shader = value;
				}
			}
		}

		public Color color
		{
			get
			{
				return _currColor;
			}
			set
			{
				if (!(_currColor == value))
				{
					_currColor = value;
					_operations.Add(OperationType.SetColor);
					_colors.Add(_currColor);
				}
			}
		}

		public Matrix4x4 matrix
		{
			get
			{
				return _currMatrix;
			}
			set
			{
				if (!(_currMatrix == value))
				{
					_currMatrix = value;
					_operations.Add(OperationType.SetMatrix);
					_matrices.Add(_currMatrix);
				}
			}
		}

		public void BeginGuard()
		{
			_operationCountOnGuard = _operations.Count;
		}

		public void EndGuard()
		{
			bool flag = _operations.Count > _operationCountOnGuard;
			_operationCountOnGuard = -1;
			if (flag)
			{
				Debug.LogError("New gizmos were drawn to the front buffer!  Make sure to never keep a reference to a Drawer, always get a new one every time you want to start drawing.");
			}
		}

		public void RelativeTo(Transform transform)
		{
			matrix = transform.localToWorldMatrix;
		}

		public void PushMatrix()
		{
			_matrixStack.Push(_currMatrix);
		}

		public void PopMatrix()
		{
			matrix = _matrixStack.Pop();
		}

		public void ResetMatrixAndColorState()
		{
			matrix = Matrix4x4.identity;
			color = Color.white;
		}

		public void DrawMesh(Mesh mesh, Matrix4x4 matrix)
		{
			setWireMode(false);
			drawMeshInternal(mesh, matrix);
		}

		public void DrawMesh(Mesh mesh, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			DrawMesh(mesh, Matrix4x4.TRS(position, rotation, scale));
		}

		public void DrawWireMesh(Mesh mesh, Matrix4x4 matrix)
		{
			setWireMode(true);
			drawMeshInternal(mesh, matrix);
		}

		public void DrawWireMesh(Mesh mesh, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			DrawWireMesh(mesh, Matrix4x4.TRS(position, rotation, scale));
		}

		public void DrawLine(Vector3 a, Vector3 b)
		{
			_operations.Add(OperationType.DrawLine);
			_lines.Add(new Line(a, b));
		}

		public void DrawCube(Vector3 position, Vector3 size)
		{
			DrawMesh(cubeMesh, position, Quaternion.identity, size);
		}

		public void DrawWireCube(Vector3 position, Vector3 size)
		{
			DrawWireMesh(wireCubeMesh, position, Quaternion.identity, size);
		}

		public void DrawSphere(Vector3 center, float radius)
		{
			if (sphereMesh == null)
			{
				throw new InvalidOperationException("Cannot draw a sphere because the Runtime Gizmo Manager does not have a sphere mesh assigned!");
			}
			DrawMesh(sphereMesh, center, Quaternion.identity, Vector3.one * radius * 2f);
		}

		public void DrawWireSphere(Pose pose, float radius, int numSegments = 32)
		{
			_operations.Add(OperationType.DrawWireSphere);
			_wireSpheres.Add(new WireSphere
			{
				pose = pose,
				radius = radius,
				numSegments = numSegments
			});
		}

		public void DrawWireSphere(Vector3 center, float radius, int numSegments = 32)
		{
			DrawWireSphere(new Pose(center, Quaternion.identity), radius, numSegments);
		}

		public void DrawEllipsoid(Vector3 foci1, Vector3 foci2, float minorAxis)
		{
			PushMatrix();
			Vector3 pos = (foci1 + foci2) / 2f;
			Quaternion q = Quaternion.LookRotation(foci1 - foci2);
			float z = Mathf.Sqrt(Mathf.Pow(Vector3.Distance(foci1, foci2) / 2f, 2f) + Mathf.Pow(minorAxis / 2f, 2f)) * 2f;
			Vector3 s = new Vector3(minorAxis, minorAxis, z);
			matrix = Matrix4x4.TRS(pos, q, s);
			DrawWireSphere(Vector3.zero, 0.5f);
			PopMatrix();
		}

		public void DrawWireCapsule(Vector3 start, Vector3 end, float radius)
		{
			Vector3 vector = (end - start).normalized * radius;
			Vector3 vector2 = Vector3.Slerp(vector, -vector, 0.5f);
			Vector3 vector3 = Vector3.Cross(vector, vector2).normalized * radius;
			float magnitude = (start - end).magnitude;
			DrawLineWireCircle(start, vector, radius, 8);
			DrawLineWireCircle(end, -vector, radius, 8);
			DrawLine(start + vector3, end + vector3);
			DrawLine(start - vector3, end - vector3);
			DrawLine(start + vector2, end + vector2);
			DrawLine(start - vector2, end - vector2);
			DrawWireArc(start, vector3, vector2, radius, 0.5f, 8);
			DrawWireArc(start, vector2, -vector3, radius, 0.5f, 8);
			DrawWireArc(end, vector3, -vector2, radius, 0.5f, 8);
			DrawWireArc(end, vector2, vector3, radius, 0.5f, 8);
		}

		private void DrawLineWireCircle(Vector3 center, Vector3 normal, float radius, int numCircleSegments = 16)
		{
			DrawWireArc(center, normal, Vector3.Slerp(normal, -normal, 0.5f), radius, 1f, numCircleSegments);
		}

		public void DrawWireArc(Vector3 center, Vector3 normal, Vector3 radialStartDirection, float radius, float fractionOfCircleToDraw, int numCircleSegments = 16)
		{
			normal = normal.normalized;
			Vector3 vector = radialStartDirection.normalized * radius;
			int num = (int)((float)numCircleSegments * fractionOfCircleToDraw);
			Quaternion quaternion = Quaternion.AngleAxis(360f / (float)numCircleSegments, normal);
			for (int i = 0; i < num; i++)
			{
				Vector3 vector2 = quaternion * vector;
				DrawLine(center + vector, center + vector2);
				vector = vector2;
			}
		}

		public void DrawColliders(GameObject gameObject, bool useWireframe = true, bool traverseHierarchy = true, bool drawTriggers = false)
		{
			PushMatrix();
			if (traverseHierarchy)
			{
				gameObject.GetComponentsInChildren(_colliderList);
			}
			else
			{
				gameObject.GetComponents(_colliderList);
			}
			for (int i = 0; i < _colliderList.Count; i++)
			{
				Collider collider = _colliderList[i];
				RelativeTo(collider.transform);
				if (!collider.isTrigger || drawTriggers)
				{
					DrawCollider(collider, true, true);
				}
			}
			PopMatrix();
		}

		public void DrawCollider(Collider collider, bool useWireframe = true, bool skipMatrixSetup = false)
		{
			if (!skipMatrixSetup)
			{
				PushMatrix();
				RelativeTo(collider.transform);
			}
			if (collider is BoxCollider)
			{
				BoxCollider boxCollider = collider as BoxCollider;
				if (useWireframe)
				{
					DrawWireCube(boxCollider.center, boxCollider.size);
				}
				else
				{
					DrawCube(boxCollider.center, boxCollider.size);
				}
			}
			else if (collider is SphereCollider)
			{
				SphereCollider sphereCollider = collider as SphereCollider;
				if (useWireframe)
				{
					DrawWireSphere(sphereCollider.center, sphereCollider.radius);
				}
				else
				{
					DrawSphere(sphereCollider.center, sphereCollider.radius);
				}
			}
			else if (collider is CapsuleCollider)
			{
				CapsuleCollider capsuleCollider = collider as CapsuleCollider;
				if (useWireframe)
				{
					Vector3 vector;
					switch (capsuleCollider.direction)
					{
					case 0:
						vector = Vector3.right;
						break;
					case 1:
						vector = Vector3.up;
						break;
					default:
						vector = Vector3.forward;
						break;
					}
					DrawWireCapsule(capsuleCollider.center + vector * (capsuleCollider.height / 2f - capsuleCollider.radius), capsuleCollider.center - vector * (capsuleCollider.height / 2f - capsuleCollider.radius), capsuleCollider.radius);
				}
				else
				{
					Vector3 zero = Vector3.zero;
					zero += Vector3.one * capsuleCollider.radius * 2f;
					zero += new Vector3((capsuleCollider.direction == 0) ? 1 : 0, (capsuleCollider.direction == 1) ? 1 : 0, (capsuleCollider.direction == 2) ? 1 : 0) * (capsuleCollider.height - capsuleCollider.radius * 2f);
					DrawCube(capsuleCollider.center, zero);
				}
			}
			else if (collider is MeshCollider)
			{
				MeshCollider meshCollider = collider as MeshCollider;
				if (meshCollider.sharedMesh != null)
				{
					if (useWireframe)
					{
						DrawWireMesh(meshCollider.sharedMesh, Matrix4x4.identity);
					}
					else
					{
						DrawMesh(meshCollider.sharedMesh, Matrix4x4.identity);
					}
				}
			}
			if (!skipMatrixSetup)
			{
				PopMatrix();
			}
		}

		public void DrawPosition(Vector3 pos, Color lerpColor, float lerpCoeff, float? overrideScale = null)
		{
			float num;
			if (overrideScale.HasValue)
			{
				num = overrideScale.Value;
			}
			else
			{
				num = 0.06f;
				Camera current = Camera.current;
				Vector4 vector = matrix * pos;
				if (current != null)
				{
					float num2 = Vector3.Distance(vector, current.transform.position);
					num *= num2;
				}
			}
			float num3 = num / 2f;
			float alpha = 0.6f;
			color = Color.red;
			if (lerpCoeff != 0f)
			{
				color = color.LerpHSV(lerpColor, lerpCoeff);
			}
			DrawLine(pos, pos + Vector3.right * num3);
			color = Color.black.WithAlpha(alpha);
			if (lerpCoeff != 0f)
			{
				color = color.LerpHSV(lerpColor, lerpCoeff);
			}
			DrawLine(pos, pos - Vector3.right * num3);
			color = Color.green;
			if (lerpCoeff != 0f)
			{
				color = color.LerpHSV(lerpColor, lerpCoeff);
			}
			DrawLine(pos, pos + Vector3.up * num3);
			color = Color.black.WithAlpha(alpha);
			if (lerpCoeff != 0f)
			{
				color = color.LerpHSV(lerpColor, lerpCoeff);
			}
			DrawLine(pos, pos - Vector3.up * num3);
			color = Color.blue;
			if (lerpCoeff != 0f)
			{
				color = color.LerpHSV(lerpColor, lerpCoeff);
			}
			DrawLine(pos, pos + Vector3.forward * num3);
			color = Color.black.WithAlpha(alpha);
			if (lerpCoeff != 0f)
			{
				color = color.LerpHSV(lerpColor, lerpCoeff);
			}
			DrawLine(pos, pos - Vector3.forward * num3);
		}

		public void DrawPosition(Vector3 pos)
		{
			DrawPosition(pos, Color.white, 0f, null);
		}

		public void DrawPosition(Vector3 pos, float overrideScale)
		{
			DrawPosition(pos, Color.white, 0f, overrideScale);
		}

		public void DrawRect(Transform frame, Rect rect)
		{
			PushMatrix();
			matrix = frame.localToWorldMatrix;
			DrawLine(rect.Corner00(), rect.Corner01());
			DrawLine(rect.Corner01(), rect.Corner11());
			DrawLine(rect.Corner11(), rect.Corner10());
			DrawLine(rect.Corner10(), rect.Corner00());
			PopMatrix();
		}

		public void ClearAllGizmos()
		{
			_operations.Clear();
			_matrices.Clear();
			_colors.Clear();
			_lines.Clear();
			_wireSpheres.Clear();
			_meshes.Clear();
			_isInWireMode = false;
			_currMatrix = Matrix4x4.identity;
			_currColor = Color.white;
		}

		public void DrawAllGizmosToScreen()
		{
			try
			{
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				int num5 = 0;
				int currPass = -1;
				_currMatrix = Matrix4x4.identity;
				_currColor = Color.white;
				GL.wireframe = false;
				for (int i = 0; i < _operations.Count; i++)
				{
					OperationType operationType = _operations[i];
					switch (operationType)
					{
					case OperationType.SetMatrix:
						_currMatrix = _matrices[num++];
						break;
					case OperationType.SetColor:
						_currColor = _colors[num2++];
						currPass = -1;
						break;
					case OperationType.ToggleWireframe:
						GL.wireframe = !GL.wireframe;
						break;
					case OperationType.DrawLine:
					{
						setPass(ref currPass, true, null);
						GL.Begin(1);
						Line line = _lines[num3++];
						GL.Vertex(_currMatrix.MultiplyPoint(line.a));
						GL.Vertex(_currMatrix.MultiplyPoint(line.b));
						GL.End();
						break;
					}
					case OperationType.DrawWireSphere:
					{
						setPass(ref currPass, true, null);
						GL.Begin(1);
						WireSphere wireSphere = _wireSpheres[num4++];
						drawWireSphereNow(wireSphere, ref currPass);
						GL.End();
						break;
					}
					case OperationType.DrawMesh:
						if (GL.wireframe)
						{
							setPass(ref currPass, true, null);
						}
						else
						{
							setPass(ref currPass, false, null);
						}
						Graphics.DrawMeshNow(_meshes[num5++], _currMatrix * _matrices[num++]);
						break;
					default:
						throw new InvalidOperationException("Unexpected operation type " + operationType);
					}
				}
			}
			finally
			{
				GL.wireframe = false;
			}
		}

		private void drawLineNow(Vector3 a, Vector3 b)
		{
			GL.Vertex(_currMatrix.MultiplyPoint(a));
			GL.Vertex(_currMatrix.MultiplyPoint(b));
		}

		private void drawWireArcNow(Vector3 center, Vector3 normal, Vector3 radialStartDirection, float radius, float fractionOfCircleToDraw, int numCircleSegments = 16)
		{
			normal = normal.normalized;
			Vector3 vector = radialStartDirection.normalized * radius;
			int num = (int)((float)numCircleSegments * fractionOfCircleToDraw);
			Quaternion quaternion = Quaternion.AngleAxis(360f / (float)numCircleSegments, normal);
			for (int i = 0; i < num; i++)
			{
				Vector3 vector2 = quaternion * vector;
				drawLineNow(center + vector, center + vector2);
				vector = vector2;
			}
		}

		private void setCurrentPassColorIfNew(Color desiredColor, ref int curPass)
		{
			if (_currColor != desiredColor)
			{
				_currColor = desiredColor;
				setPass(ref curPass, true, null);
			}
		}

		private void drawPlaneSoftenedWireArcNow(Vector3 position, Vector3 circleNormal, Vector3 radialStartDirection, float radius, Color inFrontOfPlaneColor, Color behindPlaneColor, Vector3 planeNormal, ref int curPass, float fractionOfCircleToDraw = 1f, int numCircleSegments = 16)
		{
			Color currColor = _currColor;
			Vector3 b = planeNormal.Cross(circleNormal);
			Quaternion quaternion = Quaternion.AngleAxis(360f / (float)numCircleSegments, circleNormal);
			Vector3 vector = radialStartDirection * radius;
			for (int i = 0; i < numCircleSegments + 1; i++)
			{
				Vector3 vector2 = quaternion * vector;
				float num = vector.SignedAngle(b, circleNormal);
				float num2 = vector2.SignedAngle(b, circleNormal);
				bool flag = num < 0f;
				bool flag2 = num2 < 0f;
				if (flag != flag2)
				{
					Color value = Color.Lerp(inFrontOfPlaneColor, behindPlaneColor, 0.5f);
					GL.End();
					setPass(ref curPass, true, value);
					GL.Begin(1);
				}
				else if (flag)
				{
					GL.End();
					setPass(ref curPass, true, inFrontOfPlaneColor);
					GL.Begin(1);
				}
				else
				{
					GL.End();
					setPass(ref curPass, true, behindPlaneColor);
					GL.Begin(1);
				}
				drawLineNow(vector, vector2);
				vector = vector2;
			}
			_currColor = currColor;
		}

		private void drawWireSphereNow(WireSphere wireSphere, ref int curPass)
		{
			Vector3 position = wireSphere.pose.position;
			Quaternion rotation = wireSphere.pose.rotation;
			Vector3 vector = _currMatrix.MultiplyPoint3x4(position);
			Vector3 normalized = (Camera.current.transform.position - vector).normalized;
			Vector3 vector2 = _currMatrix.inverse.MultiplyVector(normalized);
			drawWireArcNow(position, vector2, vector2.Perpendicular(), wireSphere.radius, 1f, wireSphere.numSegments);
			Vector3 vector3 = rotation * Vector3.right;
			Vector3 vector4 = rotation * Vector3.up;
			Vector3 vector5 = rotation * Vector3.forward;
			drawPlaneSoftenedWireArcNow(position, vector4, vector3, wireSphere.radius, _currColor, _currColor.WithAlpha(_currColor.a * 0.1f), vector2, ref curPass, 1f, wireSphere.numSegments);
			drawPlaneSoftenedWireArcNow(position, vector5, vector4, wireSphere.radius, _currColor, _currColor.WithAlpha(_currColor.a * 0.1f), vector2, ref curPass, 1f, wireSphere.numSegments);
			drawPlaneSoftenedWireArcNow(position, vector3, vector5, wireSphere.radius, _currColor, _currColor.WithAlpha(_currColor.a * 0.1f), vector2, ref curPass, 1f, wireSphere.numSegments);
		}

		private void setPass(ref int currPass, bool isUnlit, Color? desiredCurrColor = null)
		{
			bool flag = false;
			if (desiredCurrColor.HasValue)
			{
				flag = _currColor != desiredCurrColor.Value;
				_currColor = desiredCurrColor.Value;
			}
			int num = (isUnlit ? ((_currColor.a < 1f) ? 1 : 0) : ((!(_currColor.a < 1f)) ? 2 : 3));
			if (currPass != num || flag)
			{
				currPass = num;
				_gizmoMaterial.color = _currColor;
				_gizmoMaterial.SetPass(currPass);
			}
		}

		private void drawMeshInternal(Mesh mesh, Matrix4x4 matrix)
		{
			if (mesh == null)
			{
				throw new InvalidOperationException("Mesh cannot be null!");
			}
			_operations.Add(OperationType.DrawMesh);
			_meshes.Add(mesh);
			_matrices.Add(matrix);
		}

		private void setWireMode(bool wireMode)
		{
			if (_isInWireMode != wireMode)
			{
				_operations.Add(OperationType.ToggleWireframe);
				_isInWireMode = wireMode;
			}
		}
	}
}
