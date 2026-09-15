using UnityEngine;

public class MoveProducerUIConnector : UIConnector
{
	public MoveProducer moveProducer;

	public override void Connect()
	{
		Debug.LogError("MoveProducerUIConnector obsolete but still in use");
	}
}
