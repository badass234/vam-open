using UnityEngine;

public class HandAnimatorManagerVR : MonoBehaviour
{
	public StateModel[] stateModels;

	private Animator handAnimator;

	public int currentState = 100;

	private int lastState = -1;

	public bool action;

	public bool hold;

	public string changeKey = "joystick button 9";

	public string actionKey = "joystick button 15";

	public string holdKey = "Axis 12";

	public int numberOfAnimations = 8;

	private void Start()
	{
		string[] joystickNames = Input.GetJoystickNames();
		string[] array = joystickNames;
		foreach (string message in array)
		{
			Debug.Log(message);
		}
		handAnimator = GetComponent<Animator>();
	}

	private void Update()
	{
		if (Input.GetKeyUp(changeKey))
		{
			currentState = (currentState + 1) % (numberOfAnimations + 1);
		}
		if (Input.GetAxis(holdKey) > 0f)
		{
			hold = true;
		}
		else
		{
			hold = false;
		}
		if (Input.GetKey(actionKey))
		{
			action = true;
		}
		else
		{
			action = false;
		}
		if (lastState != currentState)
		{
			lastState = currentState;
			handAnimator.SetInteger("State", currentState);
			TurnOnState(currentState);
		}
		handAnimator.SetBool("Action", action);
		handAnimator.SetBool("Hold", hold);
	}

	private void TurnOnState(int stateNumber)
	{
		StateModel[] array = stateModels;
		foreach (StateModel stateModel in array)
		{
			if (stateModel.stateNumber == stateNumber && !stateModel.go.activeSelf)
			{
				stateModel.go.SetActive(true);
			}
			else if (stateModel.go.activeSelf)
			{
				stateModel.go.SetActive(false);
			}
		}
	}
}
