using UnityEngine;

namespace Battlehub.UIControls
{
	[RequireComponent(typeof(RectTransform))]
	public class VirtualizingTreeViewDropMarker : VirtualizingItemDropMarker
	{
		private VirtualizingTreeView m_treeView;

		private RectTransform m_siblingGraphicsRectTransform;

		public GameObject ChildGraphics;

		public override ItemDropAction Action
		{
			get
			{
				return base.Action;
			}
			set
			{
				base.Action = value;
				ChildGraphics.SetActive(base.Action == ItemDropAction.SetLastChild);
				SiblingGraphics.SetActive(base.Action != ItemDropAction.SetLastChild);
			}
		}

		protected override void AwakeOverride()
		{
			base.AwakeOverride();
			m_treeView = GetComponentInParent<VirtualizingTreeView>();
			m_siblingGraphicsRectTransform = SiblingGraphics.GetComponent<RectTransform>();
		}

		public override void SetTraget(VirtualizingItemContainer item)
		{
			base.SetTraget(item);
			if (!(item == null))
			{
				VirtualizingTreeViewItem virtualizingTreeViewItem = (VirtualizingTreeViewItem)item;
				if (virtualizingTreeViewItem != null)
				{
					m_siblingGraphicsRectTransform.offsetMin = new Vector2(virtualizingTreeViewItem.Indent, m_siblingGraphicsRectTransform.offsetMin.y);
				}
				else
				{
					m_siblingGraphicsRectTransform.offsetMin = new Vector2(0f, m_siblingGraphicsRectTransform.offsetMin.y);
				}
			}
		}

		public override void SetPosition(Vector2 position)
		{
			if (base.Item == null)
			{
				return;
			}
			if (!m_treeView.CanReparent)
			{
				base.SetPosition(position);
				return;
			}
			RectTransform rectTransform = base.Item.RectTransform;
			VirtualizingTreeViewItem virtualizingTreeViewItem = (VirtualizingTreeViewItem)base.Item;
			Vector2 sizeDelta = m_rectTransform.sizeDelta;
			sizeDelta.y = rectTransform.rect.height;
			m_rectTransform.sizeDelta = sizeDelta;
			Camera cam = null;
			if (base.ParentCanvas.renderMode == RenderMode.WorldSpace || base.ParentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
			{
				cam = m_treeView.Camera;
			}
			Vector2 localPoint;
			if (!m_treeView.CanReorder)
			{
				if (virtualizingTreeViewItem.CanDrop)
				{
					Action = ItemDropAction.SetLastChild;
					base.RectTransform.position = rectTransform.position;
				}
			}
			else if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, position, cam, out localPoint))
			{
				if (localPoint.y > (0f - rectTransform.rect.height) / 4f)
				{
					Action = ItemDropAction.SetPrevSibling;
					base.RectTransform.position = rectTransform.position;
				}
				else if (localPoint.y < rectTransform.rect.height / 4f - rectTransform.rect.height && !virtualizingTreeViewItem.HasChildren)
				{
					Action = ItemDropAction.SetNextSibling;
					base.RectTransform.position = rectTransform.position;
					base.RectTransform.localPosition = base.RectTransform.localPosition - new Vector3(0f, rectTransform.rect.height * base.ParentCanvas.scaleFactor, 0f);
				}
				else if (virtualizingTreeViewItem.CanDrop)
				{
					Action = ItemDropAction.SetLastChild;
					base.RectTransform.position = rectTransform.position;
				}
			}
		}
	}
}
