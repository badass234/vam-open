using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.Utils
{
	public class TakeSnapshot : MonoBehaviour
	{
		public GameObject CameraPrefab;

		public GameObject TargetPrefab;

		public GameObject FallbackPrefab;

		public Vector3 Scale = new Vector3(0.9f, 0.9f, 0.9f);

		public Image TargetImage;

		private Texture2D m_texture;

		private void Start()
		{
			if (!(TargetPrefab == null))
			{
				Run();
			}
		}

		private void OnDestroy()
		{
			if (m_texture != null)
			{
				Object.Destroy(m_texture);
			}
		}

		public Sprite Run()
		{
			GameObject gameObject = ((!(CameraPrefab != null)) ? new GameObject() : Object.Instantiate(CameraPrefab));
			if (!gameObject.GetComponent<Camera>())
			{
				Camera camera = gameObject.AddComponent<Camera>();
				camera.orthographic = true;
				camera.orthographicSize = 1f;
			}
			ObjectToTexture objectToTexture = gameObject.GetComponent<ObjectToTexture>();
			if (objectToTexture == null)
			{
				objectToTexture = gameObject.AddComponent<ObjectToTexture>();
			}
			objectToTexture.defaultScale = Scale;
			if (m_texture != null)
			{
				Object.Destroy(m_texture);
			}
			m_texture = objectToTexture.TakeObjectSnapshot(TargetPrefab, FallbackPrefab);
			Sprite sprite = null;
			if (m_texture != null)
			{
				sprite = Sprite.Create(m_texture, new Rect(0f, 0f, m_texture.width, m_texture.height), new Vector2(0.5f, 0.5f));
				if (TargetImage != null)
				{
					TargetImage.sprite = sprite;
				}
			}
			Object.Destroy(gameObject);
			return sprite;
		}
	}
}
