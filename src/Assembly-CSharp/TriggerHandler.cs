using UnityEngine;

public interface TriggerHandler
{
	void RemoveTrigger(Trigger t);

	void DuplicateTrigger(Trigger t);

	RectTransform CreateTriggerActionsUI();

	RectTransform CreateTriggerActionMiniUI();

	RectTransform CreateTriggerActionDiscreteUI();

	RectTransform CreateTriggerActionTransitionUI();

	void RemoveTriggerActionUI(RectTransform rt);
}
