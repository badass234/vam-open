using UnityEngine;

public class FreeControllerV3UIConnector : UIConnector
{
	public FreeControllerV3 controller;

	public override void Connect()
	{
		Debug.LogError("FreeControllerV3UIConnector obsolete but still in use");
	}
}
