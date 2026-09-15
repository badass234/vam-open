using UnityEngine;
using UnityEngine.UI;

namespace MVR.Hub
{
	public class HubResourceItemDetailUI : HubResourceItemUI
	{
		public new HubResourceItemDetail connectedItem;

		public Button closeDetailButton;

		public Button closeDetailButtonAlt;

		public GameObject hadErrorIndicator;

		public Text errorText;

		public Button navigateToOverviewButton;

		public GameObject hasUpdatesIndicator;

		public Text updatesText;

		public Button navigateToUpdatesButton;

		public GameObject hasReviewsIndicator;

		public Text reviewsText;

		public Button navigateToReviewsButton;

		public Button navigateToHistoryButton;

		public Button navigateToDiscussionButton;

		public GameObject hasPromotionalLinkIndicator;

		public Text promotionalLinkText;

		public Button navigateToPromotionalLinkButton;

		public PointerEnterExitAction promtionalLinkButtonEnterExitAction;

		public GameObject hasOtherCreatorsIndicator;

		public RectTransform creatorSupportContent;

		public GameObject hubDownloadableIndicatorAlt;

		public GameObject hubDownloadableNegativeIndicatorAlt;

		public Text externalDownloadUrl;

		public Button goToExternalDownloadUrlButton;

		public RectTransform packageContent;

		public Button downloadAllButton;

		public GameObject downloadAvailableIndicator;
	}
}
