using System;
using System.Collections.Generic;

namespace UnityEngine.EventSystems
{
	public class OVRInputModule : PointerInputModule
	{
		[Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
		public enum InputMode
		{
			Mouse,
			Buttons
		}

		[Tooltip("Object which points with Z axis. E.g. CentreEyeAnchor from OVRCameraRig")]
		public Transform rayTransform;

		[Tooltip("Gamepad button to act as gaze click")]
		public OVRInput.Button joyPadClickButton = OVRInput.Button.One;

		[Tooltip("Keyboard button to act as gaze click")]
		public KeyCode gazeClickKey = KeyCode.Space;

		[Header("Physics")]
		[Tooltip("Perform an sphere cast to determine correct depth for gaze pointer")]
		public bool performSphereCastForGazepointer;

		[Tooltip("Match the gaze pointer normal to geometry normal for physics colliders")]
		public bool matchNormalOnPhysicsColliders;

		[Header("Gamepad Stick Scroll")]
		[Tooltip("Enable scrolling with the right stick on a gamepad")]
		public bool useRightStickScroll = true;

		[Tooltip("Deadzone for right stick to prevent accidental scrolling")]
		public float rightStickDeadZone = 0.15f;

		[Header("Touchpad Swipe Scroll")]
		[Tooltip("Enable scrolling by swiping the GearVR touchpad")]
		public bool useSwipeScroll = true;

		[Tooltip("Minimum trackpad movement in pixels to start swiping")]
		public float swipeDragThreshold = 2f;

		[Tooltip("Distance scrolled when swipe scroll occurs")]
		public float swipeDragScale = 1f;

		[Tooltip("Invert X axis on touchpad")]
		public bool InvertSwipeXAxis;

		[NonSerialized]
		public OVRRaycaster activeGraphicRaycaster;

		[Header("Dragging")]
		[Tooltip("Minimum pointer movement in degrees to start dragging")]
		public float angleDragThreshold = 1f;

		private float m_NextAction;

		private Vector2 m_LastMousePosition;

		private Vector2 m_MousePosition;

		[Header("Standalone Input Module")]
		[SerializeField]
		private string m_HorizontalAxis = "Horizontal";

		[SerializeField]
		private string m_VerticalAxis = "Vertical";

		[SerializeField]
		private string m_SubmitButton = "Submit";

		[SerializeField]
		private string m_CancelButton = "Cancel";

		[SerializeField]
		private float m_InputActionsPerSecond = 10f;

		[SerializeField]
		private bool m_AllowActivationOnMobileDevice;

		protected Dictionary<int, OVRPointerEventData> m_VRRayPointerData = new Dictionary<int, OVRPointerEventData>();

		private readonly MouseState m_MouseState = new MouseState();

		[Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
		public InputMode inputMode => InputMode.Mouse;

		public bool allowActivationOnMobileDevice
		{
			get
			{
				return m_AllowActivationOnMobileDevice;
			}
			set
			{
				m_AllowActivationOnMobileDevice = value;
			}
		}

		public float inputActionsPerSecond
		{
			get
			{
				return m_InputActionsPerSecond;
			}
			set
			{
				m_InputActionsPerSecond = value;
			}
		}

		public string horizontalAxis
		{
			get
			{
				return m_HorizontalAxis;
			}
			set
			{
				m_HorizontalAxis = value;
			}
		}

		public string verticalAxis
		{
			get
			{
				return m_VerticalAxis;
			}
			set
			{
				m_VerticalAxis = value;
			}
		}

		public string submitButton
		{
			get
			{
				return m_SubmitButton;
			}
			set
			{
				m_SubmitButton = value;
			}
		}

		public string cancelButton
		{
			get
			{
				return m_CancelButton;
			}
			set
			{
				m_CancelButton = value;
			}
		}

		protected OVRInputModule()
		{
		}

		public override void UpdateModule()
		{
			m_LastMousePosition = m_MousePosition;
			m_MousePosition = Input.mousePosition;
		}

		public override bool IsModuleSupported()
		{
			return m_AllowActivationOnMobileDevice || Input.mousePresent;
		}

		public override bool ShouldActivateModule()
		{
			if (!base.ShouldActivateModule())
			{
				return false;
			}
			bool buttonDown = Input.GetButtonDown(m_SubmitButton);
			buttonDown |= Input.GetButtonDown(m_CancelButton);
			buttonDown |= !Mathf.Approximately(Input.GetAxisRaw(m_HorizontalAxis), 0f);
			buttonDown |= !Mathf.Approximately(Input.GetAxisRaw(m_VerticalAxis), 0f);
			buttonDown |= (m_MousePosition - m_LastMousePosition).sqrMagnitude > 0f;
			return buttonDown | Input.GetMouseButtonDown(0);
		}

		public override void ActivateModule()
		{
			base.ActivateModule();
			m_MousePosition = Input.mousePosition;
			m_LastMousePosition = Input.mousePosition;
			GameObject gameObject = base.eventSystem.currentSelectedGameObject;
			if (gameObject == null)
			{
				gameObject = base.eventSystem.firstSelectedGameObject;
			}
			base.eventSystem.SetSelectedGameObject(gameObject, GetBaseEventData());
		}

		public override void DeactivateModule()
		{
			base.DeactivateModule();
			ClearSelection();
		}

		private bool SendSubmitEventToSelectedObject()
		{
			if (base.eventSystem.currentSelectedGameObject == null)
			{
				return false;
			}
			BaseEventData baseEventData = GetBaseEventData();
			if (Input.GetButtonDown(m_SubmitButton))
			{
				ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.submitHandler);
			}
			if (Input.GetButtonDown(m_CancelButton))
			{
				ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.cancelHandler);
			}
			return baseEventData.used;
		}

