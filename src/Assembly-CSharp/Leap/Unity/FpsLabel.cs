using System;
using UnityEngine;

namespace Leap.Unity
{
	public class FpsLabel : MonoBehaviour
	{
		[SerializeField]
		private LeapProvider _provider;

		[SerializeField]
		private TextMesh _frameRateText;

		private SmoothedFloat _smoothedRenderFps = new SmoothedFloat();

		private void OnEnable()
		{
			if (_provider == null)
			{
				_provider = Hands.Provider;
			}
			if (_frameRateText == null)
			{
				_frameRateText = GetComponentInChildren<TextMesh>();
				if (_frameRateText == null)
				{
					Debug.LogError("Could not enable FpsLabel because no TextMesh was specified!");
					base.enabled = false;
				}
			}
			_smoothedRenderFps.delay = 0.3f;
			_smoothedRenderFps.reset = true;
		}

		private void Update()
		{
			_frameRateText.text = string.Empty;
			if (_provider != null)
			{
				Frame currentFrame = _provider.CurrentFrame;
				if (currentFrame != null)
				{
					TextMesh frameRateText = _frameRateText;
					frameRateText.text = frameRateText.text + "Data FPS:" + _provider.CurrentFrame.CurrentFramesPerSecond.ToString("f2");
					_frameRateText.text += Environment.NewLine;
				}
			}
			if (Time.smoothDeltaTime > Mathf.Epsilon)
			{
				_smoothedRenderFps.Update(1f / Time.smoothDeltaTime, Time.deltaTime);
			}
			TextMesh frameRateText2 = _frameRateText;
			frameRateText2.text = frameRateText2.text + "Render FPS:" + Mathf.RoundToInt(_smoothedRenderFps.value).ToString("f2");
		}
	}
}
