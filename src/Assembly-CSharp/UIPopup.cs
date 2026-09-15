using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIPopup : MonoBehaviour, ISelectHandler, IDeselectHandler, IEventSystemHandler
{
	public delegate void OnOpenPopup();

	public delegate void OnValueChange(string value);

	[Serializable]
	public class PopupChangeEvent : UnityEvent<string>
	{
	}

	public bool alwaysOpen;

	public bool dynamicTextSize = true;

	public int minTextSize = 14;

	public Color normalColor = Color.white;

	public Color normalBackgroundColor = Color.white;

	public Color selectColor = Color.blue;

	public Button topButton;

	public RectTransform popupPanel;

	[SerializeField]
	protected float _popupPanelHeight = 350f;

	public RectTransform buttonParent;

	public Image backgroundImage;

	public Text labelText;

	public Slider sliderControl;

	[SerializeField]
	protected bool _showSlider = true;

	public bool useFiltering;

	public InputField filterField;

	public InputFieldAction filterFieldAction;

	public Text filterFieldPlaceholderText;

	public Text filterCountText;

	public RectTransform maxIndicator;

	private int maxNumber = 400;

	protected Dictionary<int, int> valueIndexToButtonIndex;

	protected Dictionary<int, int> buttonIndexToValueIndex;

	protected string _filter = string.Empty;

	public RectTransform popupButtonPrefab;

	[SerializeField]
	private RectTransform[] _popupTransforms;

	[SerializeField]
	private Button[] _popupButtons;

	private int _filteredNumPopupValues;

	[SerializeField]
	private int _numPopupValues;

	private string[] _filterCompareValues;

	[SerializeField]
	private string[] _popupValues;

	public bool useDifferentDisplayValues;

	[SerializeField]
	private string[] _displayPopupValues;

	public float topBottomBuffer = 5f;

	protected ScrollRect scrollRect;

	private bool _visible;

	public OnOpenPopup onOpenPopupHandlers;

	public OnValueChange onValueChangeHandlers;

	public PopupChangeEvent onValueChangeUnityEvent;

	[SerializeField]
	private string _currentValue = string.Empty;

	protected int _currentValueIndex = -1;

	protected int _currentButtonIndex = -1;

	protected bool _currentValueChanged;

	protected int _currentHighlightIndex = -1;

	private Vector3 popupPanelRelativePosition;

	public float popupPanelHeight
	{
		get
		{
			return _popupPanelHeight;
		}
		set
		{
			if (_popupPanelHeight == value)
			{
				return;
			}
			_popupPanelHeight = value;
			if (popupPanel != null)
			{
				bool flag = _visible;
				if (flag)
				{
					MakeInvisible();
				}
				Vector2 sizeDelta = popupPanel.sizeDelta;
				sizeDelta.y = _popupPanelHeight;
				popupPanel.sizeDelta = sizeDelta;
				if (flag)
				{
					MakeVisible();
				}
			}
		}
	}

	public bool showSlider
	{
		get
		{
			return _showSlider;
		}
		set
		{
			if (_showSlider == value)
			{
				return;
			}
			_showSlider = value;
			if (topButton != null)
			{
				RectTransform component = topButton.GetComponent<RectTransform>();
				if (component != null)
				{
					Vector2 offsetMax = component.offsetMax;
					if (_showSlider && sliderControl != null)
					{
						RectTransform component2 = sliderControl.GetComponent<RectTransform>();
						if (component2 != null)
						{
							offsetMax.y = -5f - component2.sizeDelta.y;
						}
						else
						{
							offsetMax.y = -5f;
						}
					}
					else
					{
						offsetMax.y = -5f;
					}
					component.offsetMax = offsetMax;
				}
			}
			if (sliderControl != null)
			{
				if (_showSlider)
				{
					sliderControl.gameObject.SetActive(true);
				}
				else
				{
					sliderControl.gameObject.SetActive(false);
				}
			}
		}
	}

	public string filter
	{
		get
		{
			return _filter;
		}
		set
		{
			string text = value.ToLower();
			if (_filter != text)
			{
				_filter = text;
				if (filterField != null)
				{
					filterField.text = _filter;
				}
				visible = true;
				SyncFilter();
				ClearPanel();
				CreatePanelButtons();
			}
		}
	}

	public int numPopupValues
	{
		get
		{
			return _numPopupValues;
		}
		set
		{
			if (_numPopupValues == value || !(base.gameObject != null))
			{
				return;
			}
			ClearPanel();
			int num = _numPopupValues;
			_numPopupValues = value;
			string[] array = new string[_numPopupValues];
			string[] array2 = new string[_numPopupValues];
			string[] array3 = null;
			if (useFiltering)
			{
				array3 = new string[_numPopupValues];
			}
			for (int i = 0; i < _numPopupValues; i++)
			{
				if (i < num)
				{
					if (_displayPopupValues != null && i < _displayPopupValues.Length)
					{
						if (_displayPopupValues[i] == null)
						{
							array2[i] = string.Empty;
						}
						else
						{
							array2[i] = _displayPopupValues[i];
						}
						if (useFiltering && useDifferentDisplayValues)
						{
							array3[i] = array2[i].ToLower();
						}
					}
					if (_popupValues[i] == null)
					{
						array[i] = string.Empty;
					}
					else
					{
						array[i] = _popupValues[i];
					}
					if (useFiltering && !useDifferentDisplayValues)
					{
						array3[i] = array[i].ToLower();
					}
				}
				else
				{
					array[i] = string.Empty;
					array2[i] = string.Empty;
					if (useFiltering)
					{
						array3[i] = string.Empty;
					}
				}
			}
			_popupValues = array;
			_displayPopupValues = array2;
			_filterCompareValues = array3;
			SyncFilter();
			CreatePanelButtons();
			SyncSlider();
		}
	}

	public string[] popupValues => _popupValues;

	public string[] displayPopupValues => _displayPopupValues;

	public bool visible
	{
		get
		{
			return _visible;
		}
		set
		{
			if (_visible == value)
			{
				return;
			}
			_visible = value;
			if (alwaysOpen)
			{
				_visible = true;
			}
			if (_visible)
			{
				if (onOpenPopupHandlers != null)
				{
					onOpenPopupHandlers();
					if (useFiltering)
					{
						SyncFilter();
						ClearPanel();
						CreatePanelButtons();
					}
				}
				else
				{
					HighlightCurrentValue();
				}
				MakeVisible();
			}
			else
			{
				MakeInvisible();
			}
		}
	}

	public string currentValue
	{
		get
		{
			return _currentValue;
		}
		set
		{
			if (_currentValue != value || _currentValueIndex == -1)
			{
				SetCurrentValue(value);
			}
		}
	}

	public string currentValueNoCallback
	{
		get
		{
			return _currentValue;
		}
		set
		{
			if (_currentValue != value || _currentValueIndex == -1)
			{
				SetCurrentValue(value, false);
			}
		}
	}

	public string label
	{
		get
		{
			if (labelText != null)
			{
				return labelText.text;
			}
			return null;
		}
		set
		{
			if (labelText != null)
			{
				labelText.text = value;
			}
		}
	}

	public Color labelTextColor
	{
		get
		{
			if (labelText != null)
			{
				return labelText.color;
			}
			return Color.black;
		}
		set
		{
			if (labelText != null)
			{
				labelText.color = value;
			}
		}
	}

	protected void SyncFilter()
	{
		if (useFiltering)
		{
			if (valueIndexToButtonIndex == null)
			{
				valueIndexToButtonIndex = new Dictionary<int, int>();
			}
			else
			{
				valueIndexToButtonIndex.Clear();
			}
			if (buttonIndexToValueIndex == null)
			{
				buttonIndexToValueIndex = new Dictionary<int, int>();
			}
			else
			{
				buttonIndexToValueIndex.Clear();
			}
			int num = 0;
			bool flag = false;
			for (int i = 0; i < _numPopupValues; i++)
			{
				if (_filter == null || _filter == string.Empty || _filterCompareValues[i].Contains(_filter))
				{
					valueIndexToButtonIndex.Add(i, num);
					buttonIndexToValueIndex.Add(num, i);
					num++;
					if (num == maxNumber)
					{
						flag = true;
						break;
					}
				}
			}
			if (filterCountText != null)
			{
				filterCountText.text = num.ToString() + " / " + _numPopupValues;
			}
			if (maxIndicator != null)
			{
				maxIndicator.gameObject.SetActive(flag);
			}
			if (filterFieldPlaceholderText != null)
			{
				if (flag)
				{
					filterFieldPlaceholderText.text = "Filter (max limited)";
				}
				else
				{
					filterFieldPlaceholderText.text = "Filter...";
				}
			}
			_filteredNumPopupValues = num;
		}
		else
		{
			_filteredNumPopupValues = _numPopupValues;
		}
	}

	protected void SelectFilter()
	{
		visible = true;
	}

	public void SubmitFilter()
	{
		if (!useFiltering)
		{
			return;
		}
		if (_filteredNumPopupValues > 0)
		{
			int value2;
			if (_currentHighlightIndex != -1)
			{
				int value;
				if (buttonIndexToValueIndex.TryGetValue(_currentHighlightIndex, out value))
				{
					currentValue = _popupValues[value];
				}
			}
			else if (_currentButtonIndex == -1 && buttonIndexToValueIndex.TryGetValue(0, out value2))
			{
				currentValue = _popupValues[value2];
			}
		}
		visible = false;
	}

	protected void SyncFilter(string f)
	{
		filter = f;
	}

	public void setPopupValue(int index, string text)
	{
		if (index >= 0 && index < _numPopupValues)
		{
			_popupValues[index] = text;
			if (useFiltering && !useDifferentDisplayValues)
			{
				_filterCompareValues[index] = text.ToLower();
			}
			SetPanelButtonText(index);
			_currentValueChanged = true;
		}
	}

	public void setDisplayPopupValue(int index, string text)
	{
		if (index >= 0 && index < _numPopupValues)
		{
			_displayPopupValues[index] = text;
			if (useFiltering && useDifferentDisplayValues)
			{
				_filterCompareValues[index] = text.ToLower();
			}
			SetPanelButtonText(index);
			_currentValueChanged = true;
		}
	}

	protected void HighlightCurrentValue()
	{
		if (_popupButtons == null)
		{
			return;
		}
		int num = -1;
		for (int i = 0; i < _popupButtons.Length; i++)
		{
			if (_popupButtons[i] != null)
			{
				int value = i;
				if (useFiltering)
				{
					buttonIndexToValueIndex.TryGetValue(i, out value);
				}
				ColorBlock colors = _popupButtons[i].colors;
				if (_popupValues[value] == currentValue)
				{
					num = i;
					colors.normalColor = selectColor;
				}
				else
				{
					colors.normalColor = normalColor;
				}
				_popupButtons[i].colors = colors;
			}
		}
		if (num != -1 && scrollRect != null && scrollRect.vertical && !scrollRect.horizontal)
		{
			RectTransform component = _popupButtons[num].GetComponent<RectTransform>();
			float height = component.rect.height;
			float num2 = (scrollRect.viewport.rect.height - height) / 2f;
			float num3 = (float)num * height - num2;
			float num4 = Mathf.Clamp01(num3 / (scrollRect.content.rect.height - scrollRect.viewport.rect.height));
			scrollRect.verticalNormalizedPosition = 1f - num4;
		}
	}

	private void MakeInvisible()
	{
		ClearHighlightButton();
		if (popupPanel != null)
		{
			popupPanel.gameObject.SetActive(false);
			if (base.transform.parent != null)
			{
				popupPanel.transform.SetParent(base.transform);
			}
		}
	}

	private void MakeVisible()
	{
		if (popupPanel != null)
		{
			popupPanel.gameObject.SetActive(true);
			if (base.transform.parent != null)
			{
				popupPanel.transform.SetParent(base.transform.parent);
			}
		}
	}

	protected void SetCurrentValue(string value, bool callBack = true, bool forceSet = false, bool leaveOpen = false)
	{
		if (_currentValue != value || forceSet)
		{
			_currentValue = value;
			if (popupPanel != null && !alwaysOpen && !leaveOpen)
			{
				visible = false;
			}
			if (callBack && onValueChangeHandlers != null)
			{
				onValueChangeHandlers(_currentValue);
			}
			if (callBack && onValueChangeUnityEvent != null)
			{
				onValueChangeUnityEvent.Invoke(_currentValue);
			}
		}
		_currentValueChanged = true;
		_currentValueIndex = -1;
		_currentButtonIndex = -1;
		for (int i = 0; i < _popupValues.Length; i++)
		{
			if (!(_currentValue == _popupValues[i]))
			{
				continue;
			}
			_currentValueIndex = i;
			if (useFiltering)
			{
				int value2;
				if (valueIndexToButtonIndex.TryGetValue(i, out value2))
				{
					_currentButtonIndex = value2;
					if (sliderControl != null)
					{
						sliderControl.value = value2;
					}
				}
			}
			else
			{
				_currentButtonIndex = i;
				if (sliderControl != null)
				{
					sliderControl.value = _currentValueIndex;
				}
			}
			break;
		}
		if (!(topButton != null))
		{
			return;
		}
		Text[] componentsInChildren = topButton.GetComponentsInChildren<Text>(true);
		if (componentsInChildren == null)
		{
			return;
		}
		if (dynamicTextSize)
		{
			if (!componentsInChildren[0].resizeTextForBestFit)
			{
				componentsInChildren[0].resizeTextMaxSize = componentsInChildren[0].fontSize;
				componentsInChildren[0].resizeTextMinSize = minTextSize;
				componentsInChildren[0].resizeTextForBestFit = dynamicTextSize;
			}
		}
		else
		{
			componentsInChildren[0].resizeTextForBestFit = dynamicTextSize;
		}
		if (useDifferentDisplayValues && _currentValueIndex != -1)
		{
			componentsInChildren[0].text = _displayPopupValues[_currentValueIndex];
		}
		else
		{
			componentsInChildren[0].text = _currentValue;
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (backgroundImage != null)
		{
			backgroundImage.color = selectColor;
		}
		else if (topButton != null)
		{
			ColorBlock colors = topButton.colors;
			colors.normalColor = selectColor;
			topButton.colors = colors;
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		if (backgroundImage != null)
		{
			backgroundImage.color = normalBackgroundColor;
		}
		else if (topButton != null)
		{
			ColorBlock colors = topButton.colors;
			colors.normalColor = normalColor;
			topButton.colors = colors;
		}
	}

	public void SetPreviousValue()
	{
		if (_popupValues == null || _currentButtonIndex <= 0)
		{
			return;
		}
		if (useFiltering)
		{
			int value;
			if (buttonIndexToValueIndex.TryGetValue(_currentButtonIndex - 1, out value))
			{
				SetCurrentValue(_popupValues[value], true, false, true);
			}
		}
		else
		{
			SetCurrentValue(_popupValues[_currentButtonIndex - 1], true, false, true);
		}
	}

	public void SetNextValue()
	{
		if (_popupValues == null || _currentButtonIndex >= _filteredNumPopupValues - 1)
		{
			return;
		}
		if (useFiltering)
		{
			int value;
			if (buttonIndexToValueIndex.TryGetValue(_currentButtonIndex + 1, out value))
			{
				SetCurrentValue(_popupValues[value], true, false, true);
			}
		}
		else
		{
			SetCurrentValue(_popupValues[_currentButtonIndex + 1], true, false, true);
		}
	}

	protected void ClearHighlightButton()
	{
		if (_popupButtons != null && _currentHighlightIndex != -1 && _currentHighlightIndex < _popupButtons.Length)
		{
			_popupButtons[_currentHighlightIndex].OnPointerExit(null);
		}
		_currentHighlightIndex = -1;
	}

	protected void HighlightCurrentButton()
	{
		if (_popupButtons != null && _currentHighlightIndex != -1 && _currentHighlightIndex < _popupButtons.Length)
		{
			_popupButtons[_currentHighlightIndex].OnPointerEnter(null);
		}
	}

	protected void HighlightPreviousButton()
	{
		if (_currentHighlightIndex >= 0)
		{
			int currentHighlightIndex = _currentHighlightIndex;
			ClearHighlightButton();
			_currentHighlightIndex = currentHighlightIndex - 1;
			HighlightCurrentButton();
		}
		filterField.caretPosition = filterField.text.Length;
	}

	protected void HighlightNextButton()
	{
		if (_popupButtons != null && _currentHighlightIndex < _popupButtons.Length - 1)
		{
			int currentHighlightIndex = _currentHighlightIndex;
			ClearHighlightButton();
			_currentHighlightIndex = currentHighlightIndex + 1;
			HighlightCurrentButton();
		}
	}

	public void Toggle()
	{
		visible = !visible;
	}

	private void ClearPanel()
	{
		ClearHighlightButton();
		RectTransform rectTransform = ((!(buttonParent != null)) ? popupPanel : buttonParent);
		if (rectTransform != null)
		{
			List<GameObject> list = new List<GameObject>();
			foreach (RectTransform item in rectTransform)
			{
				if (!popupButtonPrefab || !(popupButtonPrefab.gameObject == item.gameObject))
				{
					list.Add(item.gameObject);
				}
			}
			foreach (GameObject item2 in list)
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(item2);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(item2);
				}
			}
		}
		_popupTransforms = null;
		_popupButtons = null;
	}

	private void SetPanelButtonText(int index)
	{
		string text = _popupValues[index];
		int value = index;
		bool flag = true;
		if (useFiltering && !valueIndexToButtonIndex.TryGetValue(index, out value))
		{
			flag = false;
		}
		if (!flag)
		{
			return;
		}
		RectTransform rectTransform = _popupTransforms[value];
		if (!(rectTransform != null))
		{
			return;
		}
		Button component = rectTransform.GetComponent<Button>();
		if (component != null)
		{
			_popupButtons[value] = component;
			string popupValueCopy = string.Copy(text);
			component.onClick.RemoveAllListeners();
			component.onClick.AddListener(delegate
			{
				SetCurrentValue(popupValueCopy, true, true);
			});
		}
		Text[] componentsInChildren = component.GetComponentsInChildren<Text>(true);
		if (componentsInChildren == null)
		{
			return;
		}
		if (dynamicTextSize)
		{
			if (!componentsInChildren[0].resizeTextForBestFit)
			{
				componentsInChildren[0].resizeTextMaxSize = componentsInChildren[0].fontSize;
				componentsInChildren[0].resizeTextMinSize = minTextSize;
				componentsInChildren[0].resizeTextForBestFit = dynamicTextSize;
			}
		}
		else
		{
			componentsInChildren[0].resizeTextForBestFit = dynamicTextSize;
		}
		if (useDifferentDisplayValues)
		{
			componentsInChildren[0].text = _displayPopupValues[index];
		}
		else
		{
			componentsInChildren[0].text = text;
		}
	}

	private void CreatePanelButtons()
	{
		if (_popupValues == null || !(popupButtonPrefab != null) || !(base.gameObject != null))
		{
			return;
		}
		_popupTransforms = new RectTransform[_filteredNumPopupValues];
		_popupButtons = new Button[_filteredNumPopupValues];
		int num = 1;
		float num2 = 2f * topBottomBuffer;
		float num3 = num2;
		_currentButtonIndex = -1;
		for (int i = 0; i < _filteredNumPopupValues; i++)
		{
			int value = i;
			if (!useFiltering || buttonIndexToValueIndex.TryGetValue(i, out value))
			{
				if (_currentValue == _popupValues[value])
				{
					_currentButtonIndex = i;
				}
				RectTransform rectTransform = UnityEngine.Object.Instantiate(popupButtonPrefab);
				_popupTransforms[i] = rectTransform;
				rectTransform.gameObject.SetActive(true);
				SetPanelButtonText(value);
				float height = rectTransform.rect.height;
				num2 += height;
				num3 += height;
				if (buttonParent != null)
				{
					rectTransform.SetParent(buttonParent, false);
				}
				else
				{
					rectTransform.SetParent(popupPanel, false);
				}
				Vector2 zero = Vector2.zero;
				zero.y = (float)num * (0f - height) + height * 0.5f - topBottomBuffer;
				rectTransform.anchoredPosition = zero;
				num++;
			}
		}
		if (topButton != null)
		{
			RectTransform component = topButton.GetComponent<RectTransform>();
			if (component != null)
			{
				num2 += component.rect.height;
			}
		}
		if (buttonParent == null)
		{
			RectTransform component2 = GetComponent<RectTransform>();
			if (component2 != null)
			{
				Vector2 sizeDelta = component2.sizeDelta;
				sizeDelta.y = num2;
				component2.sizeDelta = sizeDelta;
			}
		}
		else
		{
			Vector2 sizeDelta2 = buttonParent.sizeDelta;
			sizeDelta2.y = num3;
			buttonParent.sizeDelta = sizeDelta2;
		}
		HighlightCurrentValue();
	}

	private void SyncSlider()
	{
		if (sliderControl != null)
		{
			sliderControl.minValue = 0f;
			sliderControl.maxValue = Mathf.Max(_filteredNumPopupValues - 1, 0f);
		}
	}

	private void InitSlider()
	{
		if (!(sliderControl != null))
		{
			return;
		}
		sliderControl.minValue = 0f;
		sliderControl.maxValue = Mathf.Max(_filteredNumPopupValues - 1, 0f);
		sliderControl.onValueChanged.AddListener(delegate
		{
			int num = Mathf.FloorToInt(sliderControl.value);
			if (num >= 0 && num < _filteredNumPopupValues)
			{
				int value = num;
				if (useFiltering)
				{
					buttonIndexToValueIndex.TryGetValue(num, out value);
				}
				currentValue = _popupValues[value];
			}
		});
	}

	private void TestDelegate(string test)
	{
		Debug.Log("TestDelegate on " + base.name + " called with " + test);
	}

	private void Start()
	{
		scrollRect = GetComponentInChildren<ScrollRect>(true);
		if (popupPanel != null)
		{
			popupPanelRelativePosition = popupPanel.localPosition;
		}
		if (filterFieldAction != null)
		{
			InputFieldAction inputFieldAction = filterFieldAction;
			inputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(inputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SubmitFilter));
			InputFieldAction inputFieldAction2 = filterFieldAction;
			inputFieldAction2.onSelectedHandlers = (InputFieldAction.OnSelected)Delegate.Combine(inputFieldAction2.onSelectedHandlers, new InputFieldAction.OnSelected(SelectFilter));
			InputFieldAction inputFieldAction3 = filterFieldAction;
			inputFieldAction3.onUpHandlers = (InputFieldAction.OnUp)Delegate.Combine(inputFieldAction3.onUpHandlers, new InputFieldAction.OnUp(HighlightPreviousButton));
			InputFieldAction inputFieldAction4 = filterFieldAction;
			inputFieldAction4.onDownHandlers = (InputFieldAction.OnDown)Delegate.Combine(inputFieldAction4.onDownHandlers, new InputFieldAction.OnDown(HighlightNextButton));
		}
		if (alwaysOpen)
		{
			_visible = true;
			MakeVisible();
		}
		else
		{
			_visible = false;
			if (popupPanel != null)
			{
				popupPanel.gameObject.SetActive(false);
			}
		}
		SyncFilter();
		ClearPanel();
		CreatePanelButtons();
		InitSlider();
	}

	private void Update()
	{
		if (_currentValueChanged)
		{
			HighlightCurrentValue();
		}
		if (popupPanel != null)
		{
			popupPanel.position = base.transform.localToWorldMatrix.MultiplyPoint3x4(popupPanelRelativePosition);
		}
		_currentValueChanged = false;
	}
}
