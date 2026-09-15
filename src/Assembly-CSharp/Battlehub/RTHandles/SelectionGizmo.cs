using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTHandles
{
	public class SelectionGizmo : MonoBehaviour, IGL
	{
		public bool DrawRay = true;

		public Camera SceneCamera;

		private ExposeToEditor m_exposeToEditor;

		private void Awake()
		{
			if (SceneCamera == null)
			{
				SceneCamera = Camera.main;
			}
			m_exposeToEditor = GetComponent<ExposeToEditor>();
		}

		private void Start()
		{
			if (GLRenderer.Instance == null)
			{
				GameObject gameObject = new GameObject();
				gameObject.name = "GLRenderer";
				gameObject.AddComponent<GLRenderer>();
			}
			if (SceneCamera != null && !SceneCamera.GetComponent<GLCamera>())
			{
				SceneCamera.gameObject.AddComponent<GLCamera>();
			}
			if (m_exposeToEditor != null)
			{
				GLRenderer.Instance.Add(this);
			}
			if (!RuntimeSelection.IsSelected(base.gameObject))
			{
				Object.Destroy(this);
			}
		}

		private void OnEnable()
		{
			if (m_exposeToEditor != null && GLRenderer.Instance != null)
			{
				GLRenderer.Instance.Add(this);
			}
		}

		private void OnDisable()
		{
			if (GLRenderer.Instance != null)
			{
				GLRenderer.Instance.Remove(this);
			}
		}

		public void Draw(int cullingMask)
		{
			if (!RuntimeTools.ShowSelectionGizmos)
			{
				return;
			}
			RTLayer rTLayer = RTLayer.SceneView;
			if (((uint)cullingMask & (uint)rTLayer) != 0)
			{
				Bounds bounds = m_exposeToEditor.Bounds;
				Transform transform = m_exposeToEditor.BoundsObject.transform;
				RuntimeHandles.DrawBounds(ref bounds, transform.position, transform.rotation, transform.lossyScale);
				if (RuntimeTools.DrawSelectionGizmoRay)
				{
					RuntimeHandles.DrawBoundRay(ref bounds, transform.TransformPoint(bounds.center), Quaternion.identity, transform.lossyScale);
				}
			}
		}
	}
}
