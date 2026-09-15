public class SetDAZMorphControl : JSONStorable
{
	public JSONStorableBool enabledJSON;

	protected void SyncEnabled(bool b)
	{
		SetDAZMorph[] components = GetComponents<SetDAZMorph>();
		SetDAZMorph[] array = components;
		foreach (SetDAZMorph setDAZMorph in array)
		{
			setDAZMorph.enabled = b;
		}
	}

	protected void Init()
	{
		enabledJSON = new JSONStorableBool("enabled", true, SyncEnabled);
		RegisterBool(enabledJSON);
	}

	public override void InitUI()
	{
		if (UITransform != null)
		{
			SetDAZMorphControlUI componentInChildren = UITransform.GetComponentInChildren<SetDAZMorphControlUI>(true);
			if (componentInChildren != null && enabledJSON != null)
			{
				enabledJSON.toggle = componentInChildren.enabledToggle;
			}
		}
	}

	public override void InitUIAlt()
	{
		if (UITransformAlt != null)
		{
			SetDAZMorphControlUI componentInChildren = UITransformAlt.GetComponentInChildren<SetDAZMorphControlUI>(true);
			if (componentInChildren != null && enabledJSON != null)
			{
				enabledJSON.toggleAlt = componentInChildren.enabledToggle;
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
