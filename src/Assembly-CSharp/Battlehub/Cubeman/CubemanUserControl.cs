using UnityEngine;

namespace Battlehub.Cubeman
{
	[RequireComponent(typeof(CubemanCharacter))]
	public class CubemanUserControl : MonoBehaviour
	{
		public Transform Cam;

		private CubemanCharacter m_Character;

		private Vector3 m_CamForward;

		private Vector3 m_Move;

		private bool m_Jump;

		public bool HandleInput;

		private void Start()
		{
			if (Cam == null)
			{
				if (Camera.main != null)
				{
					Cam = Camera.main.transform;
				}
				if (!(Cam == null))
				{
				}
			}
			m_Character = GetComponent<CubemanCharacter>();
		}

		private void Update()
		{
			if (!m_Jump)
			{
				m_Jump = Input.GetKey(KeyCode.Space) && HandleInput;
			}
		}

		private void FixedUpdate()
		{
			float num = Input.GetAxis("Horizontal");
			float num2 = Input.GetAxis("Vertical");
			bool key = Input.GetKey(KeyCode.C);
			key = false;
			if (!HandleInput)
			{
				num = (num2 = 0f);
				key = false;
			}
			if (Cam != null)
			{
				m_CamForward = Vector3.Scale(Cam.forward, new Vector3(1f, 0f, 1f)).normalized;
				m_Move = num2 * m_CamForward + num * Cam.right;
			}
			else
			{
				m_Move = num2 * Vector3.forward + num * Vector3.right;
			}
			if (Input.GetKey(KeyCode.LeftShift))
			{
				m_Move *= 0.5f;
			}
			if (m_Character.enabled)
			{
				m_Character.Move(m_Move, key, m_Jump);
			}
			m_Jump = false;
		}
	}
}
