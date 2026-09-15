using UnityEngine;
using UnityEngine.UI;

namespace MeshVR
{
	public class DAZClothingItemOpenInCreatorControl : JSONStorable
	{
		protected JSONStorableAction openInCreatorAction;

		protected DAZDynamic dd;

		protected Text currentTagsText;

		protected void OpenInCreator()
		{
			if (!(dd != null))
			{
				return;
			}
			DAZClothingItem componentInParent = GetComponentInParent<DAZClothingItem>();
			if (componentInParent != null && componentInParent.characterSelector != null && componentInParent.type == DAZDynamicItem.Type.Custom)
			{
				if (componentInParent.gender == DAZDynamicItem.Gender.Female)
				{
					componentInParent.characterSelector.SetActiveClothingItem(componentInParent, false);
					componentInParent.characterSelector.LoadFemaleClothingCreatorItem(dd);
				}
				else if (componentInParent.gender == DAZDynamicItem.Gender.Male)
				{
					componentInParent.characterSelector.SetActiveClothingItem(componentInParent, false);
					componentInParent.characterSelector.LoadMaleClothingCreatorItem(dd);
				}
			}
		}

		protected override void InitUI(Transform t, bool isAlt)
		{
			base.InitUI(t, isAlt);
			if (t != null)
			{
				DAZClothingItemOpenInCreatorControlUI componentInChildren = t.GetComponentInChildren<DAZClothingItemOpenInCreatorControlUI>(true);
				if (dd != null && componentInChildren != null)
				{
					openInCreatorAction.RegisterButton(componentInChildren.openButton, isAlt);
				}
				currentTagsText = componentInChildren.currentTagsText;
			}
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
