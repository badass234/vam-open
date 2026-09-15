using UnityEngine;

public class EnableSwitch : MonoBehaviour
{
	public GameObject[] SwitchTargets;

	public bool SetActive(int target)
	{
		if (target < 0 || target >= SwitchTargets.Length)
		{
			return false;
		}
		for (int i = 0; i < SwitchTargets.Length; i++)
		{
			SwitchTargets[i].SetActive(false);
		}
		SwitchTargets[target].SetActive(true);
		return true;
	}
}
