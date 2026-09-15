using UnityEngine.UI;

public class ForceProducerV2UI : UIProvider
{
	public Toggle onToggle;

	public Toggle useForceToggle;

	public Toggle useTorqueToggle;

	public Slider forceFactorSlider;

	public Slider torqueFactorSlider;

	public Slider maxForceSlider;

	public Slider maxTorqueSlider;

	public Slider forceQuicknessSlider;

	public Slider torqueQuicknessSlider;

	public Button selectReceiverAtomFromSceneButton;

	public UIPopup receiverAtomSelectionPopup;

	public UIPopup receiverSelectionPopup;
}
