using UnityEngine;
using UnityEngine.UI;

public class UIDynamic : MonoBehaviour
{
	public float height
	{
		get
		{
			RectTransform component = GetComponent<RectTransform>();
			if (component != null)
			{
				return component.sizeDelta.y;
			}
			return 0f;
		}
		set
		{
			LayoutElement component = GetComponent<LayoutElement>();
			if (component != null)
			{
				if (value > component.minHeight)
				{
					component.preferredHeight = value;
				}
				else
				{
					component.preferredHeight = component.minHeight;
				}
			}
			RectTransform component2 = GetComponent<RectTransform>();
			if (component2 != null)
			{
				Vector2 sizeDelta = component2.sizeDelta;
				sizeDelta.y = value;
				component2.sizeDelta = sizeDelta;
			}
		}
	}
}
