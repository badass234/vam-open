using System.Collections.Generic;
using MeshVR;
using UnityEngine;

public class DAZClothingItemControl : JSONStorable
{
	protected DAZClothingItem _clothingItem;

	protected PresetManagerControl presetManagerControl;

	public JSONStorableBool lockedJSON;

	public JSONStorableBool disableAnatomyJSON;

	protected JSONStorableBool isRealClothingItemJSON;

	protected JSONStorableBool enableJointSpringAndDamperAdjustJSON;

	protected JSONStorableBool enableBreastJointAdjustJSON;

	protected JSONStorableBool enableGluteJointAdjustJSON;

	protected JSONStorableFloat breastJointSpringAndDamperMultiplierJSON;

	protected JSONStorableFloat gluteJointSpringAndDamperMultiplierJSON;

	public DAZClothingItem clothingItem
	{
		get
		{
			return _clothingItem;
		}
		set
		{
			if (_clothingItem != value)
			{
				_clothingItem = value;
				SyncClothingItem();
			}
		}
	}

	protected void SyncClothingItem()
	{
		if (_clothingItem != null)
		{
			lockedJSON = new JSONStorableBool("locked", clothingItem.locked, SyncLocked);
			lockedJSON.isStorable = false;
			lockedJSON.isRestorable = false;
			RegisterBool(lockedJSON);
			disableAnatomyJSON = new JSONStorableBool("disableAnatomy", clothingItem.disableAnatomy, SyncDisableAnatomy);
			RegisterBool(disableAnatomyJSON);
			isRealClothingItemJSON = new JSONStorableBool("isRealClothingItem", clothingItem.isRealItem, SyncIsRealClothingItem);
			RegisterBool(isRealClothingItemJSON);
			if (_clothingItem.gender == DAZDynamicItem.Gender.Female)
			{
				enableJointSpringAndDamperAdjustJSON = new JSONStorableBool("enableJointSpringAndDamperAdjust", clothingItem.jointAdjustEnabled, SyncEnableJointSpringAndDamperAdjust);
				RegisterBool(enableJointSpringAndDamperAdjustJSON);
				enableBreastJointAdjustJSON = new JSONStorableBool("enableBreastJointAdjust", clothingItem.adjustFemaleBreastJointSpringAndDamper, SyncEnableBreastJointAdjust);
				RegisterBool(enableBreastJointAdjustJSON);
				enableGluteJointAdjustJSON = new JSONStorableBool("enableGluteJointAdjust", clothingItem.adjustFemaleGluteJointSpringAndDamper, SyncEnableGluteJointAdjust);
				RegisterBool(enableGluteJointAdjustJSON);
				breastJointSpringAndDamperMultiplierJSON = new JSONStorableFloat("breastJointSpringAndDamperMultiplier", clothingItem.breastJointSpringAndDamperMultiplier, SyncBreastJointSpringAndDamperMultiplier, 1f, 10f);
				RegisterFloat(breastJointSpringAndDamperMultiplierJSON);
				gluteJointSpringAndDamperMultiplierJSON = new JSONStorableFloat("gluteJointSpringAndDamperMultiplier", clothingItem.gluteJointSpringAndDamperMultiplier, SyncGluteJointSpringAndDamperMultiplier, 1f, 10f);
				RegisterFloat(gluteJointSpringAndDamperMultiplierJSON);
			}
		}
	}

	public void Delete()
	{
		if (clothingItem != null)
		{
			clothingItem.transform.SetParent(null);
			Object.Destroy(clothingItem.gameObject);
		}
	}

	public void RefreshClothingItemThumbnail(string dynamicItemPath)
	{
		if (clothingItem != null)
		{
			clothingItem.RefreshClothingItemThumbnail(dynamicItemPath);
		}
	}

	public void RefreshClothingItems()
	{
		if (clothingItem != null)
		{
			clothingItem.RefreshClothingItems();
		}
	}

	public bool IsClothingUIDAvailable(string uid)
	{
		if (clothingItem != null)
		{
			return clothingItem.IsClothingUIDAvailable(uid);
		}
		return false;
	}

	public HashSet<string> GetAllClothingOtherTags()
	{
		if (clothingItem != null)
		{
			return clothingItem.GetAllClothingOtherTags();
		}
		return null;
	}

	protected void SyncLocked(bool b)
	{
		if (clothingItem != null)
		{
			clothingItem.locked = b;
		}
		if (presetManagerControl != null)
		{
			presetManagerControl.lockParams = b;
		}
	}

	protected void SyncDisableAnatomy(bool b)
	{
		if (clothingItem != null)
		{
			clothingItem.disableAnatomy = b;
			clothingItem.SyncClothingAdjustments();
		}
	}

	protected void SyncIsRealClothingItem(bool b)
	{
		if (clothingItem != null)
		{
			clothingItem.isRealItem = b;
		}
	}

	public void ResetIsRealClothingItem()
	{
		if (isRealClothingItemJSON != null)
		{
			isRealClothingItemJSON.val = true;
			isRealClothingItemJSON.defaultVal = true;
		}
	}

	protected void SyncEnableJointSpringAndDamperAdjust(bool b)
	{
		if (clothingItem != null)
		{
			clothingItem.jointAdjustEnabled = b;
		}
	}

	protected void SyncEnableBreastJointAdjust(bool b)
	{
		if (clothingItem != null)
		{
			clothingItem.adjustFemaleBreastJointSpringAndDamper = b;
			clothingItem.SyncClothingAdjustments();
		}
	}

	protected void SyncEnableGluteJointAdjust(bool b)
	{
		if (clothingItem != null)
		{
			clothingItem.adjustFemaleGluteJointSpringAndDamper = b;
			clothingItem.SyncClothingAdjustments();
		}
	}

