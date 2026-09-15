using UnityEngine;

public class CollisionTriggerGroup : JSONStorableTriggerHandler
{
	public RectTransform triggerPrefab;

	public CollisionTrigger[] collisionTriggers;

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		CollisionTriggerGroupUI componentInChildren = UITransform.GetComponentInChildren<CollisionTriggerGroupUI>();
		if (!(componentInChildren != null))
		{
			return;
		}
		if (componentInChildren.triggerContentManager != null)
		{
			if (triggerPrefab != null)
			{
				CollisionTrigger[] array = collisionTriggers;
				foreach (CollisionTrigger collisionTrigger in array)
				{
					if (collisionTrigger.trigger != null && triggerPrefab != null)
					{
						RectTransform rectTransform = CreateTriggerActionsUI();
						if (rectTransform != null)
						{
							rectTransform.SetParent(componentInChildren.transform, false);
							rectTransform.gameObject.SetActive(false);
							collisionTrigger.UITransform = rectTransform;
							RectTransform rectTransform2 = Object.Instantiate(triggerPrefab);
							componentInChildren.triggerContentManager.AddItem(rectTransform2);
							collisionTrigger.trigger.displayName = collisionTrigger.name;
							collisionTrigger.trigger.triggerPanel = rectTransform2;
							collisionTrigger.InitUI();
						}
					}
				}
			}
			else
			{
				Debug.LogError("Attempted to InitUI on CollisionTriggerGroup when triggerPrefab not set");
			}
		}
		else
		{
			Debug.LogError("Attempt to InitUI on CollisionTriggerGroup when triggerContentManager not set");
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			InitUI();
		}
	}
}
