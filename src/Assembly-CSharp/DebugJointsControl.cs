using UnityEngine;

public class DebugJointsControl : JSONStorable
{
	public DebugJoints debugJoints;

	protected JSONStorableBool debugJointsJSON;

	protected void SyncDebugJoints(bool b)
	{
		if (debugJoints != null)
		{
			debugJoints.showJoints = b;
		}
	}

	protected void Init()
	{
		debugJointsJSON = new JSONStorableBool("debugJoints", false, SyncDebugJoints);
		debugJointsJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(debugJointsJSON);
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (t != null)
		{
			DebugJointsControlUI componentInChildren = t.GetComponentInChildren<DebugJointsControlUI>(true);
			if (componentInChildren != null)
			{
				debugJointsJSON.RegisterToggle(componentInChildren.debugJointsToggle, isAlt);
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
