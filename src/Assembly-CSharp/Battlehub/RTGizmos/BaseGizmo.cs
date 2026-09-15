using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTGizmos
{
	public abstract class BaseGizmo : MonoBehaviour, IGL
	{
		public float GridSize = 1f;

		public Color LineColor = new Color(0f, 1f, 0f, 0.75f);

		public Color HandlesColor = new Color(0f, 1f, 0f, 0.75f);

		public Color SelectionColor = new Color(1f, 1f, 0f, 1f);

		public bool EnableUndo = true;

		public KeyCode UnitSnapKey = KeyCode.LeftControl;

		public Camera SceneCamera;

		public float SelectionMargin = 10f;

		public Transform Target;

		private bool m_isDragging;

		private int m_dragIndex;

		private Plane m_dragPlane;

		private Vector3 m_prevPoint;

		private Vector3 m_normal;

		private Vector3[] m_handlesNormals;

		private Vector3[] m_handlesPositions;

		private Matrix4x4 m_handlesTransform;

		private Matrix4x4 m_handlesInverseTransform;

		protected int DragIndex => m_dragIndex;

		protected bool IsDragging => m_isDragging;

		protected abstract Matrix4x4 HandlesTransform { get; }

		protected virtual Vector3[] HandlesPositions => m_handlesPositions;

		protected virtual Vector3[] HandlesNormals => m_handlesNormals;

		private void Awake()
		{
			AwakeOverride();
		}

		private void Start()
		{
			if (SceneCamera == null)
			{
				SceneCamera = RuntimeEditorApplication.ActiveSceneCamera;
			}
			if (SceneCamera == null)
			{
				SceneCamera = Camera.main;
			}
			if (Target == null)
			{
				Target = base.transform;
			}
			if (EnableUndo && !RuntimeUndoComponent.IsInitialized)
			{
				GameObject gameObject = new GameObject();
				gameObject.name = "RuntimeUndo";
				gameObject.AddComponent<RuntimeUndoComponent>();
			}
			if (GLRenderer.Instance == null)
			{
				GameObject gameObject2 = new GameObject();
				gameObject2.name = "GLRenderer";
				gameObject2.AddComponent<GLRenderer>();
			}
			if (SceneCamera != null && !SceneCamera.GetComponent<GLCamera>())
			{
				SceneCamera.gameObject.AddComponent<GLCamera>();
			}
			if (GLRenderer.Instance != null)
			{
				GLRenderer.Instance.Add(this);
			}
			StartOverride();
		}

		private void OnEnable()
		{
			if (GLRenderer.Instance != null)
			{
				GLRenderer.Instance.Add(this);
			}
			OnEnableOverride();
		}

		private void OnDisable()
		{
			if (GLRenderer.Instance != null)
			{
				GLRenderer.Instance.Remove(this);
			}
			OnDisableOverride();
		}

		private void OnDestroy()
		{
			if (GLRenderer.Instance != null)
			{
				GLRenderer.Instance.Remove(this);
			}
			if (RuntimeTools.ActiveTool == this)
			{
				RuntimeTools.ActiveTool = null;
			}
			OnDestroyOverride();
		}

		private void Update()
		{
			Vector3 point;
			if (Input.GetMouseButtonDown(0))
			{
				if (RuntimeTools.IsPointerOverGameObject())
				{
					return;
				}
				if (SceneCamera == null)
				{
					Debug.LogError("Camera is null");
					return;
				}
				if (RuntimeTools.IsViewing || RuntimeTools.ActiveTool != null || (RuntimeEditorApplication.ActiveSceneCamera != null && !RuntimeEditorApplication.IsPointerOverWindow(RuntimeWindowType.SceneView)))
				{
					return;
				}
				Vector2 pointer = Input.mousePosition;
				m_dragIndex = Hit(pointer, HandlesPositions, HandlesNormals);
				if (m_dragIndex >= 0 && OnBeginDrag(m_dragIndex))
				{
					m_handlesTransform = HandlesTransform;
					m_handlesInverseTransform = Matrix4x4.TRS(Target.position, Target.rotation, Target.localScale).inverse;
					m_dragPlane = GetDragPlane();
					m_isDragging = GetPointOnDragPlane(Input.mousePosition, out m_prevPoint);
					m_normal = HandlesNormals[m_dragIndex].normalized;
					if (m_isDragging)
					{
						RuntimeTools.ActiveTool = this;
					}
					if (EnableUndo)
					{
						bool isRecording = RuntimeUndo.IsRecording;
						if (!isRecording)
						{
							RuntimeUndo.BeginRecord();
						}
						RecordOverride();
						if (!isRecording)
						{
							RuntimeUndo.EndRecord();
						}
					}
				}
			}
			else if (Input.GetMouseButtonUp(0))
			{
				if (m_isDragging)
				{
					OnDrop();
					bool isRecording2 = RuntimeUndo.IsRecording;
					if (!isRecording2)
					{
						RuntimeUndo.BeginRecord();
					}
					RecordOverride();
					if (!isRecording2)
					{
						RuntimeUndo.EndRecord();
					}
					m_isDragging = false;
					RuntimeTools.ActiveTool = null;
				}
			}
			else if (m_isDragging && GetPointOnDragPlane(Input.mousePosition, out point))
			{
				Vector3 vector = m_handlesInverseTransform.MultiplyVector(point - m_prevPoint);
				vector = Vector3.Project(vector, m_normal);
				if (InputController.GetKey(UnitSnapKey) || RuntimeTools.UnitSnapping)
				{
					Vector3 zero = Vector3.zero;
					if (Mathf.Abs(vector.x * 1.5f) >= GridSize)
					{
						zero.x = GridSize * Mathf.Sign(vector.x);
					}
					if (Mathf.Abs(vector.y * 1.5f) >= GridSize)
					{
						zero.y = GridSize * Mathf.Sign(vector.y);
					}
					if (Mathf.Abs(vector.z * 1.5f) >= GridSize)
					{
						zero.z = GridSize * Mathf.Sign(vector.z);
					}
					if (zero != Vector3.zero && OnDrag(m_dragIndex, zero))
					{
						m_prevPoint = point;
					}
				}
				else if (OnDrag(m_dragIndex, vector))
				{
					m_prevPoint = point;
				}
			}
			UpdateOverride();
		}

		protected virtual void AwakeOverride()
		{
			m_handlesPositions = RuntimeGizmos.GetHandlesPositions();
			m_handlesNormals = RuntimeGizmos.GetHandlesNormals();
		}

		protected virtual void StartOverride()
		{
		}

		protected virtual void OnEnableOverride()
		{
		}

		protected virtual void OnDisableOverride()
		{
		}

		protected virtual void OnDestroyOverride()
		{
		}

		protected virtual void UpdateOverride()
		{
		}

		protected virtual void RecordOverride()
		{
		}

		protected virtual bool OnBeginDrag(int index)
		{
			return true;
		}

		protected virtual bool OnDrag(int index, Vector3 offset)
		{
			return true;
		}

		protected virtual void OnDrop()
		{
		}

		void IGL.Draw(int cullingMask)
		{
			RTLayer rTLayer = RTLayer.SceneView;
			if (((uint)cullingMask & (uint)rTLayer) != 0 && !(Target == null))
			{
				DrawOverride();
			}
		}

		protected virtual void DrawOverride()
		{
		}

		protected virtual bool HitOverride(int index, Vector3 vertex, Vector3 normal)
		{
			return true;
		}

		private int Hit(Vector2 pointer, Vector3[] vertices, Vector3[] normals)
		{
			float num = float.MaxValue;
			int result = -1;
			for (int i = 0; i < vertices.Length; i++)
			{
				Vector3 vector = normals[i];
				vector = HandlesTransform.MultiplyVector(vector);
				Vector3 vertex = vertices[i];
				Vector3 vector2 = HandlesTransform.MultiplyPoint(vertices[i]);
				if (!(Mathf.Abs(Vector3.Dot((SceneCamera.transform.position - vector2).normalized, vector.normalized)) > 0.999f) && HitOverride(i, vertex, vector))
				{
					Vector2 vector3 = SceneCamera.WorldToScreenPoint(vector2);
					float magnitude = (vector3 - pointer).magnitude;
					if (magnitude < num && magnitude <= SelectionMargin)
					{
						num = magnitude;
						result = i;
					}
				}
			}
			return result;
		}

		protected Plane GetDragPlane()
		{
			Vector3 normalized = (SceneCamera.transform.position - HandlesTransform.MultiplyPoint(HandlesPositions[m_dragIndex])).normalized;
			Vector3 inPoint = m_handlesTransform.MultiplyPoint(Vector3.zero);
			return new Plane(normalized, inPoint);
		}

		protected bool GetPointOnDragPlane(Vector3 screenPos, out Vector3 point)
		{
			return GetPointOnDragPlane(m_dragPlane, screenPos, out point);
		}

		protected bool GetPointOnDragPlane(Plane dragPlane, Vector3 screenPos, out Vector3 point)
		{
			Ray ray = SceneCamera.ScreenPointToRay(screenPos);
			float enter;
			if (dragPlane.Raycast(ray, out enter))
			{
				point = ray.GetPoint(enter);
				return true;
			}
			point = Vector3.zero;
			return false;
		}
	}
}
