using System;
using MVR.FileManagement;

namespace MVR.FileManagementSecure
{
	[Serializable]
	public class ShortCut
	{
		public string path;

		public string package = string.Empty;

		public string creator = string.Empty;

		public string packageFilter;

		public string displayName;

		public bool isLatest = true;

		public bool flatten;

		public bool includeRegularDirsInFlatten;

		public DirectoryEntry directoryEntry;

		public bool isHidden
		{
			get
			{
				if (directoryEntry != null)
				{
					return directoryEntry.IsHidden();
				}
				return false;
			}
		}

		public bool IsSameAs(ShortCut otherShortCut)
		{
			if (otherShortCut.path != path)
			{
				return false;
			}
			if (otherShortCut.package != package)
			{
				return false;
			}
			if (otherShortCut.creator != creator)
			{
				return false;
			}
			if (otherShortCut.packageFilter != packageFilter)
			{
				return false;
			}
			if (otherShortCut.displayName != displayName)
			{
				return false;
			}
			if (otherShortCut.flatten != flatten)
			{
				return false;
			}
			return true;
		}
	}
}
