using UnityEngine;

public class CenterTransformOnCam : MonoBehaviour
{
	public Transform Camera;

	private void Update()
	{
		Vector3 position = new Vector3(Camera.position.x, Camera.position.y - 0.2f, Camera.position.z);
		base.transform.position = position;
	}
}
