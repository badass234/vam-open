using UnityEngine;

public class wind_rot : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		base.transform.Rotate(Vector3.up, 0.05f);
	}
}
