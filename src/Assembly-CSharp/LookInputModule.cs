using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;
using Valve.VR;
using Weelco.VRKeyboard;

public class LookInputModule : StandaloneInputModule
{
	public interface IPointerMoveHandler : IEventSystemHandler
	{
		void OnPointerMove(PointerEventData eventData);
	}

	public enum Mode
	{
		Pointer,
		Submit
	}

	protected List<RaycastResult> m_RaycastResultCacheRight = new List<RaycastResult>();

	private static LookInputModule _singleton;

	public bool disableStandaloneProcess;

	public string submitButtonName = "Fire1";

	public bool useEitherControllerButtons;

	public SteamVR_Action_Boolean interactAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("UIInteract");

	public SteamVR_Action_Vector2 scrollAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("UIScroll");

	public JoystickControl.Axis controlAxis = JoystickControl.Axis.Triggers;

	public JoystickControl.Axis discreteControlAxis = JoystickControl.Axis.DPadY;

	public bool invertDiscreteControlAxis = true;

	public bool useSmoothAxis = true;

	public float smoothAxisMultiplier = 0.01f;

	public float steppedAxisStepsPerSecond = 10f;

	public float worldScale = 1f;

	private bool _guiRaycastHit;

	private bool _mouseRaycastHit;

	private bool _controlAxisUsed;

	public Mode mode;

	public bool useLookDrag = true;

	public bool useLookDragSlider = true;

	public bool useLookDragScrollbar;

	public bool useCursor = true;

	public float normalCursorScale = 0.0005f;

	public bool scaleCursorWithDistance = true;

	public RectTransform cursor;

	public RectTransform cursorRight;

	public RectTransform cursorMouse;

	public Transform keyboardTransform;

	public VRKeyboardFull[] vrKeyboards;

	protected Transform currentKeyboardTransform;

	public bool useSelectColor = true;

	public bool useSelectColorOnButton;

	public bool useSelectColorOnToggle;

	public Color selectColor = Color.blue;

	public bool ignoreInputsWhenLookAway = true;

	public bool deselectWhenLookAway;

	public RectTransform anchorForControlCopy;

	protected GameObject controlCopy;

	public Camera referenceCamera;

	private PointerEventData lookData;

	private PointerEventData lookDataRight;

	private Color currentSelectedNormalColor;

	private bool currentSelectedNormalColorValid;

	private Color currentSelectedHighlightedColor;

	private GameObject currentLook;

	private GameObject currentLookRight;

	private GameObject currentLookMouse;

	private GameObject currentPressed;

	private GameObject currentPressedRight;

	private GameObject currentPressedMouse;

	private GameObject currentDragging;

	private GameObject currentDraggingRight;

	private GameObject currentDraggingMouse;

	private InputField currentInputField;

	private GameObject cachedCurrentSelectedGameObject;

	private InputField cachedCurrentSelectedGameObjectInputField;

	private KeyEventHandler currentKeyEventHandler;

	private float nextAxisActionTime;

	protected float axisAccumulation;

	protected bool axisOn;

	protected bool discreteAxisOn;

	public static ExecuteEvents.EventFunction<IPointerMoveHandler> pointerMoveHandler => Execute;

	public static LookInputModule singleton => _singleton;

	public bool guiRaycastHit => _guiRaycastHit;

	public bool mouseRaycastHit => _mouseRaycastHit;

	public bool controlAxisUsed => _controlAxisUsed;

	public bool inputFieldActive
	{
		get
		{
			if (currentInputField != null)
			{
				return true;
			}
			if (currentKeyEventHandler != null)
			{
				return true;
			}
			if (cachedCurrentSelectedGameObject != base.eventSystem.currentSelectedGameObject)
			{
				cachedCurrentSelectedGameObject = base.eventSystem.currentSelectedGameObject;
				if (cachedCurrentSelectedGameObject != null)
				{
					cachedCurrentSelectedGameObjectInputField = cachedCurrentSelectedGameObject.GetComponent<InputField>();
				}
				else
				{
					cachedCurrentSelectedGameObjectInputField = null;
				}
			}
			if (cachedCurrentSelectedGameObjectInputField != null)
			{
				return true;
			}
			return false;
		}
	}

	private static void Execute(IPointerMoveHandler handler, BaseEventData eventData)
	{
		handler.OnPointerMove(ExecuteEvents.ValidateEventData<PointerEventData>(eventData));
	}

