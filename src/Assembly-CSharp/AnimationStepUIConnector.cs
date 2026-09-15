using UnityEngine;

public class AnimationStepUIConnector : UIConnector
{
	public AnimationStep animationStep;

	public override void Connect()
	{
		Debug.LogError("AnimationStepUIConnector obsolete but still in use");
	}
}
