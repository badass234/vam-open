using UnityEngine;

public class RotateHologram : MonoBehaviour
{
	private void Update()
	{
		base.transform.Rotate(new Vector3(0f, 25f, 0f) * Time.deltaTime);
	}
}
