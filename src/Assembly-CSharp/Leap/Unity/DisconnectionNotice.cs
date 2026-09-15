using UnityEngine;
using UnityEngine.UI;

namespace Leap.Unity
{
	public class DisconnectionNotice : MonoBehaviour
	{
		public float fadeInTime = 1f;

		public float fadeOutTime = 1f;

		public AnimationCurve fade;

		public int waitFrames = 10;

		public Color onColor = Color.white;

		private Controller leap_controller_;

		private float fadedIn;

		private int frames_disconnected_;

		private void Start()
		{
			leap_controller_ = new Controller();
			SetAlpha(0f);
		}

		private void SetAlpha(float alpha)
		{
			GetComponent<UnityEngine.UI.Image>().color = Color.Lerp(Color.clear, onColor, alpha);
		}

		private bool IsConnected()
		{
			return leap_controller_.IsConnected;
		}

		private void Update()
		{
			if (IsConnected())
			{
				frames_disconnected_ = 0;
			}
			else
			{
				frames_disconnected_++;
			}
			if (frames_disconnected_ < waitFrames)
			{
				fadedIn -= Time.deltaTime / fadeOutTime;
			}
			else
			{
				fadedIn += Time.deltaTime / fadeInTime;
			}
			fadedIn = Mathf.Clamp(fadedIn, 0f, 1f);
			SetAlpha(fade.Evaluate(fadedIn));
		}
	}
}
