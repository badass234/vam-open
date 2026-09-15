using UnityEngine;

public class VRWebBrowserUIConnector : UIConnector
{
	public VRWebBrowser webBrowser;

	public override void Connect()
	{
		Debug.LogError("VRWebBrowserUIConnector obsolete but still in use");
	}
}
