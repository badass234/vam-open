using UnityEngine.UI;

public class VRWebBrowserUI : UIProvider
{
	public Toggle fullMouseClickOnDownToggle;

	public Toggle disableInteractionToggle;

	public InputField urlInput;

	public InputFieldAction urlInputAction;

	public Text navigatedURLText;

	public Text hoveredURLText;

	public Button goButton;

	public Button backButton;

	public Button forwardButton;

	public Button copyToClipboardButton;

	public Button copyFromClipboardButton;

	public Button homeButton;

	public UIPopup quickSitesPopup;
}
