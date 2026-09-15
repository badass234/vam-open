namespace UnityEngine.EventSystems
{
	public static class PointerEventDataExtension
	{
		public static bool IsVRPointer(this PointerEventData pointerEventData)
		{
			return pointerEventData is OVRPointerEventData;
		}

		public static Ray GetRay(this PointerEventData pointerEventData)
		{
			OVRPointerEventData oVRPointerEventData = pointerEventData as OVRPointerEventData;
			return oVRPointerEventData.worldSpaceRay;
		}

		public static Vector2 GetSwipeStart(this PointerEventData pointerEventData)
		{
			OVRPointerEventData oVRPointerEventData = pointerEventData as OVRPointerEventData;
			return oVRPointerEventData.swipeStart;
		}

		public static void SetSwipeStart(this PointerEventData pointerEventData, Vector2 start)
		{
			OVRPointerEventData oVRPointerEventData = pointerEventData as OVRPointerEventData;
			oVRPointerEventData.swipeStart = start;
		}
	}
}
