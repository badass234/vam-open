public class PossessControl : JSONStorable
{
	protected JSONStorableAction startPossessAndAlignJSON;

	protected JSONStorableAction startTwoStagePossessJSON;

	protected JSONStorableAction startTwoStagePossessNoClearJSON;

	protected JSONStorableAction startHandPossessJSON;

	protected JSONStorableAction stopHandPossessJSON;

	protected JSONStorableAction stopAllPossessJSON;

	protected JSONStorableAction startUnpossessJSON;

	protected void StartPossessAndAlign()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.SelectModePossessAndAlign();
		}
	}

	protected void StartTwoStagePossess()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.SelectModeTwoStagePossess();
		}
	}

	protected void StartTwoStagePossessNoClear()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.SelectModeTwoStagePossessNoClear();
		}
	}

	protected void StartHandPossess()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.SelectModePossess(true);
		}
	}

	protected void StopHandPossess()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.ClearPossess(true);
		}
	}

	protected void StopAllPossess()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.ClearPossess();
		}
	}

	protected void StartUnpossess()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.SelectModeUnpossess();
		}
	}

	protected void Init()
	{
		startPossessAndAlignJSON = new JSONStorableAction("StartPossessAndAlign", StartPossessAndAlign);
		RegisterAction(startPossessAndAlignJSON);
		startTwoStagePossessJSON = new JSONStorableAction("StartTwoStagePossess", StartTwoStagePossess);
		RegisterAction(startTwoStagePossessJSON);
		startTwoStagePossessNoClearJSON = new JSONStorableAction("StartTwoStagePossessNoClear", StartTwoStagePossessNoClear);
		RegisterAction(startTwoStagePossessNoClearJSON);
		startHandPossessJSON = new JSONStorableAction("StartHandPossess", StartHandPossess);
		RegisterAction(startHandPossessJSON);
		stopHandPossessJSON = new JSONStorableAction("StopHandPossess", StopHandPossess);
		RegisterAction(stopHandPossessJSON);
		stopAllPossessJSON = new JSONStorableAction("StopAllPossess", StopAllPossess);
		RegisterAction(stopAllPossessJSON);
		startUnpossessJSON = new JSONStorableAction("StartUnpossess", StartUnpossess);
		RegisterAction(startUnpossessJSON);
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
