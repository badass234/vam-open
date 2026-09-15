public class InteractionControl : JSONStorable
{
	protected JSONStorableAction selectModeFreeMoveMouseAction;

	protected JSONStorableAction selectModeOffAction;

	protected JSONStorableAction resetNavigationRigPositionRotationAction;

	protected JSONStorableAction moveToSceneLoadPositionAction;

	protected void SelectModeFreeMoveMouse()
	{
		if (SuperController.singleton != null)
		{
			if (SuperController.singleton.gameMode == SuperController.GameMode.Play)
			{
				SuperController.singleton.SelectModeFreeMoveMouse();
			}
			else
			{
				SuperController.singleton.Error("Trigger tried to set mode to free mouse look while in edit game mode. This trigger only works in play mode");
			}
		}
	}

	protected void SelectModeOff()
	{
		if (SuperController.singleton != null)
		{
			if (SuperController.singleton.gameMode == SuperController.GameMode.Play)
			{
				SuperController.singleton.SelectModeOff();
			}
			else
			{
				SuperController.singleton.Error("Trigger tried to set mode to off while in edit game mode. This trigger only works in play mode");
			}
		}
	}

	protected void ResetNavigationRigPositionRotation()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.ResetNavigationRigPositionRotation();
		}
	}

	protected void MoveToSceneLoadPosition()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.MoveToSceneLoadPosition();
		}
	}

	protected void Init()
	{
		selectModeFreeMoveMouseAction = new JSONStorableAction("SelectModeFreeMoveMouse", SelectModeFreeMoveMouse);
		RegisterAction(selectModeFreeMoveMouseAction);
		selectModeOffAction = new JSONStorableAction("SelectModeOff", SelectModeOff);
		RegisterAction(selectModeOffAction);
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
