using UnityEngine;

namespace Battlehub.RTHandles
{
	public class GameFerryBehavior : MonoBehaviour
	{
		private Animator m_animator;

		private GameFerry m_ferry;

		private int m_shortNameHash;

		private void Start()
		{
			m_animator = GetComponent<Animator>();
			m_shortNameHash = m_animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
			m_ferry = m_animator.GetComponent<GameFerry>();
		}

		private void Update()
		{
			AnimatorStateInfo currentAnimatorStateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
			if (m_shortNameHash != currentAnimatorStateInfo.shortNameHash)
			{
				if (!currentAnimatorStateInfo.IsName("IdleForward") && !currentAnimatorStateInfo.IsName("IdleBackward"))
				{
					m_ferry.Lock();
				}
				else
				{
					m_ferry.Unlock();
				}
				m_shortNameHash = currentAnimatorStateInfo.shortNameHash;
			}
		}
	}
}
