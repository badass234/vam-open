using UnityEngine;
using UnityEngine.UI;

namespace MeshVR
{
	public class DAZDynamicItemOpenInCreatorControl : JSONStorable
	{
		protected JSONStorableAction openInCreatorAction;

		protected DAZDynamic dd;

		protected Text currentTagsText;

		protected void OpenInCreator()
		{
			if (dd != null)
			{
				DAZDynamicItem componentInParent = GetComponentInParent<DAZDynamicItem>();
				if (componentInParent != null && componentInParent.characterSelector != null)
				{
					componentInParent.characterSelector.LoadDynamicCreatorItem(componentInParent, dd);
				}
			}
		}

		protected override void InitUI(Transform t, bool isAlt)
		{
			base.InitUI(t, isAlt);
			if (!(t != null))
			{
				return;
			}
			DAZDynamicItemOpenInCreatorControlUI componentInChildren = t.GetComponentInChildren<DAZDynamicItemOpenInCreatorControlUI>(true);
			if (dd != null && componentInChildren != null)
			{
				if (componentInChildren.openButton != null)
				{
					componentInChildren.openButton.interactable = true;
				}
				openInCreatorAction.RegisterButton(componentInChildren.openButton, isAlt);
			}
			currentTagsText = componentInChildren.currentTagsText;
		}

		protected virtual void Init()
		{
			dd = GetComponent<DAZDynamic>();
			if (dd != null)
			{
				openInCreatorAction = new JSONStorableAction("OpenInCreator", OpenInCreator);
				RegisterAction(openInCreatorAction);
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

		private void Update()
		{
			if (dd != null && currentTagsText != null)
			{
				currentTagsText.text = dd.tags;
			}
		}
	}
}
