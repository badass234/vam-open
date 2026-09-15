using UnityEngine;

public class SetTransformScaleUIConnector : UIConnector
{
	public SetTransformScale transformScale;

	public override void Connect()
	{
		Debug.LogError("SetTransformScaleUIConnector obsolete but still in use");
	}
}
