using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser
{
	[RequireComponent(typeof(Rigidbody))]
	public class SimpleFPSController : MonoBehaviour
	{
		public float lookSpeed = 100f;

		public float moveSpeed = 10f;

		public float moveForce = 20000f;

		public float jumpForce = 50f;

		public float dampening = 2f;

		private Vector3 bottom = new Vector3(0f, -1f, 0f);

		private Camera head;

		private Rigidbody body;

		private float lookPitch;

		public bool Grounded
		{
			get
			{
				RaycastHit[] array = Physics.SphereCastAll(new Ray(base.transform.position + bottom + base.transform.up * 0.01f, Physics.gravity.normalized), 0.1f, 0.1f);
				for (int i = 0; i < array.Length; i++)
				{
					if (!(array[i].rigidbody == body))
					{
						return true;
					}
				}
				return false;
			}
		}

		public void Awake()
		{
			head = GetComponentInChildren<Camera>();
			body = GetComponent<Rigidbody>();
		}

		public void Update()
		{
			Vector2 vector = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * lookSpeed;
			Quaternion quaternion = Quaternion.AngleAxis(vector.x, Vector3.up);
			base.transform.localRotation *= quaternion;
			lookPitch += 0f - vector.y;
			lookPitch = Mathf.Clamp(lookPitch, -90f, 90f);
			head.transform.localRotation = Quaternion.Euler(lookPitch, 0f, 0f);
			if (Input.GetButtonDown("Jump") && Grounded)
			{
				body.AddForce(-Physics.gravity.normalized * jumpForce, ForceMode.Impulse);
			}
		}

		public void FixedUpdate()
		{
			if (Time.frameCount < 5)
			{
				return;
			}
			bool grounded = Grounded;
			if (grounded)
			{
				body.drag = dampening;
			}
			else
			{
				body.drag = 0f;
			}
			Vector3 velocity = body.velocity;
			velocity.y = 0f;
			Vector3 vector = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
			if (vector.magnitude > 1f)
			{
				vector = vector.normalized;
			}
			if (velocity.magnitude > moveSpeed)
			{
				return;
			}
			vector = base.transform.TransformVector(vector);
			Vector3 force = vector * moveForce * Time.deltaTime;
			if (force.magnitude > 0f)
			{
				if (!grounded)
				{
					force *= 0.5f;
				}
				body.AddForce(force, ForceMode.Force);
			}
		}
	}
}
