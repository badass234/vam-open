using UnityEngine;

namespace MeshVR
{
	public class ScreenUICamera : MonoBehaviour
	{
		public static Camera screenUICamera;

		private void Awake()
		{
			screenUICamera = GetComponent<Camera>();
		}
	}
}
