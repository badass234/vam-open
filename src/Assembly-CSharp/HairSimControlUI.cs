using UnityEngine;
using UnityEngine.UI;

public class HairSimControlUI : UIProvider
{
	public Button restoreAllFromDefaultsButton;

	public Button saveToStore1Button;

	public Button restoreFromStore1Button;

	public Button resetAndStartStyleModeButton;

	public Button startStyleModeButton;

	public Button cancelStyleModeButton;

	public Button keepStyleButton;

	public Toggle styleModelAllowControlOtherNodesToggle;

	public Slider styleJointsSearchDistanceSlider;

	public Button rebuildStyleJointsButton;

	public Button clearStyleJointsButton;

	public Slider styleModeGravityMultiplierSlider;

	public Slider styleModeCollisionRadiusSlider;

	public Slider styleModeCollisionRadiusRootSlider;

	public Toggle styleModeShowCurlsToggle;

	public Slider styleModeUpHairPullStrengthSlider;

	public Text simNearbyJointCountText;

	public Text styleStatusText;

	public Transform styleModePanel;

	public Toggle styleModeShowTool1Toggle;

	public Toggle styleModeShowTool2Toggle;

	public Toggle styleModeShowTool3Toggle;

	public Toggle styleModeShowTool4Toggle;

	public Button copyPhysicsParametersButton;

	public Button pastePhysicsParametersButton;

	public Button undoPastePhysicsParametersButton;

	public Toggle simulationEnabledToggle;

	public Toggle collisionEnabledToggle;

	public Slider collisionRadiusSlider;

	public Slider collisionRadiusRootSlider;

	public Slider dragSlider;

	public Slider weightSlider;

	public Toggle usePaintedRigidityToggle;

	public RectTransform paintedRigidityIndicatorPanel;

	public Slider rootRigiditySlider;

	public Slider mainRigiditySlider;

	public Slider tipRigiditySlider;

	public Slider rigidityRolloffPowerSlider;

	public Slider jointRigiditySlider;

	public Slider frictionSlider;

	public Slider gravityMultiplierSlider;

	public Slider iterationsSlider;

	public Slider clingSlider;

	public Slider clingRolloffSlider;

	public Slider snapSlider;

	public Slider bendResistanceSlider;

	public Slider windXSlider;

	public Slider windYSlider;

	public Slider windZSlider;

	public Button copyLightingParametersButton;

	public Button pasteLightingParametersButton;

	public Button undoPasteLightingParametersButton;

	public UIPopup shaderTypePopup;

	public HSVColorPicker rootColorPicker;

	public HSVColorPicker tipColorPicker;

	public HSVColorPicker specularColorPicker;

	public Slider colorRolloffSlider;

	public Slider diffuseSoftnessSlider;

	public Slider primarySpecularSharpnessSlider;

	public Slider secondarySpecularSharpnessSlider;

	public Slider specularShiftSlider;

	public Slider fresnelPowerSlider;

	public Slider fresnelAttenuationSlider;

	public Slider randomColorPowerSlider;

	public Slider randomColorOffsetSlider;

	public Slider IBLFactorSlider;

	public Slider normalRandomizeSlider;

	public Button copyLookParametersButton;

	public Button pasteLookParametersButton;

	public Button undoPasteLookParametersButton;

	public Slider curlXSlider;

	public Slider curlYSlider;

	public Slider curlZSlider;

	public Slider curlScaleSlider;

	public Slider curlScaleRandomnessSlider;

	public Slider curlFrequencySlider;

	public Slider curlFrequencyRandomnessSlider;

	public Toggle curlAllowReverseToggle;

	public Toggle curlAllowFlipAxisToggle;

	public Slider curlNormalAdjustSlider;

	public Slider curlRootSlider;

	public Slider curlMidSlider;

	public Slider curlTipSlider;

	public Slider curlMidpointSlider;

	public Slider curlCurvePowerSlider;

	public Slider length1Slider;

	public Slider length2Slider;

	public Slider length3Slider;

	public Slider maxSpreadSlider;

	public Slider spreadRootSlider;

	public Slider spreadMidSlider;

	public Slider spreadTipSlider;

	public Slider spreadMidpointSlider;

	public Slider spreadCurvePowerSlider;

	public Slider widthSlider;

	public Slider densitySlider;

	public Slider detailSlider;
}
