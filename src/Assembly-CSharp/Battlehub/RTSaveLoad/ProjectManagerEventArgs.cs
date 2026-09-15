using System;

namespace Battlehub.RTSaveLoad
{
	public class ProjectManagerEventArgs : EventArgs
	{
		public ProjectItem[] ProjectItems { get; private set; }

		public ProjectItem ProjectItem
		{
			get
			{
				if (ProjectItems == null || ProjectItems.Length == 0)
				{
					return null;
				}
				return ProjectItems[0];
			}
		}

		public ProjectManagerEventArgs(ProjectItem[] items)
		{
			ProjectItems = items;
		}

		public ProjectManagerEventArgs(ProjectItem item)
		{
			ProjectItems = new ProjectItem[1] { item };
		}
	}
}
