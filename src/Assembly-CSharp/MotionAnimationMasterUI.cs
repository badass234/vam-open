using UnityEngine;
using UnityEngine.UI;

public class MotionAnimationMasterUI : UIProvider
{
	public Toggle linkToAudioSourceControlToggle;

	public Slider audioSourceTimeOffsetSlider;

	public UIPopup audioSourceControlAtomSelectionPopup;

	public UIPopup audioSourceControlSelectionPopup;

	public Slider playbackCounterSlider;

	public Slider startTimestepSlider;

	public Slider stopTimestepSlider;

	public Slider loopbackTimeSlider;

	public Slider playbackSpeedSlider;

	public Toggle autoPlayToggle;

	public Toggle loopToggle;

	public Toggle autoRecordStopToggle;

	public Toggle showRecordPathsToggle;

	public Toggle showStartMarkersToggle;

	public Button clearAllAnimationButton;

	public Button selectControllersArmedForRecordButton;

	public Button armAllControlledControllersForRecordButton;

	public Button startRecordModeButton;

	public Button startRecordButton;

	public Button stopRecordButton;

	public Button stopRecordModeButton;

	public Button startPlaybackButton;

	public Button stopPlaybackButton;

	public Button trimAnimationButton;

	public Slider desiredLengthSlider;

	public Button setToDesiredLengthButton;

	public Button seekToBeginningButton;

	public Button resetAnimationButton;

	public GameObject advancedPanel;

	public Button openAdvancedPanelButton;

	public Button closeAdvancedPanelButton;

	public Button selectTriggersInTimeRangeButton;

	public Button clearSelectedTriggersButton;

	public Button adjustTimeOfSelectedTriggersButton;

	public Button sortTriggersByStartTimeButton;

	public Button copySelectedTriggersAndPasteToTimeButton;

	public Button copyFromSceneMasterButton;

	public Button copyToSceneMasterButton;

	public Slider triggerSelectFromTimeSlider;

	public Slider triggerSelectToTimeSlider;

	public Slider triggerTimeAdjustmentSlider;

	public Slider triggerPasteToTimeSlider;

	public RectTransform triggerActionsParent;

	public ScrollRectContentManager triggerContentManager;

	public Button addTriggerButton;

	public Button clearAllTriggersButton;
}
