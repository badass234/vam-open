using UnityEngine;

namespace MHLab
{
	public class Example : MonoBehaviour
	{
		private float _screenMiddleX;

		private float _screenMiddleY;

		private void Start()
		{
			_screenMiddleX = Screen.width / 2;
			_screenMiddleY = Screen.height / 2;
		}

		private void OnGUI()
		{
			if (GUI.Button(new Rect(10f, 10f, 50f, 22f), "it-IT"))
			{
				Singleton<Localizatron>.Instance.SetLanguage("it-IT");
			}
			if (GUI.Button(new Rect(70f, 10f, 50f, 22f), "en-EN"))
			{
				Singleton<Localizatron>.Instance.SetLanguage("en-EN");
			}
			GUI.Label(new Rect(_screenMiddleX - 50f, _screenMiddleY - 11f, 100f, 22f), Singleton<Localizatron>.Instance.Translate("Hello World"));
		}
	}
}
