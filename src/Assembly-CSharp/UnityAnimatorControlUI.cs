using UnityEngine;
using UnityEngine.UI;

public class UnityAnimatorControlUI : UIProvider
{
	public Button animatorResetButton;

	public Toggle animatorEnabledToggle;

	public GameObject animatorIsPlayingIndicator;

	public Button animatorPlayButton;

	public Button animatorPauseButton;

	public Slider animatorSpeedSlider;

	public UIPopup animationSelectionPopup;

	public Toggle useCrossFadeToggle;

	public Slider crossFadeTimeSlider;

	public Text currentAnimationNameText;

	public RectTransform sequenceContainer;

	public Toggle loopSequenceToggle;

	public Button restartAnimationSequenceButton;

	public Button addAnimationToSequenceButton;

	public Button clearAndAddAnimationToSequenceButton;

	public Button clearSequenceButton;

	public Button nextClipInSequenceButton;

	public Button previousClipInSequenceButton;

	public Slider animationRotationSpeedSlider;

	public Slider animationRotationDegressForActionSlider;

	public Button animationRotateButton;
}
