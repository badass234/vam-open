using UnityEngine.UI;

public class FreeControllerV3UI : UIProvider
{
	public Button resetButton;

	public Button selectRootButton;

	public Text UIDText;

	public ToggleGroupValue positionToggleGroup;

	public ToggleGroupValue rotationToggleGroup;

	public Slider holdPositionSpringSlider;

	public Slider holdPositionDamperSlider;

	public Slider holdPositionMaxForceSlider;

	public Slider holdRotationSpringSlider;

	public Slider holdRotationDamperSlider;

	public Slider holdRotationMaxForceSlider;

	public Slider complyPositionSpringSlider;

	public Slider complyPositionDamperSlider;

	public Slider complyRotationSpringSlider;

	public Slider complyRotationDamperSlider;

	public Slider complyJointRotationDriveSpringSlider;

	public Slider complyPositionThresholdSlider;

	public Slider complyRotationThresholdSlider;

	public Slider complySpeedSlider;

	public Slider linkPositionSpringSlider;

	public Slider linkPositionDamperSlider;

	public Slider linkPositionMaxForceSlider;

	public Slider linkRotationSpringSlider;

	public Slider linkRotationDamperSlider;

	public Slider linkRotationMaxForceSlider;

	public UIPopup linkToSelectionPopup;

	public UIPopup linkToAtomSelectionPopup;

	public Slider jointRotationDriveSpringSlider;

	public Slider jointRotationDriveDamperSlider;

	public Slider jointRotationDriveMaxForceSlider;

	public Slider jointRotationDriveXTargetSlider;

	public Slider jointRotationDriveYTargetSlider;

	public Slider jointRotationDriveZTargetSlider;

	public Button selectLinkToFromSceneButton;

	public Button selectAlignToFromSceneButton;

	public Toggle onToggle;

	public Toggle detachControlToggle;

	public Slider massSlider;

	public Slider dragSlider;

	public Toggle maxVelocityEnableToggle;

	public Slider maxVelocitySlider;

	public Slider angularDragSlider;

	public Toggle physicsEnabledToggle;

	public Toggle collisionEnabledToggle;

	public Toggle useGravityWhenOffToggle;

	public Toggle interactableInPlayModeToggle;

	public Toggle deactivateOtherControlsOnPossessToggle;

	public Text deactivateOtherControlsListText;

	public Toggle possessableToggle;

	public Toggle canGrabPositionToggle;

	public Toggle canGrabRotationToggle;

	public Toggle freezeAtomPhysicsWhenGrabbedToggle;

	public SetTextFromFloat xPositionText;

	public InputField xPositionInputField;

	public InputFieldAction xPositionInputFieldAction;

	public Button xPositionMinus1Button;

	public Button xPositionMinusPoint1Button;

	public Button xPositionMinusPoint01Button;

	public Button xPosition0Button;

	public Button xPositionPlusPoint01Button;

	public Button xPositionPlusPoint1Button;

	public Button xPositionPlus1Button;

	public Button xPositionSnapPoint1Button;

	public Toggle xPositionLockToggle;

	public Toggle xPositionLocalLockToggle;

	public SetTextFromFloat yPositionText;

	public InputField yPositionInputField;

	public InputFieldAction yPositionInputFieldAction;

	public Button yPositionMinus1Button;

	public Button yPositionMinusPoint1Button;

	public Button yPositionMinusPoint01Button;

	public Button yPosition0Button;

	public Button yPositionPlusPoint01Button;

	public Button yPositionPlusPoint1Button;

	public Button yPositionPlus1Button;

	public Button yPositionSnapPoint1Button;

	public Toggle yPositionLockToggle;

	public Toggle yPositionLocalLockToggle;

	public SetTextFromFloat zPositionText;

	public InputField zPositionInputField;

	public InputFieldAction zPositionInputFieldAction;

	public Button zPositionMinus1Button;

	public Button zPositionMinusPoint1Button;

	public Button zPositionMinusPoint01Button;

	public Button zPosition0Button;

	public Button zPositionPlusPoint01Button;

	public Button zPositionPlusPoint1Button;

	public Button zPositionPlus1Button;

	public Button zPositionSnapPoint1Button;

	public Toggle zPositionLockToggle;

	public Toggle zPositionLocalLockToggle;

	public SetTextFromFloat xRotationText;

	public InputField xRotationInputField;

	public InputFieldAction xRotationInputFieldAction;

	public Button xRotationMinus45Button;

	public Button xRotationMinus5Button;

	public Button xRotationMinusPoint5Button;

	public Button xRotation0Button;

	public Button xRotationPlusPoint5Button;

	public Button xRotationPlus5Button;

	public Button xRotationPlus45Button;

	public Button xRotationSnap1Button;

	public Toggle xRotationLockToggle;

	public SetTextFromFloat yRotationText;

	public InputField yRotationInputField;

	public InputFieldAction yRotationInputFieldAction;

	public Button yRotationMinus45Button;

	public Button yRotationMinus5Button;

	public Button yRotationMinusPoint5Button;

	public Button yRotation0Button;

	public Button yRotationPlusPoint5Button;

