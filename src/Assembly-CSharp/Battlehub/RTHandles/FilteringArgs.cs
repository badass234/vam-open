using System;
using UnityEngine;

namespace Battlehub.RTHandles
{
	public class FilteringArgs : EventArgs
	{
		private bool m_cancel;

		public bool Cancel
		{
			get
			{
				return m_cancel;
			}
			set
			{
				if (value)
				{
					m_cancel = true;
				}
			}
		}

		public GameObject Object { get; set; }

		public void Reset()
		{
			m_cancel = false;
		}
	}
}
