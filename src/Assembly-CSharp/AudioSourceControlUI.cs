using UnityEngine.UI;

public class AudioSourceControlUI : UIProvider
{
	public Toggle loopToggle;

	public Slider volumeSlider;

	public Slider pitchSlider;

	public Slider stereoPanSlider;

	public Slider minDistanceSlider;

	public Slider maxDistanceSlider;

	public Slider spatialBlendSlider;

	public Slider stereoSpreadSlider;

	public Toggle spatializeToggle;

	public UIPopup audioRolloffModePopup;

	public Slider delayBetweenQueuedClipsSlider;

	public Slider volumeTriggerQuicknessSlider;

	public Slider volumeTriggerMultiplierSlider;

	public Slider recentMaxVolumeSlider;

	public Toggle equalizeVolumeSlider;

	public Text playingClipNameText;

	public Button startMicrophoneInputButton;

	public Button endMicrophoneInputButton;
}
