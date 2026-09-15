using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class TriggerActionDiscreteUI : TriggerActionUI
{
	public Button copyButton;

	public Button pasteButton;

	public Button testButton;

	public Button resetTestButton;

	public RectTransform audioClipPopupsContainer;

	public UIPopup audioClipTypePopup;

	public UIPopup audioClipCategoryPopup;

	public UIPopup audioClipPopup;

	public UIPopup stringChooserActionPopup;

	public Button chooseSceneFilePathButton;

	public Button choosePresetFilePathButton;

	public Text sceneFilePathText;

	public Text presetFilePathText;

	public Toggle boolValueToggle;

	public UIDynamicSlider floatValueDynamicSlider;

	public Slider floatValueSlider;

	public Toggle useTimerToggle;

	public RectTransform curveContainer;

	public UILineRenderer curveLineRenderer;

	public Slider timerLengthSlider;

	public UIPopup timerTypePopup;

	public Toggle useSecondTimerPointToggle;

	public UIDynamicSlider secondTimerPointValueDynamicSlider;

	public Slider secondTimerPointValueSlider;

	public Slider secondTimerPointCurveLocationSlider;

	public InputField stringValueField;

	public InputFieldAction stringValueFieldAction;

	public RectTransform colorPickerContainer;

	public HSVColorPicker colorPicker;

	public UIPopup stringChooserValuePopup;
}
