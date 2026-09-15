using UnityEngine;

public class ImageControlUIConnector : UIConnector
{
	public ImageControl imageControl;

	public override void Connect()
	{
		Debug.LogError("ImageControlUIConnector obsolete but still in use");
	}
}
