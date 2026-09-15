using UnityEngine;
using UnityEngine.UI;

namespace MVR.FileManagement
{
	public class PackageInfo : JSONStorable
	{
		protected JSONStorableString packageUidJSON;

		protected RectTransform descriptionContainer;

		protected JSONStorableString descriptionJSON;

		protected RectTransform instructionsContainer;

		protected JSONStorableString instructionsJSON;

		protected JSONStorableAction openPackageInManagerAction;

		protected JSONStorableAction openOnHubAction;

		protected RectTransform packageInfoPanel;

		protected Button promotionalButton;

		protected Text promotionalButtonText;

		protected JSONStorableAction goToPromotionalLinkAction;

		protected JSONStorableAction copyPromotionalLinkAction;

		protected string _promotionalLink;

		protected JSONStorableString promotionalLinkJSON;

		protected VarPackage _package;

		protected void SyncDescriptionContainer()
		{
			string text = null;
			if (descriptionJSON != null)
			{
				text = descriptionJSON.val;
			}
			if (descriptionContainer != null)
			{
				if (text != null && text != string.Empty)
				{
					descriptionContainer.gameObject.SetActive(true);
				}
				else
				{
					descriptionContainer.gameObject.SetActive(false);
				}
			}
		}

		protected void SyncDescription(string s)
		{
			SyncDescriptionContainer();
		}

		protected void SyncInstructionsContainer()
		{
			string text = null;
			if (instructionsJSON != null)
			{
				text = instructionsJSON.val;
			}
			if (instructionsContainer != null)
			{
				if (text != null && text != string.Empty)
				{
					instructionsContainer.gameObject.SetActive(true);
				}
				else
				{
					instructionsContainer.gameObject.SetActive(false);
				}
			}
		}

		protected void SyncInstructions(string s)
		{
			SyncInstructionsContainer();
		}

		private void OpenPackageInManager()
		{
			string val = packageUidJSON.val;
			if (val != null && val != string.Empty)
			{
				SuperController.singleton.OpenPackageInManager(val);
			}
		}

		protected void SyncOpenOnHubButton()
		{
			bool active = _package != null && _package.IsOnHub;
			if (openOnHubAction.button != null)
			{
				openOnHubAction.button.gameObject.SetActive(active);
			}
			if (openOnHubAction.buttonAlt != null)
			{
				openOnHubAction.buttonAlt.gameObject.SetActive(active);
			}
		}

		private void OpenOnHub()
		{
			if (_package != null)
			{
				_package.OpenOnHub();
			}
		}

		protected void SyncPackagePanel()
		{
			if (packageInfoPanel != null)
			{
				if (_package != null)
				{
					packageInfoPanel.gameObject.SetActive(true);
				}
				else
				{
					packageInfoPanel.gameObject.SetActive(false);
				}
			}
		}

		protected void SyncPromotionalButton()
		{
			if (promotionalButton != null)
			{
				if (!SuperController.singleton.promotionalDisabled && _promotionalLink != null && _promotionalLink != string.Empty)
				{
					promotionalButton.gameObject.SetActive(true);
				}
				else
				{
					promotionalButton.gameObject.SetActive(false);
				}
			}
			if (promotionalButtonText != null)
			{
				promotionalButtonText.text = _promotionalLink;
			}
			if (copyPromotionalLinkAction != null && copyPromotionalLinkAction.button != null)
			{
				if (!SuperController.singleton.promotionalDisabled && _promotionalLink != null && _promotionalLink != string.Empty)
				{
					copyPromotionalLinkAction.button.gameObject.SetActive(true);
				}
				else
				{
					copyPromotionalLinkAction.button.gameObject.SetActive(false);
				}
			}
		}

		protected void GoToPromotionalLink()
		{
			if (promotionalButtonText != null)
			{
				SuperController.singleton.OpenLinkInBrowser(promotionalButtonText.text);
			}
		}

		protected void CopyPromotionalLink()
		{
			if (promotionalButtonText != null)
			{
				GUIUtility.systemCopyBuffer = promotionalButtonText.text;
			}
		}

		protected void SyncPromotionalLink(string s)
		{
			_promotionalLink = s;
			SyncPromotionalButton();
		}

		public void SetPackage(VarPackage vp)
		{
			_package = vp;
			if (vp != null)
			{
				packageUidJSON.val = vp.Uid;
				descriptionJSON.val = vp.Description;
				instructionsJSON.val = vp.Instructions;
				promotionalLinkJSON.val = vp.PromotionalLink;
			}
			else
			{
				packageUidJSON.val = string.Empty;
				descriptionJSON.val = string.Empty;
				instructionsJSON.val = string.Empty;
				promotionalLinkJSON.val = string.Empty;
			}
			SyncOpenOnHubButton();
			SyncPackagePanel();
		}

		protected void Init()
		{
			packageUidJSON = new JSONStorableString("packageUid", string.Empty);
			packageUidJSON.interactable = false;
			descriptionJSON = new JSONStorableString("description", string.Empty, SyncDescription);
			descriptionJSON.interactable = false;
			instructionsJSON = new JSONStorableString("instructions", string.Empty, SyncInstructions);
			instructionsJSON.interactable = false;
			promotionalLinkJSON = new JSONStorableString("promotionalLink", string.Empty, SyncPromotionalLink);
			goToPromotionalLinkAction = new JSONStorableAction("GoToPromotionalLink", GoToPromotionalLink);
			RegisterAction(goToPromotionalLinkAction);
			copyPromotionalLinkAction = new JSONStorableAction("CopyPromotionalLink", CopyPromotionalLink);
			RegisterAction(copyPromotionalLinkAction);
			openPackageInManagerAction = new JSONStorableAction("OpenPackageInManager", OpenPackageInManager);
			RegisterAction(openPackageInManagerAction);
			openOnHubAction = new JSONStorableAction("OpenOnHub", OpenOnHub);
			RegisterAction(openOnHubAction);
		}

		protected override void InitUI(Transform t, bool isAlt)
		{
			if (!(t != null))
			{
				return;
			}
			PackageInfoUI componentInChildren = t.GetComponentInChildren<PackageInfoUI>(true);
			if (componentInChildren != null)
			{
				copyPromotionalLinkAction.RegisterButton(componentInChildren.copyPromotionalLinkButton, isAlt);
				if (!isAlt)
				{
					packageInfoPanel = componentInChildren.packageInfoPanel;
					SyncPackagePanel();
					descriptionContainer = componentInChildren.descriptionContainer;
					SyncDescriptionContainer();
					instructionsContainer = componentInChildren.instructionsContainer;
					SyncInstructionsContainer();
					promotionalButton = componentInChildren.promotionalButton;
					promotionalButtonText = componentInChildren.promotionalButtonText;
					SyncPromotionalButton();
				}
				packageUidJSON.RegisterText(componentInChildren.packageUidText, isAlt);
				descriptionJSON.RegisterInputField(componentInChildren.descriptionField, isAlt);
				instructionsJSON.RegisterInputField(componentInChildren.instructionsField, isAlt);
				goToPromotionalLinkAction.RegisterButton(componentInChildren.promotionalButton, isAlt);
				openPackageInManagerAction.RegisterButton(componentInChildren.openPackageInManagerButton, isAlt);
				openOnHubAction.RegisterButton(componentInChildren.openOnHubButton, isAlt);
				SyncOpenOnHubButton();
			}
		}

		protected override void Awake()
		{
			if (!awakecalled)
			{
				base.Awake();
				Init();
				InitUI();
				InitUIAlt();
			}
		}
	}
}
