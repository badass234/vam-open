using UnityEngine;
using UnityEngine.UI;

namespace MVR.Hub
{
	public class HubResourcePackageUI : MonoBehaviour
	{
		public HubResourcePackage connectedItem;

		public Button resourceButton;

		public Text nameText;

		public Text licenseTypeText;

		public Text fileSizeText;

		public GameObject isDependencyIndicator;

		public GameObject notOnHubIndicator;

		public GameObject alreadyHaveIndicator;

		public Button openInPackageManagerButton;

		public GameObject alreadyHaveSceneIndicator;

		public Button openSceneButton;

		public Button downloadButton;

		public GameObject updateAvailableIndicator;

		public Button updateButton;

		public Text updateMsgText;

		public GameObject isDownloadQueuedIndicator;

		public GameObject isDownloadingIndicator;

		public GameObject isDownloadedIndicator;

		public Slider progressSlider;
	}
}
