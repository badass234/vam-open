using UnityEngine.UI;

public class SubSceneUI : UIProvider
{
	public Button beginBrowseButton;

	public Toggle autoSetSubSceneUIDToSignatureOnBrowseLoadToggle;

	public Text packageUidText;

	public Button clearPackageUidButton;

	public InputField creatorNameInputField;

	public Text storedCreatorNameText;

	public Button setToYourCreatorNameButton;

	public InputField signatureInputField;

	public InputField storeNameInputField;

	public Toggle loadOnRestoreFromOtherSubSceneToggle;

	public Button addLooseAtomsToSubSceneButton;

	public Button isolateEditSubSceneButton;

	public Toggle drawContainedAtomsLinesToggle;

	public UIDynamicButton storeSubSceneButton;

	public UIDynamicButton loadSubSceneButton;

	public Button clearSubSceneButton;

	public Button unparentAllAtomsButton;
}
