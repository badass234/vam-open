using System;

namespace Battlehub.UIControls
{
	public class TreeViewItemContainerData : ItemContainerData
	{
		private TreeViewItemContainerData m_parent;

		public TreeViewItemContainerData Parent
		{
			get
			{
				return m_parent;
			}
			set
			{
				if (m_parent != value)
				{
					TreeViewItemContainerData parent = m_parent;
					m_parent = value;
					if (TreeViewItemContainerData.ParentChanged != null)
					{
						TreeViewItemContainerData.ParentChanged(this, new VirtualizingParentChangedEventArgs(parent, m_parent));
					}
				}
			}
		}

		public int Indent { get; set; }

		public bool CanExpand { get; set; }

		public bool IsExpanded { get; set; }

		public static event EventHandler<VirtualizingParentChangedEventArgs> ParentChanged;

		public bool IsDescendantOf(TreeViewItemContainerData ancestor)
		{
			if (ancestor == null)
			{
				return true;
			}
			if (ancestor == this)
			{
				return false;
			}
			for (TreeViewItemContainerData treeViewItemContainerData = this; treeViewItemContainerData != null; treeViewItemContainerData = treeViewItemContainerData.Parent)
			{
				if (ancestor == treeViewItemContainerData)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasChildren(VirtualizingTreeView treeView)
		{
			if (treeView == null)
			{
				return false;
			}
			int num = treeView.IndexOf(base.Item);
			TreeViewItemContainerData treeViewItemContainerData = (TreeViewItemContainerData)treeView.GetItemContainerData(num + 1);
			return treeViewItemContainerData != null && treeViewItemContainerData.Parent == this;
		}

		public TreeViewItemContainerData FirstChild(VirtualizingTreeView treeView)
		{
			if (!HasChildren(treeView))
			{
				return null;
			}
			int num = treeView.IndexOf(base.Item);
			num++;
			return (TreeViewItemContainerData)treeView.GetItemContainerData(num);
		}

		public TreeViewItemContainerData NextChild(VirtualizingTreeView treeView, TreeViewItemContainerData currentChild)
		{
			if (currentChild == null)
			{
				throw new ArgumentNullException("currentChild");
			}
			int num = treeView.IndexOf(currentChild.Item);
			num++;
			TreeViewItemContainerData treeViewItemContainerData = (TreeViewItemContainerData)treeView.GetItemContainerData(num);
			while (treeViewItemContainerData != null && treeViewItemContainerData.IsDescendantOf(this))
			{
				if (treeViewItemContainerData.Parent == this)
				{
					return treeViewItemContainerData;
				}
				num++;
				treeViewItemContainerData = (TreeViewItemContainerData)treeView.GetItemContainerData(num);
			}
			return null;
		}

		public TreeViewItemContainerData LastChild(VirtualizingTreeView treeView)
		{
			if (!HasChildren(treeView))
			{
				return null;
			}
			int num = treeView.IndexOf(base.Item);
			TreeViewItemContainerData result = null;
			while (true)
			{
				num++;
				TreeViewItemContainerData treeViewItemContainerData = (TreeViewItemContainerData)treeView.GetItemContainerData(num);
				if (treeViewItemContainerData == null || !treeViewItemContainerData.IsDescendantOf(this))
				{
					break;
				}
				if (treeViewItemContainerData.Parent == this)
				{
					result = treeViewItemContainerData;
				}
			}
			return result;
		}

		public TreeViewItemContainerData LastDescendant(VirtualizingTreeView treeView)
		{
			if (!HasChildren(treeView))
			{
				return null;
			}
			int num = treeView.IndexOf(base.Item);
			TreeViewItemContainerData result = null;
			while (true)
			{
				num++;
				TreeViewItemContainerData treeViewItemContainerData = (TreeViewItemContainerData)treeView.GetItemContainerData(num);
				if (treeViewItemContainerData == null || !treeViewItemContainerData.IsDescendantOf(this))
				{
					break;
				}
				result = treeViewItemContainerData;
			}
			return result;
		}

		public override string ToString()
		{
			return base.ToString();
		}
	}
}
