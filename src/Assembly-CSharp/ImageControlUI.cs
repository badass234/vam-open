using UnityEngine;
using UnityEngine.UI;

public class ImageControlUI : UIProvider
{
	public Button loadButton;

	public Button copyToClipboardButton;

	public Button copyFromClipboardButton;

	public Button clearUrlButton;

	public Button fileBrowseButton;

	public InputField urlInputField;

	public InputFieldAction urlInputFieldAction;

	public Toggle allowImageTilingToggle;

	public Toggle playVideoWhenReadyToggle;

	public Toggle useAnamorphicVideoAspectRatioToggle;

	public GameObject videoIsReadyIndicator;

	public GameObject videoIsLoadingIndicator;

	public GameObject videoHadErrorIndicator;

	public Toggle loopVideoToggle;

	public Slider playbackTimeSilder;

	public Button playVideoButton;

	public Button pauseVideoButton;

	public Button stopVideoButton;

	public Button seekToVideoStartButton;
}
