using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Weelco.VRInput
{
	public class VRHitInputModule : VRInputModule
	{
		private Dictionary<IUIHitPointer, VRInputControllerData> controllerData = new Dictionary<IUIHitPointer, VRInputControllerData>();

		public override void AddController(IUIPointer controller)
		{
			if (controller is IUIHitPointer)
			{
				controllerData.Add(controller as IUIHitPointer, new VRInputControllerData());
			}
		}

		public override void RemoveController(IUIPointer controller)
		{
			if (controller is IUIHitPointer)
			{
				controllerData.Remove(controller as IUIHitPointer);
			}
		}

		public override void Process()
		{
			foreach (KeyValuePair<IUIHitPointer, VRInputControllerData> controllerDatum in controllerData)
			{
				IUIHitPointer key = controllerDatum.Key;
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
						key.OnExitControl(value.currentPoint);
					}
					if (gameObject != null)
					{
						key.OnEnterControl(gameObject);
					}
				}
				value.currentPoint = gameObject;
				HandlePointerExitAndEnter(value.pointerEvent, value.currentPoint);
				if (key.ButtonDown())
				{
					ClearSelection();
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
						if (gameObject2 != null)
						{
							value.pointerEvent.pointerPress = gameObject2;
							value.currentPressed = gameObject2;
							Select(value.currentPressed);
						}
						ExecuteEvents.Execute(value.currentPressed, value.pointerEvent, ExecuteEvents.beginDragHandler);
						ExecuteEvents.Execute(key.target.gameObject, value.pointerEvent, ExecuteEvents.beginDragHandler);
						value.pointerEvent.pointerDrag = value.currentPressed;
					}
				}
				if (key.ButtonUp())
				{
					if ((bool)value.currentPressed)
					{
						value.pointerEvent.current = value.currentPressed;
						ExecuteEvents.Execute(value.currentPressed, value.pointerEvent, ExecuteEvents.pointerUpHandler);
						ExecuteEvents.Execute(key.target.gameObject, value.pointerEvent, ExecuteEvents.pointerUpHandler);
						value.pointerEvent.rawPointerPress = null;
						value.pointerEvent.pointerPress = null;
						value.currentPressed = null;
					}
					ClearSelection();
				}
				if (base.eventSystem.currentSelectedGameObject != null)
				{
					value.pointerEvent.current = base.eventSystem.currentSelectedGameObject;
					ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, GetBaseEventData(), ExecuteEvents.updateSelectedHandler);
				}
			}
		}

		private void Select(GameObject go)
		{
			ClearSelection();
			if ((bool)ExecuteEvents.GetEventHandler<ISelectHandler>(go))
			{
				base.eventSystem.SetSelectedGameObject(go);
			}
		}
	}
}
