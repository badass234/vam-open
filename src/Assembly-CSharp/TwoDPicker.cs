using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TwoDPicker : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IEventSystemHandler
{
	public Image Selector;

	public RectTransform Container;

	[SerializeField]
	protected float _xVal;

	[SerializeField]
	protected float _yVal;

	public float xVal
	{
		get
		{
			return _xVal;
		}
		set
		{
			if (_xVal != value)
			{
				_xVal = value;
				_xVal = Mathf.Clamp01(_xVal);
				SetSelectorPositionFromXYVal();
			}
		}
	}

	public float yVal
	{
		get
		{
			return _yVal;
		}
		set
		{
			if (_yVal != value)
			{
				_yVal = value;
				_yVal = Mathf.Clamp01(_yVal);
				SetSelectorPositionFromXYVal();
			}
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (Selector != null)
		{
			SetDraggedPosition(eventData);
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
	}

	public void OnDrag(PointerEventData data)
	{
		if (Selector != null)
		{
			SetDraggedPosition(data);
		}
	}

	protected virtual void SetXYValFromSelectorPosition()
	{
		if (Selector != null)
		{
			RectTransform component = Selector.GetComponent<RectTransform>();
			if (Container == null)
			{
				Container = GetComponent<RectTransform>();
			}
			if (Container != null && component != null)
			{
				Vector2 anchoredPosition = component.anchoredPosition;
				_xVal = Mathf.Clamp01(anchoredPosition.x / Container.rect.width);
				_yVal = Mathf.Clamp01(anchoredPosition.y / Container.rect.height);
				SetSelectorPositionFromXYVal();
			}
		}
	}

	protected virtual void SetSelectorPositionFromXYVal()
	{
		if (Selector != null)
		{
			RectTransform component = Selector.GetComponent<RectTransform>();
			if (Container == null)
			{
				Container = GetComponent<RectTransform>();
			}
			if (Container != null && component != null)
			{
				Vector2 anchoredPosition = default(Vector2);
				anchoredPosition.x = Container.rect.width * _xVal;
				anchoredPosition.y = Container.rect.height * _yVal;
				component.anchoredPosition = anchoredPosition;
			}
		}
	}

	protected void SetDraggedPosition(PointerEventData data)
	{
		if (Selector != null)
		{
			RectTransform component = Selector.GetComponent<RectTransform>();
			if (Container == null)
			{
				Container = GetComponent<RectTransform>();
			}
			Vector3 worldPoint;
			if (Container != null && component != null && RectTransformUtility.ScreenPointToWorldPointInRectangle(Container, data.position, data.pressEventCamera, out worldPoint))
			{
				component.position = worldPoint;
				component.rotation = Container.rotation;
				SetXYValFromSelectorPosition();
			}
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
	}

	public virtual void Awake()
	{
		SetSelectorPositionFromXYVal();
	}
}
