using System;

namespace Battlehub.UIControls
{
	public class VirtualizingParentChangedEventArgs : EventArgs
	{
		public TreeViewItemContainerData OldParent { get; private set; }

		public TreeViewItemContainerData NewParent { get; private set; }

		public VirtualizingParentChangedEventArgs(TreeViewItemContainerData oldParent, TreeViewItemContainerData newParent)
		{
			OldParent = oldParent;
			NewParent = newParent;
		}
	}
}
