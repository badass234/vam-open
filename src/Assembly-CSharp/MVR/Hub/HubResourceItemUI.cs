using UnityEngine;
using UnityEngine.UI;

namespace MVR.Hub
{
	public class HubResourceItemUI : MonoBehaviour
	{
		public HubResourceItem connectedItem;

		public Text titleText;

		public Text tagLineText;

		public Text versionText;

		public Text payTypeText;

		public Text categoryText;

		public Button payTypeAndCategorySelectButton;

		public Button creatorSelectButton;

		public Image licenseColorImage;

		public GameObject hasLicenseIndicator;

		public Text licenseTypeText;

		public Text creatorText;

		public RawImage creatorIconImage;

		public RawImage thumbnailImage;

		public GameObject hubDownloadableIndicator;

		public GameObject hubDownloadableNegativeIndicator;

		public GameObject hubHostedIndicator;

		public GameObject hubHostedNegativeIndicator;

		public GameObject hasDependenciesIndicator;

		public GameObject hasDependenciesNegativeIndicator;

		public GameObject inLibraryIndicator;

		public GameObject updateAvailableIndicator;

		public Text updateMsgText;

		public Text dependencyCountText;

		public Text downloadCountText;

		public Text ratingsCountText;

		public Slider ratingSlider;

		public Text lastUpdateText;

		public Button openDetailButton;
	}
}
