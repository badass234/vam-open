using UnityEngine;

public class LipSyncDemo_SetCurrentTarget : MonoBehaviour
{
	public EnableSwitch[] SwitchTargets;

	private int targetSet;

	private void Start()
	{
		OVRMessenger.AddListener<OVRTouchpad.TouchEvent>("Touchpad", LocalTouchEventCallback);
		targetSet = 0;
		SwitchTargets[0].SetActive(0);
		SwitchTargets[1].SetActive(0);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			targetSet = 0;
			SetCurrentTarget();
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			targetSet = 1;
			SetCurrentTarget();
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			targetSet = 2;
			SetCurrentTarget();
		}
		else if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			targetSet = 3;
			SetCurrentTarget();
		}
		else if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			targetSet = 4;
			SetCurrentTarget();
		}
		else if (Input.GetKeyDown(KeyCode.Alpha6))
		{
			targetSet = 5;
			SetCurrentTarget();
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}

	private void SetCurrentTarget()
	{
		switch (targetSet)
		{
		case 0:
			SwitchTargets[0].SetActive(0);
			SwitchTargets[1].SetActive(0);
			break;
		case 1:
			SwitchTargets[0].SetActive(0);
			SwitchTargets[1].SetActive(1);
			break;
		case 2:
			SwitchTargets[0].SetActive(1);
			SwitchTargets[1].SetActive(2);
			break;
		case 3:
			SwitchTargets[0].SetActive(1);
			SwitchTargets[1].SetActive(3);
			break;
		case 4:
			SwitchTargets[0].SetActive(2);
			SwitchTargets[1].SetActive(4);
			break;
		case 5:
			SwitchTargets[0].SetActive(2);
			SwitchTargets[1].SetActive(5);
			break;
		}
	}

	private void LocalTouchEventCallback(OVRTouchpad.TouchEvent touchEvent)
	{
		switch (touchEvent)
		{
		case OVRTouchpad.TouchEvent.Left:
			targetSet--;
			if (targetSet < 0)
			{
				targetSet = 3;
			}
			SetCurrentTarget();
			break;
		case OVRTouchpad.TouchEvent.Right:
			targetSet++;
			if (targetSet > 3)
			{
				targetSet = 0;
			}
			SetCurrentTarget();
			break;
		}
	}
}
