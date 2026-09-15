using UnityEngine;

public class HairEditingCamera : MonoBehaviour
{
	public Transform yawT;

	public Transform pitchT;

	public float speed = 100f;

	private void Update()
	{
		if (Input.GetMouseButton(0))
		{
			float y = yawT.localEulerAngles.y + Input.GetAxis("Mouse X") * Time.deltaTime * speed;
			yawT.localEulerAngles = new Vector3(0f, y, 0f);
			float x = pitchT.localEulerAngles.x + Input.GetAxis("Mouse Y") * Time.deltaTime * speed;
			pitchT.localEulerAngles = new Vector3(x, 0f, 0f);
		}
		if (new Rect(0f, 0f, Screen.width, Screen.height).Contains(Input.mousePosition))
		{
			base.transform.Translate(Vector3.forward * Input.GetAxis("Mouse ScrollWheel") * speed * 0.02f);
		}
	}
}
