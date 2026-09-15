using UnityEngine;

namespace Weelco
{
	public class MouseCameraRotation : MonoBehaviour
	{
		[Tooltip("If false, press Left Ctrl button for rotation")]
		public bool alwaysRotate;

		public bool lerpBack = true;

		public float speedH = 2.5f;

		public float speedV = 2.5f;

		public float speedL = 7f;

		private float yaw;

		private float pitch;

		private void Start()
		{
			yaw = base.transform.rotation.eulerAngles.y;
		}

		private void Update()
		{
			if (alwaysRotate)
			{
				MouseRotate();
			}
			else if (Input.GetKey(KeyCode.LeftControl))
			{
				MouseRotate();
			}
			else if (lerpBack)
			{
				LerpBack();
			}
		}

		private void MouseRotate()
		{
			yaw += speedH * Input.GetAxis("Mouse X");
			pitch -= speedV * Input.GetAxis("Mouse Y");
			base.transform.eulerAngles = new Vector3(pitch, yaw, 0f);
		}

		private void LerpBack()
		{
			if (!base.transform.rotation.Equals(Quaternion.Euler(Vector3.zero)))
			{
				yaw = 0f;
				pitch = 0f;
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(Vector3.zero), Time.deltaTime * speedL);
			}
		}
	}
}
