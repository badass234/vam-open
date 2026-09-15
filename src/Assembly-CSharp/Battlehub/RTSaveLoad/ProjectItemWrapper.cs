using System;
using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	public class ProjectItemWrapper : ScriptableObject
	{
		[NonSerialized]
		private ProjectItem m_projectItem;

		public ProjectItem ProjectItem
		{
			get
			{
				return m_projectItem;
			}
			set
			{
				m_projectItem = value;
				if (m_projectItem == null)
				{
					base.name = "None";
				}
				else
				{
					base.name = m_projectItem.Name;
				}
			}
		}

		public bool IsNone => ProjectItem == null;

		public bool IsScene => !IsNone && ProjectItem.IsScene;

		public bool IsFolder => !IsNone && ProjectItem.IsFolder;
	}
}
