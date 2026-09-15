using UnityEngine;

public class MaterialOptionsUIConnector : UIConnector
{
	public MaterialOptions[] materialOptionsList;

	public override void Connect()
	{
		Debug.LogError("MaterialOptionsUIConnector obsolete but still in use");
	}
}
