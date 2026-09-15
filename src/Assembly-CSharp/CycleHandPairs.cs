using Leap.Unity;
using UnityEngine;

public class CycleHandPairs : MonoBehaviour
{
	public HandModelManager HandPool;

	public string[] GroupNames;

	private int currentGroup;

	private KeyCode[] keyCodes = new KeyCode[6]
	{
		KeyCode.Alpha1,
		KeyCode.Alpha2,
		KeyCode.Alpha3,
		KeyCode.Alpha4,
		KeyCode.Alpha5,
		KeyCode.Alpha6
	};

	public int CurrentGroup
	{
		get
		{
			return currentGroup;
		}
		set
		{
			disableAllGroups();
			currentGroup = value;
			HandPool.EnableGroup(GroupNames[value]);
		}
	}

	private void Start()
	{
		HandPool = GetComponent<HandModelManager>();
		disableAllGroups();
		CurrentGroup = 0;
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.RightArrow) && CurrentGroup < GroupNames.Length - 1)
		{
			CurrentGroup++;
		}
		if (Input.GetKeyUp(KeyCode.LeftArrow) && CurrentGroup > 0)
		{
			CurrentGroup--;
		}
		for (int i = 0; i < keyCodes.Length; i++)
		{
			if (Input.GetKeyDown(keyCodes[i]))
			{
				HandPool.ToggleGroup(GroupNames[i]);
			}
		}
		if (Input.GetKeyUp(KeyCode.Alpha0))
		{
			disableAllGroups();
		}
	}

	private void disableAllGroups()
	{
		for (int i = 0; i < GroupNames.Length; i++)
		{
			HandPool.DisableGroup(GroupNames[i]);
		}
	}
}
