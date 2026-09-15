using UnityEngine;

public class JSONStorableTriggerHandler : JSONStorable, TriggerHandler
{
	public RectTransform triggerActionsPrefab;

	public RectTransform triggerActionMiniPrefab;

	public RectTransform triggerActionDiscretePrefab;

	public RectTransform triggerActionTransitionPrefab;

	public virtual void RemoveTrigger(Trigger trigger)
	{
	}

	public virtual void DuplicateTrigger(Trigger trigger)
	{
	}

	public RectTransform CreateTriggerActionsUI()
	{
		RectTransform result = null;
		if (triggerActionsPrefab != null)
		{
			result = Object.Instantiate(triggerActionsPrefab);
		}
		else
		{
			Debug.LogError("Attempted to make TriggerActionsUI when prefab was not set");
		}
		return result;
	}

	public RectTransform CreateTriggerActionMiniUI()
	{
		RectTransform result = null;
		if (triggerActionMiniPrefab != null)
		{
			result = Object.Instantiate(triggerActionMiniPrefab);
		}
		else
		{
			Debug.LogError("Attempted to make TriggerActionMiniUI when prefab was not set");
		}
		return result;
	}

	public RectTransform CreateTriggerActionDiscreteUI()
	{
		RectTransform result = null;
		if (triggerActionDiscretePrefab != null)
		{
			result = Object.Instantiate(triggerActionDiscretePrefab);
		}
		else
		{
			Debug.LogError("Attempted to make TriggerActionDiscreteUI when prefab was not set");
		}
		return result;
	}

	public RectTransform CreateTriggerActionTransitionUI()
	{
		RectTransform result = null;
		if (triggerActionTransitionPrefab != null)
		{
			result = Object.Instantiate(triggerActionTransitionPrefab);
		}
		else
		{
			Debug.LogError("Attempted to make TriggerActionTransitionUI when prefab was not set");
		}
		return result;
	}

	public void RemoveTriggerActionUI(RectTransform rt)
	{
		if (rt != null)
		{
			Object.Destroy(rt.gameObject);
		}
	}
}
