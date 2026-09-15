using System.Collections.Generic;
using System.Linq;
using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTHandles
{
	public class PositionHandle : BaseHandle
	{
		public float GridSize = 1f;

		private Vector3 m_cursorPosition;

		private Vector3 m_currentPosition;

		private Vector3 m_prevPoint;

		private Matrix4x4 m_matrix;

		private Matrix4x4 m_inverse;

		private Vector2 m_prevMousePosition;

		private int[] m_targetLayers;

		private Transform[] m_snapTargets;

		private Bounds[] m_snapTargetsBounds;

		private ExposeToEditor[] m_allExposedToEditor;

		public bool SnapToGround;

		public KeyCode SnapToGroundKey = KeyCode.G;

		public KeyCode SnappingKey = KeyCode.V;

		public KeyCode SnappingToggle = KeyCode.LeftShift;

		private bool m_isInSnappingMode;

		private Vector3[] m_boundingBoxCorners = new Vector3[8];

		private Vector3 m_handleOffset;

		private bool IsInSnappingMode => m_isInSnappingMode || RuntimeTools.IsSnapping;

		protected override Vector3 HandlePosition
		{
			get
			{
				return base.transform.position + m_handleOffset;
			}
			set
			{
				base.transform.position = value - m_handleOffset;
			}
		}

		protected override RuntimeTool Tool => RuntimeTool.Move;

		protected override float CurrentGridUnitSize => GridSize;

		protected override void OnEnableOverride()
		{
			base.OnEnableOverride();
			m_isInSnappingMode = false;
			RuntimeTools.IsSnapping = false;
			m_handleOffset = Vector3.zero;
			m_targetLayers = null;
			m_snapTargets = null;
			m_snapTargetsBounds = null;
			m_allExposedToEditor = null;
			RuntimeTools.IsSnappingChanged += OnSnappingChanged;
			OnSnappingChanged();
		}

		protected override void OnDisableOverride()
		{
			base.OnDisableOverride();
			RuntimeTools.IsSnapping = false;
			m_targetLayers = null;
			m_snapTargets = null;
			m_snapTargetsBounds = null;
			m_allExposedToEditor = null;
			RuntimeTools.IsSnappingChanged -= OnSnappingChanged;
		}

		protected override void UpdateOverride()
		{
			base.UpdateOverride();
			if (RuntimeTools.IsPointerOverGameObject())
			{
				return;
			}
			if (base.IsDragging && (SnapToGround || InputController.GetKey(SnapToGroundKey)) && SelectedAxis != RuntimeHandleAxis.Y)
			{
				SnapActiveTargetsToGround(base.ActiveTargets, SceneCamera, true);
				base.transform.position = base.Targets[0].position;
			}
			if (HightlightOnHover && !base.IsDragging)
			{
				SelectedAxis = Hit();
			}
			if (InputController.GetKeyDown(SnappingKey))
			{
				if (base.LockObject == null || !base.LockObject.IsPositionLocked)
				{
					m_isInSnappingMode = true;
					if (InputController.GetKey(SnappingToggle))
					{
						RuntimeTools.IsSnapping = !RuntimeTools.IsSnapping;
					}
					BeginSnap();
					m_prevMousePosition = Input.mousePosition;
				}
			}
			else if (InputController.GetKeyUp(SnappingKey))
			{
				SelectedAxis = RuntimeHandleAxis.None;
				m_isInSnappingMode = false;
				if (!IsInSnappingMode)
				{
					m_handleOffset = Vector3.zero;
				}
				if (Model != null && Model is PositionHandleModel)
				{
					((PositionHandleModel)Model).IsVertexSnapping = false;
				}
			}
			if (!IsInSnappingMode)
			{
				return;
			}
			Vector2 vector = Input.mousePosition;
			if (RuntimeTools.SnappingMode == SnappingMode.BoundingBox)
			{
				if (base.IsDragging)
				{
					SelectedAxis = RuntimeHandleAxis.Snap;
					if (m_prevMousePosition != vector)
					{
						m_prevMousePosition = vector;
						float minDistance = float.MaxValue;
						Vector3 minPoint = Vector3.zero;
						bool minPointFound = false;
						for (int i = 0; i < m_allExposedToEditor.Length; i++)
						{
							ExposeToEditor exposeToEditor = m_allExposedToEditor[i];
							Bounds bounds = exposeToEditor.Bounds;
							m_boundingBoxCorners[0] = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z);
							m_boundingBoxCorners[1] = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, 0f - bounds.extents.z);
							m_boundingBoxCorners[2] = bounds.center + new Vector3(bounds.extents.x, 0f - bounds.extents.y, bounds.extents.z);
							m_boundingBoxCorners[3] = bounds.center + new Vector3(bounds.extents.x, 0f - bounds.extents.y, 0f - bounds.extents.z);
							m_boundingBoxCorners[4] = bounds.center + new Vector3(0f - bounds.extents.x, bounds.extents.y, bounds.extents.z);
							m_boundingBoxCorners[5] = bounds.center + new Vector3(0f - bounds.extents.x, bounds.extents.y, 0f - bounds.extents.z);
							m_boundingBoxCorners[6] = bounds.center + new Vector3(0f - bounds.extents.x, 0f - bounds.extents.y, bounds.extents.z);
							m_boundingBoxCorners[7] = bounds.center + new Vector3(0f - bounds.extents.x, 0f - bounds.extents.y, 0f - bounds.extents.z);
							GetMinPoint(ref minDistance, ref minPoint, ref minPointFound, exposeToEditor.BoundsObject.transform);
						}
						if (minPointFound)
						{
							HandlePosition = minPoint;
						}
					}
					return;
				}
				SelectedAxis = RuntimeHandleAxis.None;
				if (!(m_prevMousePosition != vector))
				{
					return;
				}
				m_prevMousePosition = vector;
				float minDistance2 = float.MaxValue;
				Vector3 minPoint2 = Vector3.zero;
				bool minPointFound2 = false;
				for (int j = 0; j < m_snapTargets.Length; j++)
				{
					Transform tr = m_snapTargets[j];
					Bounds bounds2 = m_snapTargetsBounds[j];
					m_boundingBoxCorners[0] = bounds2.center + new Vector3(bounds2.extents.x, bounds2.extents.y, bounds2.extents.z);
					m_boundingBoxCorners[1] = bounds2.center + new Vector3(bounds2.extents.x, bounds2.extents.y, 0f - bounds2.extents.z);
					m_boundingBoxCorners[2] = bounds2.center + new Vector3(bounds2.extents.x, 0f - bounds2.extents.y, bounds2.extents.z);
					m_boundingBoxCorners[3] = bounds2.center + new Vector3(bounds2.extents.x, 0f - bounds2.extents.y, 0f - bounds2.extents.z);
					m_boundingBoxCorners[4] = bounds2.center + new Vector3(0f - bounds2.extents.x, bounds2.extents.y, bounds2.extents.z);
					m_boundingBoxCorners[5] = bounds2.center + new Vector3(0f - bounds2.extents.x, bounds2.extents.y, 0f - bounds2.extents.z);
					m_boundingBoxCorners[6] = bounds2.center + new Vector3(0f - bounds2.extents.x, 0f - bounds2.extents.y, bounds2.extents.z);
					m_boundingBoxCorners[7] = bounds2.center + new Vector3(0f - bounds2.extents.x, 0f - bounds2.extents.y, 0f - bounds2.extents.z);
					if (base.Targets[j] != null)
					{
						GetMinPoint(ref minDistance2, ref minPoint2, ref minPointFound2, tr);
					}
				}
				if (minPointFound2)
				{
					m_handleOffset = minPoint2 - base.transform.position;
				}
				return;
			}
			if (base.IsDragging)
			{
				SelectedAxis = RuntimeHandleAxis.Snap;
				if (!(m_prevMousePosition != vector))
				{
					return;
				}
				m_prevMousePosition = vector;
				Ray ray = SceneCamera.ScreenPointToRay(vector);
				LayerMask layerMask = 16;
				layerMask = ~(int)layerMask;
				for (int k = 0; k < m_snapTargets.Length; k++)
				{
					m_targetLayers[k] = m_snapTargets[k].gameObject.layer;
					m_snapTargets[k].gameObject.layer = 4;
				}
				GameObject gameObject = null;
				RaycastHit hitInfo;
				if (Physics.Raycast(ray, out hitInfo, float.PositiveInfinity, layerMask))
				{
					gameObject = hitInfo.collider.gameObject;
				}
				else
				{
					float num = float.MaxValue;
					for (int l = 0; l < m_allExposedToEditor.Length; l++)
					{
						ExposeToEditor exposeToEditor2 = m_allExposedToEditor[l];
						Bounds bounds3 = exposeToEditor2.Bounds;
						m_boundingBoxCorners[0] = bounds3.center + new Vector3(bounds3.extents.x, bounds3.extents.y, bounds3.extents.z);
						m_boundingBoxCorners[1] = bounds3.center + new Vector3(bounds3.extents.x, bounds3.extents.y, 0f - bounds3.extents.z);
						m_boundingBoxCorners[2] = bounds3.center + new Vector3(bounds3.extents.x, 0f - bounds3.extents.y, bounds3.extents.z);
						m_boundingBoxCorners[3] = bounds3.center + new Vector3(bounds3.extents.x, 0f - bounds3.extents.y, 0f - bounds3.extents.z);
						m_boundingBoxCorners[4] = bounds3.center + new Vector3(0f - bounds3.extents.x, bounds3.extents.y, bounds3.extents.z);
						m_boundingBoxCorners[5] = bounds3.center + new Vector3(0f - bounds3.extents.x, bounds3.extents.y, 0f - bounds3.extents.z);
						m_boundingBoxCorners[6] = bounds3.center + new Vector3(0f - bounds3.extents.x, 0f - bounds3.extents.y, bounds3.extents.z);
						m_boundingBoxCorners[7] = bounds3.center + new Vector3(0f - bounds3.extents.x, 0f - bounds3.extents.y, 0f - bounds3.extents.z);
						for (int m = 0; m < m_boundingBoxCorners.Length; m++)
						{
							Vector2 vector2 = SceneCamera.WorldToScreenPoint(exposeToEditor2.BoundsObject.transform.TransformPoint(m_boundingBoxCorners[m]));
							float magnitude = (vector2 - vector).magnitude;
							if (magnitude < num)
							{
								gameObject = exposeToEditor2.gameObject;
								num = magnitude;
							}
						}
					}
				}
				if (gameObject != null)
				{
					float minDistance3 = float.MaxValue;
					Vector3 minPoint3 = Vector3.zero;
					bool minPointFound3 = false;
					Transform meshTransform;
					Mesh mesh = GetMesh(gameObject, out meshTransform);
					GetMinPoint(meshTransform, ref minDistance3, ref minPoint3, ref minPointFound3, mesh);
					if (minPointFound3)
					{
						HandlePosition = minPoint3;
					}
				}
				for (int n = 0; n < m_snapTargets.Length; n++)
				{
					m_snapTargets[n].gameObject.layer = m_targetLayers[n];
				}
				return;
			}
			SelectedAxis = RuntimeHandleAxis.None;
			if (m_prevMousePosition != vector)
			{
				m_prevMousePosition = vector;
				float minDistance4 = float.MaxValue;
				Vector3 minPoint4 = Vector3.zero;
				bool minPointFound4 = false;
				for (int num2 = 0; num2 < base.RealTargets.Length; num2++)
				{
					Transform transform = base.RealTargets[num2];
					Transform meshTransform2;
					Mesh mesh2 = GetMesh(transform.gameObject, out meshTransform2);
					GetMinPoint(meshTransform2, ref minDistance4, ref minPoint4, ref minPointFound4, mesh2);
				}
				if (minPointFound4)
				{
					m_handleOffset = minPoint4 - base.transform.position;
				}
			}
		}

		private void GetMinPoint(Transform meshTransform, ref float minDistance, ref Vector3 minPoint, ref bool minPointFound, Mesh mesh)
		{
			if (!(mesh != null) || !mesh.isReadable)
			{
				return;
			}
			Vector3[] vertices = mesh.vertices;
			for (int i = 0; i < vertices.Length; i++)
			{
				Vector3 position = vertices[i];
				position = meshTransform.TransformPoint(position);
				Vector3 vector = SceneCamera.WorldToScreenPoint(position);
				vector.z = 0f;
				Vector3 mousePosition = Input.mousePosition;
				mousePosition.z = 0f;
				float magnitude = (vector - mousePosition).magnitude;
				if (magnitude < minDistance)
				{
					minPointFound = true;
					minDistance = magnitude;
					minPoint = position;
				}
			}
		}

		private static Mesh GetMesh(GameObject go, out Transform meshTransform)
		{
			Mesh result = null;
			meshTransform = null;
			MeshFilter componentInChildren = go.GetComponentInChildren<MeshFilter>();
			if (componentInChildren != null)
			{
				result = componentInChildren.sharedMesh;
				meshTransform = componentInChildren.transform;
			}
			else
			{
				SkinnedMeshRenderer componentInChildren2 = go.GetComponentInChildren<SkinnedMeshRenderer>();
				if (componentInChildren2 != null)
				{
					result = componentInChildren2.sharedMesh;
					meshTransform = componentInChildren2.transform;
				}
				else
				{
					MeshCollider componentInChildren3 = go.GetComponentInChildren<MeshCollider>();
					if (componentInChildren3 != null)
					{
						result = componentInChildren3.sharedMesh;
						meshTransform = componentInChildren3.transform;
					}
				}
			}
			return result;
		}

		protected override void OnDrop()
		{
			base.OnDrop();
			if (SnapToGround || InputController.GetKey(SnapToGroundKey))
			{
				SnapActiveTargetsToGround(base.ActiveTargets, SceneCamera, true);
				base.transform.position = base.Targets[0].position;
			}
		}

		private static void SnapActiveTargetsToGround(Transform[] targets, Camera camera, bool rotate)
		{
			Plane[] array = GeometryUtility.CalculateFrustumPlanes(camera);
			foreach (Transform activeTarget in targets)
			{
				Ray ray = new Ray(activeTarget.position, Vector3.up);
				bool flag = false;
				Vector3 origin = activeTarget.position;
				for (int j = 0; j < array.Length; j++)
				{
					float enter;
					if (array[j].Raycast(ray, out enter))
					{
						flag = true;
						origin = ray.GetPoint(enter);
					}
				}
				if (!flag)
				{
					continue;
				}
				ray = new Ray(origin, Vector3.down);
				RaycastHit[] array2 = (from hit in Physics.RaycastAll(ray)
					where !hit.transform.IsChildOf(activeTarget)
					select hit).ToArray();
				if (array2.Length == 0)
				{
					continue;
				}
				float num = float.PositiveInfinity;
				RaycastHit raycastHit = array2[0];
				for (int k = 0; k < array2.Length; k++)
				{
					float magnitude = (activeTarget.position - array2[k].point).magnitude;
					if (magnitude < num)
					{
						num = magnitude;
						raycastHit = array2[k];
					}
				}
				activeTarget.position += raycastHit.point - activeTarget.position;
				if (rotate)
				{
					activeTarget.rotation = Quaternion.FromToRotation(activeTarget.up, raycastHit.normal) * activeTarget.rotation;
				}
			}
		}

		private void OnSnappingChanged()
		{
			if (RuntimeTools.IsSnapping)
			{
				BeginSnap();
				return;
			}
			m_handleOffset = Vector3.zero;
			if (Model != null && Model is PositionHandleModel)
			{
				((PositionHandleModel)Model).IsVertexSnapping = false;
			}
		}

		private void BeginSnap()
		{
			if (SceneCamera == null)
			{
				return;
			}
			if (Model != null && Model is PositionHandleModel)
			{
				((PositionHandleModel)Model).IsVertexSnapping = true;
			}
			HashSet<Transform> hashSet = new HashSet<Transform>();
			List<Transform> list = new List<Transform>();
			List<Bounds> list2 = new List<Bounds>();
			if (base.Target != null)
			{
				for (int i = 0; i < base.RealTargets.Length; i++)
				{
					Transform transform = base.RealTargets[i];
					if (!(transform != null))
					{
						continue;
					}
					list.Add(transform);
					hashSet.Add(transform);
					ExposeToEditor component = transform.GetComponent<ExposeToEditor>();
					if (component != null)
					{
						list2.Add(component.Bounds);
						continue;
					}
					MeshFilter component2 = transform.GetComponent<MeshFilter>();
					if (component2 != null && component2.sharedMesh != null)
					{
						list2.Add(component2.sharedMesh.bounds);
						continue;
					}
					SkinnedMeshRenderer component3 = transform.GetComponent<SkinnedMeshRenderer>();
					if (component3 != null && component3.sharedMesh != null)
					{
						list2.Add(component3.sharedMesh.bounds);
						continue;
					}
					Bounds item = new Bounds(Vector3.zero, Vector3.zero);
					list2.Add(item);
				}
			}
			m_snapTargets = list.ToArray();
			m_targetLayers = new int[m_snapTargets.Length];
			m_snapTargetsBounds = list2.ToArray();
			Plane[] planes = GeometryUtility.CalculateFrustumPlanes(SceneCamera);
			ExposeToEditor[] array = Object.FindObjectsOfType<ExposeToEditor>();
			List<ExposeToEditor> list3 = new List<ExposeToEditor>();
			foreach (ExposeToEditor exposeToEditor in array)
			{
				if (exposeToEditor.CanSnap && GeometryUtility.TestPlanesAABB(planes, new Bounds(exposeToEditor.transform.TransformPoint(exposeToEditor.Bounds.center), Vector3.zero)) && !hashSet.Contains(exposeToEditor.transform))
				{
					list3.Add(exposeToEditor);
				}
			}
			m_allExposedToEditor = list3.ToArray();
		}

		private void GetMinPoint(ref float minDistance, ref Vector3 minPoint, ref bool minPointFound, Transform tr)
		{
			for (int i = 0; i < m_boundingBoxCorners.Length; i++)
			{
				Vector3 vector = tr.TransformPoint(m_boundingBoxCorners[i]);
				Vector3 vector2 = SceneCamera.WorldToScreenPoint(vector);
				vector2.z = 0f;
				Vector3 mousePosition = Input.mousePosition;
				mousePosition.z = 0f;
				float magnitude = (vector2 - mousePosition).magnitude;
				if (magnitude < minDistance)
				{
					minPointFound = true;
					minDistance = magnitude;
					minPoint = vector;
				}
			}
		}

		private bool HitSnapHandle()
		{
			Vector3 vector = SceneCamera.WorldToScreenPoint(HandlePosition);
			Vector3 mousePosition = Input.mousePosition;
			return vector.x - 10f <= mousePosition.x && mousePosition.x <= vector.x + 10f && vector.y - 10f <= mousePosition.y && mousePosition.y <= vector.y + 10f;
		}

		private bool HitQuad(Vector3 axis, Matrix4x4 matrix, float size)
		{
			Ray ray = SceneCamera.ScreenPointToRay(Input.mousePosition);
			Plane plane = new Plane(matrix.MultiplyVector(axis).normalized, matrix.MultiplyPoint(Vector3.zero));
			float enter;
			if (!plane.Raycast(ray, out enter))
			{
				return false;
			}
			Vector3 point = ray.GetPoint(enter);
			point = matrix.inverse.MultiplyPoint(point);
			Vector3 lhs = matrix.inverse.MultiplyVector(SceneCamera.transform.position - HandlePosition);
			float num = Mathf.Sign(Vector3.Dot(lhs, Vector3.right));
			float num2 = Mathf.Sign(Vector3.Dot(lhs, Vector3.up));
			float num3 = Mathf.Sign(Vector3.Dot(lhs, Vector3.forward));
			point.x *= num;
			point.y *= num2;
			point.z *= num3;
			float num4 = -0.01f;
			bool flag = point.x >= num4 && point.x <= size && point.y >= num4 && point.y <= size && point.z >= num4 && point.z <= size;
			if (flag)
			{
				base.DragPlane = GetDragPlane(matrix, axis);
			}
			return flag;
		}

		private RuntimeHandleAxis Hit()
		{
			float screenScale = RuntimeHandles.GetScreenScale(HandlePosition, SceneCamera);
			m_matrix = Matrix4x4.TRS(HandlePosition, base.Rotation, Vector3.one);
			m_inverse = m_matrix.inverse;
			Matrix4x4 matrix = Matrix4x4.TRS(HandlePosition, base.Rotation, new Vector3(screenScale, screenScale, screenScale));
			float size = 0.3f * screenScale;
			if (HitQuad(Vector3.up * 1f, m_matrix, size))
			{
				return RuntimeHandleAxis.XZ;
			}
			if (HitQuad(Vector3.right * 1f, m_matrix, size))
			{
				return RuntimeHandleAxis.YZ;
			}
			if (HitQuad(Vector3.forward * 1f, m_matrix, size))
			{
				return RuntimeHandleAxis.XY;
			}
			float distanceToAxis;
			bool flag = HitAxis(Vector3.up * 1f, matrix, out distanceToAxis);
			float distanceToAxis2;
			flag |= HitAxis(Vector3.forward * 1f, matrix, out distanceToAxis2);
			float distanceToAxis3;
			if (flag | HitAxis(Vector3.right * 1f, matrix, out distanceToAxis3))
			{
				if (distanceToAxis <= distanceToAxis2 && distanceToAxis <= distanceToAxis3)
				{
					return RuntimeHandleAxis.Y;
				}
				if (distanceToAxis3 <= distanceToAxis && distanceToAxis3 <= distanceToAxis2)
				{
					return RuntimeHandleAxis.X;
				}
				return RuntimeHandleAxis.Z;
			}
			return RuntimeHandleAxis.None;
		}

		protected override bool OnBeginDrag()
		{
			SelectedAxis = Hit();
			m_currentPosition = HandlePosition;
			m_cursorPosition = HandlePosition;
			if (IsInSnappingMode)
			{
				return HitSnapHandle();
			}
			if (SelectedAxis == RuntimeHandleAxis.XZ)
			{
				return GetPointOnDragPlane(Input.mousePosition, out m_prevPoint);
			}
			if (SelectedAxis == RuntimeHandleAxis.YZ)
			{
				return GetPointOnDragPlane(Input.mousePosition, out m_prevPoint);
			}
			if (SelectedAxis == RuntimeHandleAxis.XY)
			{
				return GetPointOnDragPlane(Input.mousePosition, out m_prevPoint);
			}
			if (SelectedAxis != 0)
			{
				base.DragPlane = GetDragPlane();
				bool pointOnDragPlane = GetPointOnDragPlane(Input.mousePosition, out m_prevPoint);
				if (!pointOnDragPlane)
				{
					SelectedAxis = RuntimeHandleAxis.None;
				}
				return pointOnDragPlane;
			}
			return false;
		}

		protected override void OnDrag()
		{
			Vector3 point;
			if (IsInSnappingMode || !GetPointOnDragPlane(Input.mousePosition, out point))
			{
				return;
			}
			Vector3 vector = m_inverse.MultiplyVector(point - m_prevPoint);
			float magnitude = vector.magnitude;
			if (SelectedAxis == RuntimeHandleAxis.X)
			{
				vector.y = (vector.z = 0f);
			}
			else if (SelectedAxis == RuntimeHandleAxis.Y)
			{
				vector.x = (vector.z = 0f);
			}
			else if (SelectedAxis == RuntimeHandleAxis.Z)
			{
				vector.x = (vector.y = 0f);
			}
			if (base.LockObject != null)
			{
				if (base.LockObject.PositionX)
				{
					vector.x = 0f;
				}
				if (base.LockObject.PositionY)
				{
					vector.y = 0f;
				}
				if (base.LockObject.PositionZ)
				{
					vector.z = 0f;
				}
			}
			if ((double)base.EffectiveGridUnitSize == 0.0)
			{
				vector = m_matrix.MultiplyVector(vector).normalized * magnitude;
				base.transform.position += vector;
				m_prevPoint = point;
				return;
			}
			vector = m_matrix.MultiplyVector(vector).normalized * magnitude;
			m_cursorPosition += vector;
			Vector3 vector2 = m_cursorPosition - m_currentPosition;
			Vector3 zero = Vector3.zero;
			if (Mathf.Abs(vector2.x * 1.5f) >= base.EffectiveGridUnitSize)
			{
				zero.x = base.EffectiveGridUnitSize * Mathf.Sign(vector2.x);
			}
			if (Mathf.Abs(vector2.y * 1.5f) >= base.EffectiveGridUnitSize)
			{
				zero.y = base.EffectiveGridUnitSize * Mathf.Sign(vector2.y);
			}
			if (Mathf.Abs(vector2.z * 1.5f) >= base.EffectiveGridUnitSize)
			{
				zero.z = base.EffectiveGridUnitSize * Mathf.Sign(vector2.z);
			}
			m_currentPosition += zero;
			HandlePosition = m_currentPosition;
			m_prevPoint = point;
		}

		protected override void DrawOverride()
		{
			RuntimeHandles.DoPositionHandle(HandlePosition, base.Rotation, SelectedAxis, IsInSnappingMode, base.LockObject);
		}
	}
}
