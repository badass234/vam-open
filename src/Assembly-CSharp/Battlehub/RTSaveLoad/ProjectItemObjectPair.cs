using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	public class ProjectItemObjectPair
	{
		public bool IsNone => ProjectItem == null && Object is NoneItem;

		public bool IsSceneObject => ProjectItem == null && !(Object is NoneItem) && Object != null;

		public bool IsScene => !IsNone && ProjectItem != null && ProjectItem.IsScene;

		public bool IsFolder => !IsNone && ProjectItem != null && ProjectItem.IsFolder;

		public bool IsResource => !IsNone && ProjectItem != null && !ProjectItem.IsFolder && !ProjectItem.IsScene;

		public ProjectItem ProjectItem { get; private set; }

		public Object Object { get; private set; }

		public ProjectItemObjectPair(ProjectItem projectItem, Object obj)
		{
			ProjectItem = projectItem;
			Object = obj;
		}
	}
}