		private bool AllowMoveEventProcessing(float time)
		{
			bool buttonDown = Input.GetButtonDown(m_HorizontalAxis);
			buttonDown |= Input.GetButtonDown(m_VerticalAxis);
			return buttonDown | (time > m_NextAction);
		}

		private Vector2 GetRawMoveVector()
		{
			Vector2 zero = Vector2.zero;
			zero.x = Input.GetAxisRaw(m_HorizontalAxis);
			zero.y = Input.GetAxisRaw(m_VerticalAxis);
			if (Input.GetButtonDown(m_HorizontalAxis))
			{
				if (zero.x < 0f)
				{
					zero.x = -1f;
				}
				if (zero.x > 0f)
				{
					zero.x = 1f;
				}
			}
			if (Input.GetButtonDown(m_VerticalAxis))
			{
				if (zero.y < 0f)
				{
					zero.y = -1f;
				}
				if (zero.y > 0f)
				{
					zero.y = 1f;
				}
			}
			return zero;
		}

		private bool SendMoveEventToSelectedObject()
		{
			float unscaledTime = Time.unscaledTime;
			if (!AllowMoveEventProcessing(unscaledTime))
			{
				return false;
			}
			Vector2 rawMoveVector = GetRawMoveVector();
			AxisEventData axisEventData = GetAxisEventData(rawMoveVector.x, rawMoveVector.y, 0.6f);
			if (!Mathf.Approximately(axisEventData.moveVector.x, 0f) || !Mathf.Approximately(axisEventData.moveVector.y, 0f))
			{
				ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
			}
			m_NextAction = unscaledTime + 1f / m_InputActionsPerSecond;
			return axisEventData.used;
		}

		private bool SendUpdateEventToSelectedObject()
		{
			if (base.eventSystem.currentSelectedGameObject == null)
			{
				return false;
			}
			BaseEventData baseEventData = GetBaseEventData();
			ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.updateSelectedHandler);
			return baseEventData.used;
		}

