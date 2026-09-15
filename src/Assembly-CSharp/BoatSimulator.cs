using UnityEngine;

public class BoatSimulator : MonoBehaviour
{
	private Rigidbody rigid;

	private bool keyPressedW;

	private bool keyPressedA;

	private bool keyPressedS;

	private bool keyPressedD;

	private void Start()
	{
		rigid = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.W))
		{
			keyPressedW = true;
		}
		if (Input.GetKeyDown(KeyCode.A))
		{
			keyPressedA = true;
		}
		if (Input.GetKeyDown(KeyCode.S))
		{
			keyPressedS = true;
		}
		if (Input.GetKeyDown(KeyCode.D))
		{
			keyPressedD = true;
		}
		if (Input.GetKeyUp(KeyCode.W))
		{
			keyPressedW = false;
		}
		if (Input.GetKeyUp(KeyCode.A))
		{
			keyPressedA = false;
		}
		if (Input.GetKeyUp(KeyCode.S))
		{
			keyPressedS = false;
		}
		if (Input.GetKeyUp(KeyCode.D))
		{
			keyPressedD = false;
		}
		if (keyPressedW)
		{
			rigid.AddForce(base.transform.right * 500f * Time.deltaTime);
		}
		if (keyPressedS)
		{
			rigid.AddForce(-base.transform.right * 500f * Time.deltaTime);
		}
		if (keyPressedD)
		{
			rigid.AddTorque(base.transform.up * 200f * Time.deltaTime);
		}
		if (keyPressedA)
		{
			rigid.AddTorque(-base.transform.up * 200f * Time.deltaTime);
		}
	}
}
