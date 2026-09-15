using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser
{
	[RequireComponent(typeof(Browser))]
	public class BallBrowserSpawner : MonoBehaviour, INewWindowHandler
	{
		public Transform spawnPosition;

		public float size;

		public void Start()
		{
			GetComponent<Browser>().SetNewWindowHandler(Browser.NewWindowAction.NewBrowser, this);
		}

		public Browser CreateBrowser(Browser parent)
		{
			GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			gameObject.AddComponent<Rigidbody>();
			gameObject.transform.localScale = new Vector3(size, size, size);
			gameObject.transform.position = spawnPosition.position + Vector3.one * Random.value * 0.01f;
			Browser browser = gameObject.AddComponent<Browser>();
			browser.UIHandler = null;
			browser.Resize(110, 110);
			return browser;
		}
	}
}
