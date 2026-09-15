using UnityEngine;
using UnityEngine.UI;

public class FreeControllerV3GUI : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	protected FreeControllerV3 _controller;

	public FreeControllerV3UI controllerUI;

	[SerializeField]
	[HideInInspector]
	protected MotionAnimationControl _motionController;

	public Text headerText;

	public Text UIDText;

	public ToggleGroupValue positionToggleGroup;

	public ToggleGroupValue rotationToggleGroup;

	public Slider holdPositionSpringSlider;

	public Slider holdPositionDamperSlider;

	public Slider holdPositionMaxForceSlider;

	public Slider holdRotationSpringSlider;

	public Slider holdRotationDamperSlider;

	public Slider holdRotationMaxForceSlider;

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

	public Slider massSlider;

	public Toggle physicsEnabledToggle;

	public Toggle collisionEnabledToggle;

	public Toggle useGravityWhenOffToggle;

	public Toggle interactableInPlayModeToggle;

	public Toggle possessableToggle;

	public Toggle canGrabPositionToggle;

	public Toggle canGrabRotationToggle;

	public SetTextFromFloat xPositionText;

	public Button xPositionMinus1Button;

	public Button xPositionMinusPoint1Button;

	public Button xPositionMinusPoint01Button;

	public Button xPosition0Button;

	public Button xPositionPlusPoint01Button;

	public Button xPositionPlusPoint1Button;

	public Button xPositionPlus1Button;

	public Button xPositionSnapPoint1Button;

	public Toggle xPositionLockToggle;

	public SetTextFromFloat yPositionText;

	public Button yPositionMinus1Button;

	public Button yPositionMinusPoint1Button;

	public Button yPositionMinusPoint01Button;

	public Button yPosition0Button;

	public Button yPositionPlusPoint01Button;

	public Button yPositionPlusPoint1Button;

	public Button yPositionPlus1Button;

	public Button yPositionSnapPoint1Button;

	public Toggle yPositionLockToggle;

	public SetTextFromFloat zPositionText;

	public Button zPositionMinus1Button;

	public Button zPositionMinusPoint1Button;

	public Button zPositionMinusPoint01Button;

	public Button zPosition0Button;

	public Button zPositionPlusPoint01Button;

	public Button zPositionPlusPoint1Button;

	public Button zPositionPlus1Button;

	public Button zPositionSnapPoint1Button;

	public Toggle zPositionLockToggle;

	public SetTextFromFloat xRotationText;

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

	public Button zRotationMinus45Button;

	public Button zRotationMinus5Button;

	public Button zRotationMinusPoint5Button;

	public Button zRotation0Button;

	public Button zRotationPlusPoint5Button;

	public Button zRotationPlus5Button;

	public Button zRotationPlus45Button;

	public Button zRotationSnap1Button;

	public Toggle zRotationLockToggle;

	public Toggle armedForRecordToggle;

	public Toggle playbackEnabledToggle;

	public Button clearAnimationButton;

	public FreeControllerV3 controller
	{
		get
		{
			return _controller;
		}
		set
		{
			if (_controller != value)
			{
				_controller = value;
				ResyncController();
			}
		}
	}

	public MotionAnimationControl motionController
	{
		get
		{
			return _motionController;
		}
		set
		{
			if (_motionController != value)
			{
				_motionController = value;
				ResyncMotionController();
			}
		}
	}

	protected void ResyncController()
	{
		if (_controller != null)
		{
			_controller.linkToSelectionPopup = linkToSelectionPopup;
			_controller.linkToAtomSelectionPopup = linkToAtomSelectionPopup;
			_controller.xPositionText = xPositionText;
			_controller.yPositionText = yPositionText;
			_controller.zPositionText = zPositionText;
			_controller.xRotationText = xRotationText;
			_controller.yRotationText = yRotationText;
			_controller.zRotationText = zRotationText;
			_controller.UITransforms = new Transform[1];
			_controller.UITransforms[0] = base.transform;
			if (UIDText != null)
			{
				UIDText.text = _controller.name;
			}
			_controller.UIDText = UIDText;
			if (headerText != null)
			{
				headerText.text = string.Empty;
			}
		}
		if (controllerUI != null)
		{
			controllerUI.positionToggleGroup = positionToggleGroup;
			controllerUI.rotationToggleGroup = rotationToggleGroup;
			controllerUI.holdPositionSpringSlider = holdPositionSpringSlider;
			controllerUI.holdPositionDamperSlider = holdPositionDamperSlider;
			controllerUI.holdPositionMaxForceSlider = holdPositionMaxForceSlider;
			controllerUI.holdRotationSpringSlider = holdRotationSpringSlider;
			controllerUI.holdRotationDamperSlider = holdRotationDamperSlider;
			controllerUI.holdRotationMaxForceSlider = holdRotationMaxForceSlider;
			controllerUI.linkPositionSpringSlider = linkPositionSpringSlider;
			controllerUI.linkPositionDamperSlider = linkPositionDamperSlider;
			controllerUI.linkPositionMaxForceSlider = linkPositionMaxForceSlider;
			controllerUI.linkRotationSpringSlider = linkRotationSpringSlider;
			controllerUI.linkRotationDamperSlider = linkRotationDamperSlider;
			controllerUI.linkRotationMaxForceSlider = linkRotationMaxForceSlider;
			controllerUI.linkToSelectionPopup = linkToSelectionPopup;
			controllerUI.linkToAtomSelectionPopup = linkToAtomSelectionPopup;
			controllerUI.selectLinkToFromSceneButton = selectLinkToFromSceneButton;
			controllerUI.selectAlignToFromSceneButton = selectAlignToFromSceneButton;
			controllerUI.jointRotationDriveSpringSlider = jointRotationDriveSpringSlider;
			controllerUI.jointRotationDriveDamperSlider = jointRotationDriveDamperSlider;
			controllerUI.jointRotationDriveMaxForceSlider = jointRotationDriveMaxForceSlider;
			controllerUI.jointRotationDriveXTargetSlider = jointRotationDriveXTargetSlider;
			controllerUI.jointRotationDriveYTargetSlider = jointRotationDriveYTargetSlider;
			controllerUI.jointRotationDriveZTargetSlider = jointRotationDriveZTargetSlider;
			controllerUI.massSlider = massSlider;
			controllerUI.physicsEnabledToggle = physicsEnabledToggle;
			controllerUI.onToggle = onToggle;
			controllerUI.collisionEnabledToggle = collisionEnabledToggle;
			controllerUI.useGravityWhenOffToggle = useGravityWhenOffToggle;
			controllerUI.interactableInPlayModeToggle = interactableInPlayModeToggle;
			controllerUI.possessableToggle = possessableToggle;
			controllerUI.canGrabPositionToggle = canGrabPositionToggle;
			controllerUI.canGrabRotationToggle = canGrabRotationToggle;
			controllerUI.xPositionLockToggle = xPositionLockToggle;
			controllerUI.yPositionLockToggle = yPositionLockToggle;
			controllerUI.zPositionLockToggle = zPositionLockToggle;
			controllerUI.xRotationLockToggle = xRotationLockToggle;
			controllerUI.yRotationLockToggle = yRotationLockToggle;
			controllerUI.zRotationLockToggle = zRotationLockToggle;
			controllerUI.xPositionMinus1Button = xPositionMinus1Button;
			controllerUI.xPositionMinusPoint1Button = xPositionMinusPoint1Button;
			controllerUI.xPositionMinusPoint01Button = xPositionMinusPoint01Button;
			controllerUI.xPositionPlusPoint01Button = xPositionPlusPoint01Button;
			controllerUI.xPositionPlusPoint1Button = xPositionPlusPoint1Button;
			controllerUI.xPositionPlus1Button = xPositionPlus1Button;
			controllerUI.xPosition0Button = xPosition0Button;
			controllerUI.xPositionText = xPositionText;
			controllerUI.xPositionSnapPoint1Button = xPositionSnapPoint1Button;
			controllerUI.yPositionMinus1Button = yPositionMinus1Button;
			controllerUI.yPositionMinusPoint1Button = yPositionMinusPoint1Button;
			controllerUI.yPositionMinusPoint01Button = yPositionMinusPoint01Button;
			controllerUI.yPositionPlusPoint01Button = yPositionPlusPoint01Button;
			controllerUI.yPositionPlusPoint1Button = yPositionPlusPoint1Button;
			controllerUI.yPositionPlus1Button = yPositionPlus1Button;
			controllerUI.yPosition0Button = yPosition0Button;
			controllerUI.yPositionText = yPositionText;
			controllerUI.yPositionSnapPoint1Button = yPositionSnapPoint1Button;
			controllerUI.zPositionMinus1Button = zPositionMinus1Button;
			controllerUI.zPositionMinusPoint1Button = zPositionMinusPoint1Button;
			controllerUI.zPositionMinusPoint01Button = zPositionMinusPoint01Button;
			controllerUI.zPositionPlusPoint01Button = zPositionPlusPoint01Button;
			controllerUI.zPositionPlusPoint1Button = zPositionPlusPoint1Button;
			controllerUI.zPositionPlus1Button = zPositionPlus1Button;
			controllerUI.zPosition0Button = zPosition0Button;
			controllerUI.zPositionText = zPositionText;
			controllerUI.zPositionSnapPoint1Button = zPositionSnapPoint1Button;
			controllerUI.xRotationMinus45Button = xRotationMinus45Button;
			controllerUI.xRotationMinus5Button = xRotationMinus5Button;
			controllerUI.xRotationMinusPoint5Button = xRotationMinusPoint5Button;
			controllerUI.xRotationPlusPoint5Button = xRotationPlusPoint5Button;
			controllerUI.xRotationPlus5Button = xRotationPlus5Button;
			controllerUI.xRotationPlus45Button = xRotationPlus45Button;
			controllerUI.xRotation0Button = xRotation0Button;
			controllerUI.xRotationText = xRotationText;
			controllerUI.xRotationSnap1Button = xRotationSnap1Button;
			controllerUI.yRotationMinus45Button = yRotationMinus45Button;
			controllerUI.yRotationMinus5Button = yRotationMinus5Button;
			controllerUI.yRotationMinusPoint5Button = yRotationMinusPoint5Button;
			controllerUI.yRotationPlusPoint5Button = yRotationPlusPoint5Button;
			controllerUI.yRotationPlus5Button = yRotationPlus5Button;
			controllerUI.yRotationPlus45Button = yRotationPlus45Button;
			controllerUI.yRotation0Button = yRotation0Button;
			controllerUI.yRotationText = yRotationText;
			controllerUI.yRotationSnap1Button = yRotationSnap1Button;
			controllerUI.zRotationMinus45Button = zRotationMinus45Button;
			controllerUI.zRotationMinus5Button = zRotationMinus5Button;
			controllerUI.zRotationMinusPoint5Button = zRotationMinusPoint5Button;
			controllerUI.zRotationPlusPoint5Button = zRotationPlusPoint5Button;
			controllerUI.zRotationPlus5Button = zRotationPlus5Button;
			controllerUI.zRotationPlus45Button = zRotationPlus45Button;
			controllerUI.zRotation0Button = zRotation0Button;
			controllerUI.zRotationText = zRotationText;
			controllerUI.zRotationSnap1Button = zRotationSnap1Button;
			controllerUI.UIDText = UIDText;
		}
	}

	protected void ResyncMotionController()
	{
		if (_motionController != null && _controller != null)
		{
			_motionController.overrideId = _controller.name + "Animation";
		}
	}

	public void ResyncAll()
	{
		ResyncController();
		ResyncMotionController();
	}
}
