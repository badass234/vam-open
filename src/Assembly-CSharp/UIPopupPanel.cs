using UnityEngine;
using UnityEngine.EventSystems;

public class UIPopupPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	public UIPopup popup;

	public int framesToClose = 100;

	protected int _timer;

	protected bool eligibleForClose;

	public void OnPointerEnter(PointerEventData ed)
	{
		eligibleForClose = true;
		_timer = 0;
	}

	public void OnPointerExit(PointerEventData ed)
	{
		if (eligibleForClose)
		{
			_timer = framesToClose;
		}
		eligibleForClose = false;
	}
}
