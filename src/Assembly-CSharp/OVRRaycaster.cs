using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class OVRRaycaster : GraphicRaycaster, IPointerEnterHandler, IEventSystemHandler
{
	private struct RaycastHit
	{
		public Graphic graphic;

		public Vector3 worldPos;

		public bool fromMouse;
	}

	[Tooltip("A world space pointer for this canvas")]
	public GameObject pointer;

	public int sortOrder;

	[NonSerialized]
	private Canvas m_Canvas;

	[NonSerialized]
	private List<RaycastHit> m_RaycastResults = new List<RaycastHit>();

	[NonSerialized]
	private static readonly List<RaycastHit> s_SortedGraphics = new List<RaycastHit>();

	private Canvas canvas
	{
		get
		{
			if (m_Canvas != null)
			{
				return m_Canvas;
			}
			m_Canvas = GetComponent<Canvas>();
			return m_Canvas;
		}
	}

	public override Camera eventCamera => canvas.worldCamera;

	public override int sortOrderPriority => sortOrder;

	protected OVRRaycaster()
	{
	}

	private void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList, Ray ray, bool checkForBlocking)
	{
		if (canvas == null)
		{
			return;
		}
		float num = float.MaxValue;
		if (checkForBlocking && base.blockingObjects != 0)
		{
			float farClipPlane = eventCamera.farClipPlane;
			if (base.blockingObjects == BlockingObjects.ThreeD || base.blockingObjects == BlockingObjects.All)
			{
				UnityEngine.RaycastHit[] array = Physics.RaycastAll(ray, farClipPlane, m_BlockingMask);
				if (array.Length > 0 && array[0].distance < num)
				{
					num = array[0].distance;
				}
			}
			if (base.blockingObjects == BlockingObjects.TwoD || base.blockingObjects == BlockingObjects.All)
			{
				RaycastHit2D[] rayIntersectionAll = Physics2D.GetRayIntersectionAll(ray, farClipPlane, m_BlockingMask);
				if (rayIntersectionAll.Length > 0 && rayIntersectionAll[0].fraction * farClipPlane < num)
				{
					num = rayIntersectionAll[0].fraction * farClipPlane;
				}
			}
		}
		m_RaycastResults.Clear();
		GraphicRaycast(canvas, ray, m_RaycastResults);
		for (int i = 0; i < m_RaycastResults.Count; i++)
		{
			GameObject gameObject = m_RaycastResults[i].graphic.gameObject;
			bool flag = true;
			if (base.ignoreReversedGraphics)
			{
				Vector3 direction = ray.direction;
				Vector3 rhs = gameObject.transform.rotation * Vector3.forward;
				flag = Vector3.Dot(direction, rhs) > 0f;
			}
			if (eventCamera.transform.InverseTransformPoint(m_RaycastResults[i].worldPos).z <= 0f)
			{
				flag = false;
			}
			if (flag)
			{
				float num2 = Vector3.Distance(ray.origin, m_RaycastResults[i].worldPos);
				if (!(num2 >= num))
				{
					RaycastResult raycastResult = default(RaycastResult);
					raycastResult.gameObject = gameObject;
					raycastResult.module = this;
					raycastResult.distance = num2;
					raycastResult.index = resultAppendList.Count;
					raycastResult.depth = m_RaycastResults[i].graphic.depth;
					raycastResult.worldPosition = m_RaycastResults[i].worldPos;
					RaycastResult item = raycastResult;
					resultAppendList.Add(item);
				}
			}
		}
	}

	public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
	{
		if (eventData.IsVRPointer())
		{
			Raycast(eventData, resultAppendList, eventData.GetRay(), true);
		}
	}

	public void RaycastPointer(PointerEventData eventData, List<RaycastResult> resultAppendList)
	{
		if (pointer != null && pointer.activeInHierarchy)
		{
			Raycast(eventData, resultAppendList, new Ray(eventCamera.transform.position, (pointer.transform.position - eventCamera.transform.position).normalized), false);
		}
	}

	private void GraphicRaycast(Canvas canvas, Ray ray, List<RaycastHit> results)
	{
		IList<Graphic> graphicsForCanvas = GraphicRegistry.GetGraphicsForCanvas(canvas);
		s_SortedGraphics.Clear();
		RaycastHit item = default(RaycastHit);
		for (int i = 0; i < graphicsForCanvas.Count; i++)
		{
			Graphic graphic = graphicsForCanvas[i];
			Vector3 worldPos;
			if (graphic.depth != -1 && !(pointer == graphic.gameObject) && RayIntersectsRectTransform(graphic.rectTransform, ray, out worldPos))
			{
				Vector2 sp = eventCamera.WorldToScreenPoint(worldPos);
				if (graphic.Raycast(sp, eventCamera))
				{
					item.graphic = graphic;
					item.worldPos = worldPos;
					item.fromMouse = false;
					s_SortedGraphics.Add(item);
				}
			}
		}
		s_SortedGraphics.Sort((RaycastHit g1, RaycastHit g2) => g2.graphic.depth.CompareTo(g1.graphic.depth));
		for (int j = 0; j < s_SortedGraphics.Count; j++)
		{
			results.Add(s_SortedGraphics[j]);
		}
	}

	public Vector2 GetScreenPosition(RaycastResult raycastResult)
	{
		return eventCamera.WorldToScreenPoint(raycastResult.worldPosition);
	}

	private static bool RayIntersectsRectTransform(RectTransform rectTransform, Ray ray, out Vector3 worldPos)
	{
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		float enter;
		if (!new Plane(array[0], array[1], array[2]).Raycast(ray, out enter))
		{
			worldPos = Vector3.zero;
			return false;
		}
		Vector3 point = ray.GetPoint(enter);
		Vector3 vector = array[3] - array[0];
		Vector3 vector2 = array[1] - array[0];
		float num = Vector3.Dot(point - array[0], vector);
		float num2 = Vector3.Dot(point - array[0], vector2);
		if (num < vector.sqrMagnitude && num2 < vector2.sqrMagnitude && num >= 0f && num2 >= 0f)
		{
			worldPos = array[0] + num2 * vector2 / vector2.sqrMagnitude + num * vector / vector.sqrMagnitude;
			return true;
		}
		worldPos = Vector3.zero;
		return false;
	}

	public bool IsFocussed()
	{
		OVRInputModule oVRInputModule = EventSystem.current.currentInputModule as OVRInputModule;
		return (bool)oVRInputModule && oVRInputModule.activeGraphicRaycaster == this;
	}

	public void OnPointerEnter(PointerEventData e)
	{
		if (e.IsVRPointer())
		{
			OVRInputModule oVRInputModule = EventSystem.current.currentInputModule as OVRInputModule;
			oVRInputModule.activeGraphicRaycaster = this;
		}
	}
}
