using Battlehub.Cubeman;
using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTHandles
{
	public class EditorCharacter : MonoBehaviour
	{
		private Rigidbody m_rigidBody;

		private CubemanCharacter m_character;

		private bool m_isKinematic;

		private bool m_isEnabled;

		private void Start()
		{
			m_character = GetComponent<CubemanCharacter>();
			m_rigidBody = GetComponent<Rigidbody>();
		}

		private void Update()
		{
			if (InputController.GetKeyDown(KeyCode.V))
			{
				m_isKinematic = m_rigidBody.isKinematic;
				m_rigidBody.isKinematic = true;
				m_isEnabled = m_character.Enabled;
				m_character.Enabled = false;
			}
			else if (InputController.GetKeyUp(KeyCode.V))
			{
				m_rigidBody.isKinematic = m_isKinematic;
				m_character.Enabled = m_isEnabled;
			}
			if (base.transform.position.y < -5000f)
			{
				m_rigidBody.isKinematic = true;
				m_character.Enabled = false;
			}
		}

		public void OnSelected(ExposeToEditor obj)
		{
			if (EditorDemo.Instance != null && EditorDemo.Instance.EnableCharacters)
			{
				EnableCharacter(obj.gameObject);
			}
		}

		private void EnableCharacter(GameObject obj)
		{
			if ((bool)m_rigidBody)
			{
				m_rigidBody.isKinematic = false;
				m_character.Enabled = true;
				CubemanUserControl component = obj.GetComponent<CubemanUserControl>();
				if (component != null)
				{
					component.HandleInput = true;
				}
			}
		}

		public void OnUnselected(ExposeToEditor obj)
		{
			Rigidbody component = obj.GetComponent<Rigidbody>();
			if ((bool)component)
			{
				component.isKinematic = true;
			}
			CubemanCharacter component2 = obj.GetComponent<CubemanCharacter>();
			if (component2 != null)
			{
				component2.Move(Vector3.zero, false, false);
				component2.Enabled = false;
			}
			CubemanUserControl component3 = obj.GetComponent<CubemanUserControl>();
			if (component3 != null)
			{
				component3.HandleInput = false;
			}
		}
	}
}
