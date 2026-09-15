using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Weelco.VRInput
{
	public class VRGazeInputModule : VRInputModule
	{
		private GameObject lastActiveButton;

		private GameObject currentLook;

		private float lookTimer;

		private Dictionary<UIGazePointer, VRInputControllerData> controllerData = new Dictionary<UIGazePointer, VRInputControllerData>();

		public override void AddController(IUIPointer controller)
		{
			if (controller is UIGazePointer)
			{
				controllerData.Add(controller as UIGazePointer, new VRInputControllerData());
			}
		}

		public override void RemoveController(IUIPointer controller)
		{
			if (controller is UIGazePointer)
			{
				controllerData.Remove(controller as UIGazePointer);
			}
		}

		public override void Process()
		{
			foreach (KeyValuePair<UIGazePointer, VRInputControllerData> controllerDatum in controllerData)
			{
				UIGazePointer key = controllerDatum.Key;
				VRInputControllerData value = controllerDatum.Value;
				UpdateCameraPosition(key);
				if (value.pointerEvent == null)
				{
					value.pointerEvent = new VRInputEventData(base.eventSystem);
				}
				else
				{
					value.pointerEvent.Reset();
				}
				value.pointerEvent.controller = key;
				value.pointerEvent.delta = Vector2.zero;
				value.pointerEvent.position = new Vector2(GetCameraSize().x * 0.5f, GetCameraSize().y * 0.5f);
				base.eventSystem.RaycastAll(value.pointerEvent, m_RaycastResultCache);
				value.pointerEvent.pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(m_RaycastResultCache);
				m_RaycastResultCache.Clear();
				if (value.pointerEvent.pointerCurrentRaycast.distance > 0f)
				{
					key.LimitLaserDistance(value.pointerEvent.pointerCurrentRaycast.distance + 0.01f);
				}
				GameObject gameObject = value.pointerEvent.pointerCurrentRaycast.gameObject;
				if (value.currentPoint != gameObject)
				{
					if (value.currentPoint != null)
					{
						ExecuteEvents.ExecuteHierarchy(value.currentPressed, value.pointerEvent, ExecuteEvents.pointerUpHandler);
						key.OnExitControl(value.currentPoint);
					}
					if (gameObject != null)
					{
						key.OnEnterControl(gameObject);
					}
				}
				value.currentPoint = gameObject;
				currentLook = gameObject;
				if (gameObject == null)
				{
					ClearSelection();
				}
				HandlePointerExitAndEnter(value.pointerEvent, value.currentPoint);
				Image gazeProgressBar = key.GazeProgressBar;
				float gazeClickTimer = key.GazeClickTimer;
				float gazeClickTimerDelay = key.GazeClickTimerDelay;
				if (currentLook != null && gazeClickTimer > 0f)
				{
					bool flag = false;
					if (currentLook.transform.gameObject.GetComponent<Button>() != null)
					{
						flag = true;
					}
					if (currentLook.transform.parent != null)
					{
						if (currentLook.transform.parent.gameObject.GetComponent<Button>() != null)
						{
							flag = true;
						}
						if (currentLook.transform.parent.gameObject.GetComponent<Toggle>() != null)
						{
							flag = true;
						}
						if (currentLook.transform.parent.gameObject.GetComponent<Slider>() != null)
						{
							flag = true;
						}
						if (currentLook.transform.parent.parent != null)
						{
							if (currentLook.transform.parent.parent.gameObject.GetComponent<Slider>() != null && currentLook.name != "Handle")
							{
								flag = true;
							}
							if (currentLook.transform.parent.parent.gameObject.GetComponent<Toggle>() != null)
							{
								flag = true;
							}
						}
					}
					if (flag)
					{
						if (lastActiveButton == currentLook)
						{
							if ((bool)gazeProgressBar)
							{
								if (gazeProgressBar.isActiveAndEnabled)
								{
									gazeProgressBar.fillAmount = (Time.realtimeSinceStartup - lookTimer) / gazeClickTimer;
								}
								else if (Time.realtimeSinceStartup - lookTimer > 0f)
								{
									gazeProgressBar.fillAmount = 0f;
									gazeProgressBar.gameObject.SetActive(true);
									ExecuteEvents.ExecuteHierarchy(value.currentPressed, value.pointerEvent, ExecuteEvents.pointerUpHandler);
								}
							}
							if (!(Time.realtimeSinceStartup - lookTimer > gazeClickTimer))
							{
								continue;
							}
							if ((bool)gazeProgressBar)
							{
								gazeProgressBar.gameObject.SetActive(false);
							}
							value.pointerEvent.pressPosition = value.pointerEvent.position;
							value.pointerEvent.pointerPressRaycast = value.pointerEvent.pointerCurrentRaycast;
							value.pointerEvent.pointerPress = null;
							if (value.currentPoint != null)
							{
								value.currentPressed = value.currentPoint;
								value.pointerEvent.current = value.currentPressed;
								GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy(value.currentPressed, value.pointerEvent, ExecuteEvents.pointerDownHandler);
								ExecuteEvents.Execute(key.target.gameObject, value.pointerEvent, ExecuteEvents.pointerDownHandler);
								if (gameObject2 == null)
								{
									gameObject2 = ExecuteEvents.ExecuteHierarchy(value.currentPressed, value.pointerEvent, ExecuteEvents.pointerClickHandler);
									ExecuteEvents.Execute(key.target.gameObject, value.pointerEvent, ExecuteEvents.pointerClickHandler);
									if (gameObject2 != null)
									{
										value.currentPressed = gameObject2;
									}
								}
								else
								{
									value.currentPressed = gameObject2;
									ExecuteEvents.Execute(gameObject2, value.pointerEvent, ExecuteEvents.pointerClickHandler);
									ExecuteEvents.Execute(key.target.gameObject, value.pointerEvent, ExecuteEvents.pointerClickHandler);
								}
							}
							lookTimer = Time.realtimeSinceStartup + gazeClickTimer * gazeClickTimerDelay;
						}
						else
						{
							lastActiveButton = currentLook;
							lookTimer = Time.realtimeSinceStartup;
							if ((bool)gazeProgressBar && gazeProgressBar.isActiveAndEnabled)
							{
								gazeProgressBar.gameObject.SetActive(false);
							}
						}
					}
					else
					{
						lastActiveButton = null;
						if ((bool)gazeProgressBar && gazeProgressBar.isActiveAndEnabled)
						{
							gazeProgressBar.gameObject.SetActive(false);
						}
						ClearSelection();
					}
				}
				else
				{
					if ((bool)gazeProgressBar)
					{
						gazeProgressBar.gameObject.SetActive(false);
					}
					lastActiveButton = null;
					ClearSelection();
				}
			}
		}
	}
}
