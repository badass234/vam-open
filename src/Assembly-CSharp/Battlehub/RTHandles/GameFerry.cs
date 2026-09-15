using Battlehub.Cubeman;
using UnityEngine;

namespace Battlehub.RTHandles
{
	public class GameFerry : MonoBehaviour
	{
		private FixedJoint m_joint;

		private Rigidbody m_rig;

		private void Start()
		{
			m_joint = base.gameObject.AddComponent<FixedJoint>();
		}

		private void OnTriggerEnter(Collider c)
		{
			if ((bool)c.GetComponent<CubemanCharacter>())
			{
				Rigidbody component = c.GetComponent<Rigidbody>();
				m_rig = component;
			}
		}

		private void OnTriggerExit(Collider c)
		{
			if ((bool)c.GetComponent<CubemanCharacter>())
			{
				m_rig = null;
			}
		}

		public void Lock()
		{
			if (!m_joint)
			{
				m_joint = base.gameObject.AddComponent<FixedJoint>();
			}
			m_joint.connectedBody = m_rig;
			m_joint.breakForce = float.PositiveInfinity;
		}

		public void Unlock()
		{
			if ((bool)m_joint)
			{
				m_joint.breakForce = 0.0001f;
			}
		}
	}
}
