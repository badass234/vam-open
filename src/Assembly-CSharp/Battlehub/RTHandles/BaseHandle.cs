using System.Collections.Generic;
using System.Linq;
using Battlehub.RTCommon;
using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTHandles
{
	public abstract class BaseHandle : MonoBehaviour, IGL
	{
		public bool EnableUndo = true;

		public bool HightlightOnHover = true;

		public KeyCode UnitSnapKey = KeyCode.LeftControl;

		public Camera SceneCamera;

		public BaseHandleModel Model;

		public float SelectionMargin = 10f;

		private Transform[] m_activeTargets;

		private Transform[] m_activeRealTargets;

		private Transform[] m_realTargets;

		private Transform[] m_commonCenter;

		private Transform[] m_commonCenterTarget;

		[SerializeField]
		private Transform[] m_targets;

		private RuntimeHandleAxis m_selectedAxis;

		private bool m_isDragging;

		private Plane m_dragPlane;

		protected float EffectiveGridUnitSize { get; private set; }

		protected LockObject LockObject
		{
			get
			{
				return RuntimeTools.LockAxes;
			}
			set
			{
				RuntimeTools.LockAxes = value;
			}
		}

		protected virtual Vector3 HandlePosition
		{
			get
			{
				return base.transform.position;
			}
			set
			{
				base.transform.position = value;
			}
		}

		protected Transform[] ActiveTargets => m_activeTargets;

		protected Transform[] RealTargets
		{
			get
			{
				if (m_realTargets == null)
				{
					return Targets;
				}
				return m_realTargets;
			}
		}

		public Transform[] Targets
		{
			get
			{
				return Targets_Internal;
			}
			set
			{
				DestroyCommonCenter();
				m_realTargets = value;
				GetActiveRealTargets();
				Targets_Internal = value;
				if (Targets_Internal != null && Targets_Internal.Length != 0 && RuntimeTools.PivotMode == RuntimePivotMode.Center && ActiveTargets.Length > 1)
				{
					Vector3 position = Targets_Internal[0].position;
					for (int i = 1; i < Targets_Internal.Length; i++)
					{
						position += Targets_Internal[i].position;
					}
					position /= (float)Targets_Internal.Length;
					m_commonCenter = new Transform[1];
					m_commonCenter[0] = new GameObject
					{
						name = "CommonCenter"
					}.transform;
					m_commonCenter[0].SetParent(base.transform.parent, true);
					m_commonCenter[0].position = position;
					m_commonCenterTarget = new Transform[m_realTargets.Length];
					for (int j = 0; j < m_commonCenterTarget.Length; j++)
					{
						GameObject gameObject = new GameObject();
						gameObject.name = "ActiveTarget " + m_realTargets[j].name;
						GameObject gameObject2 = gameObject;
						gameObject2.transform.SetParent(m_commonCenter[0]);
						gameObject2.transform.position = m_realTargets[j].position;
						gameObject2.transform.rotation = m_realTargets[j].rotation;
						gameObject2.transform.localScale = m_realTargets[j].localScale;
						m_commonCenterTarget[j] = gameObject2.transform;
					}
					LockObject lockObject = LockObject;
					Targets_Internal = m_commonCenter;
					LockObject = lockObject;
				}
			}
		}

		private Transform[] Targets_Internal
		{
			get
			{
				return m_targets;
			}
			set
			{
				m_targets = value;
				if (m_targets == null)
				{
					LockObject = LockAxes.Eval(null);
					m_activeTargets = null;
					return;
				}
				m_targets = m_targets.Where((Transform t) => t != null && t.hideFlags == HideFlags.None).ToArray();
				HashSet<Transform> hashSet = new HashSet<Transform>();
				for (int i = 0; i < m_targets.Length; i++)
				{
					if (m_targets[i] != null && !hashSet.Contains(m_targets[i]))
					{
						hashSet.Add(m_targets[i]);
					}
				}
				m_targets = hashSet.ToArray();
				if (m_targets.Length == 0)
				{
					LockObject = LockAxes.Eval(new LockAxes[0]);
					m_activeTargets = new Transform[0];
					return;
				}
				if (m_targets.Length == 1)
				{
					m_activeTargets = new Transform[1] { m_targets[0] };
				}
				for (int j = 0; j < m_targets.Length; j++)
				{
					Transform transform = m_targets[j];
					Transform parent = transform.parent;
					while (parent != null)
					{
						if (hashSet.Contains(parent))
						{
							hashSet.Remove(transform);
							break;
						}
						parent = parent.parent;
					}
				}
				m_activeTargets = hashSet.ToArray();
				LockObject = LockAxes.Eval((from t in m_activeTargets
					where t.GetComponent<LockAxes>() != null
					select t.GetComponent<LockAxes>()).ToArray());
				if (m_activeTargets.Any((Transform target) => target.gameObject.isStatic))
				{
					LockObject = new LockObject();
					LockObject.PositionX = (LockObject.PositionY = (LockObject.PositionZ = true));
					LockObject.RotationX = (LockObject.RotationY = (LockObject.RotationZ = true));
					LockObject.ScaleX = (LockObject.ScaleY = (LockObject.ScaleZ = true));
					LockObject.RotationScreen = true;
				}
				if (m_activeTargets != null && m_activeTargets.Length > 0)
				{
					base.transform.position = m_activeTargets[0].position;
				}
			}
		}

		public Transform Target
		{
			get
			{
				if (Targets == null || Targets.Length == 0)
				{
					return null;
				}
				return Targets[0];
			}
		}

		public bool IsDragging => m_isDragging;

		protected abstract RuntimeTool Tool { get; }

		protected Quaternion Rotation
		{
			get
			{
				if (Targets == null || Targets.Length <= 0 || Target == null)
				{
					return Quaternion.identity;
				}
				return (RuntimeTools.PivotRotation != 0) ? Quaternion.identity : Target.rotation;
			}
		}

		protected virtual RuntimeHandleAxis SelectedAxis
		{
			get
			{
				return m_selectedAxis;
			}
			set
			{
				m_selectedAxis = value;
				if (Model != null)
				{
					Model.Select(SelectedAxis);
				}
			}
		}

		protected Plane DragPlane
		{
			get
			{
				return m_dragPlane;
			}
			set
			{
				m_dragPlane = value;
			}
		}

		protected abstract float CurrentGridUnitSize { get; }

		private void GetActiveRealTargets()
		{
			if (m_realTargets == null)
			{
				m_activeRealTargets = null;
				return;
			}
			m_realTargets = m_realTargets.Where((Transform t) => t != null && t.hideFlags == HideFlags.None).ToArray();
			HashSet<Transform> hashSet = new HashSet<Transform>();
			for (int i = 0; i < m_realTargets.Length; i++)
			{
				if (m_realTargets[i] != null && !hashSet.Contains(m_realTargets[i]))
				{
					hashSet.Add(m_realTargets[i]);
				}
			}
			m_realTargets = hashSet.ToArray();
			if (m_realTargets.Length == 0)
			{
				m_activeRealTargets = new Transform[0];
				return;
			}
			if (m_realTargets.Length == 1)
			{
				m_activeRealTargets = new Transform[1] { m_realTargets[0] };
			}
			for (int j = 0; j < m_realTargets.Length; j++)
			{
				Transform transform = m_realTargets[j];
				Transform parent = transform.parent;
				while (parent != null)
				{
					if (hashSet.Contains(parent))
					{
						hashSet.Remove(transform);
						break;
					}
					parent = parent.parent;
				}
			}
			m_activeRealTargets = hashSet.ToArray();
		}

		private void Awake()
		{
			if (m_targets != null && m_targets.Length > 0)
			{
				Targets = m_targets;
			}
			RuntimeTools.PivotModeChanged += OnPivotModeChanged;
			RuntimeTools.ToolChanged += OnRuntimeToolChanged;
			RuntimeTools.LockAxesChanged += OnLockAxesChanged;
			AwakeOverride();
		}

		private void Start()
		{
			if (SceneCamera == null)
			{
				SceneCamera = Camera.main;
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
			if (Targets == null || Targets.Length == 0)
			{
				Targets = new Transform[1] { base.transform };
			}
			if (GLRenderer.Instance != null)
			{
				GLRenderer.Instance.Add(this);
			}
			if (Targets[0].position != base.transform.position)
			{
				base.transform.position = Targets[0].position;
			}
			if (Model != null)
			{
				BaseHandleModel baseHandleModel = Object.Instantiate(Model, base.transform.parent);
				baseHandleModel.name = Model.name;
				Model = baseHandleModel;
				Model.SetLock(LockObject);
			}
			StartOverride();
		}

		private void OnEnable()
		{
			OnEnableOverride();
			if (Model != null)
			{
				Model.gameObject.SetActive(true);
				SyncModelTransform();
			}
			else if (GLRenderer.Instance != null)
			{
				GLRenderer.Instance.Add(this);
			}
			RuntimeUndo.UndoCompleted += OnUndoCompleted;
			RuntimeUndo.RedoCompleted += OnRedoCompleted;
		}

		private void OnDisable()
		{
			if (GLRenderer.Instance != null)
			{
				GLRenderer.Instance.Remove(this);
			}
			DestroyCommonCenter();
			if (Model != null)
			{
				Model.gameObject.SetActive(false);
			}
			OnDisableOverride();
			RuntimeUndo.UndoCompleted -= OnUndoCompleted;
			RuntimeUndo.RedoCompleted -= OnRedoCompleted;
		}

		private void OnDestroy()
		{
			RuntimeTools.ToolChanged -= OnRuntimeToolChanged;
			RuntimeTools.PivotModeChanged -= OnPivotModeChanged;
			RuntimeTools.LockAxesChanged -= OnLockAxesChanged;
			if (GLRenderer.Instance != null)
			{
				GLRenderer.Instance.Remove(this);
			}
			if (RuntimeTools.ActiveTool == this)
			{
				RuntimeTools.ActiveTool = null;
			}
			DestroyCommonCenter();
			if (Model != null && Model.gameObject != null && !Model.gameObject.IsPrefab())
			{
				Object.Destroy(Model);
			}
			OnDestroyOverride();
		}

		private void DestroyCommonCenter()
		{
			if (m_commonCenter != null)
			{
				for (int i = 0; i < m_commonCenter.Length; i++)
				{
					Object.Destroy(m_commonCenter[i].gameObject);
				}
			}
			if (m_commonCenterTarget != null)
			{
				for (int j = 0; j < m_commonCenterTarget.Length; j++)
				{
					Object.Destroy(m_commonCenterTarget[j].gameObject);
				}
			}
			m_commonCenter = null;
			m_commonCenterTarget = null;
		}

		private void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				if ((RuntimeTools.Current != Tool && RuntimeTools.Current != 0) || RuntimeTools.IsViewing || RuntimeTools.IsPointerOverGameObject())
				{
					return;
				}
				if (SceneCamera == null)
				{
					Debug.LogError("Camera is null");
					return;
				}
				if (RuntimeTools.ActiveTool != null || (RuntimeEditorApplication.ActiveSceneCamera != null && !RuntimeEditorApplication.IsPointerOverWindow(RuntimeWindowType.SceneView)))
				{
					return;
				}
				m_isDragging = OnBeginDrag();
				if (m_isDragging)
				{
					RuntimeTools.ActiveTool = this;
					RecordTransform();
				}
				else
				{
					RuntimeTools.ActiveTool = null;
				}
			}
			else if (Input.GetMouseButtonUp(0))
			{
				TryCancelDrag();
			}
			else if (m_isDragging)
			{
				if (InputController.GetKey(UnitSnapKey) || RuntimeTools.UnitSnapping)
				{
					EffectiveGridUnitSize = CurrentGridUnitSize;
				}
				else
				{
					EffectiveGridUnitSize = 0f;
				}
				OnDrag();
			}
			UpdateOverride();
			if (Model != null)
			{
				SyncModelTransform();
			}
			if (m_isDragging && RuntimeTools.PivotMode == RuntimePivotMode.Center && m_commonCenterTarget != null && m_realTargets != null && m_realTargets.Length > 1)
			{
				for (int i = 0; i < m_commonCenterTarget.Length; i++)
				{
					Transform transform = m_commonCenterTarget[i];
					Transform transform2 = m_realTargets[i];
					transform2.transform.position = transform.position;
					transform2.transform.rotation = transform.rotation;
					transform2.transform.localScale = transform.localScale;
				}
			}
		}

		private void SyncModelTransform()
		{
			Vector3 handlePosition = HandlePosition;
			Model.transform.position = handlePosition;
			Model.transform.rotation = Rotation;
			float screenScale = RuntimeHandles.GetScreenScale(handlePosition, SceneCamera);
			Model.transform.localScale = Vector3.one * screenScale;
		}

		private void TryCancelDrag()
		{
			if (m_isDragging)
			{
				OnDrop();
				RecordTransform();
				m_isDragging = false;
				RuntimeTools.ActiveTool = null;
			}
		}

		protected virtual void AwakeOverride()
		{
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
			if (Targets == null || Targets.Length <= 0 || !(Targets[0] != null) || !(Targets[0].position != base.transform.position))
			{
				return;
			}
			if (IsDragging)
			{
				Vector3 vector = base.transform.position - Targets[0].position;
				for (int i = 0; i < ActiveTargets.Length; i++)
				{
					if (ActiveTargets[i] != null)
					{
						ActiveTargets[i].position += vector;
					}
				}
			}
			else
			{
				base.transform.position = Targets[0].position;
				base.transform.rotation = Targets[0].rotation;
			}
		}

		protected virtual bool OnBeginDrag()
		{
			return false;
		}

		protected virtual void OnDrag()
		{
		}

		protected virtual void OnDrop()
		{
		}

		protected virtual void OnRuntimeToolChanged()
		{
			TryCancelDrag();
		}

		protected virtual void OnPivotModeChanged()
		{
			if (RealTargets != null)
			{
				Targets = RealTargets;
			}
			if (RuntimeTools.PivotMode != 0)
			{
				m_realTargets = null;
			}
			if (Target != null)
			{
				base.transform.position = Target.position;
			}
		}

		private void OnLockAxesChanged()
		{
			if (Model != null && !Model.gameObject.IsPrefab())
			{
				Model.SetLock(LockObject);
			}
		}

		protected virtual void RecordTransform()
		{
			if (EnableUndo)
			{
				RuntimeUndo.BeginRecord();
				for (int i = 0; i < m_activeRealTargets.Length; i++)
				{
					RuntimeUndo.RecordTransform(m_activeRealTargets[i]);
				}
				RuntimeUndo.EndRecord();
			}
		}

		private void OnRedoCompleted()
		{
			if (RuntimeTools.PivotMode == RuntimePivotMode.Center && m_realTargets != null)
			{
				Targets = m_realTargets;
			}
		}

		private void OnUndoCompleted()
		{
			if (RuntimeTools.PivotMode == RuntimePivotMode.Center && m_realTargets != null)
			{
				Targets = m_realTargets;
			}
		}

		protected virtual bool HitCenter()
		{
			Vector2 vector = SceneCamera.WorldToScreenPoint(base.transform.position);
			Vector2 vector2 = Input.mousePosition;
			return (vector2 - vector).magnitude <= SelectionMargin;
		}

		protected virtual bool HitAxis(Vector3 axis, Matrix4x4 matrix, out float distanceToAxis)
		{
			axis = matrix.MultiplyVector(axis);
			Vector2 vector = SceneCamera.WorldToScreenPoint(base.transform.position);
			Vector2 vector2 = SceneCamera.WorldToScreenPoint(axis + base.transform.position);
			Vector3 vector3 = vector2 - vector;
			float magnitude = vector3.magnitude;
			vector3.Normalize();
			if (vector3 != Vector3.zero)
			{
				return HitScreenAxis(out distanceToAxis, vector, vector3, magnitude);
			}
			Vector2 vector4 = Input.mousePosition;
			distanceToAxis = (vector - vector4).magnitude;
			bool flag = distanceToAxis <= SelectionMargin;
			if (!flag)
			{
				distanceToAxis = float.PositiveInfinity;
			}
			else
			{
				distanceToAxis = 0f;
			}
			return flag;
		}

		protected virtual bool HitScreenAxis(out float distanceToAxis, Vector2 screenVectorBegin, Vector3 screenVector, float screenVectorMag)
		{
			Vector2 normalized = PerpendicularClockwise(screenVector).normalized;
			Vector2 vector = Input.mousePosition;
			Vector2 vector2 = vector - screenVectorBegin;
			distanceToAxis = Mathf.Abs(Vector2.Dot(normalized, vector2));
			Vector2 rhs = vector2 - normalized * distanceToAxis;
			float num = Vector2.Dot(screenVector, rhs);
			bool flag = num <= screenVectorMag + SelectionMargin && num >= 0f - SelectionMargin && distanceToAxis <= SelectionMargin;
			if (!flag)
			{
				distanceToAxis = float.PositiveInfinity;
			}
			else if (screenVectorMag < SelectionMargin)
			{
				distanceToAxis = 0f;
			}
			return flag;
		}

		protected virtual Plane GetDragPlane(Matrix4x4 matrix, Vector3 axis)
		{
			Plane result = new Plane(matrix.MultiplyVector(axis).normalized, matrix.MultiplyPoint(Vector3.zero));
			return result;
		}

		protected virtual Plane GetDragPlane()
		{
			return new Plane(SceneCamera.cameraToWorldMatrix.MultiplyVector(Vector3.forward).normalized, base.transform.position);
		}

		protected virtual bool GetPointOnDragPlane(Vector3 screenPos, out Vector3 point)
		{
			return GetPointOnDragPlane(m_dragPlane, screenPos, out point);
		}

		protected virtual bool GetPointOnDragPlane(Plane dragPlane, Vector3 screenPos, out Vector3 point)
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

		private static Vector2 PerpendicularClockwise(Vector2 vector2)
		{
			return new Vector2(0f - vector2.y, vector2.x);
		}

		void IGL.Draw(int cullingMask)
		{
			RTLayer rTLayer = RTLayer.SceneView;
			if (((uint)cullingMask & (uint)rTLayer) != 0 && Model == null)
			{
				DrawOverride();
			}
		}

		protected virtual void DrawOverride()
		{
		}
	}
}
