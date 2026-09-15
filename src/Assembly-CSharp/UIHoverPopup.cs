using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverPopup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	public enum MoveTo
	{
		None,
		Parent,
		Grandparent
	}

	public GameObject popup;

	public bool enableShowOnHover = true;

	public bool enableHideOnExit = true;

	[Tooltip("To allow it to pop over other objects in hierarchy")]
	public MoveTo moveTo;

	protected Transform originalParent;

	protected bool wasMoved;

	protected bool isInteracting;

	public void TogglePopup()
	{
		if (popup != null)
		{
			if (popup.activeSelf)
			{
				HidePopup();
			}
			else
			{
				ShowPopup();
			}
		}
	}

	public void ShowPopup()
	{
		if (!(popup != null))
		{
			return;
		}
		isInteracting = true;
		switch (moveTo)
		{
		case MoveTo.Parent:
			if (base.transform.parent != null)
			{
				originalParent = popup.transform.parent;
				popup.transform.SetParent(base.transform.parent, true);
				wasMoved = true;
			}
			break;
		case MoveTo.Grandparent:
			if (base.transform.parent != null && base.transform.parent.parent != null)
			{
				originalParent = popup.transform.parent;
				popup.transform.SetParent(base.transform.parent.parent, true);
				wasMoved = true;
			}
			break;
		}
		popup.SetActive(true);
		isInteracting = false;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (enableShowOnHover)
		{
			ShowPopup();
		}
	}

	public void HidePopup()
	{
		if (popup != null)
		{
			if (wasMoved)
			{
				popup.transform.SetParent(originalParent, true);
				wasMoved = false;
			}
			popup.SetActive(false);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (enableHideOnExit)
		{
			HidePopup();
		}
	}

	private void OnEnable()
	{
		if (!isInteracting)
		{
			HidePopup();
		}
	}

	private void OnDisable()
	{
		HidePopup();
	}
}