	private PointerEventData GetLookPointerEventData(bool isRight = false)
	{
		Vector2 position = default(Vector2);
		if (referenceCamera != null)
		{
			position.x = referenceCamera.pixelWidth / 2;
			position.y = referenceCamera.pixelHeight / 2;
		}
		else
		{
			position.x = XRSettings.eyeTextureWidth / 2;
			position.y = XRSettings.eyeTextureHeight / 2;
		}
		if (isRight)
		{
			if (lookDataRight == null)
			{
				lookDataRight = new PointerEventData(base.eventSystem);
			}
			lookDataRight.Reset();
			lookDataRight.delta = Vector2.zero;
			lookDataRight.scrollDelta = Vector2.zero;
			lookDataRight.position = position;
			m_RaycastResultCacheRight.Clear();
			base.eventSystem.RaycastAll(lookDataRight, m_RaycastResultCacheRight);
			lookDataRight.pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(m_RaycastResultCacheRight);
			if (lookDataRight.pointerCurrentRaycast.gameObject != null)
			{
				_guiRaycastHit = true;
			}
			else
			{
				_guiRaycastHit = false;
			}
			m_RaycastResultCacheRight.Clear();
			return lookDataRight;
		}
		if (lookData == null)
		{
			lookData = new PointerEventData(base.eventSystem);
		}
		lookData.Reset();
		lookData.delta = Vector2.zero;
		lookData.scrollDelta = Vector2.zero;
		lookData.position = position;
		m_RaycastResultCache.Clear();
		base.eventSystem.RaycastAll(lookData, m_RaycastResultCache);
		lookData.pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(m_RaycastResultCache);
		if (lookData.pointerCurrentRaycast.gameObject != null)
		{
			_guiRaycastHit = true;
		}
		else
		{
			_guiRaycastHit = false;
		}
		m_RaycastResultCache.Clear();
		return lookData;
	}

	private void UpdateCursor(PointerEventData lookData, RectTransform curs)
	{
		if (!(curs != null))
		{
			return;
		}
		if (useCursor)
		{
			if (lookData.pointerEnter != null)
			{
				RectTransform component = lookData.pointerEnter.GetComponent<RectTransform>();
				Vector3 worldPoint;
				if (RectTransformUtility.ScreenPointToWorldPointInRectangle(component, lookData.position, lookData.enterEventCamera, out worldPoint))
				{
					curs.gameObject.SetActive(true);
					curs.position = worldPoint;
					curs.rotation = component.rotation;
					if (scaleCursorWithDistance)
					{
						float num = (worldPoint - lookData.enterEventCamera.transform.position).magnitude / worldScale;
						float num2 = num * normalCursorScale;
						if (num2 < normalCursorScale)
						{
							num2 = normalCursorScale;
						}
						Vector3 localScale = default(Vector3);
						localScale.x = num2;
						localScale.y = num2;
						localScale.z = num2;
						curs.localScale = localScale;
					}
				}
				else
				{
					curs.gameObject.SetActive(false);
				}
			}
			else
			{
				curs.gameObject.SetActive(false);
			}
		}
		else
		{
			curs.gameObject.SetActive(false);
		}
	}

	private void SetSelectedColor(GameObject go)
	{
		if (!useSelectColor)
		{
			return;
		}
		if (!useSelectColorOnButton && (bool)go.GetComponent<Button>())
		{
			currentSelectedNormalColorValid = false;
			return;
		}
		if (!useSelectColorOnToggle && (bool)go.GetComponent<Toggle>())
		{
			currentSelectedNormalColorValid = false;
			return;
		}
		Selectable component = go.GetComponent<Selectable>();
		if (component != null)
		{
			ColorBlock colors = component.colors;
			currentSelectedNormalColor = colors.normalColor;
			currentSelectedNormalColorValid = true;
			currentSelectedHighlightedColor = colors.highlightedColor;
			colors.normalColor = selectColor;
			colors.highlightedColor = selectColor;
			component.colors = colors;
		}
	}

	private void RestoreColor(GameObject go)
	{
		if (useSelectColor && currentSelectedNormalColorValid)
		{
			Selectable component = go.GetComponent<Selectable>();
			if (component != null)
			{
				ColorBlock colors = component.colors;
				colors.normalColor = currentSelectedNormalColor;
				colors.highlightedColor = currentSelectedHighlightedColor;
				component.colors = colors;
			}
		}
	}

	private void ClearKeyboardInput()
	{
		currentInputField = null;
		currentKeyEventHandler = null;
		if (keyboardTransform != null)
		{
			keyboardTransform.gameObject.SetActive(false);
		}
		if (currentKeyboardTransform != null)
		{
			currentKeyboardTransform.gameObject.SetActive(false);
			currentKeyboardTransform = null;
		}
	}

	public new void ClearSelection()
	{
		if ((bool)base.eventSystem.currentSelectedGameObject)
		{
			if (controlCopy != null)
			{
				UnityEngine.Object.Destroy(controlCopy);
				controlCopy = null;
			}
			RestoreColor(base.eventSystem.currentSelectedGameObject);
			base.eventSystem.SetSelectedGameObject(null);
		}
		ClearKeyboardInput();
	}

