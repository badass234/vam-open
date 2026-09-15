using UnityEngine;

namespace MeshVR
{
	public class ScreenUICameraConnector : MonoBehaviour
	{
		protected Canvas canvas;

		private void Awake()
		{
			canvas = GetComponent<Canvas>();
		}

		protected void CheckCamera()
		{
			if (canvas != null && canvas.worldCamera == null)
			{
				canvas.worldCamera = ScreenUICamera.screenUICamera;
			}
		}

		private void Update()
		{
			CheckCamera();
		}
	}
}
