using System;

namespace Battlehub.RTCommon
{
	public class Record
	{
		private object m_state;

		private object m_target;

		private ApplyCallback m_applyCallback;

		private PurgeCallback m_purgeCallback;

		public object Target => m_target;

		public object State => m_state;

		public Record(object target, object state, ApplyCallback applyCallback, PurgeCallback purgeCallback)
		{
			if (applyCallback == null)
			{
				throw new ArgumentNullException("callback");
			}
			m_target = target;
			m_applyCallback = applyCallback;
			m_purgeCallback = purgeCallback;
			if (state != null)
			{
				m_state = state;
			}
		}

		public bool Apply()
		{
			return m_applyCallback(this);
		}

		public void Purge()
		{
			m_purgeCallback(this);
		}
	}
}
