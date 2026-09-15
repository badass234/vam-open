using UnityEngine;
using UnityEngine.UI;

namespace MVR.FileManagement
{
	public class PackageInfoUI : UIProvider
	{
		public RectTransform packageInfoPanel;

		public Text packageUidText;

		public Button openPackageInManagerButton;

		public Button openOnHubButton;

		public Button promotionalButton;

		public Text promotionalButtonText;

		public Button copyPromotionalLinkButton;

		public RectTransform descriptionContainer;

		public InputField descriptionField;

		public RectTransform instructionsContainer;

		public InputField instructionsField;
	}
}
