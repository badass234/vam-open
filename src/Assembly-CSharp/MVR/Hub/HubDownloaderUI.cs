using UnityEngine;
using UnityEngine.UI;

namespace MVR.Hub
{
	public class HubDownloaderUI : UIProvider
	{
		public RectTransform panel;

		public RectTransform packagesContainer;

		public Text infoText;

		public Text downloadInfoText;

		public Button openPanelButton;

		public Button closePanelButton;

		public Button clearTrackedPackagesButton;

		public InputField packageNameInputField;

		public Button findPackageButton;

		public Button downloadAllTrackedPackagesButton;

		public GameObject disabledIndicator;

		public Button enableHubDownloaderButton;

		public Button rejectHubDownloaderButton;
	}
}
