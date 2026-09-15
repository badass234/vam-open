using UnityEngine;

public class MirrorReflectionUIConnector : UIConnector
{
	public MirrorReflection mirrorReflection;

	public override void Connect()
	{
		Debug.LogError("MirrorReflectionUIConnector obsolete but still in use");
	}
}
