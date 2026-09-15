using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Battlehub.Utils
{
	public class BtnRepeat : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IEventSystemHandler
	{
		public float Interval = 0.1f;

		private bool m_repeat;

		private float m_timeElapsed;

		public UnityEvent RepeatClick;

		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
			m_timeElapsed = 0f;
			m_repeat = true;
		}

		void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
		{
			m_timeElapsed = 0f;
			m_repeat = false;
		}

		private void Update()
		{
			if (m_repeat)
			{
				m_timeElapsed += Time.deltaTime;
				if (m_timeElapsed >= Interval)
				{
					RepeatClick.Invoke();
					m_timeElapsed = 0f;
				}
			}
		}
	}
}
