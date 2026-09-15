using System.Collections.Generic;
using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser
{
	[RequireComponent(typeof(Browser))]
	public class DemoList : MonoBehaviour
	{
		protected List<string> demoSites = new List<string>
		{
			"localGame://demo/MouseShow.html", "http://js1k.com/2013-spring/demo/1487", "http://js1k.com/2014-dragons/demo/1868", "http://js1k.com/2015-hypetrain/demo/2231", "http://js1k.com/2015-hypetrain/demo/2313", "http://js1k.com/2015-hypetrain/demo/2331", "http://js1k.com/2015-hypetrain/demo/2315", "http://js1k.com/2015-hypetrain/demo/2161", "http://js1k.com/2013-spring/demo/1533", "http://js1k.com/2014-dragons/demo/1969",
			"http://www.snappymaria.com/misc/TouchEventTest.html"
		};

		public Browser demoBrowser;

		private Browser panelBrowser;

		private int currentIndex;

		protected void Start()
		{
			panelBrowser = GetComponent<Browser>();
			panelBrowser.RegisterFunction("go", delegate(JSONNode args)
			{
				DemoNav(args[0].Check());
			});
			demoBrowser.onLoad += delegate
			{
				panelBrowser.CallFunction("setDisplayedUrl", demoBrowser.Url);
			};
			demoBrowser.Url = demoSites[0];
		}

		private void DemoNav(int dir)
		{
			if (dir > 0)
			{
				currentIndex = (currentIndex + 1) % demoSites.Count;
			}
			else
			{
				currentIndex = (currentIndex - 1 + demoSites.Count) % demoSites.Count;
			}
			demoBrowser.Url = demoSites[currentIndex];
		}
	}
}
