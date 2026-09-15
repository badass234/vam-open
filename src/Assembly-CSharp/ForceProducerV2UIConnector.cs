using UnityEngine;

public class ForceProducerV2UIConnector : UIConnector
{
	public ForceProducerV2 forceProducer;

	public override void Connect()
	{
		Debug.LogError("ForceProducerV2UIonnector obsolete but still in use");
	}
}
