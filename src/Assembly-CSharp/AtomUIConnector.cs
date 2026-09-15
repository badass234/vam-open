using UnityEngine;

public class AtomUIConnector : UIConnector
{
	public Atom atom;

	public override void Connect()
	{
		Debug.LogError("AtomUIConnector obsolete but still in use");
	}
}
