using UnityEngine;

public class GpuGrabSphere : MonoBehaviour
{
	public float radius;

	public int id;

	public int enabledThisFrame;

	public int numFramesToGrabOnEnable;

	public int frameCountdown;

	public float WorldRadius => radius * base.transform.lossyScale.x;

	private void OnEnable()
	{
		if (Application.isPlaying)
		{
			frameCountdown = numFramesToGrabOnEnable;
			GPUCollidersManager.RegisterGrabSphere(this);
		}
	}

	private void OnDisable()
	{
		if (Application.isPlaying)
		{
			GPUCollidersManager.DeregisterGrabSphere(this);
		}
	}
}
