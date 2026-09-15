using UnityEngine;

public class PlayModeUI : JSONStorable
{
	public Transform uiRoot;

	public FreeControllerV3 uiController;

	public UIVisibility visibilityControl;

	public MoveAndRotateAsHUDAnchor anchorMover;

	public Canvas canvas;

	public bool startingEnabled;

	protected JSONStorableBool enabledJSON;

	public bool startingAlwaysVisible;

	protected JSONStorableBool alwaysVisibleJSON;

	public bool startingUseWorldPlacement;

	protected JSONStorableBool useWorldPlacementJSON;

	protected void SyncEnabled(bool b)
	{
		if (uiRoot != null)
		{
			uiRoot.gameObject.SetActive(b);
		}
	}

	protected void SyncAlwaysVisible(bool b)
	{
		if (!(visibilityControl != null))
		{
			return;
		}
		if (b)
		{
			if (useWorldPlacementJSON == null || !useWorldPlacementJSON.val)
			{
				visibilityControl.keepVisible = b;
			}
			else
			{
				visibilityControl.keepVisible = false;
			}
		}
		else
		{
			visibilityControl.keepVisible = false;
		}
	}

	protected void SyncUseWorldPlacement(bool b)
	{
		if (anchorMover != null)
		{
			anchorMover.enabled = b;
		}
		if (uiController != null)
		{
			uiController.enabled = !b;
		}
	}

	protected void Init()
	{
		enabledJSON = new JSONStorableBool("enabled", startingEnabled, SyncEnabled);
		RegisterBool(enabledJSON);
		alwaysVisibleJSON = new JSONStorableBool("alwaysVisible", startingAlwaysVisible, SyncAlwaysVisible);
		RegisterBool(alwaysVisibleJSON);
		useWorldPlacementJSON = new JSONStorableBool("useWorldPlacement", startingUseWorldPlacement, SyncUseWorldPlacement);
		RegisterBool(useWorldPlacementJSON);
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
		}
	}
}
