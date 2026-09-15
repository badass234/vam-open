using UnityEngine;

public class MVRScriptController
{
	public GameObject gameObject;

	public MVRScript script;

	public Transform configUI;

	public Transform customUI;

	public void OpenUI()
	{
		if (customUI != null)
		{
			customUI.gameObject.SetActive(true);
		}
	}

	public void CloseUI()
	{
		if (customUI != null)
		{
			customUI.gameObject.SetActive(false);
		}
	}
}
