using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputFieldAction : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	public delegate void OnSubmit();

	public delegate void OnSelected();

	public delegate void OnUp();

	public delegate void OnDown();

	public OnSubmit onSubmitHandlers;

	public OnSelected onSelectedHandlers;

	public OnUp onUpHandlers;

	public OnDown onDownHandlers;

	public InputField inputField;

	protected bool wasFocusedLastFrame;

	public void Submit()
	{
		if (onSubmitHandlers != null)
		{
			onSubmitHandlers();
		}
	}

	public void Select()
	{
		if (LookInputModule.singleton != null)
		{
			LookInputModule.singleton.Select(base.gameObject);
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (onSelectedHandlers != null)
		{
			onSelectedHandlers();
		}
	}

	public void Up()
	{
		if (onUpHandlers != null)
		{
			onUpHandlers();
		}
	}

	public void Down()
	{
		if (onDownHandlers != null)
		{
			onDownHandlers();
		}
	}

	private void Awake()
	{
		if (inputField == null)
		{
			inputField = GetComponent<InputField>();
		}
	}

	private void Update()
	{
		if (wasFocusedLastFrame)
		{
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				Submit();
			}
			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				Up();
			}
			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				Down();
			}
		}
		wasFocusedLastFrame = inputField != null && inputField.isFocused;
	}
}
