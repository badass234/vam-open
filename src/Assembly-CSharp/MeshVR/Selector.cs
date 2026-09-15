using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeshVR
{
	public class Selector : MonoBehaviour
	{
		public enum SelectMode
		{
			SelectBox,
			PaintBox
		}

		public static Selector singleton;

		protected static bool _hideBackfaces = false;

		public static HashSet<Selectable> selectables = new HashSet<Selectable>();

		public SelectMode selectMode = SelectMode.PaintBox;

		public Camera mouseCamera;

		public Canvas canvas;

		public Image selectionBox;

		public LayerMask selectColliderMask;

		public KeyCode removeKey = KeyCode.LeftControl;

		private Vector3 startScreenPos;

		private BoxCollider worldCollider;

		private RectTransform rt;

		private bool isSelecting;

		public static bool hideBackfaces
		{
			get
			{
				return _hideBackfaces;
			}
			set
			{
				if (_hideBackfaces != value)
				{
					_hideBackfaces = value;
				}
			}
		}

		public static void Activate()
		{
			if (singleton != null)
			{
				singleton.enabled = true;
			}
		}

		public static void Deactivate()
		{
			if (singleton != null)
			{
				singleton.enabled = false;
			}
		}

		public static void RemoveAll()
		{
			selectables.Clear();
		}

		private void Awake()
		{
			singleton = this;
			Deactivate();
			if (selectionBox != null)
			{
				rt = selectionBox.GetComponent<RectTransform>();
				rt.pivot = Vector2.one * 0.5f;
				rt.anchorMin = Vector2.zero;
				rt.anchorMax = Vector2.zero;
				selectionBox.gameObject.SetActive(false);
			}
		}

		private void Update()
		{
			if (SuperController.singleton != null && SuperController.singleton.GetMouseSelect())
			{
				Ray ray = mouseCamera.ScreenPointToRay(Input.mousePosition);
				RaycastHit[] array = Physics.RaycastAll(ray, 50f, selectColliderMask);
				if (array != null && array.Length > 0)
				{
					RaycastHit[] array2 = array;
					foreach (RaycastHit raycastHit in array2)
					{
						Selectable componentInParent = raycastHit.transform.GetComponentInParent<Selectable>();
						if (componentInParent != null)
						{
							UpdateSelection(componentInParent, !componentInParent.isSelected);
							return;
						}
					}
				}
				if (selectionBox == null)
				{
					return;
				}
				startScreenPos = Input.mousePosition;
				isSelecting = true;
			}
			if (selectionBox == null)
			{
				return;
			}
			if (SuperController.singleton != null && SuperController.singleton.GetMouseRelease())
			{
				isSelecting = false;
			}
			selectionBox.gameObject.SetActive(isSelecting);
			if (_hideBackfaces)
			{
				Camera camera = null;
				camera = ((!(SuperController.singleton != null) || !(SuperController.singleton.lookCamera != null)) ? singleton.mouseCamera : SuperController.singleton.lookCamera);
				if (camera != null)
				{
					Vector3 forward = camera.transform.forward;
					foreach (Selectable selectable in selectables)
					{
						if (Vector3.Dot(forward, selectable.transform.forward) > 0f)
						{
							selectable.isHidden = true;
						}
						else
						{
							selectable.isHidden = false;
						}
					}
				}
			}
			else
			{
				foreach (Selectable selectable2 in selectables)
				{
					selectable2.isHidden = false;
				}
			}
			if (!isSelecting)
			{
				return;
			}
			Bounds bounds = default(Bounds);
			if (selectMode == SelectMode.SelectBox)
			{
				bounds.center = Vector3.Lerp(startScreenPos, Input.mousePosition, 0.5f);
				bounds.size = new Vector3(Mathf.Abs(startScreenPos.x - Input.mousePosition.x), Mathf.Abs(startScreenPos.y - Input.mousePosition.y), 0f);
			}
			else
			{
				bounds.center = Input.mousePosition;
				Vector3 size = default(Vector3);
				size.x = 60f;
				size.y = 60f;
				size.z = 0f;
				bounds.size = size;
			}
			if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || canvas.renderMode == RenderMode.ScreenSpaceCamera)
			{
				rt.anchoredPosition = bounds.center;
				rt.sizeDelta = bounds.size;
			}
			else
			{
				Debug.LogError("Selector canvas is not in ScreenSpaceOverlay or ScreenSpcaeCamera mode");
			}
			bool key = Input.GetKey(removeKey);
			if (key)
			{
				selectionBox.color = Color.red;
			}
			else
			{
				selectionBox.color = Color.white;
			}
			foreach (Selectable selectable3 in selectables)
			{
				if (selectable3.isHidden)
				{
					continue;
				}
				Vector3 point = mouseCamera.WorldToScreenPoint(selectable3.transform.position);
				point.z = 0f;
				if (key)
				{
					if (bounds.Contains(point))
					{
						UpdateSelection(selectable3, false);
					}
				}
				else if (bounds.Contains(point))
				{
					UpdateSelection(selectable3, true);
				}
			}
		}

		private void UpdateSelection(Selectable s, bool value)
		{
			s.isSelected = value;
		}
	}
}
