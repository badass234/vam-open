using UnityEngine;

namespace AQUAS
{
	public class AQUAS_Walk : MonoBehaviour
	{
		public float m_moveSpeed = 10f;

		public CharacterController m_controller;

		private void Start()
		{
			if (m_controller == null)
			{
				m_controller = GetComponent<CharacterController>();
			}
		}

		private void Update()
		{
			if (m_controller != null && m_controller.enabled)
			{
				Vector3 vector = Input.GetAxis("Vertical") * base.transform.TransformDirection(Vector3.forward) * m_moveSpeed;
				m_controller.Move(vector * Time.deltaTime);
				Vector3 vector2 = Input.GetAxis("Horizontal") * base.transform.TransformDirection(Vector3.right) * m_moveSpeed;
				m_controller.Move(vector2 * Time.deltaTime);
				m_controller.SimpleMove(Physics.gravity);
			}
		}
	}
}
