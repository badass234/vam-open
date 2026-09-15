using UnityEngine;

public class RobotArmController : MonoBehaviour
{
	public Transform section1;

	public Transform section2;

	public Transform actuator;

	public float speed = 40f;

	private void Update()
	{
		if (Input.GetKey(KeyCode.A))
		{
			section1.Rotate(0f, speed * Time.deltaTime, 0f, Space.World);
		}
		if (Input.GetKey(KeyCode.D))
		{
			section1.Rotate(0f, (0f - speed) * Time.deltaTime, 0f, Space.World);
		}
		if (Input.GetKey(KeyCode.W))
		{
			section1.Rotate(0f, speed * Time.deltaTime, 0f, Space.Self);
		}
		if (Input.GetKey(KeyCode.S))
		{
			section1.Rotate(0f, (0f - speed) * Time.deltaTime, 0f, Space.Self);
		}
		if (Input.GetKey(KeyCode.T))
		{
			section2.Rotate(0f, speed * Time.deltaTime, 0f, Space.Self);
		}
		if (Input.GetKey(KeyCode.G))
		{
			section2.Rotate(0f, (0f - speed) * Time.deltaTime, 0f, Space.Self);
		}
		if (Input.GetKey(KeyCode.Y))
		{
			actuator.Rotate(0f, speed * Time.deltaTime, 0f, Space.Self);
		}
		if (Input.GetKey(KeyCode.H))
		{
			actuator.Rotate(0f, (0f - speed) * Time.deltaTime, 0f, Space.Self);
		}
	}
}
