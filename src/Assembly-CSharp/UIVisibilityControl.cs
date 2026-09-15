using UnityEngine;

public class UIVisibilityControl : JSONStorable
{
	public GameObject objectToControl;

	public JSONStorableBool onlyVisibleWhenMainUIOpenJSON;

	private void Update()
	{
		if (SuperController.singleton != null && objectToControl != null)
		{
			objectToControl.SetActive(!onlyVisibleWhenMainUIOpenJSON.val || SuperController.singleton.MainHUDVisible);
		}
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		base.InitUI(t, isAlt);
		if (t != null)
		{
			UIVisibilityControlUI componentInChildren = t.GetComponentInChildren<UIVisibilityControlUI>(true);
			if (componentInChildren != null)
			{
				onlyVisibleWhenMainUIOpenJSON.RegisterToggle(componentInChildren.onlyVisibleWhenMainUIOpenToggle);
			}
		}
	}

	protected void Init()
	{
		onlyVisibleWhenMainUIOpenJSON = new JSONStorableBool("onlyVisibleWhenMainUIOpen", false);
		RegisterBool(onlyVisibleWhenMainUIOpenJSON);
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
