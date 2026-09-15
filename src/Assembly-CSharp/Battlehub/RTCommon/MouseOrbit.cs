using UnityEngine;

namespace Battlehub.RTCommon
{
	[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
	public class MouseOrbit : MonoBehaviour
	{
		private Camera m_camera;

		public Transform Target;

		public float Distance = 5f;

		public float XSpeed = 5f;

		public float YSpeed = 5f;

		public float YMinLimit = -360f;

		public float YMaxLimit = 360f;

		public float DistanceMin = 0.5f;

		public float DistanceMax = 5000f;

		private float m_x;

		private float m_y;

		private Vector3 lastMousePosition;

		private void Awake()
		{
			m_camera = GetComponent<Camera>();
		}

		private void Start()
		{
			SyncAngles();
		}

		public void SyncAngles()
		{
			Vector3 eulerAngles = base.transform.eulerAngles;
			m_x = eulerAngles.y;
			m_y = eulerAngles.x;
		}

		private void LateUpdate()
		{
			float num = Input.GetAxis("Mouse X");
			float num2 = Input.GetAxis("Mouse Y");
			Vector3 mousePosition = Input.mousePosition;
			Vector3 vector = mousePosition - lastMousePosition;
			if (num == 0f && num2 == 0f && (vector.x != 0f || vector.y != 0f))
			{
				num = vector.x * 0.1f;
				num2 = vector.y * 0.1f;
			}
			lastMousePosition = mousePosition;
			num *= XSpeed;
			num2 *= YSpeed;
			m_x += num;
			m_y -= num2;
			m_y = ClampAngle(m_y, YMinLimit, YMaxLimit);
			Zoom();
		}

		public void Zoom()
		{
			Quaternion quaternion = Quaternion.Euler(m_y, m_x, 0f);
			base.transform.rotation = quaternion;
			float axis = Input.GetAxis("Mouse ScrollWheel");
			if (m_camera.orthographic)
			{
				m_camera.orthographicSize -= axis * m_camera.orthographicSize;
				if (m_camera.orthographicSize < 0.01f)
				{
					m_camera.orthographicSize = 0.01f;
				}
			}
			Distance = Mathf.Clamp(Distance - axis * Mathf.Max(1f, Distance), DistanceMin, DistanceMax);
			Vector3 vector = new Vector3(0f, 0f, 0f - Distance);
			Vector3 position = quaternion * vector + Target.position;
			base.transform.position = position;
		}

		public static float ClampAngle(float angle, float min, float max)
		{
			if (angle < -360f)
			{
				angle += 360f;
			}
			if (angle > 360f)
			{
				angle -= 360f;
			}
			return Mathf.Clamp(angle, min, max);
		}
	}
}