	public void Select(GameObject go)
	{
		if (go != null)
		{
			if (!ExecuteEvents.GetEventHandler<ISelectHandler>(go) || (bool)go.GetComponent<IgnoreSelect>())
			{
				return;
			}
			ClearKeyboardInput();
			if (anchorForControlCopy != null)
			{
				Slider component = go.GetComponent<Slider>();
				UIPopup component2 = go.GetComponent<UIPopup>();
				Transform transform = null;
				if (component != null)
				{
					transform = component.transform.parent;
				}
				else if (component2 != null)
				{
					transform = component2.transform;
				}
				if (transform != null)
				{
					RectTransform component3 = transform.GetComponent<RectTransform>();
					Rect rect = component3.rect;
					Vector2 sizeDelta = default(Vector2);
					sizeDelta.x = rect.width;
					sizeDelta.y = rect.height;
					anchorForControlCopy.sizeDelta = sizeDelta;
					controlCopy = UnityEngine.Object.Instantiate(transform.gameObject);
					controlCopy.transform.SetParent(anchorForControlCopy);
					controlCopy.transform.localRotation = Quaternion.identity;
					controlCopy.transform.localPosition = Vector3.zero;
					controlCopy.transform.localScale = Vector3.one;
					RectTransform component4 = controlCopy.GetComponent<RectTransform>();
					component4.offsetMin = component3.offsetMin;
					component4.offsetMax = component3.offsetMax;
					component4.anchoredPosition = Vector3.zero;
					if (component != null)
					{
						Slider componentInChildren = controlCopy.GetComponentInChildren<Slider>();
						if (componentInChildren != null)
						{
							SliderTrack sliderTrack = componentInChildren.gameObject.AddComponent<SliderTrack>();
							sliderTrack.master = component;
						}
					}
					else if (component2 != null)
					{
						UIPopup component5 = controlCopy.GetComponent<UIPopup>();
						if (component5 != null)
						{
							UIPopupTrack uIPopupTrack = component5.gameObject.AddComponent<UIPopupTrack>();
							uIPopupTrack.master = component2;
						}
					}
				}
			}
			SetSelectedColor(go);
			if (base.eventSystem.currentSelectedGameObject == null || base.eventSystem.currentSelectedGameObject != go)
			{
				base.eventSystem.SetSelectedGameObject(go);
			}
			currentInputField = go.GetComponent<InputField>();
			if (currentInputField != null)
			{
				VRKeyboardLink componentInParent = currentInputField.GetComponentInParent<VRKeyboardLink>();
				if (componentInParent != null)
				{
					currentKeyboardTransform = componentInParent.VRKeyboardTransformToUse;
				}
				if (currentKeyboardTransform != null)
				{
					currentKeyboardTransform.gameObject.SetActive(true);
				}
				else if (keyboardTransform != null)
				{
					keyboardTransform.gameObject.SetActive(true);
				}
			}
			Component[] components = go.GetComponents<Component>();
			Component[] array = components;
			foreach (Component component6 in array)
			{
				if (component6 is KeyEventHandler)
				{
					currentKeyEventHandler = component6 as KeyEventHandler;
					break;
				}
			}
			if (currentKeyEventHandler != null && keyboardTransform != null)
			{
				keyboardTransform.gameObject.SetActive(true);
			}
		}
		else
		{
			ClearSelection();
		}
	}

	public static void SelectGameObject(GameObject go)
	{
		if (_singleton != null)
		{
			_singleton.Select(go);
		}
	}

