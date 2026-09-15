using System;
using UnityEngine;

[Serializable]
public class Rotate : MonoBehaviour
{
	public virtual void Update()
	{
		base.transform.Rotate(0f, Time.deltaTime * 10f, 0f);
	}
}
