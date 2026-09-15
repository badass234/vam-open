using System;
using Battlehub.RTCommon;
using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTHandles
{
	public class RuntimeSceneView : RuntimeSelectionComponent
	{
		public KeyCode FocusKey = KeyCode.F;

		public KeyCode SnapToGridKey = KeyCode.S;

		public KeyCode RotateKey = KeyCode.LeftAlt;

		public KeyCode RotateKey2 = KeyCode.RightAlt;

		public KeyCode RotateKey3 = KeyCode.AltGr;

		public Texture2D ViewTexture;

		public Texture2D MoveTexture;

		public Transform Pivot;

		public Transform SecondaryPivot;

		private bool m_pan;

		private Plane m_dragPlane;

		private bool m_rotate;

		private bool m_handleInput;

		private bool m_lockInput;

		private Vector3 m_lastMousePosition;

		private MouseOrbit m_mouseOrbit;

		public float ZoomSensitivity = 8f;

		public float PanSensitivity = 100f;

		private IAnimationInfo m_focusAnimation;

		private Transform m_autoFocusTransform;

		public float GridSize = 1f;

		protected override bool IPointerOverEditorArea => RuntimeEditorApplication.IsPointerOverWindow(this) || !RuntimeEditorApplication.IsOpened;

		protected override void AwakeOverride()
		{
			base.AwakeOverride();
			if (Run.Instance == null)
			{
				GameObject gameObject = new GameObject();
				gameObject.name = "Run";
				gameObject.AddComponent<Run>();
			}
			if (Pivot == null)
			{
				GameObject gameObject2 = new GameObject();
				gameObject2.transform.SetParent(base.transform, false);
				gameObject2.name = "Pivot";
				Pivot = gameObject2.transform;
			}
			if (SecondaryPivot == null)
			{
				GameObject gameObject3 = new GameObject();
				gameObject3.transform.SetParent(base.transform, false);
				gameObject3.name = "SecondaryPivot";
				SecondaryPivot = gameObject3.transform;
			}
		}

		protected override void OnEnableOverride()
		{
			if (!(SceneCamera == null))
			{
				base.OnEnableOverride();
				if (SceneCamera != null)
				{
					SetSceneCamera(SceneCamera);
				}
			}
		}

		protected override void UpdateOverride()
		{
			base.UpdateOverride();
			if (!(RuntimeTools.ActiveTool != null))
			{
				HandleInput();
				if (RuntimeEditorApplication.IsPointerOverWindow(this))
				{
					SetCursor();
				}
				else
				{
					CursorHelper.ResetCursor(this);
				}
			}
		}

		protected override void SetCursor()
		{
			if (!IPointerOverEditorArea)
			{
				CursorHelper.ResetCursor(this);
			}
			else if (m_pan)
			{
				if (m_rotate && RuntimeTools.Current == RuntimeTool.View)
				{
					CursorHelper.SetCursor(this, ViewTexture, (!(ViewTexture != null)) ? Vector2.zero : new Vector2(ViewTexture.width / 2, ViewTexture.height / 2), CursorMode.Auto);
				}
				else
				{
					CursorHelper.SetCursor(this, MoveTexture, (!(MoveTexture != null)) ? Vector2.zero : new Vector2(MoveTexture.width / 2, MoveTexture.height / 2), CursorMode.Auto);
				}
			}
			else if (m_rotate)
			{
				CursorHelper.SetCursor(this, ViewTexture, (!(ViewTexture != null)) ? Vector2.zero : new Vector2(ViewTexture.width / 2, ViewTexture.height / 2), CursorMode.Auto);
			}
			else if (RuntimeTools.Current == RuntimeTool.View)
			{
				CursorHelper.SetCursor(this, MoveTexture, (!(MoveTexture != null)) ? Vector2.zero : new Vector2(MoveTexture.width / 2, MoveTexture.height / 2), CursorMode.Auto);
			}
			else if (!InputController.GetKey(RotateKey) && !InputController.GetKey(RotateKey2) && !InputController.GetKey(RotateKey3))
			{
				CursorHelper.ResetCursor(this);
			}
		}

		public void LockInput()
		{
			m_lockInput = true;
		}

		public void UnlockInput()
		{
			m_lockInput = false;
			if (m_mouseOrbit != null)
			{
				Pivot.position = SceneCamera.transform.position + SceneCamera.transform.forward * m_mouseOrbit.Distance;
				SecondaryPivot.position = Pivot.position;
				m_mouseOrbit.Target = Pivot;
				m_mouseOrbit.SyncAngles();
			}
		}

		public void OnProjectionChanged()
		{
			float num = SceneCamera.fieldOfView * ((float)Math.PI / 180f);
			float magnitude = (SceneCamera.transform.position - Pivot.position).magnitude;
			float orthographicSize = magnitude * Mathf.Sin(num / 2f);
			SceneCamera.orthographicSize = orthographicSize;
		}

		public void SnapToGrid()
		{
			GameObject[] gameObjects = RuntimeSelection.gameObjects;
			if (gameObjects != null && gameObjects.Length != 0)
			{
				Transform transform = gameObjects[0].transform;
				Vector3 position = transform.position;
				if ((double)GridSize < 0.01)
				{
					GridSize = 0.01f;
				}
				position.x = Mathf.Round(position.x / GridSize) * GridSize;
				position.y = Mathf.Round(position.y / GridSize) * GridSize;
				position.z = Mathf.Round(position.z / GridSize) * GridSize;
				Vector3 vector = position - transform.position;
				for (int i = 0; i < gameObjects.Length; i++)
				{
					gameObjects[i].transform.position += vector;
				}
			}
		}

		public void Focus()
		{
			if (RuntimeSelection.activeTransform == null)
			{
				return;
			}
			m_autoFocusTransform = RuntimeSelection.activeTransform;
			if (RuntimeSelection.activeTransform.gameObject.hideFlags != 0)
			{
				return;
			}
			Bounds bounds = CalculateBounds(RuntimeSelection.activeTransform);
			float num = SceneCamera.fieldOfView * ((float)Math.PI / 180f);
			float num2 = Mathf.Max(bounds.extents.y, bounds.extents.x, bounds.extents.z) * 2f;
			float num3 = Mathf.Abs(num2 / Mathf.Sin(num / 2f));
			Pivot.position = bounds.center;
			SecondaryPivot.position = RuntimeSelection.activeTransform.position;
			m_focusAnimation = new Vector3AnimationInfo(SceneCamera.transform.position, Pivot.position - num3 * SceneCamera.transform.forward, 0.1f, AnimationInfo<object, Vector3>.EaseOutCubic, delegate(object target, Vector3 value, float t, bool completed)
			{
				if ((bool)SceneCamera)
				{
					SceneCamera.transform.position = value;
				}
			});
			Run.Instance.Animation(m_focusAnimation);
			Run.Instance.Animation(new FloatAnimationInfo(m_mouseOrbit.Distance, num3, 0.1f, AnimationInfo<object, Vector3>.EaseOutCubic, delegate(object target, float value, float t, bool completed)
			{
				if ((bool)m_mouseOrbit)
				{
					m_mouseOrbit.Distance = value;
				}
			}));
			Run.Instance.Animation(new FloatAnimationInfo(SceneCamera.orthographicSize, num2, 0.1f, AnimationInfo<object, Vector3>.EaseOutCubic, delegate(object target, float value, float t, bool completed)
			{
				if ((bool)SceneCamera)
				{
					SceneCamera.orthographicSize = value;
				}
			}));
		}

		private Bounds CalculateBounds(Transform t)
		{
			Renderer componentInChildren = t.GetComponentInChildren<Renderer>();
			if ((bool)componentInChildren)
			{
				Bounds totalBounds = componentInChildren.bounds;
				if (totalBounds.size == Vector3.zero && totalBounds.center != componentInChildren.transform.position)
				{
					totalBounds = TransformBounds(componentInChildren.transform.localToWorldMatrix, totalBounds);
				}
				CalculateBounds(t, ref totalBounds);
				if (totalBounds.extents == Vector3.zero)
				{
					totalBounds.extents = new Vector3(0.5f, 0.5f, 0.5f);
				}
				return totalBounds;
			}
			return new Bounds(t.position, new Vector3(0.5f, 0.5f, 0.5f));
		}

		private void CalculateBounds(Transform t, ref Bounds totalBounds)
		{
			foreach (Transform item in t)
			{
				Renderer component = item.GetComponent<Renderer>();
				if ((bool)component)
				{
					Bounds bounds = component.bounds;
					if (bounds.size == Vector3.zero && bounds.center != component.transform.position)
					{
						bounds = TransformBounds(component.transform.localToWorldMatrix, bounds);
					}
					totalBounds.Encapsulate(bounds.min);
					totalBounds.Encapsulate(bounds.max);
				}
				CalculateBounds(item, ref totalBounds);
			}
		}

		public static Bounds TransformBounds(Matrix4x4 matrix, Bounds bounds)
		{
			Vector3 center = matrix.MultiplyPoint(bounds.center);
			Vector3 extents = bounds.extents;
			Vector3 vector = matrix.MultiplyVector(new Vector3(extents.x, 0f, 0f));
			Vector3 vector2 = matrix.MultiplyVector(new Vector3(0f, extents.y, 0f));
			Vector3 vector3 = matrix.MultiplyVector(new Vector3(0f, 0f, extents.z));
			extents.x = Mathf.Abs(vector.x) + Mathf.Abs(vector2.x) + Mathf.Abs(vector3.x);
			extents.y = Mathf.Abs(vector.y) + Mathf.Abs(vector2.y) + Mathf.Abs(vector3.y);
			extents.z = Mathf.Abs(vector.z) + Mathf.Abs(vector2.z) + Mathf.Abs(vector3.z);
			Bounds result = default(Bounds);
			result.center = center;
			result.extents = extents;
			return result;
		}

		private void Pan()
		{
			Vector3 point;
			Vector3 point2;
			if (GetPointOnDragPlane(Input.mousePosition, out point) && GetPointOnDragPlane(m_lastMousePosition, out point2))
			{
				Vector3 vector = point - point2;
				m_lastMousePosition = Input.mousePosition;
				SceneCamera.transform.position -= vector;
				Pivot.position -= vector;
				SecondaryPivot.position -= vector;
			}
		}

		private bool GetPointOnDragPlane(Vector3 mouse, out Vector3 point)
		{
			Ray ray = SceneCamera.ScreenPointToRay(mouse);
			float enter;
			if (m_dragPlane.Raycast(ray, out enter))
			{
				point = ray.GetPoint(enter);
				return true;
			}
			point = Vector3.zero;
			return false;
		}

		protected override bool CanSelect(GameObject go)
		{
			ExposeToEditor component = go.GetComponent<ExposeToEditor>();
			return component != null && component.CanSelect;
		}

		private void HandleInput()
		{
			if (RuntimeTools.AutoFocus && !(RuntimeTools.ActiveTool != null) && !(m_autoFocusTransform == null) && !(m_autoFocusTransform.position == SecondaryPivot.position) && (m_focusAnimation == null || !m_focusAnimation.InProgress))
			{
				Vector3 vector = m_autoFocusTransform.position - SecondaryPivot.position;
				SceneCamera.transform.position += vector;
				Pivot.transform.position += vector;
				SecondaryPivot.transform.position += vector;
			}
			if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
			{
				m_handleInput = false;
				m_mouseOrbit.enabled = false;
				m_rotate = false;
				SetCursor();
				return;
			}
			bool flag = RuntimeEditorApplication.IsActiveWindow(RuntimeWindowType.GameView);
			if (!flag)
			{
				float axis = Input.GetAxis("Mouse ScrollWheel");
				if (axis != 0f && !RuntimeTools.IsPointerOverGameObject())
				{
					m_mouseOrbit.Zoom();
				}
			}
			if (m_lockInput)
			{
				return;
			}
			if (!flag)
			{
				if (InputController.GetKeyDown(SnapToGridKey) && InputController.GetKey(base.ModifierKey))
				{
					SnapToGrid();
				}
				if (InputController.GetKeyDown(FocusKey))
				{
					Focus();
				}
				bool flag2 = InputController.GetKey(RotateKey) || InputController.GetKey(RotateKey2) || InputController.GetKey(RotateKey3);
				bool flag3 = Input.GetMouseButton(2) || Input.GetMouseButton(1) || (Input.GetMouseButton(0) && RuntimeTools.Current == RuntimeTool.View);
				if (flag3 != m_pan)
				{
					m_pan = flag3;
					if (m_pan)
					{
						if (RuntimeTools.Current != RuntimeTool.View)
						{
							m_rotate = false;
						}
						m_dragPlane = new Plane(-SceneCamera.transform.forward, Pivot.position);
					}
					SetCursor();
				}
				else if (flag2 != m_rotate)
				{
					m_rotate = flag2;
					SetCursor();
				}
			}
			RuntimeTools.IsViewing = m_rotate || m_pan;
			bool flag4 = RuntimeTools.IsViewing || flag;
			if (!IPointerOverEditorArea)
			{
				return;
			}
			bool mouseButtonDown = Input.GetMouseButtonDown(0);
			bool mouseButtonDown2 = Input.GetMouseButtonDown(1);
			bool mouseButtonDown3 = Input.GetMouseButtonDown(2);
			if (mouseButtonDown || mouseButtonDown2 || mouseButtonDown3)
			{
				m_handleInput = !base.PositionHandle.IsDragging;
				m_lastMousePosition = Input.mousePosition;
				if (m_rotate)
				{
					m_mouseOrbit.enabled = true;
				}
			}
			if (m_handleInput && flag4 && m_pan && (!m_rotate || RuntimeTools.Current != RuntimeTool.View))
			{
				Pan();
			}
		}

		public override void SetSceneCamera(Camera camera)
		{
			base.SetSceneCamera(camera);
			SceneCamera.fieldOfView = 60f;
			OnProjectionChanged();
			m_mouseOrbit = SceneCamera.gameObject.GetComponent<MouseOrbit>();
			if (m_mouseOrbit == null)
			{
				m_mouseOrbit = SceneCamera.gameObject.AddComponent<MouseOrbit>();
			}
			UnlockInput();
			m_mouseOrbit.enabled = false;
		}
	}
}
