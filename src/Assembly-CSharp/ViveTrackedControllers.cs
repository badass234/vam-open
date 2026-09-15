using UnityEngine;
using Valve.VR;

public class ViveTrackedControllers : MonoBehaviour
{
	public enum TouchpadDirection
	{
		None,
		Up,
		Down,
		Right,
		Left
	}

	public enum HapticAxis
	{
		X,
		Y,
		XY
	}

	public static ViveTrackedControllers singleton;

	public SteamVR_TrackedObject viveObjectLeft;

	public SteamVR_TrackedObject viveObjectRight;

	public float scrollClick = 0.25f;

	private GameObject viveObjectLeftModelGO;

	private GameObject viveObjectLeftCanvasGO;

	private GameObject viveObjectRightModelGO;

	private GameObject viveObjectRightCanvasGO;

	private float _lastLeftTriggerValue;

	private float _lastRightTriggerValue;

	private bool _leftTriggerIsFullClicking;

	private Vector2 _leftTouchDownPosition = new Vector2(0f, 0f);

	private TouchpadDirection _leftPressDownTouchpadDirection;

	private Vector2 _rightTouchDownPosition = new Vector2(0f, 0f);

	private TouchpadDirection _rightPressDownTouchpadDirection;

	private Vector2 _leftTouchPosition = new Vector2(0f, 0f);

	private Vector2 _rightTouchPosition = new Vector2(0f, 0f);

	private TouchpadDirection _leftTouchpadDirection;

	private TouchpadDirection _rightTouchpadDirection;

	private Vector2 _leftLastTouchPosition = new Vector2(0f, 0f);

	private Vector2 _rightLastTouchPosition = new Vector2(0f, 0f);

	private Vector2 _leftScrollPosition;

	private Vector2 _rightScrollPosition;

	public bool leftTouchedThisFrame => false;

	public bool rightTouchedThisFrame => false;

	public bool leftUntouchedThisFrame => false;

	public bool rightUntouchedThisFrame => false;

	public bool leftTouching => false;

	public bool rightTouching => false;

	public bool bothTouching => false;

	public bool leftGrippedThisFrame => false;

	public bool rightGrippedThisFrame => false;

	public bool leftUngrippedThisFrame => false;

	public bool rightUngrippedThisFrame => false;

	public bool leftGripping => false;

	public bool rightGripping => false;

	public bool leftTouchpadPressedThisFrame => false;

	public bool rightTouchpadPressedThisFrame => false;

	public bool leftTouchpadUnpressedThisFrame => false;

	public bool rightTouchpadUnpressedThisFrame => false;

	public bool leftTouchpadPressing => false;

	public bool rightTouchpadPressing => false;

	public bool leftMenuPressedThisFrame => false;

	public bool rightMenuPressedThisFrame => false;

	public bool leftMenuUnpressedThisFrame => false;

	public bool rightMenuUnpressedThisFrame => false;

	public bool leftMenuPressing => false;

	public bool rightMenuPressing => false;

	public bool leftTriggerPressing => false;

	public bool rightTriggerPressing => false;

	public bool leftTriggerPressedThisFrame => false;

	public bool rightTriggerPressedThisFrame => false;

	public bool leftTriggerUnpressedThisFrame => false;

	public bool rightTriggerUnpressedThisFrame => false;

	public float leftTriggerValue => 0f;

	public float rightTriggerValue => 0f;

	public bool leftTriggerFullClickThisFrame => false;

	public bool leftTriggerFullUnclickThisFrame => false;

	public bool rightTriggerFullClickThisFrame => false;

	public bool rightTriggerFullUnclickThisFrame => false;

	public bool rightTriggerIsFullClicking => false;

	public bool leftTriggerIsFullClicking => _leftTriggerIsFullClicking;

	public Vector2 leftTouchDownPosition => _leftTouchDownPosition;

	public TouchpadDirection leftPressDownTouchpadDirection => _leftPressDownTouchpadDirection;

	public Vector2 rightTouchDownPosition => _rightTouchDownPosition;

	public TouchpadDirection rightPressDownTouchpadDirection => _rightPressDownTouchpadDirection;

	public Vector2 leftTouchPosition => _leftTouchPosition;

	public Vector2 rightTouchPosition => _rightTouchPosition;

	public TouchpadDirection leftTouchpadDirection => _leftTouchpadDirection;

	public TouchpadDirection rightTouchpadDirection => _rightTouchpadDirection;

	public Vector2 GetLeftTouchDelta(bool hapticFeedback = true, HapticAxis hapticAxis = HapticAxis.X)
	{
		Vector2 result = default(Vector2);
		if (leftTouching)
		{
			result.x = _leftTouchPosition.x - _leftLastTouchPosition.x;
			result.y = _leftTouchPosition.y - _leftLastTouchPosition.y;
		}
		else
		{
			result.x = 0f;
			result.y = 0f;
		}
		return result;
	}

	public Vector2 GetRightTouchDelta(bool hapticFeedback = true, HapticAxis hapticAxis = HapticAxis.X)
	{
		Vector2 result = default(Vector2);
		if (rightTouching)
		{
			result.x = _rightTouchPosition.x - _rightLastTouchPosition.x;
			result.y = _rightTouchPosition.y - _rightLastTouchPosition.y;
		}
		else
		{
			result.x = 0f;
			result.y = 0f;
		}
		return result;
	}

	private void ProcessViveControllers()
	{
	}

	public int GetLeftTouchScroll(bool hapticFeedback = true)
	{
		return 0;
	}

	public int GetRightTouchScroll(bool hapticFeedback = true)
	{
		return 0;
	}

	public void HideLeftController()
	{
	}

	public void ShowLeftController()
	{
	}

	public void HideRightController()
	{
	}

	public void ShowRightController()
	{
	}

	private void Update()
	{
		ProcessViveControllers();
	}

	private void Awake()
	{
		singleton = this;
	}
}
