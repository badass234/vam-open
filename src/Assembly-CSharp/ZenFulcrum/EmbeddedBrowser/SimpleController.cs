using UnityEngine;
using UnityEngine.UI;

namespace ZenFulcrum.EmbeddedBrowser
{
	[RequireComponent(typeof(Browser))]
	public class SimpleController : MonoBehaviour
	{
		private Browser browser;

		public InputField urlInput;

		public void Start()
		{
			browser = GetComponent<Browser>();
			browser.onNavStateChange += delegate
			{
				if (!urlInput.isFocused)
				{
					urlInput.text = browser.Url;
				}
			};
			urlInput.onEndEdit.AddListener(delegate
			{
				if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
				{
					urlInput.DeactivateInputField();
					GoToURLInput();
				}
				else
				{
					urlInput.text = browser.Url;
				}
			});
		}

		public void GoToURLInput()
		{
			browser.Url = urlInput.text;
		}
	}
}
