using UnityEngine;

public class HandAnimatorManager : MonoBehaviour
{
	public StateModel[] stateModels;

	private Animator handAnimator;

	public int currentState = 100;

	private int lastState = -1;

	private void Start()
	{
		handAnimator = GetComponent<Animator>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.BackQuote))
		{
			currentState = 0;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			currentState = 1;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			currentState = 2;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			currentState = 3;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			currentState = 4;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			currentState = 5;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha6))
		{
			currentState = 6;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha7))
		{
			currentState = 7;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha8))
		{
			currentState = 8;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha9))
		{
			currentState = 9;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha0))
		{
			currentState = 10;
		}
		else if (Input.GetKeyDown(KeyCode.I))
		{
			currentState = 100;
		}
		if (lastState != currentState)
		{
			lastState = currentState;
			handAnimator.SetInteger("State", currentState);
			TurnOnState(currentState);
		}
		handAnimator.SetBool("Action", Input.GetMouseButton(0));
		handAnimator.SetBool("Hold", Input.GetMouseButton(1));
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
