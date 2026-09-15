using UnityEngine;

public sealed class OVRTouchpadHelper : MonoBehaviour
{
	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Start()
	{
		OVRMessenger.AddListener<OVRTouchpad.TouchEvent>("Touchpad", LocalTouchEventCallback);
	}

	private void Update()
	{
		OVRTouchpad.Update();
	}

	public void OnDisable()
	{
		OVRTouchpad.OnDisable();
	}

	private void LocalTouchEventCallback(OVRTouchpad.TouchEvent touchEvent)
	{
		switch (touchEvent)
		{
		case OVRTouchpad.TouchEvent.SingleTap:
			break;
		case OVRTouchpad.TouchEvent.DoubleTap:
			break;
		case OVRTouchpad.TouchEvent.Left:
			break;
		case OVRTouchpad.TouchEvent.Right:
			break;
		case OVRTouchpad.TouchEvent.Up:
			break;
		case OVRTouchpad.TouchEvent.Down:
			break;
		}
	}
}
