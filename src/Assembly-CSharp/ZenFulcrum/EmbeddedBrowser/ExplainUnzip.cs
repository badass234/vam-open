using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser
{
	[RequireComponent(typeof(Browser))]
	public class ExplainUnzip : MonoBehaviour
	{
		public void Start()
		{
			Browser browser = GetComponent<Browser>();
			browser.onLoad += delegate(JSONNode data)
			{
				if ((int)data["status"] == 404)
				{
					browser.LoadHTML(Resources.Load<TextAsset>("ExplainUnzip").text);
					if ((bool)HUDManager.Instance)
					{
						HUDManager.Instance.Pause();
					}
					Time.timeScale = 1f;
				}
			};
			browser.onFetchError += delegate(JSONNode data)
			{
				if (data["error"] == "ERR_ABORTED")
				{
					browser.QueuePageReplacer(delegate
					{
					}, 1f);
				}
			};
		}
	}
}
