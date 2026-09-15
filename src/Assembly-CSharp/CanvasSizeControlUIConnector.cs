using UnityEngine;

public class CanvasSizeControlUIConnector : UIConnector
{
	public CanvasSizeControl canvasSizeControl;

	public override void Connect()
	{
		Debug.LogError("CanvasSizeControlUIConnector obsolete but still in use");
	}
}
