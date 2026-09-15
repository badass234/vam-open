using UnityEngine;
using UnityEngine.EventSystems;

public class PointerEnterExitAction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	public delegate void OnEnterAction();

	public delegate void OnExitAction();

	public OnEnterAction onEnterActions;

	public OnExitAction onExitActions;

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (onEnterActions != null)
		{
			onEnterActions();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (onExitActions != null)
		{
			onExitActions();
		}
	}
}
