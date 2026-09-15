using System;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad
{
	public class SaveLoadConfig
	{
		private static Type[] m_disabledTypes;

		public static Type[] DisabledComponentTypes => m_disabledTypes;

		static SaveLoadConfig()
		{
			m_disabledTypes = new Type[0];
			m_disabledTypes = new Type[1] { typeof(Button) };
		}
	}
}
