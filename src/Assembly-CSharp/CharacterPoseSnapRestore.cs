using UnityEngine;

public class CharacterPoseSnapRestore : JSONStorable
{
	public DAZBones dazBones;

	protected JSONStorableBool enabledJSON;

	public void ForceSnapRestore()
	{
		if (!base.physicalLocked)
		{
			if (dazBones != null)
			{
				dazBones.SnapAllBonesToControls();
			}
			containingAtom.ResetPhysics(false);
		}
	}

	public void ResetPhysicsOnly()
	{
		containingAtom.ResetPhysics(false);
	}

	public void SnapRestore()
	{
		if (!base.physicalLocked && enabledJSON != null && enabledJSON.val)
		{
			if (dazBones != null)
			{
				dazBones.SnapAllBonesToControls();
			}
			containingAtom.ResetPhysics(false);
		}
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (t != null)
		{
			CharacterPoseSnapRestoreUI componentInChildren = t.GetComponentInChildren<CharacterPoseSnapRestoreUI>(true);
			if (componentInChildren != null)
			{
				enabledJSON.toggle = componentInChildren.enabledToggle;
			}
		}
	}

	protected void Init()
	{
		enabledJSON = new JSONStorableBool("enabled", true);
		enabledJSON.isStorable = false;
		enabledJSON.isRestorable = false;
		RegisterBool(enabledJSON);
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
