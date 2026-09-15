using System;
using System.Collections.Generic;
using System.Linq;
using Battlehub.RTCommon;
using Battlehub.RTSaveLoad;
using Battlehub.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTHandles
{
	public class BoxSelection : MonoBehaviour
	{
		public Sprite Graphics;

		protected Image m_image;

		protected RectTransform m_rectTransform;

		protected Canvas m_canvas;

		protected bool m_isDragging;

		protected Vector3 m_startMousePosition;

		protected Vector2 m_startPt;

		protected Vector2 m_endPt;

		public bool UseCameraSpace;

		public Camera SceneCamera;

		public BoxSelectionMethod Method;

		public int MouseButton;

		public KeyCode KeyCode;

		protected bool m_active;

		private static BoxSelection m_current;

		public bool IsDragging => m_isDragging;

		public static BoxSelection Current => m_current;

		public static event EventHandler<FilteringArgs> Filtering;

		private void Awake()
		{
			if (m_current != null)
			{
				Debug.LogWarning("Another instance of BoxSelection exists");
			}
			if (!GetComponent<PersistentIgnore>())
			{
				base.gameObject.AddComponent<PersistentIgnore>();
			}
			m_current = this;
			m_image = base.gameObject.AddComponent<Image>();
			m_image.type = Image.Type.Sliced;
			if (Graphics == null)
			{
				Graphics = Resources.Load<Sprite>("BoxSelection");
			}
			m_image.sprite = Graphics;
			m_image.raycastTarget = false;
			m_rectTransform = GetComponent<RectTransform>();
			m_rectTransform.sizeDelta = new Vector2(0f, 0f);
			m_rectTransform.pivot = new Vector2(0f, 0f);
			m_rectTransform.anchoredPosition = new Vector3(0f, 0f);
			if (SceneCamera == null)
			{
				SceneCamera = Camera.main;
			}
		}

		private void OnEnable()
		{
			m_canvas = GetComponentInParent<Canvas>();
			if (!(SceneCamera == null))
			{
				if (m_canvas == null)
				{
					GameObject gameObject = new GameObject();
					gameObject.name = "BoxSelectionCanvas";
					m_canvas = gameObject.AddComponent<Canvas>();
				}
				CanvasScaler canvasScaler = m_canvas.GetComponent<CanvasScaler>();
				if (canvasScaler == null)
				{
					canvasScaler = m_canvas.gameObject.AddComponent<CanvasScaler>();
				}
				if (UseCameraSpace)
				{
					m_canvas.worldCamera = SceneCamera;
					m_canvas.renderMode = RenderMode.ScreenSpaceCamera;
					m_canvas.planeDistance = SceneCamera.nearClipPlane + 0.05f;
				}
				else
				{
					m_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				}
				canvasScaler.referencePixelsPerUnit = 1f;
				base.transform.SetParent(m_canvas.gameObject.transform);
			}
		}

		public void SetSceneCamera(Camera camera)
		{
			SceneCamera = camera;
			if (m_canvas != null && UseCameraSpace)
			{
				m_canvas.worldCamera = camera;
			}
		}

		private void OnDestroy()
		{
			if (RuntimeTools.ActiveTool == this)
			{
				RuntimeTools.ActiveTool = null;
			}
			if (m_current == this)
			{
				m_current = null;
			}
		}

		private void LateUpdate()
		{
			if ((RuntimeTools.ActiveTool != null && RuntimeTools.ActiveTool != this) || RuntimeTools.IsViewing)
			{
				return;
			}
			if (KeyCode == KeyCode.None || InputController.GetKeyDown(KeyCode))
			{
				m_active = true;
			}
			if (!m_active)
			{
				return;
			}
			if (Input.GetMouseButtonDown(MouseButton))
			{
				m_startMousePosition = Input.mousePosition;
				m_isDragging = GetPoint(out m_startPt) && (!RuntimeEditorApplication.IsOpened || (RuntimeEditorApplication.IsPointerOverWindow(RuntimeWindowType.SceneView) && !RuntimeTools.IsPointerOverGameObject()));
				if (m_isDragging)
				{
					m_rectTransform.anchoredPosition = m_startPt;
					m_rectTransform.sizeDelta = new Vector2(0f, 0f);
					CursorHelper.SetCursor(this, null, Vector3.zero, CursorMode.Auto);
				}
				else
				{
					RuntimeTools.ActiveTool = null;
				}
			}
			else if (Input.GetMouseButtonUp(MouseButton))
			{
				if (m_isDragging)
				{
					m_isDragging = false;
					HitTest();
					m_rectTransform.sizeDelta = new Vector2(0f, 0f);
					CursorHelper.ResetCursor(this);
				}
				RuntimeTools.ActiveTool = null;
				m_active = false;
			}
			else if (m_isDragging)
			{
				GetPoint(out m_endPt);
				Vector2 vector = m_endPt - m_startPt;
				if (vector != Vector2.zero)
				{
					RuntimeTools.ActiveTool = this;
				}
				m_rectTransform.sizeDelta = new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
				m_rectTransform.localScale = new Vector3(Mathf.Sign(vector.x), Mathf.Sign(vector.y), 1f);
			}
		}

		private void HitTest()
		{
			if (!(m_rectTransform.sizeDelta.magnitude < 5f))
			{
				Vector3 center = (m_startMousePosition + Input.mousePosition) / 2f;
				center.z = 0f;
				Bounds selectionBounds = new Bounds(center, m_rectTransform.sizeDelta);
				Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(SceneCamera);
				HashSet<GameObject> hashSet = new HashSet<GameObject>();
				Renderer[] array = UnityEngine.Object.FindObjectsOfType<Renderer>();
				Collider[] array2 = UnityEngine.Object.FindObjectsOfType<Collider>();
				FilteringArgs args = new FilteringArgs();
				foreach (Renderer renderer in array)
				{
					Bounds bounds = renderer.bounds;
					GameObject go = renderer.gameObject;
					TrySelect(ref selectionBounds, hashSet, args, ref bounds, go, frustumPlanes);
				}
				foreach (Collider collider in array2)
				{
					Bounds bounds2 = collider.bounds;
					GameObject go2 = collider.gameObject;
					TrySelect(ref selectionBounds, hashSet, args, ref bounds2, go2, frustumPlanes);
				}
				RuntimeSelection.objects = hashSet.ToArray();
			}
		}

		private void TrySelect(ref Bounds selectionBounds, HashSet<GameObject> selection, FilteringArgs args, ref Bounds bounds, GameObject go, Plane[] frustumPlanes)
		{
			bool flag = ((Method == BoxSelectionMethod.LooseFitting) ? LooseFitting(ref selectionBounds, ref bounds) : ((Method != BoxSelectionMethod.BoundsCenter) ? TransformCenter(ref selectionBounds, go.transform) : BoundsCenter(ref selectionBounds, ref bounds)));
			if (!GeometryUtility.TestPlanesAABB(frustumPlanes, bounds))
			{
				flag = false;
			}
			if (!flag || selection.Contains(go))
			{
				return;
			}
			if (BoxSelection.Filtering != null)
			{
				args.Object = go;
				BoxSelection.Filtering(this, args);
				if (!args.Cancel)
				{
					selection.Add(go);
				}
				args.Reset();
			}
			else
			{
				selection.Add(go);
			}
		}

		private bool TransformCenter(ref Bounds selectionBounds, Transform tr)
		{
			Vector3 point = SceneCamera.WorldToScreenPoint(tr.position);
			point.z = 0f;
			return selectionBounds.Contains(point);
		}

		private bool BoundsCenter(ref Bounds selectionBounds, ref Bounds bounds)
		{
			Vector3 point = SceneCamera.WorldToScreenPoint(bounds.center);
			point.z = 0f;
			return selectionBounds.Contains(point);
		}

		private bool LooseFitting(ref Bounds selectionBounds, ref Bounds bounds)
		{
			Vector3 position = bounds.center + new Vector3(0f - bounds.extents.x, 0f - bounds.extents.y, 0f - bounds.extents.z);
			Vector3 position2 = bounds.center + new Vector3(0f - bounds.extents.x, 0f - bounds.extents.y, bounds.extents.z);
			Vector3 position3 = bounds.center + new Vector3(0f - bounds.extents.x, bounds.extents.y, 0f - bounds.extents.z);
			Vector3 position4 = bounds.center + new Vector3(0f - bounds.extents.x, bounds.extents.y, bounds.extents.z);
			Vector3 position5 = bounds.center + new Vector3(bounds.extents.x, 0f - bounds.extents.y, 0f - bounds.extents.z);
			Vector3 position6 = bounds.center + new Vector3(bounds.extents.x, 0f - bounds.extents.y, bounds.extents.z);
			Vector3 position7 = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, 0f - bounds.extents.z);
			Vector3 position8 = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z);
			position = SceneCamera.WorldToScreenPoint(position);
			position2 = SceneCamera.WorldToScreenPoint(position2);
			position3 = SceneCamera.WorldToScreenPoint(position3);
			position4 = SceneCamera.WorldToScreenPoint(position4);
			position5 = SceneCamera.WorldToScreenPoint(position5);
			position6 = SceneCamera.WorldToScreenPoint(position6);
			position7 = SceneCamera.WorldToScreenPoint(position7);
			position8 = SceneCamera.WorldToScreenPoint(position8);
			float x = Mathf.Min(position.x, position2.x, position3.x, position4.x, position5.x, position6.x, position7.x, position8.x);
			float x2 = Mathf.Max(position.x, position2.x, position3.x, position4.x, position5.x, position6.x, position7.x, position8.x);
			float y = Mathf.Min(position.y, position2.y, position3.y, position4.y, position5.y, position6.y, position7.y, position8.y);
			float y2 = Mathf.Max(position.y, position2.y, position3.y, position4.y, position5.y, position6.y, position7.y, position8.y);
			Vector3 vector = new Vector2(x, y);
			Vector3 vector2 = new Vector2(x2, y2);
			Bounds bounds2 = new Bounds((vector + vector2) / 2f, vector2 - vector);
			return selectionBounds.Intersects(bounds2);
		}

		private bool GetPoint(out Vector2 localPoint)
		{
			Camera cam = null;
			if (m_canvas.renderMode != 0)
			{
				cam = m_canvas.worldCamera;
			}
			return RectTransformUtility.ScreenPointToLocalPointInRectangle(m_canvas.GetComponent<RectTransform>(), Input.mousePosition, cam, out localPoint);
		}
	}
}
