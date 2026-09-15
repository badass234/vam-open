using UnityEngine;
using UnityEngine.UI;

public class SpeechBlendControlUI : UIProvider
{
	public Toggle enabledToggle;

	public Slider volumeSlider;

	public Slider maxVolumeSlider;

	public Slider volumeMultiplierSlider;

	public Slider volumeClampSlider;

	public Slider volumeThresholdSlider;

	public Slider mouthOpenFactorSlider;

	public Slider mouthOpenChangeRateSlider;

	public Slider visemeDetectionFactorSlider;

	public Slider visemeThresholdSlider;

	public Slider timeBetweenSamplingSlider;

	public Slider sampleTimeAdjustSlider;

	public Slider visemeMorphChangeRateSlider;

	public Slider visemeMorphClampSlider;

	public UIPopup voiceTypePopup;

	public UIPopup morphSetPopup;

	public GameObject advancedPanel;

	public Button openAdvancedPanelButton;

	public GameObject mouthOpenVisemeFoundIndicator;

	public GameObject mouthOpenVisemeFoundNegativeIndicator;

	public InputField mouthOpenMorphUidInputField;

	public Slider mouthOpenVisemeValueSlider;

	public Button pasteMouthOpenMorphUidButton;

	public Button resetAllAdvancedSettingsButton;
}
