using UnityEngine;
using UnityEngine.UI;

namespace MVR.FileManagement
{
	public class PackageBuilderUI : UIProvider
	{
		public Button clearAllButton;

		public Button loadMetaFromExistingPackageButton;

		public InputField creatorField;

		public InputField packageNameField;

		public InputField versionField;

		public Text statusText;

		public Toggle showDisabledToggle;

		public RectTransform packageCategoryPanel;

		public RectTransform packagesContainer;

		public RectTransform missingPackagesContainer;

		public Button scanHubForMissingPackagesButton;

		public Button selectCurrentScenePackageButton;

		public Button promotionalButton;

		public Text promotionalButtonText;

		public Button copyPromotionalLinkButton;

		public RectTransform packageReferencesContainer;

		public Toggle packageEnabledToggle;

		public Button deletePackageButton;

		public RectTransform confirmDeletePackagePanel;

		public Text confirmDeletePackageText;

		public Button confirmDeletePackageButton;

		public Button cancelDeletePackageButton;

		public InputField userNotesField;

		public Toggle pluginsAlwaysEnabledToggle;

		public Toggle pluginsAlwaysDisabledToggle;

		public Toggle ignoreMissingDependencyErrorsToggle;

		public RectTransform packPanel;

		public Slider packProgressSlider;

		public Button unpackButton;

		public RectTransform confirmUnpackPanel;

		public Button confirmUnpackButton;

		public Button cancelUnpackButton;

		public Button repackButton;

		public Button restoreFromOriginalButton;

		public RectTransform confirmRestoreFromOriginalPanel;

		public Button confirmRestoreFromOriginalButton;

		public Button cancelRestoreFromOriginalButton;

		public GameObject currentPackageIsOnHubIndicator;

		public GameObject currentPackageIsOnHubIndicator2;

		public Button openOnHubButton;

		public Button openInHubDownloaderButton;

		public GameObject currentPackageHasSceneIndicator;

		public Button openSceneButton;

		public GameObject hadReferenceIssuesIndicator;

		public RectTransform contentContainer;

		public Button addDirectoryButton;

		public Button addFileButton;

		public Button removeSelectedButton;

		public Button removeAllButton;

		public RectTransform referencesContainer;

		public UIPopup standardReferenceVersionOptionPopup;

		public UIPopup scriptReferenceVersionOptionPopup;

		public UIDynamicButton prepPackageButton;

		public Button fixReferencesButton;

		public RectTransform licenseReportContainer;

		public Text licenseReportIssueText;

		public Text nonCommercialLicenseReportIssueText;

		public InputField descriptionField;

		public UIDynamicToggle[] customOptionToggles;

		public InputField creditsField;

		public InputField instructionsField;

		public InputField promotionalField;

		public UIPopup licensePopup;

		public UIPopup secondaryLicensePopup;

		public UIPopup EAYearPopup;

		public UIPopup EAMonthPopup;

		public UIPopup EADayPopup;

		public Text licenseDescriptionText;

		public RectTransform openPrepFolderInExplorerNotice;

		public Button openPrepFolderInExplorerButton;

		public RectTransform finalizeCheckPanel;

		public RectTransform finalizingPanel;

		public Button finalizeCheckCancelButton;

		public Button finalizeCheckConfirmButton;

		public Button finalizeButton;

		public Slider finalizeProgressSlider;

		public Button cancelFinalizeButton;
	}
}
