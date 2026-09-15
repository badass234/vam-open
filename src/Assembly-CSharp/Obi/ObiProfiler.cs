using UnityEngine;

namespace Obi
{
	public class ObiProfiler : MonoBehaviour
	{
		public GUISkin skin;

		public bool showPercentages;

		public int maxVisibleThreads = 4;

		public static Oni.ProfileInfo[] info;

		public static double frameDuration;

		private float zoom = 1f;

		private Vector2 scrollPosition = Vector2.zero;

		private int numThreads = 1;

		public void OnEnable()
		{
			Oni.EnableProfiler(true);
			numThreads = Oni.GetMaxSystemConcurrency();
		}

		public void OnDisable()
		{
			Oni.EnableProfiler(false);
		}

		public void OnGUI()
		{
			GUI.skin = skin;
			int num = 20;
			int num2 = 20;
			GUI.BeginGroup(new Rect(0f, 0f, Screen.width, num), string.Empty, "Box");
			GUI.Label(new Rect(5f, 0f, 50f, num), "Zoom:");
			zoom = GUI.HorizontalSlider(new Rect(50f, 5f, 100f, num), zoom, 0.005f, 1f);
			GUI.Label(new Rect(Screen.width - 100, 0f, 100f, num), (frameDuration / 1000.0).ToString("0.###") + " ms/step");
			GUI.EndGroup();
			scrollPosition = GUI.BeginScrollView(new Rect(0f, num, Screen.width, Mathf.Min(maxVisibleThreads, numThreads) * num2 + 10), scrollPosition, new Rect(0f, 0f, (float)Screen.width / zoom, numThreads * num2));
			Oni.ProfileInfo[] array = info;
			for (int i = 0; i < array.Length; i++)
			{
				Oni.ProfileInfo profileInfo = array[i];
				GUI.color = Color.green;
				int num3 = (int)(profileInfo.start / frameDuration * (double)(Screen.width - 10) / (double)zoom);
				int num4 = (int)(profileInfo.end / frameDuration * (double)(Screen.width - 10) / (double)zoom);
				string text;
				if (showPercentages)
				{
					double num5 = (profileInfo.end - profileInfo.start) / frameDuration * 100.0;
					text = profileInfo.name + " (" + num5.ToString("0.#") + "%)";
				}
				else
				{
					double num6 = (profileInfo.end - profileInfo.start) / 1000.0;
					text = profileInfo.name + " (" + num6.ToString("0.##") + "ms)";
				}
				GUI.Box(new Rect(num3, profileInfo.threadID * num2, num4 - num3, num2), text, "thread");
			}
			GUI.EndScrollView();
		}
	}
}
