using UnityEngine;
using UnityEngine.UI;

public class AnimationPatternUI : UIProvider
{
	public Toggle onToggle;

	public Toggle autoPlayToggle;

	public Toggle pauseToggle;

	public Toggle loopToggle;

	public Toggle loopOnceToggle;

	public UIPopup curveTypeSelector;

	public Slider speedSlider;

	public Slider currentTimeSlider;

	public Toggle autoSyncStepNamesToggle;

	public Toggle hideCurveUnlessSelectedToggle;

	public RectTransform triggerActionsParent;

	public ScrollRectContentManager triggerContentManager;

	public Button addTriggerButton;

	public Button clearAllTriggersButton;

	public Button createStepAtEndButton;

	public Button resetAnimationButton;

	public Button playButton;

	public Button hideAllStepsButton;

	public Button unhideAllStepsButton;

	public Button parentAllStepsButton;

	public Button unparentAllStepsButton;
}
