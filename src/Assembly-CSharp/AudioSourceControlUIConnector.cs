using UnityEngine;

public class AudioSourceControlUIConnector : UIConnector
{
	public AudioSourceControl audioSourceControl;

	public override void Connect()
	{
		Debug.LogError("AudioSourceControlUIConnect obsolete but still in use");
	}
}
