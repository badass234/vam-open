using UnityEngine;

public class DAZPhysicsMeshesEnable : JSONStorable
{
	public DAZPhysicsMesh[] physicsMeshesToControl;

	protected bool _enabled = true;

	public JSONStorableBool enabledJSON;

	protected void SyncEnabled(bool b)
	{
		_enabled = b;
		if (physicsMeshesToControl != null)
		{
			DAZPhysicsMesh[] array = physicsMeshesToControl;
			foreach (DAZPhysicsMesh dAZPhysicsMesh in array)
			{
				dAZPhysicsMesh.alternateOn = _enabled;
			}
		}
	}

	protected void Init()
	{
		enabledJSON = new JSONStorableBool("enabled", _enabled, SyncEnabled);
		RegisterBool(enabledJSON);
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		base.InitUI(t, isAlt);
		if (t != null)
		{
			DAZPhysicsMeshesEnableUI componentInChildren = t.GetComponentInChildren<DAZPhysicsMeshesEnableUI>(true);
			if (componentInChildren != null)
			{
				enabledJSON.RegisterToggle(componentInChildren.enabledToggle, isAlt);
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
