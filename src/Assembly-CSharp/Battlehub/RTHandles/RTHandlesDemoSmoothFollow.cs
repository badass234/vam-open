using UnityEngine;

namespace Battlehub.RTHandles
{
	public class RTHandlesDemoSmoothFollow : MonoBehaviour
	{
		private Transform m_target;

		[SerializeField]
		private float distance = 10f;

		[SerializeField]
		private float height = 5f;

		[SerializeField]
		private float rotationDamping;

		[SerializeField]
		private float heightDamping;

		[SerializeField]
		public Transform target
		{
			get
			{
				return m_target;
			}
			set
			{
				m_target = value;
				float num = rotationDamping;
				float num2 = heightDamping;
				rotationDamping = float.MaxValue;
				heightDamping = float.MaxValue;
				Follow();
				heightDamping = num2;
				rotationDamping = num;
			}
		}

		private void Start()
		{
		}

		private void LateUpdate()
		{
			if ((bool)target)
			{
				Follow();
			}
		}

		private void Follow()
		{
			float y = target.eulerAngles.y;
			float b = target.position.y + height;
			float y2 = base.transform.eulerAngles.y;
			float y3 = base.transform.position.y;
			y2 = Mathf.LerpAngle(y2, y, rotationDamping * Time.deltaTime);
			y3 = Mathf.Lerp(y3, b, heightDamping * Time.deltaTime);
			Quaternion quaternion = Quaternion.Euler(0f, y2, 0f);
			base.transform.position = target.position;
			base.transform.position -= quaternion * Vector3.forward * distance;
			base.transform.position = new Vector3(base.transform.position.x, y3, base.transform.position.z);
			base.transform.LookAt(target);
		}
	}
}
