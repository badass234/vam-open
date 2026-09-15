using UnityEngine;

public class DAZDynamicItemLabel : JSONStorable
{
	protected void Init()
	{
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (t != null)
		{
			DAZDynamicItem componentInParent = GetComponentInParent<DAZDynamicItem>();
			DAZDynamicItemLabelUI componentInChildren = t.GetComponentInChildren<DAZDynamicItemLabelUI>(true);
			if (componentInParent != null && componentInChildren != null && componentInChildren.label != null)
			{
				componentInChildren.label.text = componentInParent.displayName;
			}
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
