using UnityEngine;

public class MoveWaterToCamera : MonoBehaviour
{
	public GameObject CurrenCamera;

	private void Update()
	{
		if (!(CurrenCamera == null))
		{
			Vector3 position = base.transform.position;
			position.x = CurrenCamera.transform.position.x;
			position.z = CurrenCamera.transform.position.z;
			base.transform.position = position;
			Quaternion rotation = CurrenCamera.transform.rotation;
			rotation.eulerAngles = new Vector3(rotation.eulerAngles.x, 0f, rotation.eulerAngles.z);
		}
	}
}