	private new bool SendUpdateEventToSelectedObject()
	{
		if (base.eventSystem.currentSelectedGameObject == null)
		{
			return false;
		}
		BaseEventData baseEventData = GetBaseEventData();
		ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.updateSelectedHandler);
		return baseEventData.used;
	}

	public bool AxisControllerSelected()
	{
		if (base.eventSystem.currentSelectedGameObject == null)
		{
			return false;
		}
		Slider component = base.eventSystem.currentSelectedGameObject.GetComponent<Slider>();
		if (component != null)
		{
			return true;
		}
		Scrollbar component2 = base.eventSystem.currentSelectedGameObject.GetComponent<Scrollbar>();
		if (component2 != null)
		{
			return true;
		}
		UIPopupButton component3 = base.eventSystem.currentSelectedGameObject.GetComponent<UIPopupButton>();
		UIPopup uIPopup = ((!(component3 != null)) ? base.eventSystem.currentSelectedGameObject.GetComponent<UIPopup>() : component3.popupParent);
		if (uIPopup != null)
		{
			return true;
		}
		return false;
	}

	protected void HandleAxis(float newVal)
	{
		if (base.eventSystem.currentSelectedGameObject != null)
		{
			if (useSmoothAxis)
			{
				Slider component = base.eventSystem.currentSelectedGameObject.GetComponent<Slider>();
				if (component != null)
				{
					float num = component.maxValue - component.minValue;
					component.value += newVal * smoothAxisMultiplier * num;
					_controlAxisUsed = true;
				}
				else
				{
					Scrollbar component2 = base.eventSystem.currentSelectedGameObject.GetComponent<Scrollbar>();
					if (component2 != null)
					{
						component2.value += newVal * smoothAxisMultiplier;
						_controlAxisUsed = true;
					}
					else
					{
						UIPopupButton component3 = base.eventSystem.currentSelectedGameObject.GetComponent<UIPopupButton>();
						UIPopup uIPopup = ((!(component3 != null)) ? base.eventSystem.currentSelectedGameObject.GetComponent<UIPopup>() : component3.popupParent);
						if (uIPopup != null)
						{
							_controlAxisUsed = true;
							if (axisAccumulation > 0.3f)
							{
								uIPopup.SetNextValue();
								axisAccumulation = 0f;
							}
							else if (axisAccumulation < -0.3f)
							{
								uIPopup.SetPreviousValue();
								axisAccumulation = 0f;
							}
						}
						else
						{
							_controlAxisUsed = false;
						}
					}
				}
			}
			else
			{
				_controlAxisUsed = true;
				float unscaledTime = Time.unscaledTime;
				if (unscaledTime > nextAxisActionTime)
				{
					nextAxisActionTime = unscaledTime + 1f / steppedAxisStepsPerSecond;
					AxisEventData axisEventData = GetAxisEventData(newVal, 0f, 0f);
					if (!ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler))
					{
						_controlAxisUsed = false;
					}
				}
			}
		}
		if (useSmoothAxis && !_controlAxisUsed && UITabSelector.activeTabSelector != null)
		{
			_controlAxisUsed = true;
			if (axisAccumulation > 0.6f)
			{
				UITabSelector.activeTabSelector.SelectNextTab();
				axisAccumulation = 0f;
			}
			else if (axisAccumulation < -0.6f)
			{
				UITabSelector.activeTabSelector.SelectPreviousTab();
				axisAccumulation = 0f;
			}
		}
	}

	protected bool GetSubmitLeftButtonDown()
	{
		if (OVRManager.isHmdPresent)
		{
			return OVRInput.GetDown(OVRInput.Button.Three, OVRInput.Controller.Touch);
		}
		return interactAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
	}

	protected bool GetSubmitLeftButtonUp()
	{
		if (OVRManager.isHmdPresent)
		{
			return OVRInput.GetUp(OVRInput.Button.Three, OVRInput.Controller.Touch);
		}
		return interactAction.GetStateUp(SteamVR_Input_Sources.LeftHand);
	}

	protected bool GetSubmitRightButtonDown()
	{
		if (OVRManager.isHmdPresent)
		{
			return OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.Touch);
		}
		return interactAction.GetStateDown(SteamVR_Input_Sources.RightHand);
	}

	protected bool GetSubmitRightButtonUp()
	{
		if (OVRManager.isHmdPresent)
		{
			return OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.Touch);
		}
		return interactAction.GetStateUp(SteamVR_Input_Sources.RightHand);
	}

	public void ProcessMain()
	{
		SendUpdateEventToSelectedObject();
		PointerEventData lookPointerEventData = GetLookPointerEventData();
		currentLook = lookPointerEventData.pointerCurrentRaycast.gameObject;
		if (deselectWhenLookAway && currentLook == null && currentLookRight == null)
		{
			ClearSelection();
		}
		HandlePointerExitAndEnter(lookPointerEventData, currentLook);
		UpdateCursor(lookPointerEventData, cursor);
		if ((!ignoreInputsWhenLookAway || (ignoreInputsWhenLookAway && currentLook != null)) && (GetSubmitLeftButtonDown() || (useEitherControllerButtons && GetSubmitRightButtonDown())))
		{
			lookPointerEventData.pressPosition = lookPointerEventData.position;
			lookPointerEventData.pointerPressRaycast = lookPointerEventData.pointerCurrentRaycast;
			lookPointerEventData.pointerPress = null;
			if (currentLook != null)
			{
				GameObject gameObject = null;
				if (mode == Mode.Pointer)
				{
					gameObject = ExecuteEvents.ExecuteHierarchy(currentLook, lookPointerEventData, ExecuteEvents.pointerDownHandler);
					if (gameObject == null)
					{
						gameObject = ExecuteEvents.ExecuteHierarchy(currentLook, lookPointerEventData, ExecuteEvents.pointerClickHandler);
						if (gameObject == null)
						{
							GameObject gameObject3 = (lookPointerEventData.pointerDrag = ExecuteEvents.ExecuteHierarchy(currentLook, lookPointerEventData, ExecuteEvents.beginDragHandler));
							currentDragging = gameObject3;
						}
					}
					else
					{
						currentPressed = gameObject;
						ExecuteEvents.Execute(gameObject, lookPointerEventData, ExecuteEvents.pointerClickHandler);
					}
					if (gameObject != null)
					{
						lookPointerEventData.pointerPress = gameObject;
						Select(gameObject);
						SliderControl component = gameObject.GetComponent<SliderControl>();
						if (component == null || !component.disableLookDrag)
						{
							ExecuteEvents.Execute(gameObject, lookPointerEventData, ExecuteEvents.beginDragHandler);
							lookPointerEventData.pointerDrag = gameObject;
							currentDragging = gameObject;
						}
					}
				}
				else if (mode == Mode.Submit)
				{
					gameObject = ExecuteEvents.ExecuteHierarchy(currentPressed, lookPointerEventData, ExecuteEvents.submitHandler);
					if (gameObject == null)
					{
						gameObject = ExecuteEvents.ExecuteHierarchy(currentPressed, lookPointerEventData, ExecuteEvents.selectHandler);
						if (gameObject != null)
						{
							lookPointerEventData.pointerPress = gameObject;
							currentPressed = gameObject;
							Select(gameObject);
						}
					}
				}
			}
		}
		if (GetSubmitLeftButtonUp() || (useEitherControllerButtons && GetSubmitRightButtonUp()))
		{
			if ((bool)currentDragging)
			{
				ExecuteEvents.Execute(currentDragging, lookPointerEventData, ExecuteEvents.endDragHandler);
				if (currentLook != null)
				{
					ExecuteEvents.ExecuteHierarchy(currentLook, lookPointerEventData, ExecuteEvents.dropHandler);
				}
				lookPointerEventData.pointerDrag = null;
				currentDragging = null;
			}
			if ((bool)currentPressed)
			{
				ExecuteEvents.Execute(currentPressed, lookPointerEventData, ExecuteEvents.pointerUpHandler);
				lookPointerEventData.rawPointerPress = null;
				lookPointerEventData.pointerPress = null;
				currentPressed = null;
			}
		}
		if (currentDragging != null)
		{
			ExecuteEvents.Execute(currentDragging, lookPointerEventData, ExecuteEvents.dragHandler);
		}
		if (currentLook != null)
		{
			ExecuteEvents.Execute(currentLook, lookPointerEventData, pointerMoveHandler);
		}
		if (!OVRManager.isHmdPresent)
		{
			Vector2 axis = scrollAction.GetAxis(SteamVR_Input_Sources.LeftHand);
			if (currentLook != null && !Mathf.Approximately(axis.sqrMagnitude, 0f))
			{
				GameObject eventHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(currentLook);
				if (float.IsNaN(axis.x))
				{
					axis.x = 0f;
				}
				if (float.IsNaN(axis.y))
				{
					axis.y = 0f;
				}
				axis.x = Mathf.Clamp(axis.x, -100f, 100f) * 100f;
				axis.y = Mathf.Clamp(axis.y, -100f, 100f) * 100f;
				lookPointerEventData.scrollDelta = axis;
				ExecuteEvents.ExecuteHierarchy(eventHandler, lookPointerEventData, ExecuteEvents.scrollHandler);
			}
		}
		if (ignoreInputsWhenLookAway && (!ignoreInputsWhenLookAway || !(currentLook != null)))
		{
			return;
		}
		_controlAxisUsed = false;
		if ((bool)base.eventSystem.currentSelectedGameObject || (bool)UITabSelector.activeTabSelector)
		{
			_controlAxisUsed = true;
			float axis2 = JoystickControl.GetAxis(controlAxis);
			if (axis2 > 0.01f || axis2 < -0.01f)
			{
				if (!axisOn)
				{
					axisAccumulation = Mathf.Sign(axis2);
				}
				else
				{
					axisAccumulation += axis2 * Time.deltaTime;
				}
				axisOn = true;
				HandleAxis(axis2);
			}
			else
			{
				axisOn = false;
			}
			axis2 = JoystickControl.GetAxis(discreteControlAxis);
			if (axis2 > 0.01f || axis2 < -0.01f)
			{
				if (invertDiscreteControlAxis)
				{
					axis2 = 0f - axis2;
				}
				if (!discreteAxisOn)
				{
					axisAccumulation = Mathf.Sign(axis2);
				}
				else
				{
					axisAccumulation += axis2 * Time.deltaTime;
				}
				discreteAxisOn = true;
				HandleAxis(axis2);
			}
			else
			{
				discreteAxisOn = false;
			}
		}
		else
		{
			axisAccumulation = 0f;
		}
	}

	public void ProcessMouse()
	{
		SendUpdateEventToSelectedObject();
		MouseState mousePointerEventData = GetMousePointerEventData();
		PointerEventData buttonData = mousePointerEventData.GetButtonState(PointerEventData.InputButton.Left).eventData.buttonData;
		currentLookMouse = buttonData.pointerCurrentRaycast.gameObject;
		if (currentLookMouse != null)
		{
			_mouseRaycastHit = true;
		}
		else
		{
			_mouseRaycastHit = false;
		}
		if (deselectWhenLookAway && currentLookMouse == null && currentLook == null && currentLookRight == null)
		{
			ClearSelection();
		}
		HandlePointerExitAndEnter(buttonData, currentLookMouse);
		if ((!ignoreInputsWhenLookAway || (ignoreInputsWhenLookAway && currentLookMouse != null)) && Input.GetMouseButtonDown(0))
		{
			buttonData.pressPosition = buttonData.position;
			buttonData.pointerPressRaycast = buttonData.pointerCurrentRaycast;
			buttonData.pointerPress = null;
			if (currentLookMouse != null)
			{
				GameObject gameObject = ExecuteEvents.ExecuteHierarchy(currentLookMouse, buttonData, ExecuteEvents.pointerDownHandler);
				if (gameObject == null)
				{
					gameObject = ExecuteEvents.ExecuteHierarchy(currentLookMouse, buttonData, ExecuteEvents.pointerClickHandler);
					if (gameObject == null)
					{
						GameObject gameObject3 = (buttonData.pointerDrag = ExecuteEvents.ExecuteHierarchy(currentLookMouse, buttonData, ExecuteEvents.beginDragHandler));
						currentDraggingMouse = gameObject3;
					}
				}
				else
				{
					currentPressedMouse = gameObject;
				}
				if (gameObject != null)
				{
					buttonData.pointerPress = gameObject;
					Select(gameObject);
					SliderControl component = gameObject.GetComponent<SliderControl>();
					if (component == null || !component.disableLookDrag)
					{
						ExecuteEvents.Execute(gameObject, buttonData, ExecuteEvents.beginDragHandler);
						buttonData.pointerDrag = gameObject;
						currentDraggingMouse = gameObject;
					}
				}
			}
		}
		if (Input.GetMouseButtonUp(0))
		{
			if ((bool)currentDraggingMouse)
			{
				ExecuteEvents.Execute(currentDraggingMouse, buttonData, ExecuteEvents.endDragHandler);
				if (currentLookMouse != null)
				{
					ExecuteEvents.ExecuteHierarchy(currentLookMouse, buttonData, ExecuteEvents.dropHandler);
				}
				buttonData.pointerDrag = null;
				currentDraggingMouse = null;
			}
			if ((bool)currentPressedMouse)
			{
				ExecuteEvents.Execute(currentPressedMouse, buttonData, ExecuteEvents.pointerUpHandler);
				ExecuteEvents.Execute(currentPressedMouse, buttonData, ExecuteEvents.pointerClickHandler);
				buttonData.rawPointerPress = null;
				buttonData.pointerPress = null;
				currentPressedMouse = null;
			}
		}
		if (currentDraggingMouse != null)
		{
			ExecuteEvents.Execute(currentDraggingMouse, buttonData, ExecuteEvents.dragHandler);
		}
		if (currentLookMouse != null)
		{
			ExecuteEvents.Execute(currentLookMouse, buttonData, pointerMoveHandler);
		}
		if (buttonData.pointerCurrentRaycast.gameObject != null && !Mathf.Approximately(buttonData.scrollDelta.sqrMagnitude, 0f))
		{
			GameObject eventHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(buttonData.pointerCurrentRaycast.gameObject);
			Vector2 scrollDelta = buttonData.scrollDelta * 100f;
			buttonData.scrollDelta = scrollDelta;
			ExecuteEvents.ExecuteHierarchy(eventHandler, buttonData, ExecuteEvents.scrollHandler);
		}
	}

	private void ProcessMousePressAlt(MouseButtonEventData data)
	{
		PointerEventData buttonData = data.buttonData;
		GameObject gameObject = buttonData.pointerCurrentRaycast.gameObject;
		if (data.PressedThisFrame())
		{
			buttonData.eligibleForClick = true;
			buttonData.delta = Vector2.zero;
			buttonData.dragging = false;
			buttonData.useDragThreshold = true;
			buttonData.pressPosition = buttonData.position;
			buttonData.pointerPressRaycast = buttonData.pointerCurrentRaycast;
			if (gameObject != base.eventSystem.currentSelectedGameObject)
			{
				ClearKeyboardInput();
			}
			DeselectIfSelectionChanged(gameObject, buttonData);
			GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy(gameObject, buttonData, ExecuteEvents.pointerDownHandler);
			if (gameObject2 == null)
			{
				gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			}
			float unscaledTime = Time.unscaledTime;
			if (gameObject2 == buttonData.lastPress)
			{
				float num = unscaledTime - buttonData.clickTime;
				if (num < 0.3f)
				{
					buttonData.clickCount++;
				}
				else
				{
					buttonData.clickCount = 1;
				}
				buttonData.clickTime = unscaledTime;
			}
			else
			{
				buttonData.clickCount = 1;
			}
			buttonData.pointerPress = gameObject2;
			buttonData.rawPointerPress = gameObject;
			buttonData.clickTime = unscaledTime;
			buttonData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
			if (buttonData.pointerDrag != null)
			{
				ExecuteEvents.Execute(buttonData.pointerDrag, buttonData, ExecuteEvents.initializePotentialDrag);
			}
		}
		if (data.ReleasedThisFrame())
		{
			ExecuteEvents.Execute(buttonData.pointerPress, buttonData, ExecuteEvents.pointerUpHandler);
			GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			if (buttonData.pointerPress == eventHandler && buttonData.eligibleForClick)
			{
				ExecuteEvents.Execute(buttonData.pointerPress, buttonData, ExecuteEvents.pointerClickHandler);
				Select(buttonData.pointerPress);
			}
			else if (buttonData.pointerDrag != null)
			{
				ExecuteEvents.ExecuteHierarchy(gameObject, buttonData, ExecuteEvents.dropHandler);
			}
			buttonData.eligibleForClick = false;
			buttonData.pointerPress = null;
			buttonData.rawPointerPress = null;
			if (buttonData.pointerDrag != null && buttonData.dragging)
			{
				ExecuteEvents.Execute(buttonData.pointerDrag, buttonData, ExecuteEvents.endDragHandler);
			}
			buttonData.dragging = false;
			buttonData.pointerDrag = null;
			if (gameObject != buttonData.pointerEnter)
			{
				HandlePointerExitAndEnter(buttonData, null);
				HandlePointerExitAndEnter(buttonData, gameObject);
			}
		}
	}

	public void ProcessMouseAlt(bool useCenterOfCamera = false)
	{
		SendUpdateEventToSelectedObject();
		MouseState mousePointerEventData = GetMousePointerEventData();
		MouseButtonEventData eventData = mousePointerEventData.GetButtonState(PointerEventData.InputButton.Left).eventData;
		PointerEventData buttonData = eventData.buttonData;
		if (useCenterOfCamera)
		{
			Vector3 mousePosition = Input.mousePosition;
			Vector2 position = default(Vector2);
			position.x = mousePosition.x;
			position.y = mousePosition.y;
			buttonData.delta = Vector2.zero;
			buttonData.position = position;
			m_RaycastResultCache.Clear();
			base.eventSystem.RaycastAll(buttonData, m_RaycastResultCache);
			buttonData.pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(m_RaycastResultCache);
			m_RaycastResultCache.Clear();
			eventData.buttonData = buttonData;
			currentLookMouse = buttonData.pointerCurrentRaycast.gameObject;
			HandlePointerExitAndEnter(buttonData, currentLookMouse);
			UpdateCursor(buttonData, cursorMouse);
		}
		else
		{
			if (cursorMouse != null)
			{
				cursorMouse.gameObject.SetActive(false);
			}
			currentLookMouse = buttonData.pointerCurrentRaycast.gameObject;
		}
		if (currentLookMouse != null)
		{
			_mouseRaycastHit = true;
		}
		else
		{
			_mouseRaycastHit = false;
		}
		bool pressed = mousePointerEventData.AnyPressesThisFrame();
		bool released = mousePointerEventData.AnyReleasesThisFrame();
		if (UseMouse(pressed, released, buttonData) || useCenterOfCamera)
		{
			ProcessMousePressAlt(eventData);
			ProcessMove(buttonData);
			if (currentLookMouse != null)
			{
				ExecuteEvents.Execute(currentLookMouse, buttonData, pointerMoveHandler);
			}
			ProcessDrag(buttonData);
			ProcessMousePress(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData);
			ProcessDrag(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
			ProcessMousePress(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
			ProcessDrag(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);
			if (eventData.buttonData.pointerCurrentRaycast.gameObject != null && !Mathf.Approximately(eventData.buttonData.scrollDelta.sqrMagnitude, 0f))
			{
				GameObject eventHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(eventData.buttonData.pointerCurrentRaycast.gameObject);
				buttonData.scrollDelta *= 100f;
				ExecuteEvents.ExecuteHierarchy(eventHandler, buttonData, ExecuteEvents.scrollHandler);
			}
		}
	}

	private static bool UseMouse(bool pressed, bool released, PointerEventData pointerData)
	{
		if (pressed || released || pointerData.IsPointerMoving() || pointerData.IsScrolling())
		{
			return true;
		}
		return false;
	}

	public override void Process()
	{
		_singleton = this;
		if (currentInputField != null && !currentInputField.gameObject.activeInHierarchy)
		{
			ClearKeyboardInput();
		}
		if (!disableStandaloneProcess)
		{
			base.Process();
		}
	}

	public void HideCursors()
	{
		if (cursor != null)
		{
			cursor.gameObject.SetActive(false);
		}
		if (cursorRight != null)
		{
			cursorRight.gameObject.SetActive(false);
		}
		if (cursorMouse != null)
		{
			cursorMouse.gameObject.SetActive(false);
		}
	}

	public void ProcessRight()
	{
		_singleton = this;
		SendUpdateEventToSelectedObject();
		PointerEventData lookPointerEventData = GetLookPointerEventData(true);
		currentLookRight = lookPointerEventData.pointerCurrentRaycast.gameObject;
		HandlePointerExitAndEnter(lookPointerEventData, currentLookRight);
		UpdateCursor(lookPointerEventData, cursorRight);
		if ((!ignoreInputsWhenLookAway || (ignoreInputsWhenLookAway && currentLookRight != null)) && GetSubmitRightButtonDown())
		{
			lookPointerEventData.pressPosition = lookPointerEventData.position;
			lookPointerEventData.pointerPressRaycast = lookPointerEventData.pointerCurrentRaycast;
			lookPointerEventData.pointerPress = null;
			if (currentLookRight != null)
			{
				GameObject gameObject = null;
				if (mode == Mode.Pointer)
				{
					gameObject = ExecuteEvents.ExecuteHierarchy(currentLookRight, lookPointerEventData, ExecuteEvents.pointerDownHandler);
					if (gameObject == null)
					{
						gameObject = ExecuteEvents.ExecuteHierarchy(currentLookRight, lookPointerEventData, ExecuteEvents.pointerClickHandler);
						if (gameObject == null)
						{
							GameObject gameObject3 = (lookPointerEventData.pointerDrag = ExecuteEvents.ExecuteHierarchy(currentLookRight, lookPointerEventData, ExecuteEvents.beginDragHandler));
							currentDraggingRight = gameObject3;
						}
					}
					else
					{
						currentPressedRight = gameObject;
						ExecuteEvents.Execute(gameObject, lookPointerEventData, ExecuteEvents.pointerClickHandler);
					}
					if (gameObject != null)
					{
						lookPointerEventData.pointerPress = gameObject;
						Select(gameObject);
						SliderControl component = gameObject.GetComponent<SliderControl>();
						if (component == null || !component.disableLookDrag)
						{
							ExecuteEvents.Execute(gameObject, lookPointerEventData, ExecuteEvents.beginDragHandler);
							lookPointerEventData.pointerDrag = gameObject;
							currentDraggingRight = gameObject;
						}
					}
				}
				else if (mode == Mode.Submit)
				{
					gameObject = ExecuteEvents.ExecuteHierarchy(currentLookRight, lookPointerEventData, ExecuteEvents.submitHandler);
					if (gameObject == null)
					{
						gameObject = ExecuteEvents.ExecuteHierarchy(currentLookRight, lookPointerEventData, ExecuteEvents.selectHandler);
					}
					if (gameObject != null)
					{
						lookPointerEventData.pointerPress = gameObject;
						currentPressedRight = gameObject;
						Select(gameObject);
					}
				}
			}
		}
		if (GetSubmitRightButtonUp())
		{
			if ((bool)currentDraggingRight)
			{
				ExecuteEvents.Execute(currentDraggingRight, lookPointerEventData, ExecuteEvents.endDragHandler);
				if (currentLookRight != null)
				{
					ExecuteEvents.ExecuteHierarchy(currentLookRight, lookPointerEventData, ExecuteEvents.dropHandler);
				}
				lookPointerEventData.pointerDrag = null;
				currentDraggingRight = null;
			}
			if ((bool)currentPressedRight)
			{
				ExecuteEvents.Execute(currentPressedRight, lookPointerEventData, ExecuteEvents.pointerUpHandler);
				lookPointerEventData.rawPointerPress = null;
				lookPointerEventData.pointerPress = null;
				currentPressedRight = null;
			}
		}
		if (currentDraggingRight != null)
		{
			ExecuteEvents.Execute(currentDraggingRight, lookPointerEventData, ExecuteEvents.dragHandler);
		}
		if (currentLookRight != null)
		{
			ExecuteEvents.Execute(currentLookRight, lookPointerEventData, pointerMoveHandler);
		}
		if (OVRManager.isHmdPresent)
		{
			return;
		}
		Vector2 axis = scrollAction.GetAxis(SteamVR_Input_Sources.RightHand);
		if (currentLookRight != null && !Mathf.Approximately(axis.sqrMagnitude, 0f))
		{
			GameObject eventHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(currentLookRight);
			if (float.IsNaN(axis.x))
			{
				axis.x = 0f;
			}
			if (float.IsNaN(axis.y))
			{
				axis.y = 0f;
			}
			axis.x = Mathf.Clamp(axis.x, -100f, 100f) * 100f;
			axis.y = Mathf.Clamp(axis.y, -100f, 100f) * 100f;
			lookPointerEventData.scrollDelta = axis;
			ExecuteEvents.ExecuteHierarchy(eventHandler, lookPointerEventData, ExecuteEvents.scrollHandler);
		}
	}

	protected void AddCharacterEventToList(List<Event> elist, char c)
	{
		Event @event = new Event();
		@event.keyCode = KeyCode.None;
		@event.type = EventType.KeyDown;
		@event.character = c;
		elist.Add(@event);
	}

	protected void AddKeyboardEventToList(List<Event> elist, string s)
	{
		Event @event = Event.KeyboardEvent(s);
		@event.type = EventType.KeyDown;
		@event.character = '\0';
		elist.Add(@event);
		@event = Event.KeyboardEvent(s);
		@event.type = EventType.KeyUp;
		@event.character = '\0';
		elist.Add(@event);
	}

	public void OnKeyClick(string value)
	{
		List<Event> list = new List<Event>();
		switch (value)
		{
		case "BACK":
			AddKeyboardEventToList(list, "\b");
			break;
		case "ENTER":
			AddCharacterEventToList(list, '\n');
			break;
		case ".com":
			AddCharacterEventToList(list, '.');
			AddCharacterEventToList(list, 'c');
			AddCharacterEventToList(list, 'o');
			AddCharacterEventToList(list, 'm');
			break;
		default:
			AddCharacterEventToList(list, value[0]);
			break;
		}
		if (currentInputField != null)
		{
			foreach (Event item in list)
			{
				if (item.type == EventType.KeyUp)
				{
					continue;
				}
				if (item.character == '\n' && currentInputField.lineType != InputField.LineType.MultiLineNewline)
				{
					InputFieldAction component = currentInputField.GetComponent<InputFieldAction>();
					if (component != null)
					{
						component.Submit();
						continue;
					}
					GameObject eventHandler = ExecuteEvents.GetEventHandler<ISubmitHandler>(currentInputField.gameObject);
					if (eventHandler != null)
					{
						ExecuteEvents.ExecuteHierarchy(eventHandler, null, ExecuteEvents.submitHandler);
					}
				}
				else
				{
					currentInputField.ProcessEvent(item);
				}
			}
			currentInputField.ForceLabelUpdate();
		}
		if (currentKeyEventHandler == null)
		{
			return;
		}
		foreach (Event item2 in list)
		{
			currentKeyEventHandler.AddKeyEvent(item2);
		}
	}

	protected override void Awake()
	{
		_singleton = this;
	}

	protected override void Start()
	{
		if (keyboardTransform != null)
		{
			keyboardTransform.gameObject.SetActive(false);
		}
		VRKeyboardFull[] array = vrKeyboards;
		foreach (VRKeyboardFull vRKeyboardFull in array)
		{
			if (vRKeyboardFull != null)
			{
				vRKeyboardFull.Init();
				vRKeyboardFull.OnVRKeyboardBtnClick = (VRKeyboardBase.VRKeyboardBtnClick)Delegate.Combine(vRKeyboardFull.OnVRKeyboardBtnClick, new VRKeyboardBase.VRKeyboardBtnClick(OnKeyClick));
			}
		}
		HideCursors();
	}

	protected override void OnDestroy()
	{
		VRKeyboardFull[] array = vrKeyboards;
		foreach (VRKeyboardFull vRKeyboardFull in array)
		{
			if (vRKeyboardFull != null)
			{
				vRKeyboardFull.OnVRKeyboardBtnClick = (VRKeyboardBase.VRKeyboardBtnClick)Delegate.Remove(vRKeyboardFull.OnVRKeyboardBtnClick, new VRKeyboardBase.VRKeyboardBtnClick(OnKeyClick));
			}
		}
	}
}
