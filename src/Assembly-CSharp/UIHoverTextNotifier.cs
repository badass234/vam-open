using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHoverTextNotifier : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	public delegate void TextNotifier(Text text);

	public TextNotifier onEnterNotifier;

	public TextNotifier onExitNotifier;

	protected Text text;

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (text != null && onEnterNotifier != null)
		{
			onEnterNotifier(text);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (text != null && onExitNotifier != null)
		{
			onExitNotifier(text);
		}
	}

	private void Awake()
	{
		text = base.transform.GetComponent<Text>();
	}
}
