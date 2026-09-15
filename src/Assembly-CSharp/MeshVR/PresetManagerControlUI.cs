using UnityEngine.UI;

namespace MeshVR
{
	public class PresetManagerControlUI : UIProvider
	{
		public Button browsePresetsButton;

		public Button openPresetBrowsePathInExplorerButton;

		public InputField presetNameField;

		public Toggle storePresetNameToggle;

		public Toggle loadPresetOnSelectToggle;

		public Toggle useMergeLoadToggle;

		public Toggle useMergeLoadBrowserToggle;

		public UIPopup favoriteSelectionPopup;

		public UIDynamicButton storePresetButton;

		public UIDynamicButton storePresetWithScreenshotButton;

		public UIDynamicButton storeOverlayPresetButton;

		public UIDynamicButton storeOverlayPresetWithScreenshotButton;

		public UIDynamicButton loadPresetButton;

		public UIDynamicButton loadDefaultsButton;

		public UIDynamicButton loadUserDefaultsButton;

		public UIDynamicButton storeUserDefaultsButton;

		public UIDynamicButton clearUserDefaultsButton;

		public Toggle favoriteToggle;

		public Toggle storeOptionalToggle;

		public Toggle storeOptional2Toggle;

		public Toggle storeOptional3Toggle;

		public Toggle storePresetBinaryToggle;

		public Toggle includeOptionalToggle;

		public Toggle includeOptional2Toggle;

		public Toggle includeOptional3Toggle;

		public Toggle includePhysicalToggle;

		public Toggle includeAppearanceToggle;

		public Toggle lockParamsToggle;

		public Text statusText;
	}
}
