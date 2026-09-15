using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Battlehub.UIControls
{
	public class VirtualizingTreeView : VirtualizingItemsControl<TreeViewItemDataBindingArgs>
	{
		public int Indent = 20;

		public bool CanReparent = true;

		public bool AutoExpand;

		private bool m_expandSilently;

		protected override bool CanScroll => base.CanScroll || CanReparent;

		public event EventHandler<ItemExpandingArgs> ItemExpanding;

		protected override void OnEnableOverride()
		{
			base.OnEnableOverride();
			TreeViewItemContainerData.ParentChanged += OnTreeViewItemParentChanged;
		}

		protected override void OnDisableOverride()
		{
			base.OnDisableOverride();
			TreeViewItemContainerData.ParentChanged -= OnTreeViewItemParentChanged;
		}

		protected override ItemContainerData InstantiateItemContainerData(object item)
		{
			TreeViewItemContainerData treeViewItemContainerData = new TreeViewItemContainerData();
			treeViewItemContainerData.Item = item;
			return treeViewItemContainerData;
		}

		public void AddChild(object parent, object item)
		{
			if (parent == null)
			{
				Add(item);
				return;
			}
			VirtualizingTreeViewItem virtualizingTreeViewItem = (VirtualizingTreeViewItem)GetItemContainer(parent);
			if (virtualizingTreeViewItem == null)
			{
				return;
			}
			int num = -1;
			if (virtualizingTreeViewItem.IsExpanded)
			{
				if (virtualizingTreeViewItem.HasChildren)
				{
					TreeViewItemContainerData treeViewItemContainerData = virtualizingTreeViewItem.LastDescendant();
					num = IndexOf(treeViewItemContainerData.Item) + 1;
				}
				else
				{
					num = IndexOf(virtualizingTreeViewItem.Item) + 1;
				}
			}
			else
			{
				virtualizingTreeViewItem.CanExpand = true;
			}
			if (num > -1)
			{
				TreeViewItemContainerData treeViewItemContainerData2 = (TreeViewItemContainerData)Insert(num, item);
				VirtualizingTreeViewItem virtualizingTreeViewItem2 = (VirtualizingTreeViewItem)GetItemContainer(item);
				if (virtualizingTreeViewItem2 != null)
				{
					virtualizingTreeViewItem2.Parent = virtualizingTreeViewItem.TreeViewItemData;
				}
				else
				{
					treeViewItemContainerData2.Parent = virtualizingTreeViewItem.TreeViewItemData;
				}
			}
		}

		public override void Remove(object item)
		{
			throw new NotSupportedException("This method is not supported for TreeView use RemoveChild instead");
		}

		public void RemoveChild(object parent, object item, bool isLastChild)
		{
			if (parent == null)
			{
				base.Remove(item);
			}
			else if (GetItemContainer(item) != null)
			{
				base.Remove(item);
			}
			else if (isLastChild)
			{
				VirtualizingTreeViewItem virtualizingTreeViewItem = (VirtualizingTreeViewItem)GetItemContainer(parent);
				if ((bool)virtualizingTreeViewItem)
				{
					virtualizingTreeViewItem.CanExpand = false;
				}
			}
		}

		public void ChangeParent(object parent, object item)
		{
			if (base.IsDropInProgress)
			{
				return;
			}
			ItemContainerData itemContainerData = GetItemContainerData(item);
			if (itemContainerData != null)
			{
				ItemContainerData itemContainerData2 = GetItemContainerData(parent);
				ItemContainerData[] dragItems = new ItemContainerData[1] { itemContainerData };
				if (CanDrop(dragItems, itemContainerData2))
				{
					Drop(dragItems, itemContainerData2, ItemDropAction.SetLastChild);
				}
			}
		}

		public bool IsExpanded(object item)
		{
			return ((TreeViewItemContainerData)GetItemContainerData(item))?.IsExpanded ?? false;
		}

		public void Expand(object item)
		{
			if (m_expandSilently || this.ItemExpanding == null)
			{
				return;
			}
			TreeViewItemContainerData treeViewItemContainerData = (TreeViewItemContainerData)GetItemContainerData(item);
			ItemExpandingArgs itemExpandingArgs = new ItemExpandingArgs(treeViewItemContainerData.Item);
			this.ItemExpanding(this, itemExpandingArgs);
			IEnumerable enumerable = itemExpandingArgs.Children.OfType<object>().ToArray();
			int num = IndexOf(treeViewItemContainerData.Item);
			VirtualizingTreeViewItem virtualizingTreeViewItem = (VirtualizingTreeViewItem)GetItemContainer(treeViewItemContainerData.Item);
			if (virtualizingTreeViewItem != null)
			{
				virtualizingTreeViewItem.CanExpand = enumerable != null;
			}
			else
			{
				treeViewItemContainerData.CanExpand = enumerable != null;
			}
			if (!treeViewItemContainerData.CanExpand)
			{
				return;
			}
			foreach (object item2 in enumerable)
			{
				num++;
				TreeViewItemContainerData treeViewItemContainerData2 = (TreeViewItemContainerData)Insert(num, item2);
				VirtualizingTreeViewItem virtualizingTreeViewItem2 = (VirtualizingTreeViewItem)GetItemContainer(item2);
				if (virtualizingTreeViewItem2 != null)
				{
					virtualizingTreeViewItem2.Parent = treeViewItemContainerData;
				}
				else
				{
					treeViewItemContainerData2.Parent = treeViewItemContainerData;
				}
			}
			UpdateSelectedItemIndex();
		}

		public void Collapse(object item)
		{
			TreeViewItemContainerData treeViewItemContainerData = (TreeViewItemContainerData)GetItemContainerData(item);
			int num = IndexOf(treeViewItemContainerData.Item);
			List<object> list = new List<object>();
			Collapse(treeViewItemContainerData, num + 1, list);
			if (list.Count > 0)
			{
				bool unselect = false;
				base.DestroyItems(list.ToArray(), unselect);
			}
		}

		private void Collapse(object[] items)
		{
			List<object> list = new List<object>();
			for (int i = 0; i < items.Length; i++)
			{
				int num = IndexOf(items[i]);
				if (num >= 0)
				{
					TreeViewItemContainerData item = (TreeViewItemContainerData)GetItemContainerData(num);
					Collapse(item, num + 1, list);
				}
			}
			if (list.Count > 0)
			{
				bool unselect = false;
				base.DestroyItems(list.ToArray(), unselect);
			}
		}

		private void Collapse(TreeViewItemContainerData item, int itemIndex, List<object> itemsToDestroy)
		{
			while (true)
			{
				TreeViewItemContainerData treeViewItemContainerData = (TreeViewItemContainerData)GetItemContainerData(itemIndex);
				if (treeViewItemContainerData == null || !treeViewItemContainerData.IsDescendantOf(item))
				{
					break;
				}
				itemsToDestroy.Add(treeViewItemContainerData.Item);
				itemIndex++;
			}
		}

		public override void DataBindItem(object item, ItemContainerData containerData, VirtualizingItemContainer itemContainer)
		{
			itemContainer.Clear();
			TreeViewItemDataBindingArgs treeViewItemDataBindingArgs = new TreeViewItemDataBindingArgs();
			treeViewItemDataBindingArgs.Item = item;
			treeViewItemDataBindingArgs.ItemPresenter = ((!(itemContainer.ItemPresenter == null)) ? itemContainer.ItemPresenter : base.gameObject);
			treeViewItemDataBindingArgs.EditorPresenter = ((!(itemContainer.EditorPresenter == null)) ? itemContainer.EditorPresenter : base.gameObject);
			RaiseItemDataBinding(treeViewItemDataBindingArgs);
			VirtualizingTreeViewItem virtualizingTreeViewItem = (VirtualizingTreeViewItem)itemContainer;
			virtualizingTreeViewItem.CanExpand = treeViewItemDataBindingArgs.HasChildren;
			virtualizingTreeViewItem.CanEdit = treeViewItemDataBindingArgs.CanEdit;
			virtualizingTreeViewItem.CanDrag = treeViewItemDataBindingArgs.CanDrag;
			virtualizingTreeViewItem.UpdateIndent();
		}

		private void OnTreeViewItemParentChanged(object sender, VirtualizingParentChangedEventArgs e)
		{
			TreeViewItemContainerData treeViewItemContainerData = (TreeViewItemContainerData)sender;
			TreeViewItemContainerData oldParent = e.OldParent;
			if (base.DropMarker.Action != ItemDropAction.SetLastChild && base.DropMarker.Action != 0)
			{
				if (oldParent != null && !oldParent.HasChildren(this))
				{
					VirtualizingTreeViewItem virtualizingTreeViewItem = (VirtualizingTreeViewItem)GetItemContainer(oldParent.Item);
					if (virtualizingTreeViewItem != null)
					{
						virtualizingTreeViewItem.CanExpand = false;
					}
					else
					{
						oldParent.CanExpand = false;
					}
				}
				return;
			}
			TreeViewItemContainerData newParent = e.NewParent;
			VirtualizingTreeViewItem virtualizingTreeViewItem2 = null;
			if (newParent != null)
			{
				virtualizingTreeViewItem2 = (VirtualizingTreeViewItem)GetItemContainer(newParent.Item);
			}
			if (virtualizingTreeViewItem2 != null)
			{
				if (virtualizingTreeViewItem2.CanExpand)
				{
					virtualizingTreeViewItem2.IsExpanded = true;
				}
				else
				{
					virtualizingTreeViewItem2.CanExpand = true;
					try
					{
						m_expandSilently = true;
						virtualizingTreeViewItem2.IsExpanded = true;
					}
					finally
					{
						m_expandSilently = false;
					}
				}
			}
			else if (newParent != null)
			{
				newParent.CanExpand = true;
				newParent.IsExpanded = true;
			}
			TreeViewItemContainerData treeViewItemContainerData2 = treeViewItemContainerData.FirstChild(this);
			TreeViewItemContainerData treeViewItemContainerData3 = null;
			if (virtualizingTreeViewItem2 != null)
			{
				treeViewItemContainerData3 = newParent.LastChild(this);
				if (treeViewItemContainerData3 == null)
				{
					treeViewItemContainerData3 = newParent;
				}
			}
			else
			{
				treeViewItemContainerData3 = (TreeViewItemContainerData)LastItemContainerData();
			}
			if (treeViewItemContainerData3 != treeViewItemContainerData)
			{
				TreeViewItemContainerData treeViewItemContainerData4 = treeViewItemContainerData3.LastDescendant(this);
				if (treeViewItemContainerData4 != null)
				{
					treeViewItemContainerData3 = treeViewItemContainerData4;
				}
				if (!treeViewItemContainerData3.IsDescendantOf(treeViewItemContainerData))
				{
					base.SetNextSiblingInternal(treeViewItemContainerData3, treeViewItemContainerData);
				}
			}
			if (treeViewItemContainerData2 != null)
			{
				MoveSubtree(treeViewItemContainerData, treeViewItemContainerData2);
			}
			if (oldParent != null && !oldParent.HasChildren(this))
			{
				VirtualizingTreeViewItem virtualizingTreeViewItem3 = (VirtualizingTreeViewItem)GetItemContainer(oldParent.Item);
				if (virtualizingTreeViewItem3 != null)
				{
					virtualizingTreeViewItem3.CanExpand = false;
				}
				else
				{
					oldParent.CanExpand = false;
				}
			}
		}

		private void MoveSubtree(TreeViewItemContainerData parent, TreeViewItemContainerData child)
		{
			int num = IndexOf(parent.Item);
			int num2 = IndexOf(child.Item);
			bool flag = false;
			if (num < num2)
			{
				flag = true;
			}
			TreeViewItemContainerData treeViewItemContainerData = parent;
			VirtualizingTreeViewItem virtualizingTreeViewItem = (VirtualizingTreeViewItem)GetItemContainer(treeViewItemContainerData.Item);
			if (virtualizingTreeViewItem != null)
			{
				virtualizingTreeViewItem.UpdateIndent();
			}
			while (child != null && child.IsDescendantOf(parent) && treeViewItemContainerData != child)
			{
				base.SetNextSiblingInternal(treeViewItemContainerData, child);
				virtualizingTreeViewItem = (VirtualizingTreeViewItem)GetItemContainer(child.Item);
				if (virtualizingTreeViewItem != null)
				{
					virtualizingTreeViewItem.UpdateIndent();
				}
				treeViewItemContainerData = child;
				if (flag)
				{
					num2++;
				}
				child = (TreeViewItemContainerData)GetItemContainerData(num2);
			}
		}

		protected override bool CanDrop(ItemContainerData[] dragItems, ItemContainerData dropTarget)
		{
			if (base.CanDrop(dragItems, dropTarget))
			{
				TreeViewItemContainerData treeViewItemContainerData = (TreeViewItemContainerData)dropTarget;
				for (int i = 0; i < dragItems.Length; i++)
				{
					TreeViewItemContainerData treeViewItemContainerData2 = (TreeViewItemContainerData)dragItems[i];
					if (treeViewItemContainerData == treeViewItemContainerData2 || (treeViewItemContainerData != null && treeViewItemContainerData.IsDescendantOf(treeViewItemContainerData2)))
					{
						return false;
					}
				}
			}
			return true;
		}

		protected override void Drop(ItemContainerData[] dragItems, ItemContainerData dropTarget, ItemDropAction action)
		{
			TreeViewItemContainerData treeViewItemContainerData = (TreeViewItemContainerData)dropTarget;
			switch (action)
			{
			case ItemDropAction.SetLastChild:
			{
				for (int i = 0; i < dragItems.Length; i++)
				{
					TreeViewItemContainerData treeViewItemContainerData2 = (TreeViewItemContainerData)dragItems[i];
					if (treeViewItemContainerData == null || (treeViewItemContainerData != treeViewItemContainerData2 && !treeViewItemContainerData.IsDescendantOf(treeViewItemContainerData2)))
					{
						SetParent(treeViewItemContainerData, treeViewItemContainerData2);
						continue;
					}
					break;
				}
				break;
			}
			case ItemDropAction.SetPrevSibling:
			{
				for (int j = 0; j < dragItems.Length; j++)
				{
					SetPrevSibling(treeViewItemContainerData, dragItems[j]);
				}
				break;
			}
			case ItemDropAction.SetNextSibling:
			{
				for (int num = dragItems.Length - 1; num >= 0; num--)
				{
					SetNextSiblingInternal(treeViewItemContainerData, dragItems[num]);
				}
				break;
			}
			}
			UpdateSelectedItemIndex();
		}

		protected override void SetNextSiblingInternal(ItemContainerData sibling, ItemContainerData nextSibling)
		{
			TreeViewItemContainerData treeViewItemContainerData = (TreeViewItemContainerData)sibling;
			TreeViewItemContainerData treeViewItemContainerData2 = treeViewItemContainerData.LastDescendant(this);
			if (treeViewItemContainerData2 == null)
			{
				treeViewItemContainerData2 = treeViewItemContainerData;
			}
			TreeViewItemContainerData treeViewItemContainerData3 = (TreeViewItemContainerData)nextSibling;
			TreeViewItemContainerData treeViewItemContainerData4 = treeViewItemContainerData3.FirstChild(this);
			base.SetNextSiblingInternal(treeViewItemContainerData2, nextSibling);
			if (treeViewItemContainerData4 != null)
			{
				MoveSubtree(treeViewItemContainerData3, treeViewItemContainerData4);
			}
			SetParent(treeViewItemContainerData.Parent, treeViewItemContainerData3);
			VirtualizingTreeViewItem virtualizingTreeViewItem = (VirtualizingTreeViewItem)GetItemContainer(treeViewItemContainerData3.Item);
			if (virtualizingTreeViewItem != null)
			{
				virtualizingTreeViewItem.UpdateIndent();
			}
		}

		protected override void SetPrevSibling(ItemContainerData sibling, ItemContainerData prevSibling)
		{
			TreeViewItemContainerData treeViewItemContainerData = (TreeViewItemContainerData)sibling;
			TreeViewItemContainerData treeViewItemContainerData2 = (TreeViewItemContainerData)prevSibling;
			TreeViewItemContainerData treeViewItemContainerData3 = treeViewItemContainerData2.FirstChild(this);
			base.SetPrevSibling(sibling, prevSibling);
			if (treeViewItemContainerData3 != null)
			{
				MoveSubtree(treeViewItemContainerData2, treeViewItemContainerData3);
			}
			SetParent(treeViewItemContainerData.Parent, treeViewItemContainerData2);
			VirtualizingTreeViewItem virtualizingTreeViewItem = (VirtualizingTreeViewItem)GetItemContainer(treeViewItemContainerData2.Item);
			if (virtualizingTreeViewItem != null)
			{
				virtualizingTreeViewItem.UpdateIndent();
			}
		}

		private void SetParent(TreeViewItemContainerData parent, TreeViewItemContainerData child)
		{
			VirtualizingTreeViewItem virtualizingTreeViewItem = (VirtualizingTreeViewItem)GetItemContainer(child.Item);
			if (virtualizingTreeViewItem != null)
			{
				virtualizingTreeViewItem.Parent = parent;
			}
			else
			{
				child.Parent = parent;
			}
		}

		public void UpdateIndent(object obj)
		{
			VirtualizingTreeViewItem virtualizingTreeViewItem = (VirtualizingTreeViewItem)GetItemContainer(obj);
			if (!(virtualizingTreeViewItem == null))
			{
				virtualizingTreeViewItem.UpdateIndent();
			}
		}

		protected override void DestroyItems(object[] items, bool unselect)
		{
			TreeViewItemContainerData[] source = items.Select((object item) => GetItemContainerData(item)).OfType<TreeViewItemContainerData>().ToArray();
			TreeViewItemContainerData[] array = (from container in source
				where container.Parent != null
				select container.Parent).ToArray();
			Collapse(items);
			base.DestroyItems(items, unselect);
			TreeViewItemContainerData[] array2 = array;
			foreach (TreeViewItemContainerData treeViewItemContainerData in array2)
			{
				if (!treeViewItemContainerData.HasChildren(this))
				{
					VirtualizingTreeViewItem virtualizingTreeViewItem = (VirtualizingTreeViewItem)GetItemContainer(treeViewItemContainerData.Item);
					if (virtualizingTreeViewItem != null)
					{
						virtualizingTreeViewItem.CanExpand = false;
					}
				}
			}
		}
	}
}
