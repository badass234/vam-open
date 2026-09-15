using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	[ExecuteInEditMode]
	public class ResourceMap : BundleResourceMap
	{
		[SerializeField]
		[ReadOnly]
		private int m_counter = 1;

		public int GetCounter()
		{
			return m_counter;
		}

		public int IncCounter()
		{
			m_counter++;
			return m_counter;
		}
	}
}