	protected void SyncBreastJointSpringAndDamperMultiplier(float f)
	{
		if (clothingItem != null)
		{
			clothingItem.breastJointSpringAndDamperMultiplier = f;
			clothingItem.SyncClothingAdjustments();
		}
	}

	protected void SyncGluteJointSpringAndDamperMultiplier(float f)
	{
		if (clothingItem != null)
		{
			clothingItem.gluteJointSpringAndDamperMultiplier = f;
			clothingItem.SyncClothingAdjustments();
		}
	}

	protected void Init()
	{
		presetManagerControl = GetComponent<PresetManagerControl>();
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (!(t != null))
		{
			return;
		}
		DAZDynamicItemLabelUI componentInChildren = t.GetComponentInChildren<DAZDynamicItemLabelUI>(true);
		if (_clothingItem != null && componentInChildren != null && componentInChildren.label != null)
		{
			componentInChildren.label.text = _clothingItem.displayName;
		}
		DAZClothingItemControlUI componentInChildren2 = t.GetComponentInChildren<DAZClothingItemControlUI>(true);
		if (!(componentInChildren2 != null))
		{
			return;
		}
		if (lockedJSON != null)
		{
			if (componentInChildren2.lockedToggle != null)
			{
				componentInChildren2.lockedToggle.gameObject.SetActive(true);
			}
			lockedJSON.RegisterToggle(componentInChildren2.lockedToggle, isAlt);
		}
		else if (componentInChildren2.lockedToggle != null)
		{
			componentInChildren2.lockedToggle.gameObject.SetActive(false);
		}
		if (disableAnatomyJSON != null)
		{
			if (componentInChildren2.disableAnatomyToggle != null)
			{
				componentInChildren2.disableAnatomyToggle.gameObject.SetActive(true);
			}
			disableAnatomyJSON.RegisterToggle(componentInChildren2.disableAnatomyToggle, isAlt);
		}
		else if (componentInChildren2.disableAnatomyToggle != null)
		{
			componentInChildren2.disableAnatomyToggle.gameObject.SetActive(false);
		}
		if (isRealClothingItemJSON != null)
		{
			if (componentInChildren2.isRealClothingItemToggle != null)
			{
				componentInChildren2.isRealClothingItemToggle.gameObject.SetActive(true);
			}
			isRealClothingItemJSON.RegisterToggle(componentInChildren2.isRealClothingItemToggle, isAlt);
		}
		else if (componentInChildren2.isRealClothingItemToggle != null)
		{
			componentInChildren2.isRealClothingItemToggle.gameObject.SetActive(false);
		}
		if (enableJointSpringAndDamperAdjustJSON != null)
		{
			if (componentInChildren2.enableJointSpringAndDamperAdjustToggle != null)
			{
				componentInChildren2.enableJointSpringAndDamperAdjustToggle.gameObject.SetActive(true);
			}
			enableJointSpringAndDamperAdjustJSON.RegisterToggle(componentInChildren2.enableJointSpringAndDamperAdjustToggle, isAlt);
		}
		else if (componentInChildren2.enableJointSpringAndDamperAdjustToggle != null)
		{
			componentInChildren2.enableJointSpringAndDamperAdjustToggle.gameObject.SetActive(false);
		}
		if (enableBreastJointAdjustJSON != null)
		{
			if (componentInChildren2.enableBreastJointAdjustToggle != null)
			{
				componentInChildren2.enableBreastJointAdjustToggle.gameObject.SetActive(true);
			}
			enableBreastJointAdjustJSON.RegisterToggle(componentInChildren2.enableBreastJointAdjustToggle, isAlt);
		}
		else if (componentInChildren2.enableBreastJointAdjustToggle != null)
		{
			componentInChildren2.enableBreastJointAdjustToggle.gameObject.SetActive(false);
		}
		if (enableGluteJointAdjustJSON != null)
		{
			if (componentInChildren2.enableGluteJointAdjustToggle != null)
			{
				componentInChildren2.enableGluteJointAdjustToggle.gameObject.SetActive(true);
			}
			enableGluteJointAdjustJSON.RegisterToggle(componentInChildren2.enableGluteJointAdjustToggle, isAlt);
		}
		else if (componentInChildren2.enableGluteJointAdjustToggle != null)
		{
			componentInChildren2.enableGluteJointAdjustToggle.gameObject.SetActive(false);
		}
		if (breastJointSpringAndDamperMultiplierJSON != null)
		{
			if (componentInChildren2.breastJointSpringAndDamperMultiplierSlider != null)
			{
				componentInChildren2.breastJointSpringAndDamperMultiplierSlider.transform.parent.gameObject.SetActive(true);
			}
			breastJointSpringAndDamperMultiplierJSON.RegisterSlider(componentInChildren2.breastJointSpringAndDamperMultiplierSlider, isAlt);
		}
		else if (componentInChildren2.breastJointSpringAndDamperMultiplierSlider != null)
		{
			componentInChildren2.breastJointSpringAndDamperMultiplierSlider.transform.parent.gameObject.SetActive(false);
		}
		if (gluteJointSpringAndDamperMultiplierJSON != null)
		{
			if (componentInChildren2.gluteJointSpringAndDamperMultiplierSlider != null)
			{
				componentInChildren2.gluteJointSpringAndDamperMultiplierSlider.transform.parent.gameObject.SetActive(true);
			}
			gluteJointSpringAndDamperMultiplierJSON.RegisterSlider(componentInChildren2.gluteJointSpringAndDamperMultiplierSlider, isAlt);
		}
		else if (componentInChildren2.gluteJointSpringAndDamperMultiplierSlider != null)
		{
			componentInChildren2.gluteJointSpringAndDamperMultiplierSlider.transform.parent.gameObject.SetActive(false);
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
