using UnityEngine;

public class CharacterControl2D : MonoBehaviour
{
	public float speed = 10f;

	public float jumpPower = 2f;

	private Rigidbody rigidbody;

	public void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		rigidbody.AddForce(new Vector3(Input.GetAxis("Horizontal") * speed, 0f, 0f));
		if (Input.GetButtonDown("Jump"))
		{
			rigidbody.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
		}
	}
}
