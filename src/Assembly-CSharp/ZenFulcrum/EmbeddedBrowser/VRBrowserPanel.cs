using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser
{
	public class VRBrowserPanel : MonoBehaviour, INewWindowHandler
	{
		public Browser contentBrowser;

		public Browser controlBrowser;

		public Transform keyboardLocation;

		public void Awake()
		{
			DestroyDetector destroyDetector = contentBrowser.gameObject.AddComponent<DestroyDetector>();
			destroyDetector.onDestroy += CloseBrowser;
			contentBrowser.SetNewWindowHandler(Browser.NewWindowAction.NewBrowser, this);
			contentBrowser.onLoad += delegate(JSONNode data)
			{
				controlBrowser.CallFunction("setURL", data["url"]);
			};
			controlBrowser.RegisterFunction("demoNavForward", delegate
			{
				contentBrowser.GoForward();
			});
			controlBrowser.RegisterFunction("demoNavBack", delegate
			{
				contentBrowser.GoBack();
			});
			controlBrowser.RegisterFunction("demoNavRefresh", delegate
			{
				contentBrowser.Reload();
			});
			controlBrowser.RegisterFunction("demoNavClose", delegate
			{
				CloseBrowser();
			});
			controlBrowser.RegisterFunction("goTo", delegate(JSONNode args)
			{
				contentBrowser.LoadURL(args[0], false);
			});
			VRMainControlPanel.instance.keyboard.onFocusChange += OnKeyboardOnOnFocusChange;
		}

		public void OnDestroy()
		{
			VRMainControlPanel.instance.keyboard.onFocusChange -= OnKeyboardOnOnFocusChange;
		}

		private void OnKeyboardOnOnFocusChange(Browser browser, bool editable)
		{
			if (!editable || !browser)
			{
				VRMainControlPanel.instance.MoveKeyboardUnder(null);
			}
			else if (browser == contentBrowser || browser == controlBrowser)
			{
				VRMainControlPanel.instance.MoveKeyboardUnder(this);
			}
		}

		public void CloseBrowser()
		{
			if ((bool)this && (bool)VRMainControlPanel.instance)
			{
				VRMainControlPanel.instance.DestroyPane(this);
			}
		}

		public Browser CreateBrowser(Browser parent)
		{
			VRBrowserPanel vRBrowserPanel = VRMainControlPanel.instance.OpenNewTab(this);
			vRBrowserPanel.transform.position = base.transform.position;
			vRBrowserPanel.transform.rotation = base.transform.rotation;
			return vRBrowserPanel.contentBrowser;
		}
	}
}
