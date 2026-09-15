using UnityEngine;

public class UIDynamicPopup : UIDynamic
{
	public UIPopup popup;

	public float labelSpacingRight = 10f;

	public string label
	{
		get
		{
			if (popup != null)
			{
				return popup.label;
			}
			return null;
		}
		set
		{
			if (popup != null)
			{
				popup.label = value;
			}
		}
	}

	public Color labelTextColor
	{
		get
		{
			if (popup != null)
			{
				return popup.labelTextColor;
			}
			return Color.black;
		}
		set
		{
			if (popup != null)
			{
				popup.labelTextColor = value;
			}
		}
	}

	public float labelWidth
	{
		get
		{
			if (popup != null && popup.labelText != null)
			{
				RectTransform rectTransform = popup.labelText.rectTransform;
				return rectTransform.sizeDelta.x;
			}
			return 0f;
		}
		set
		{
			if (popup != null && popup.labelText != null)
			{
				float x = value + labelSpacingRight;
				RectTransform rectTransform = popup.labelText.rectTransform;
				Vector2 sizeDelta = rectTransform.sizeDelta;
				sizeDelta.x = value;
				popup.labelText.rectTransform.sizeDelta = sizeDelta;
				if (popup.topButton != null)
				{
					RectTransform component = popup.topButton.GetComponent<RectTransform>();
					Vector2 offsetMin = component.offsetMin;
					offsetMin.x = x;
					component.offsetMin = offsetMin;
				}
				if (popup.popupPanel != null)
				{
					RectTransform popupPanel = popup.popupPanel;
					Vector2 offsetMin2 = popupPanel.offsetMin;
					offsetMin2.x = x;
					popupPanel.offsetMin = offsetMin2;
				}
				if (popup.sliderControl != null)
				{
					RectTransform component2 = popup.sliderControl.GetComponent<RectTransform>();
					Vector2 offsetMin3 = component2.offsetMin;
					offsetMin3.x = x;
					component2.offsetMin = offsetMin3;
				}
				if (popup.filterField != null)
				{
					RectTransform component3 = popup.filterField.GetComponent<RectTransform>();
					Vector2 offsetMin4 = component3.offsetMin;
					offsetMin4.x = x;
					component3.offsetMin = offsetMin4;
				}
			}
		}
	}

	public float popupPanelHeight
	{
		get
		{
			if (popup != null)
			{
				return popup.popupPanelHeight;
			}
			return 0f;
		}
		set
		{
			if (popup != null)
			{
				popup.popupPanelHeight = value;
			}
		}
	}
}
