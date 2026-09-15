using UnityEngine;
using UnityEngine.EventSystems;

namespace Weelco.VRInput
{
	public abstract class VRInputModule : BaseInputModule
	{
		private static VRInputModule _instance;

		private Camera UICamera;

		public static VRInputModule instance => _instance;

		public abstract void AddController(IUIPointer controller);

		public abstract void RemoveController(IUIPointer controller);

		protected override void Awake()
		{
			base.Awake();
			if (_instance != null)
			{
				Debug.LogWarning("Trying to instantiate multiple VRInputModule::" + this);
				Object.DestroyImmediate(base.gameObject);
			}
			_instance = this;
		}

		protected override void Start()
		{
			base.Start();
			UICamera = new GameObject("DummyCamera").AddComponent<Camera>();
			UICamera.clearFlags = CameraClearFlags.Nothing;
			UICamera.enabled = false;
			UICamera.fieldOfView = 5f;
			UICamera.nearClipPlane = 0.01f;
			Canvas[] array = Resources.FindObjectsOfTypeAll<Canvas>();
			Canvas[] array2 = array;
			foreach (Canvas canvas in array2)
			{
				canvas.worldCamera = UICamera;
			}
		}

		protected void UpdateCameraPosition(IUIPointer controller)
		{
			UICamera.transform.position = controller.target.transform.position;
			UICamera.transform.rotation = controller.target.transform.rotation;
		}

		protected Vector2 GetCameraSize()
		{
			return new Vector2(UICamera.pixelWidth, UICamera.pixelHeight);
		}

		protected void ClearSelection()
		{
			if ((bool)base.eventSystem.currentSelectedGameObject)
			{
				base.eventSystem.SetSelectedGameObject(null);
			}
		}
	}
}
