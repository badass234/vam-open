using System;
using UnityEngine;

[Serializable]
public class ChainRotate : MonoBehaviour
{
	public float speedx;

	public float speedy;

	public float speedz;

	private bool UseCenter;

	public virtual void Start()
	{
		if (UseCenter)
		{
			transform.position = GetComponent<Renderer>().bounds.center + transform.position;
		}
	}

	public virtual void Update()
	{
		transform.Rotate(speedx * Time.deltaTime, speedy * Time.deltaTime, speedz * Time.deltaTime);
	}

	public virtual void Main()
	{
	}
}
