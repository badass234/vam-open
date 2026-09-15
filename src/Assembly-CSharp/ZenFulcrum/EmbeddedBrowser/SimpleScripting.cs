using System.Collections;
using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser
{
	[RequireComponent(typeof(Browser))]
	public class SimpleScripting : MonoBehaviour
	{
		private Browser browser;

		private int colorIdx;

		private Color[] colors = new Color[4]
		{
			new Color(1f, 0f, 0f),
			new Color(1f, 1f, 0f),
			new Color(1f, 1f, 1f),
			new Color(1f, 1f, 0f)
		};

		public void Start()
		{
			browser = GetComponent<Browser>();
			browser.LoadHTML("\n<button style='background: green; color: white' onclick='greenButtonClicked(event.x, event.y)'>Green Button</button>\n\n\n<br><br>\n\n\nUsername: <input type='text' id='username' value='CouchPotato47'>\n\n\n<br><br>\n\n\n<div id='box' style='width: 200px; height: 200px;border: 1px solid black'>\n\tClick \"Change Color\"\n</div>\n\n<script>\nfunction changeColor(r, g, b, text) {\n\tvar el = document.getElementById('box');\n\tel.style.background = 'rgba(' + (r * 255) + ', ' + (g * 255) + ', ' + (b * 255) + ', 1)';\n\tel.textContent = text;\n}\n</script>\n");
			browser.RegisterFunction("greenButtonClicked", delegate(JSONNode args)
			{
				int num = args[0];
				int num2 = args[1];
				Debug.Log("The <color=green>green</color> button was clicked at " + num + ", " + num2);
			});
		}

		public void GetUsername()
		{
			browser.EvalJS("document.getElementById('username').value").Then(delegate(JSONNode username)
			{
				Debug.Log("The username is: " + username);
			}).Done();
			Debug.Log("Fetching username");
		}

		public void ChangeColor()
		{
			Color color = colors[colorIdx++ % colors.Length];
			browser.CallFunction("changeColor", color.r, color.g, color.b, "Selection Number " + colorIdx).Done();
		}

		public void GetUsername2()
		{
			StartCoroutine(_GetUsername2());
		}

		private IEnumerator _GetUsername2()
		{
			IPromise<JSONNode> promise = browser.EvalJS("document.getElementById('username').value");
			Debug.Log("Fetching username");
			yield return promise.ToWaitFor();
			Debug.Log("The username is: " + promise.Value);
		}
	}
}
