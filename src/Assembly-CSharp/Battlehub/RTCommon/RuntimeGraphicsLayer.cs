using UnityEngine;

namespace Battlehub.RTCommon
{
	public class RuntimeGraphicsLayer : MonoBehaviour
	{
		private Camera m_sceneCamera;

		private Camera m_graphicsLayerCamera;

		[SerializeField]
		private int m_graphicsLayer = 24;

		public int GraphicsLayer => m_graphicsLayer;

		private void UpdateCameraCullingMask()
		{
			m_sceneCamera.cullingMask &= ~(1 << m_graphicsLayer);
			m_graphicsLayerCamera.cullingMask = 1 << m_graphicsLayer;
			if (RuntimeEditorApplication.GameCameras != null)
			{
				for (int i = 0; i < RuntimeEditorApplication.GameCameras.Length; i++)
				{
					RuntimeEditorApplication.GameCameras[i].cullingMask &= ~(1 << m_graphicsLayer);
				}
			}
		}

		private void Awake()
		{
			RuntimeEditorApplication.ActiveSceneCameraChanged += OnActiveSceneCameraChanged;
			PrepareGraphicsLayerCamera();
		}

		private void OnDestroy()
		{
			RuntimeEditorApplication.ActiveSceneCameraChanged -= OnActiveSceneCameraChanged;
		}

		private void OnActiveSceneCameraChanged()
		{
			PrepareGraphicsLayerCamera();
		}

		private void PrepareGraphicsLayerCamera()
		{
			m_sceneCamera = RuntimeEditorApplication.ActiveSceneCamera;
			if (m_sceneCamera == null)
			{
				m_sceneCamera = Camera.main;
			}
			if (!(m_sceneCamera != null))
			{
				return;
			}
			Object.Destroy(m_graphicsLayerCamera);
			m_graphicsLayerCamera = Object.Instantiate(m_sceneCamera, m_sceneCamera.transform);
			for (int num = m_graphicsLayerCamera.transform.childCount - 1; num >= 0; num--)
			{
				Object.Destroy(m_graphicsLayerCamera.transform.GetChild(num).gameObject);
			}
			Component[] components = m_graphicsLayerCamera.GetComponents<Component>();
			foreach (Component component in components)
			{
				if (!(component is Transform) && !(component is Camera))
				{
					Object.Destroy(component);
				}
			}
			m_graphicsLayerCamera.clearFlags = CameraClearFlags.Depth;
			m_graphicsLayerCamera.transform.localPosition = Vector3.zero;
			m_graphicsLayerCamera.transform.localRotation = Quaternion.identity;
			m_graphicsLayerCamera.transform.localScale = Vector3.one;
			m_graphicsLayerCamera.name = "GraphicsLayerCamera";
			UpdateCameraCullingMask();
		}

		private void Update()
		{
			if (m_graphicsLayerCamera.fieldOfView != m_sceneCamera.fieldOfView)
			{
				m_graphicsLayerCamera.fieldOfView = m_sceneCamera.fieldOfView;
			}
			if (m_graphicsLayerCamera.orthographic != m_sceneCamera.orthographic)
			{
				m_graphicsLayerCamera.orthographic = m_sceneCamera.orthographic;
			}
			if (m_graphicsLayerCamera.orthographicSize != m_sceneCamera.orthographicSize)
			{
				m_graphicsLayerCamera.orthographicSize = m_sceneCamera.orthographicSize;
			}
			if (m_graphicsLayerCamera.rect != m_sceneCamera.rect)
			{
				m_graphicsLayerCamera.rect = m_sceneCamera.rect;
			}
			if (m_graphicsLayerCamera.enabled != m_sceneCamera.enabled)
			{
				m_graphicsLayerCamera.enabled = m_sceneCamera.enabled;
			}
		}
	}
}
