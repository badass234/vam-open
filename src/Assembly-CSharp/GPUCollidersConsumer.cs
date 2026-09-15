using UnityEngine;

public class GPUCollidersConsumer : MonoBehaviour
{
	protected virtual void OnEnable()
	{
		if (Application.isPlaying)
		{
			GPUCollidersManager.RegisterConsumer(this);
		}
	}

	protected virtual void OnDisable()
	{
		if (Application.isPlaying)
		{
			GPUCollidersManager.DeregisterConsumer(this);
		}
	}
}
