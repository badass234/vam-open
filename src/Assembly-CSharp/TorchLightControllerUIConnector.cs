using UnityEngine;

public class TorchLightControllerUIConnector : UIConnector
{
	public TorchLightController lightController;

	public override void Connect()
	{
		Debug.LogError("TorchLightControllerUIConnector obsolete but still in use");
	}
}
