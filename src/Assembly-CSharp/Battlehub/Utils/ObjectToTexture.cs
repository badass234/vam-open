using System;
using UnityEngine;

namespace Battlehub.Utils
{
	public class ObjectToTexture : MonoBehaviour
	{
		public Camera Camera;

		[HideInInspector]
		public int objectImageLayer;

		public bool DestroyScripts = true;

		public int snapshotTextureWidth = 256;

		public int snapshotTextureHeight = 256;

		public Vector3 defaultPosition = new Vector3(0f, 0f, 1f);

		public Vector3 defaultRotation = new Vector3(345.8529f, 0f, 14.28433f);

		public Vector3 defaultScale = new Vector3(1f, 1f, 1f);

		private void Awake()
		{
			if (Camera == null)
			{
				Camera = GetComponent<Camera>();
			}
		}

		private void Start()
		{
		}

		private void SetLayerRecursively(GameObject o, int layer)
		{
			Transform[] componentsInChildren = o.GetComponentsInChildren<Transform>(true);
			foreach (Transform transform in componentsInChildren)
			{
				transform.gameObject.layer = layer;
			}
		}

		public Texture2D TakeObjectSnapshot(GameObject prefab, GameObject fallback)
		{
			return TakeObjectSnapshot(prefab, fallback, defaultPosition, Quaternion.Euler(defaultRotation), defaultScale);
		}

		public Texture2D TakeObjectSnapshot(GameObject prefab, GameObject fallback, Vector3 position)
		{
			return TakeObjectSnapshot(prefab, fallback, position, Quaternion.Euler(defaultRotation), defaultScale);
		}

		public Texture2D TakeObjectSnapshot(GameObject prefab, GameObject fallback, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			if (Camera == null)
			{
				throw new InvalidOperationException("Object Image Camera must be set");
			}
			if (objectImageLayer < 0 || objectImageLayer > 31)
			{
				throw new InvalidOperationException("Object Image Layer must specify a valid layer between 0 and 31");
			}
			bool activeSelf = prefab.activeSelf;
			prefab.SetActive(false);
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab, position, rotation * Quaternion.Inverse(prefab.transform.rotation));
			if (DestroyScripts)
			{
				MonoBehaviour[] componentsInChildren = gameObject.GetComponentsInChildren<MonoBehaviour>(true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					UnityEngine.Object.DestroyImmediate(componentsInChildren[i]);
				}
			}
			prefab.SetActive(activeSelf);
			Renderer[] array = gameObject.GetComponentsInChildren<Renderer>(true);
			if (array.Length == 0 && (bool)fallback)
			{
				UnityEngine.Object.DestroyImmediate(gameObject);
				gameObject = UnityEngine.Object.Instantiate(fallback, position, rotation);
				array = new Renderer[1] { fallback.GetComponentInChildren<Renderer>(true) };
			}
			Bounds bounds = gameObject.CalculateBounds();
			float num = Camera.fieldOfView * ((float)Math.PI / 180f);
			float num2 = Mathf.Max(bounds.extents.y, bounds.extents.x, bounds.extents.z);
			float num3 = Mathf.Abs(num2 / Mathf.Sin(num / 2f));
			gameObject.SetActive(true);
			for (int j = 0; j < array.Length; j++)
			{
				array[j].gameObject.SetActive(true);
			}
			position += bounds.center;
			Camera.transform.position = position - num3 * Camera.transform.forward;
			Camera.orthographicSize = num2;
			SetLayerRecursively(gameObject, objectImageLayer);
			Camera.targetTexture = RenderTexture.GetTemporary(snapshotTextureWidth, snapshotTextureHeight, 24);
			Camera.Render();
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = Camera.targetTexture;
			Texture2D texture2D = new Texture2D(Camera.targetTexture.width, Camera.targetTexture.height);
			texture2D.ReadPixels(new Rect(0f, 0f, Camera.targetTexture.width, Camera.targetTexture.height), 0, 0);
			texture2D.Apply();
			RenderTexture.active = active;
			RenderTexture.ReleaseTemporary(Camera.targetTexture);
			UnityEngine.Object.DestroyImmediate(gameObject);
			return texture2D;
		}
	}
}
