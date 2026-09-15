using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls
{
	public class VirtualizingTreeViewItem : VirtualizingItemContainer
	{
		private TreeViewExpander m_expander;

		[SerializeField]
		private HorizontalLayoutGroup m_itemLayout;

		private Toggle m_toggle;

		private VirtualizingTreeView m_treeView;

		private TreeViewItemContainerData m_treeViewItemData;

		private VirtualizingTreeView TreeView
		{
			get
			{
				if (m_treeView == null)
				{
					m_treeView = GetComponentInParent<VirtualizingTreeView>();
				}
				return m_treeView;
			}
		}

		public float Indent => m_treeViewItemData.Indent;

		public override object Item
		{
			get
			{
				return base.Item;
			}
			set
			{
				if (base.Item != value)
				{
					base.Item = value;
					m_treeViewItemData = (TreeViewItemContainerData)TreeView.GetItemContainerData(value);
					if (m_treeViewItemData == null)
					{
						base.name = "Null";
						return;
					}
					UpdateIndent();
					m_expander.CanExpand = m_treeViewItemData.CanExpand;
					m_expander.IsOn = m_treeViewItemData.IsExpanded && m_treeViewItemData.CanExpand;
					base.name = base.Item.ToString() + " " + m_treeViewItemData.ToString();
				}
			}
		}

		public TreeViewItemContainerData TreeViewItemData => m_treeViewItemData;

		public TreeViewItemContainerData Parent
		{
			get
			{
				return m_treeViewItemData.Parent;
			}
			set
			{
				if (m_treeViewItemData != null && m_treeViewItemData.Parent != value)
				{
					m_treeViewItemData.Parent = value;
					UpdateIndent();
				}
			}
		}

		public override bool IsSelected
		{
			get
			{
				return base.IsSelected;
			}
			set
			{
				if (base.IsSelected != value)
				{
					m_toggle.isOn = value;
					base.IsSelected = value;
				}
			}
		}

		public bool CanExpand
		{
			get
			{
				return m_treeViewItemData != null && m_treeViewItemData.CanExpand;
			}
			set
			{
				if (m_treeViewItemData.CanExpand != value)
				{
					m_treeViewItemData.CanExpand = value;
					if (m_expander != null)
					{
						m_expander.CanExpand = m_treeViewItemData.CanExpand;
					}
					if (!m_treeViewItemData.CanExpand)
					{
						IsExpanded = false;
					}
				}
			}
		}

		public bool IsExpanded
		{
			get
			{
				return m_treeViewItemData.IsExpanded;
			}
			set
			{
				if (m_treeViewItemData == null || m_treeViewItemData.IsExpanded == value)
				{
					return;
				}
				m_treeViewItemData.IsExpanded = value && CanExpand;
				if (m_expander != null)
				{
					m_expander.IsOn = value && CanExpand;
				}
				if (TreeView != null)
				{
					if (m_treeViewItemData.IsExpanded)
					{
						TreeView.Expand(m_treeViewItemData.Item);
					}
					else
					{
						TreeView.Collapse(m_treeViewItemData.Item);
					}
				}
			}
		}

		public bool HasChildren => m_treeViewItemData.HasChildren(TreeView);

		public void UpdateIndent()
		{
			if (Parent != null && TreeView != null && m_itemLayout != null)
			{
				m_treeViewItemData.Indent = Parent.Indent + TreeView.Indent;
				m_itemLayout.padding = new RectOffset(m_treeViewItemData.Indent, m_itemLayout.padding.right, m_itemLayout.padding.top, m_itemLayout.padding.bottom);
				int itemIndex = TreeView.IndexOf(Item);
				SetIndent(this, ref itemIndex);
				return;
			}
			ZeroIndent();
			int itemIndex2 = TreeView.IndexOf(Item);
			if (HasChildren)
			{
				SetIndent(this, ref itemIndex2);
			}
		}

		private void ZeroIndent()
		{
			m_treeViewItemData.Indent = 0;
			if (m_itemLayout != null)
			{
				m_itemLayout.padding = new RectOffset(m_treeViewItemData.Indent, m_itemLayout.padding.right, m_itemLayout.padding.top, m_itemLayout.padding.bottom);
			}
		}

		private void SetIndent(VirtualizingTreeViewItem parent, ref int itemIndex)
		{
			while (true)
			{
				object itemAt = TreeView.GetItemAt(itemIndex + 1);
				VirtualizingTreeViewItem virtualizingTreeViewItem = (VirtualizingTreeViewItem)TreeView.GetItemContainer(itemAt);
				if (virtualizingTreeViewItem == null || virtualizingTreeViewItem.Item == null || virtualizingTreeViewItem.Parent != parent.m_treeViewItemData)
				{
					break;
				}
				virtualizingTreeViewItem.m_treeViewItemData.Indent = parent.m_treeViewItemData.Indent + TreeView.Indent;
				virtualizingTreeViewItem.m_itemLayout.padding.left = virtualizingTreeViewItem.m_treeViewItemData.Indent;
				itemIndex++;
				SetIndent(virtualizingTreeViewItem, ref itemIndex);
			}
		}

		public TreeViewItemContainerData FirstChild()
		{
			return m_treeViewItemData.FirstChild(TreeView);
		}

		public TreeViewItemContainerData NextChild(TreeViewItemContainerData currentChild)
		{
			return m_treeViewItemData.NextChild(TreeView, currentChild);
		}

		public TreeViewItemContainerData LastChild()
		{
			return m_treeViewItemData.LastChild(TreeView);
		}

		public TreeViewItemContainerData LastDescendant()
		{
			return m_treeViewItemData.LastDescendant(TreeView);
		}

		protected override void AwakeOverride()
		{
			m_toggle = GetComponent<Toggle>();
			m_toggle.interactable = false;
			m_toggle.isOn = IsSelected;
			m_expander = GetComponentInChildren<TreeViewExpander>();
			if (m_expander != null)
			{
				m_expander.CanExpand = CanExpand;
			}
		}

		protected override void StartOverride()
		{
			if (TreeView != null)
			{
				m_toggle.isOn = TreeView.IsItemSelected(Item);
				m_isSelected = m_toggle.isOn;
			}
			if (Parent != null)
			{
				m_treeViewItemData.Indent = Parent.Indent + TreeView.Indent;
				m_itemLayout.padding = new RectOffset(m_treeViewItemData.Indent, m_itemLayout.padding.right, m_itemLayout.padding.top, m_itemLayout.padding.bottom);
			}
			if (CanExpand && TreeView.AutoExpand)
			{
				IsExpanded = true;
			}
		}

		public override void Clear()
		{
			base.Clear();
		}
	}
}
