using UnityEngine.UI;

public class AtomUI : UIProvider
{
	public Toggle onToggle;

	public Toggle hiddenToggle;

	public Toggle collisionEnabledToggle;

	public Toggle freezePhysicsToggle;

	public Button resetPhysicsButton;

	public Slider resetPhysicsProgressSlider;

	public Button selectAtomParentFromSceneButton;

	public UIPopup parentAtomSelectionPopup;

	public InputField idText;

	public InputFieldAction idTextAction;

	public Text descriptionText;

	public Button savePresetButton;

	public Button saveAppearancePresetButton;

	public Button savePhysicalPresetButton;

	public Button loadPresetButton;

	public Button loadAppearancePresetButton;

	public Button loadPhysicalPresetButton;

	public Button resetButton;

	public Button resetPhysicalButton;

	public Button resetAppearanceButton;

	public Button removeButton;

	public Toggle keepParamLocksWhenPuttingBackInPoolToggle;
}
