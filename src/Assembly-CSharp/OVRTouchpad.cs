using UnityEngine;

internal static class OVRTouchpad
{
	public enum TouchEvent
	{
		SingleTap,
		DoubleTap,
		Left,
		Right,
		Up,
		Down
	}

	private static Vector3 moveAmountMouse;

	private static float minMovMagnitudeMouse = 25f;

	private static OVRTouchpadHelper touchpadHelper = new GameObject("OVRTouchpadHelper").AddComponent<OVRTouchpadHelper>();

	public static void Create()
	{
	}

	public static void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			moveAmountMouse = Input.mousePosition;
		}
		else if (Input.GetMouseButtonUp(0))
		{
			moveAmountMouse -= Input.mousePosition;
			HandleInputMouse(ref moveAmountMouse);
		}
	}

	public static void OnDisable()
	{
	}

	private static void HandleInputMouse(ref Vector3 move)
	{
		if (move.magnitude < minMovMagnitudeMouse)
		{
			OVRMessenger.Broadcast("Touchpad", TouchEvent.SingleTap);
			return;
		}
		move.Normalize();
		if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
		{
			if (move.x > 0f)
			{
				OVRMessenger.Broadcast("Touchpad", TouchEvent.Left);
			}
			else
			{
				OVRMessenger.Broadcast("Touchpad", TouchEvent.Right);
			}
		}
		else if (move.y > 0f)
		{
			OVRMessenger.Broadcast("Touchpad", TouchEvent.Down);
		}
		else
		{
			OVRMessenger.Broadcast("Touchpad", TouchEvent.Up);
		}
	}
}
