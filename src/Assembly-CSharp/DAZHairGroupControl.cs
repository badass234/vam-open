using System.Collections.Generic;
using MeshVR;
using UnityEngine;

public class DAZHairGroupControl : JSONStorable
{
	protected DAZHairGroup _hairItem;

	protected PresetManagerControl presetManagerControl;

	public JSONStorableBool lockedJSON;

	public JSONStorableBool disableAnatomyJSON;

	public DAZHairGroup hairItem
	{
		get
		{
			return _hairItem;
		}
		set
		{
			if (_hairItem != value)
			{
				_hairItem = value;
				SyncHairGroup();
			}
		}
	}

	protected void SyncHairGroup()
	{
		if (_hairItem != null)
		{
			lockedJSON = new JSONStorableBool("locked", _hairItem.locked, SyncLocked);
			lockedJSON.isStorable = false;
			lockedJSON.isRestorable = false;
			RegisterBool(lockedJSON);
			disableAnatomyJSON = new JSONStorableBool("disableAnatomy", _hairItem.disableAnatomy, SyncDisableAnatomy);
			RegisterBool(disableAnatomyJSON);
		}
	}

	public void Delete()
	{
		if (hairItem != null)
		{
			hairItem.transform.SetParent(null);
			Object.Destroy(hairItem.gameObject);
		}
	}

	public void RefreshHairItemThumbnail(string dynamicItemPath)
	{
		if (_hairItem != null)
		{
			_hairItem.RefreshHairItemThumbnail(dynamicItemPath);
		}
	}

	public void RefreshHairItems()
	{
		if (_hairItem != null)
		{
			_hairItem.RefreshHairItems();
		}
	}

	public bool IsHairItemUIDAvailable(string uid)
	{
		if (_hairItem != null)
		{
			return _hairItem.IsHairUIDAvailable(uid);
		}
		return false;
	}

	public HashSet<string> GetAllHairOtherTags()
	{
		if (_hairItem != null)
		{
			return _hairItem.GetAllHairOtherTags();
		}
		return null;
	}

	protected void SyncLocked(bool b)
	{
		if (_hairItem != null)
		{
			_hairItem.locked = b;
		}
		if (presetManagerControl != null)
		{
			presetManagerControl.lockParams = b;
		}
	}

	protected void SyncDisableAnatomy(bool b)
	{
		if (_hairItem != null)
		{
			_hairItem.disableAnatomy = b;
			_hairItem.SyncHairAdjustments();
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
		if (_hairItem != null && componentInChildren != null && componentInChildren.label != null)
		{
			componentInChildren.label.text = _hairItem.displayName;
		}
		ObjectChoice[] componentsInChildren = GetComponentsInChildren<ObjectChoice>(true);
		ObjectChoice[] array = componentsInChildren;
		foreach (ObjectChoice objectChoice in array)
		{
			JSONStorable[] componentsInChildren2 = objectChoice.GetComponentsInChildren<JSONStorable>(true);
			JSONStorable[] array2 = componentsInChildren2;
			foreach (JSONStorable jSONStorable in array2)
			{
				if (isAlt)
				{
					jSONStorable.SetUIAlt(t);
				}
				else
				{
					jSONStorable.SetUI(t);
				}
			}
		}
		DAZHairGroupControlUI componentInChildren2 = t.GetComponentInChildren<DAZHairGroupControlUI>(true);
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
