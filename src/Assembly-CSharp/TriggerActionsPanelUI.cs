using UnityEngine.UI;

public class TriggerActionsPanelUI : UIProvider
{
	public Text triggerDisplayNameText;

	public Button closeTriggerActionsPanelButton;

	public Button clearActionsButtons;

	public Button addDiscreteActionStartButton;

	public Button addTransitionActionButton;

	public Button addDiscreteActionEndButton;

	public Button copyDiscreteActionsStartButton;

	public Button pasteDiscreteActionsStartButton;

	public Button copyTransitionActionsButton;

	public Button pasteTransitionActionsButton;

	public Button copyDiscreteActionsEndButton;

	public Button pasteDiscreteActionsEndButton;

	public ScrollRectContentManager discreteActionsStartContentManager;

	public ScrollRectContentManager transitionActionsContentManager;

	public ScrollRectContentManager discreteActionsEndContentManager;
}
