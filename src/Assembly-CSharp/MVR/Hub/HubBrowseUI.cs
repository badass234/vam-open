using UnityEngine;
using UnityEngine.UI;
using ZenFulcrum.EmbeddedBrowser;

namespace MVR.Hub
{
	public class HubBrowseUI : UIProvider
	{
		public GameObject hubEnabledNegativeIndicator;

		public Button enableHubButton;

		public GameObject webBrowserEnabledNegativeIndicator;

		public Button enabledWebBrowserButton;

		public RectTransform refreshingGetInfoPanel;

		public RectTransform failedGetInfoPanel;

		public Text getInfoErrorText;

		public Button cancelGetHubInfoButton;

		public Button retryGetHubInfoButton;

		public RectTransform itemContainer;

		public ScrollRect itemScrollRect;

		public GameObject refreshIndicator;

		public Button refreshButton;

		public Text numResourcesText;

		public Text pageInfoText;

		public Button firstPageButton;

		public Button previousPageButton;

		public Button nextPageButton;

		public Button clearFiltersButton;

		public UIPopup hostedOptionPopup;

		public UIPopup payTypeFilterPopup;

		public UIPopup categoryFilterPopup;

		public UIPopup creatorFilterPopup;

		public UIPopup tagsFilterPopup;

		public InputField searchInputField;

		public Toggle searchAllToggle;

		public UIPopup sortPrimaryPopup;

		public UIPopup sortSecondaryPopup;

		public GameObject detailPanel;

		public RectTransform resourceDetailContainer;

		public Browser browser;

		public VRWebBrowser webBrowser;

		public GameObject isWebLoadingIndicator;

		public GameObject missingPackagesPanel;

		public RectTransform missingPackagesContainer;

		public Button openMissingPackagesPanelButton;

		public Button closeMissingPackagesPanelButton;

		public Button downloadAllMissingPackagesButton;

		public GameObject updatesPanel;

		public RectTransform updatesContainer;

		public Button openUpdatesPanelButton;

		public Button closeUpdatesPanelButton;

		public Button downloadAllUpdatesButton;

		public GameObject isDownloadingIndicator;

		public Text downloadQueuedCountText;

		public Button openDownloadingButton;
	}
}
