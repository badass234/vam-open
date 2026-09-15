using UnityEngine;
using UnityEngine.UI;

public class CustomUnityAssetLoaderUI : UIProvider
{
	public Button fileBrowseButton;

	public Button clearButton;

	public Text urlText;

	public Toggle importLightmapsToggle;

	public Toggle importLightProbesToggle;

	public Toggle registerCanvasesToggle;

	public Toggle showCanvasesToggle;

	public Toggle loadDllToggle;

	public UIPopup assetSelectionPopup;

	public RectTransform loadingIndicator;
}