	public Button yRotationPlus5Button;

	public Button yRotationPlus45Button;

	public Button yRotationSnap1Button;

	public Toggle yRotationLockToggle;

	public SetTextFromFloat zRotationText;

	public InputField zRotationInputField;

	public InputFieldAction zRotationInputFieldAction;

	public Button zRotationMinus45Button;

	public Button zRotationMinus5Button;

	public Button zRotationMinusPoint5Button;

	public Button zRotation0Button;

	public Button zRotationPlusPoint5Button;

	public Button zRotationPlus5Button;

	public Button zRotationPlus45Button;

	public Button zRotationSnap1Button;

	public Toggle zRotationLockToggle;

	public SetTextFromFloat xLocalPositionText;

	public InputField xLocalPositionInputField;

	public InputFieldAction xLocalPositionInputFieldAction;

	public SetTextFromFloat yLocalPositionText;

	public InputField yLocalPositionInputField;

	public InputFieldAction yLocalPositionInputFieldAction;

	public SetTextFromFloat zLocalPositionText;

	public InputField zLocalPositionInputField;

	public InputFieldAction zLocalPositionInputFieldAction;

	public SetTextFromFloat xLocalRotationText;

	public InputField xLocalRotationInputField;

	public InputFieldAction xLocalRotationInputFieldAction;

	public SetTextFromFloat yLocalRotationText;

	public InputField yLocalRotationInputField;

	public InputFieldAction yLocalRotationInputFieldAction;

	public SetTextFromFloat zLocalRotationText;

	public InputField zLocalRotationInputField;

	public InputFieldAction zLocalRotationInputFieldAction;

	public Button zeroXLocalPositionButton;

	public Button zeroYLocalPositionButton;

	public Button zeroZLocalPositionButton;

	public Button zeroXLocalRotationButton;

	public Button zeroYLocalRotationButton;

	public Button zeroZLocalRotationButton;

	public InputField xSelfRelativePositionAdjustInputField;

	public InputFieldAction xSelfRelativePositionAdjustInputFieldAction;

	public Button xSelfRelativePositionMinus1Button;

	public Button xSelfRelativePositionMinusPoint1Button;

	public Button xSelfRelativePositionMinusPoint01Button;

	public Button xSelfRelativePositionPlusPoint01Button;

	public Button xSelfRelativePositionPlusPoint1Button;

	public Button xSelfRelativePositionPlus1Button;

	public InputField ySelfRelativePositionAdjustInputField;

	public InputFieldAction ySelfRelativePositionAdjustInputFieldAction;

	public Button ySelfRelativePositionMinus1Button;

	public Button ySelfRelativePositionMinusPoint1Button;

	public Button ySelfRelativePositionMinusPoint01Button;

	public Button ySelfRelativePositionPlusPoint01Button;

	public Button ySelfRelativePositionPlusPoint1Button;

	public Button ySelfRelativePositionPlus1Button;

	public InputField zSelfRelativePositionAdjustInputField;

	public InputFieldAction zSelfRelativePositionAdjustInputFieldAction;

	public Button zSelfRelativePositionMinus1Button;

	public Button zSelfRelativePositionMinusPoint1Button;

	public Button zSelfRelativePositionMinusPoint01Button;

	public Button zSelfRelativePositionPlusPoint01Button;

	public Button zSelfRelativePositionPlusPoint1Button;

	public Button zSelfRelativePositionPlus1Button;

	public InputField xSelfRelativeRotationAdjustInputField;

	public InputFieldAction xSelfRelativeRotationAdjustInputFieldAction;

	public Button xSelfRelativeRotationMinus45Button;

	public Button xSelfRelativeRotationMinus5Button;

	public Button xSelfRelativeRotationMinusPoint5Button;

	public Button xSelfRelativeRotationPlusPoint5Button;

	public Button xSelfRelativeRotationPlus5Button;

	public Button xSelfRelativeRotationPlus45Button;

	public InputField ySelfRelativeRotationAdjustInputField;

	public InputFieldAction ySelfRelativeRotationAdjustInputFieldAction;

	public Button ySelfRelativeRotationMinus45Button;

	public Button ySelfRelativeRotationMinus5Button;

	public Button ySelfRelativeRotationMinusPoint5Button;

	public Button ySelfRelativeRotationPlusPoint5Button;

	public Button ySelfRelativeRotationPlus5Button;

	public Button ySelfRelativeRotationPlus45Button;

	public InputField zSelfRelativeRotationAdjustInputField;

	public InputFieldAction zSelfRelativeRotationAdjustInputFieldAction;

	public Button zSelfRelativeRotationMinus45Button;

	public Button zSelfRelativeRotationMinus5Button;

	public Button zSelfRelativeRotationMinusPoint5Button;

	public Button zSelfRelativeRotationPlusPoint5Button;

	public Button zSelfRelativeRotationPlus5Button;

	public Button zSelfRelativeRotationPlus45Button;

	public UIPopup positionGridModePopup;

	public Slider positionGridSlider;

	public UIPopup rotationGridModePopup;

	public Slider rotationGridSlider;
}