		private void ProcessMousePress(MouseButtonEventData data)
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
				if (buttonData.IsVRPointer())
				{
					buttonData.SetSwipeStart(Input.mousePosition);
				}
				buttonData.pointerPressRaycast = buttonData.pointerCurrentRaycast;
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

		private void ProcessMouseEvent(MouseState mouseData)
		{
			bool pressed = mouseData.AnyPressesThisFrame();
			bool released = mouseData.AnyReleasesThisFrame();
			MouseButtonEventData eventData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;
			if (UseMouse(pressed, released, eventData.buttonData))
			{
				ProcessMousePress(eventData);
				ProcessMove(eventData.buttonData);
				ProcessDrag(eventData.buttonData);
				ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData);
				ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
				ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
				ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);
				if (!Mathf.Approximately(eventData.buttonData.scrollDelta.sqrMagnitude, 0f))
				{
					GameObject eventHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(eventData.buttonData.pointerCurrentRaycast.gameObject);
					ExecuteEvents.ExecuteHierarchy(eventHandler, eventData.buttonData, ExecuteEvents.scrollHandler);
				}
			}
		}

		public override void Process()
		{
			bool flag = SendUpdateEventToSelectedObject();
			if (base.eventSystem.sendNavigationEvents)
			{
				if (!flag)
				{
					flag |= SendMoveEventToSelectedObject();
				}
				if (!flag)
				{
					SendSubmitEventToSelectedObject();
				}
			}
			ProcessMouseEvent(GetGazePointerData());
			ProcessMouseEvent(GetCanvasPointerData());
		}

		private static bool UseMouse(bool pressed, bool released, PointerEventData pointerData)
		{
			if (pressed || released || IsPointerMoving(pointerData) || pointerData.IsScrolling())
			{
				return true;
			}
			return false;
		}

		protected void CopyFromTo(OVRPointerEventData from, OVRPointerEventData to)
		{
			to.position = from.position;
			to.delta = from.delta;
			to.scrollDelta = from.scrollDelta;
			to.pointerCurrentRaycast = from.pointerCurrentRaycast;
			to.pointerEnter = from.pointerEnter;
			to.worldSpaceRay = from.worldSpaceRay;
		}

		protected new void CopyFromTo(PointerEventData from, PointerEventData to)
		{
			to.position = from.position;
			to.delta = from.delta;
			to.scrollDelta = from.scrollDelta;
			to.pointerCurrentRaycast = from.pointerCurrentRaycast;
			to.pointerEnter = from.pointerEnter;
		}

		protected bool GetPointerData(int id, out OVRPointerEventData data, bool create)
		{
			if (!m_VRRayPointerData.TryGetValue(id, out data) && create)
			{
				data = new OVRPointerEventData(base.eventSystem)
				{
					pointerId = id
				};
				m_VRRayPointerData.Add(id, data);
				return true;
			}
			return false;
		}

		protected new void ClearSelection()
		{
			BaseEventData baseEventData = GetBaseEventData();
			foreach (PointerEventData value in m_PointerData.Values)
			{
				HandlePointerExitAndEnter(value, null);
			}
			foreach (OVRPointerEventData value2 in m_VRRayPointerData.Values)
			{
				HandlePointerExitAndEnter(value2, null);
			}
			m_PointerData.Clear();
			base.eventSystem.SetSelectedGameObject(null, baseEventData);
		}

		private static Vector3 GetRectTransformNormal(RectTransform rectTransform)
		{
			Vector3[] array = new Vector3[4];
			rectTransform.GetWorldCorners(array);
			Vector3 lhs = array[3] - array[0];
			Vector3 rhs = array[1] - array[0];
			rectTransform.GetWorldCorners(array);
			return Vector3.Cross(lhs, rhs).normalized;
		}

		protected virtual MouseState GetGazePointerData()
		{
			OVRPointerEventData data;
			GetPointerData(-1, out data, true);
			data.Reset();
			data.worldSpaceRay = new Ray(rayTransform.position, rayTransform.forward);
			data.scrollDelta = GetExtraScrollDelta();
			data.button = PointerEventData.InputButton.Left;
			data.useDragThreshold = true;
			base.eventSystem.RaycastAll(data, m_RaycastResultCache);
			RaycastResult raycastResult2 = (data.pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(m_RaycastResultCache));
			m_RaycastResultCache.Clear();
			OVRRaycaster oVRRaycaster = raycastResult2.module as OVRRaycaster;
			if ((bool)oVRRaycaster)
			{
				data.position = oVRRaycaster.GetScreenPosition(raycastResult2);
				RectTransform component = raycastResult2.gameObject.GetComponent<RectTransform>();
				if (component != null)
				{
					Vector3 worldPosition = raycastResult2.worldPosition;
					Vector3 rectTransformNormal = GetRectTransformNormal(component);
					OVRGazePointer.instance.SetPosition(worldPosition, rectTransformNormal);
					OVRGazePointer.instance.RequestShow();
				}
			}
			OVRPhysicsRaycaster oVRPhysicsRaycaster = raycastResult2.module as OVRPhysicsRaycaster;
			if ((bool)oVRPhysicsRaycaster)
			{
				Vector3 worldPosition2 = raycastResult2.worldPosition;
				if (performSphereCastForGazepointer)
				{
					List<RaycastResult> list = new List<RaycastResult>();
					oVRPhysicsRaycaster.Spherecast(data, list, OVRGazePointer.instance.GetCurrentRadius());
					if (list.Count > 0 && list[0].distance < raycastResult2.distance)
					{
						worldPosition2 = list[0].worldPosition;
					}
				}
				data.position = oVRPhysicsRaycaster.GetScreenPos(raycastResult2.worldPosition);
				OVRGazePointer.instance.RequestShow();
				if (matchNormalOnPhysicsColliders)
				{
					OVRGazePointer.instance.SetPosition(worldPosition2, raycastResult2.worldNormal);
				}
				else
				{
					OVRGazePointer.instance.SetPosition(worldPosition2);
				}
			}
			OVRPointerEventData data2;
			GetPointerData(-2, out data2, true);
			CopyFromTo(data, data2);
			data2.button = PointerEventData.InputButton.Right;
			OVRPointerEventData data3;
			GetPointerData(-3, out data3, true);
			CopyFromTo(data, data3);
			data3.button = PointerEventData.InputButton.Middle;
			m_MouseState.SetButtonState(PointerEventData.InputButton.Left, GetGazeButtonState(), data);
			m_MouseState.SetButtonState(PointerEventData.InputButton.Right, PointerEventData.FramePressState.NotChanged, data2);
			m_MouseState.SetButtonState(PointerEventData.InputButton.Middle, PointerEventData.FramePressState.NotChanged, data3);
			return m_MouseState;
		}

		protected MouseState GetCanvasPointerData()
		{
			PointerEventData data;
			GetPointerData(-1, out data, true);
			data.Reset();
			data.position = Vector2.zero;
			data.scrollDelta = Input.mouseScrollDelta;
			data.button = PointerEventData.InputButton.Left;
			if ((bool)activeGraphicRaycaster)
			{
				activeGraphicRaycaster.RaycastPointer(data, m_RaycastResultCache);
				RaycastResult raycastResult2 = (data.pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(m_RaycastResultCache));
				m_RaycastResultCache.Clear();
				OVRRaycaster oVRRaycaster = raycastResult2.module as OVRRaycaster;
				if ((bool)oVRRaycaster)
				{
					Vector2 screenPosition = oVRRaycaster.GetScreenPosition(raycastResult2);
					data.delta = screenPosition - data.position;
					data.position = screenPosition;
				}
			}
			PointerEventData data2;
			GetPointerData(-2, out data2, true);
			CopyFromTo(data, data2);
			data2.button = PointerEventData.InputButton.Right;
			PointerEventData data3;
			GetPointerData(-3, out data3, true);
			CopyFromTo(data, data3);
			data3.button = PointerEventData.InputButton.Middle;
			m_MouseState.SetButtonState(PointerEventData.InputButton.Left, StateForMouseButton(0), data);
			m_MouseState.SetButtonState(PointerEventData.InputButton.Right, StateForMouseButton(1), data2);
			m_MouseState.SetButtonState(PointerEventData.InputButton.Middle, StateForMouseButton(2), data3);
			return m_MouseState;
		}

		private bool ShouldStartDrag(PointerEventData pointerEvent)
		{
			if (!pointerEvent.useDragThreshold)
			{
				return true;
			}
			if (!pointerEvent.IsVRPointer())
			{
				return (pointerEvent.pressPosition - pointerEvent.position).sqrMagnitude >= (float)(base.eventSystem.pixelDragThreshold * base.eventSystem.pixelDragThreshold);
			}
			Vector3 position = pointerEvent.pressEventCamera.transform.position;
			Vector3 normalized = (pointerEvent.pointerPressRaycast.worldPosition - position).normalized;
			Vector3 normalized2 = (pointerEvent.pointerCurrentRaycast.worldPosition - position).normalized;
			return Vector3.Dot(normalized, normalized2) < Mathf.Cos((float)Math.PI / 180f * angleDragThreshold);
		}

		private static bool IsPointerMoving(PointerEventData pointerEvent)
		{
			if (pointerEvent.IsVRPointer())
			{
				return true;
			}
			return pointerEvent.IsPointerMoving();
		}

		protected Vector2 SwipeAdjustedPosition(Vector2 originalPosition, PointerEventData pointerEvent)
		{
			return originalPosition;
		}

		protected override void ProcessDrag(PointerEventData pointerEvent)
		{
			Vector2 position = pointerEvent.position;
			bool flag = IsPointerMoving(pointerEvent);
			if (flag && pointerEvent.pointerDrag != null && !pointerEvent.dragging && ShouldStartDrag(pointerEvent))
			{
				if (pointerEvent.IsVRPointer())
				{
					pointerEvent.position = SwipeAdjustedPosition(position, pointerEvent);
				}
				ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
				pointerEvent.dragging = true;
			}
			if (pointerEvent.dragging && flag && pointerEvent.pointerDrag != null)
			{
				if (pointerEvent.IsVRPointer())
				{
					pointerEvent.position = SwipeAdjustedPosition(position, pointerEvent);
				}
				if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
				{
					ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
					pointerEvent.eligibleForClick = false;
					pointerEvent.pointerPress = null;
					pointerEvent.rawPointerPress = null;
				}
				ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
			}
		}

		protected virtual PointerEventData.FramePressState GetGazeButtonState()
		{
			bool flag = Input.GetKeyDown(gazeClickKey) || OVRInput.GetDown(joyPadClickButton);
			bool flag2 = Input.GetKeyUp(gazeClickKey) || OVRInput.GetUp(joyPadClickButton);
			if (flag && flag2)
			{
				return PointerEventData.FramePressState.PressedAndReleased;
			}
			if (flag)
			{
				return PointerEventData.FramePressState.Pressed;
			}
			if (flag2)
			{
				return PointerEventData.FramePressState.Released;
			}
			return PointerEventData.FramePressState.NotChanged;
		}

		protected Vector2 GetExtraScrollDelta()
		{
			Vector2 result = default(Vector2);
			if (useRightStickScroll)
			{
				Vector2 result2 = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
				if (Mathf.Abs(result2.x) < rightStickDeadZone)
				{
					result2.x = 0f;
				}
				if (Mathf.Abs(result2.y) < rightStickDeadZone)
				{
					result2.y = 0f;
				}
				return result2;
			}
			return result;
		}
	}
}
