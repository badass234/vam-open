using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.UIControls
{
	public class VirtualizingItemsControl<TDataBindingArgs> : VirtualizingItemsControl where TDataBindingArgs : ItemDataBindingArgs, new()
	{
		public event EventHandler<TDataBindingArgs> ItemDataBinding;

		public event EventHandler<TDataBindingArgs> ItemBeginEdit;

		public event EventHandler<TDataBindingArgs> ItemEndEdit;

		protected override void OnItemBeginEdit(object sender, EventArgs e)
		{
			if (CanHandleEvent(sender))
			{
				VirtualizingItemContainer virtualizingItemContainer = (VirtualizingItemContainer)sender;
				if (this.ItemBeginEdit != null)
				{
					TDataBindingArgs e2 = new TDataBindingArgs
					{
						Item = virtualizingItemContainer.Item,
						ItemPresenter = ((!(virtualizingItemContainer.ItemPresenter == null)) ? virtualizingItemContainer.ItemPresenter : base.gameObject),
						EditorPresenter = ((!(virtualizingItemContainer.EditorPresenter == null)) ? virtualizingItemContainer.EditorPresenter : base.gameObject)
					};
					this.ItemBeginEdit(this, e2);
				}
			}
		}

		protected override void OnItemEndEdit(object sender, EventArgs e)
		{
			if (CanHandleEvent(sender))
			{
				VirtualizingItemContainer virtualizingItemContainer = (VirtualizingItemContainer)sender;
				if (this.ItemBeginEdit != null)
				{
					TDataBindingArgs e2 = new TDataBindingArgs
					{
						Item = virtualizingItemContainer.Item,
						ItemPresenter = ((!(virtualizingItemContainer.ItemPresenter == null)) ? virtualizingItemContainer.ItemPresenter : base.gameObject),
						EditorPresenter = ((!(virtualizingItemContainer.EditorPresenter == null)) ? virtualizingItemContainer.EditorPresenter : base.gameObject)
					};
					this.ItemEndEdit(this, e2);
				}
			}
		}

		public override void DataBindItem(object item, ItemContainerData itemContainerData, VirtualizingItemContainer itemContainer)
		{
			TDataBindingArgs args = new TDataBindingArgs
			{
				Item = item,
				ItemPresenter = ((!(itemContainer.ItemPresenter == null)) ? itemContainer.ItemPresenter : base.gameObject),
				EditorPresenter = ((!(itemContainer.EditorPresenter == null)) ? itemContainer.EditorPresenter : base.gameObject)
			};
			itemContainer.Clear();
			RaiseItemDataBinding(args);
			itemContainer.CanEdit = args.CanEdit;
			itemContainer.CanDrag = args.CanDrag;
			itemContainer.CanDrop = args.CanDrop;
		}

		protected void RaiseItemDataBinding(TDataBindingArgs args)
		{
			if (this.ItemDataBinding != null)
			{
				this.ItemDataBinding(this, args);
			}
		}
	}
	public class VirtualizingItemsControl : MonoBehaviour, IPointerDownHandler, IDropHandler, IEventSystemHandler
	{
		private enum ScrollDir
		{
			None,
			Up,
			Down,
			Left,
			Right
		}

		public KeyCode MultiselectKey = KeyCode.LeftControl;

		public KeyCode RangeselectKey = KeyCode.LeftShift;

		public KeyCode SelectAllKey = KeyCode.A;

		public KeyCode RemoveKey = KeyCode.Delete;

		public bool SelectOnPointerUp;

		public bool CanUnselectAll = true;

		public bool CanEdit = true;

		private bool m_prevCanDrag;

		public bool CanDrag = true;

		public bool CanReorder = true;

		public bool ExpandChildrenWidth = true;

		public bool ExpandChildrenHeight;

		private bool m_isDropInProgress;

		private Canvas m_canvas;

		public Camera Camera;

		public float ScrollSpeed = 100f;

		private ScrollDir m_scrollDir;

		private RectTransformChangeListener m_rtcListener;

		private float m_width;

		private float m_height;

		private VirtualizingItemDropMarker m_dropMarker;

		private bool m_externalDragOperation;

		private VirtualizingItemContainer m_dropTarget;

		private ItemContainerData[] m_dragItems;

		private bool m_selectionLocked;

		private List<object> m_selectedItems;

		private HashSet<object> m_selectedItemsHS;

		private int m_selectedIndex = -1;

		private Dictionary<object, ItemContainerData> m_itemContainerData;

		private VirtualizingScrollRect m_scrollRect;

		protected virtual bool CanScroll => CanReorder;

		protected bool IsDropInProgress => m_isDropInProgress;

		public object DropTarget
		{
			get
			{
				if (m_dropTarget == null)
				{
					return null;
				}
				return m_dropTarget.Item;
			}
		}

		public ItemDropAction DropAction
		{
			get
			{
				if (m_dropMarker == null)
				{
					return ItemDropAction.None;
				}
				return m_dropMarker.Action;
			}
		}

		protected VirtualizingItemDropMarker DropMarker => m_dropMarker;

		public int SelectedItemsCount
		{
			get
			{
				if (m_selectedItems == null)
				{
					return 0;
				}
				return m_selectedItems.Count;
			}
		}

		public virtual IEnumerable SelectedItems
		{
			get
			{
				return m_selectedItems;
			}
			set
			{
				if (m_selectionLocked)
				{
					return;
				}
				m_selectionLocked = true;
				IList selectedItems = m_selectedItems;
				if (value != null)
				{
					m_selectedItems = value.OfType<object>().ToList();
					m_selectedItemsHS = new HashSet<object>(m_selectedItems);
					for (int num = m_selectedItems.Count - 1; num >= 0; num--)
					{
						object obj = m_selectedItems[num];
						ItemContainerData value2;
						if (m_itemContainerData.TryGetValue(obj, out value2))
						{
							value2.IsSelected = true;
						}
						VirtualizingItemContainer itemContainer = GetItemContainer(obj);
						if (itemContainer != null)
						{
							itemContainer.IsSelected = true;
						}
					}
					if (m_selectedItems.Count == 0)
					{
						m_selectedIndex = -1;
					}
					else
					{
						m_selectedIndex = IndexOf(m_selectedItems[0]);
					}
				}
				else
				{
					m_selectedItems = null;
					m_selectedItemsHS = null;
					m_selectedIndex = -1;
				}
				List<object> list = new List<object>();
				if (selectedItems != null)
				{
					for (int i = 0; i < selectedItems.Count; i++)
					{
						object obj2 = selectedItems[i];
						if (m_selectedItemsHS == null || !m_selectedItemsHS.Contains(obj2))
						{
							ItemContainerData value3;
							if (m_itemContainerData.TryGetValue(obj2, out value3))
							{
								value3.IsSelected = false;
							}
							list.Add(obj2);
							VirtualizingItemContainer itemContainer2 = GetItemContainer(obj2);
							if (itemContainer2 != null)
							{
								itemContainer2.IsSelected = false;
							}
						}
					}
				}
				if (this.SelectionChanged != null)
				{
					object[] newItems = ((m_selectedItems != null) ? m_selectedItems.ToArray() : new object[0]);
					this.SelectionChanged(this, new SelectionChangedArgs(list.ToArray(), newItems));
				}
				m_selectionLocked = false;
			}
		}

		public object SelectedItem
		{
			get
			{
				if (m_selectedItems == null || m_selectedItems.Count == 0)
				{
					return null;
				}
				return m_selectedItems[0];
			}
			set
			{
				SelectedIndex = IndexOf(value);
			}
		}

		public int SelectedIndex
		{
			get
			{
				if (SelectedItem == null)
				{
					return -1;
				}
				return m_selectedIndex;
			}
			set
			{
				if (m_selectedIndex == value || m_selectionLocked)
				{
					return;
				}
				m_selectionLocked = true;
				ItemContainerData value2;
				if (SelectedItem != null && m_itemContainerData.TryGetValue(SelectedItem, out value2))
				{
					value2.IsSelected = false;
				}
				VirtualizingItemContainer itemContainer = GetItemContainer(SelectedItem);
				if (itemContainer != null)
				{
					itemContainer.IsSelected = false;
				}
				m_selectedIndex = value;
				object obj = null;
				if (m_selectedIndex >= 0 && m_selectedIndex < m_scrollRect.ItemsCount)
				{
					obj = m_scrollRect.Items[m_selectedIndex];
					ItemContainerData value3;
					if (obj != null && m_itemContainerData.TryGetValue(obj, out value3))
					{
						value3.IsSelected = true;
					}
					VirtualizingItemContainer itemContainer2 = GetItemContainer(obj);
					if (itemContainer2 != null)
					{
						itemContainer2.IsSelected = true;
					}
				}
				object[] array = ((obj == null) ? new object[0] : new object[1] { obj });
				object[] array2 = ((m_selectedItems != null) ? m_selectedItems.Except(array).ToArray() : new object[0]);
				foreach (object obj2 in array2)
				{
					ItemContainerData value4;
					if (obj2 != null && m_itemContainerData.TryGetValue(obj2, out value4))
					{
						value4.IsSelected = false;
					}
					VirtualizingItemContainer itemContainer3 = GetItemContainer(obj2);
					if (itemContainer3 != null)
					{
						itemContainer3.IsSelected = false;
					}
				}
				m_selectedItems = array.ToList();
				m_selectedItemsHS = new HashSet<object>(m_selectedItems);
				if (this.SelectionChanged != null)
				{
					this.SelectionChanged(this, new SelectionChangedArgs(array2, array));
				}
				m_selectionLocked = false;
			}
		}

		public IEnumerable Items
		{
			get
			{
				return m_scrollRect.Items;
			}
			set
			{
				if (value == null)
				{
					m_scrollRect.Items = null;
					m_scrollRect.verticalNormalizedPosition = 1f;
					m_scrollRect.horizontalNormalizedPosition = 0f;
					m_itemContainerData = new Dictionary<object, ItemContainerData>();
					return;
				}
				List<object> list = value.OfType<object>().ToList();
				m_itemContainerData = new Dictionary<object, ItemContainerData>();
				for (int i = 0; i < list.Count; i++)
				{
					m_itemContainerData.Add(list[i], InstantiateItemContainerData(list[i]));
				}
				m_scrollRect.Items = list;
			}
		}

		public event EventHandler<ItemArgs> ItemBeginDrag;

		public event EventHandler<ItemDropCancelArgs> ItemBeginDrop;

		public event EventHandler<ItemDropArgs> ItemDrop;

		public event EventHandler<ItemArgs> ItemEndDrag;

		public event EventHandler<SelectionChangedArgs> SelectionChanged;

		public event EventHandler<ItemArgs> ItemDoubleClick;

		public event EventHandler<ItemsCancelArgs> ItemsRemoving;

		public event EventHandler<ItemsRemovedArgs> ItemsRemoved;

		public bool IsItemSelected(object obj)
		{
			if (m_selectedItemsHS == null)
			{
				return false;
			}
			return m_selectedItemsHS.Contains(obj);
		}

		protected virtual ItemContainerData InstantiateItemContainerData(object item)
		{
			ItemContainerData itemContainerData = new ItemContainerData();
			itemContainerData.Item = item;
			return itemContainerData;
		}

		private void Awake()
		{
			m_scrollRect = GetComponent<VirtualizingScrollRect>();
			if (m_scrollRect == null)
			{
				Debug.LogError("Scroll Rect is required");
			}
			m_scrollRect.ItemDataBinding += OnScrollRectItemDataBinding;
			m_dropMarker = GetComponentInChildren<VirtualizingItemDropMarker>(true);
			m_rtcListener = GetComponentInChildren<RectTransformChangeListener>();
			if (m_rtcListener != null)
			{
				m_rtcListener.RectTransformChanged += OnViewportRectTransformChanged;
			}
			if (Camera == null)
			{
				Camera = Camera.main;
			}
			m_prevCanDrag = CanDrag;
			OnCanDragChanged();
			AwakeOverride();
		}

		private void Start()
		{
			m_canvas = GetComponentInParent<Canvas>();
			StartOverride();
		}

		private void Update()
		{
			if (m_scrollDir != 0)
			{
				float num = m_scrollRect.content.rect.height - m_scrollRect.viewport.rect.height;
				float num2 = 0f;
				if (num > 0f)
				{
					num2 = ScrollSpeed / 10f * (1f / num);
				}
				float num3 = m_scrollRect.content.rect.width - m_scrollRect.viewport.rect.width;
				float num4 = 0f;
				if (num3 > 0f)
				{
					num4 = ScrollSpeed / 10f * (1f / num3);
				}
				if (m_scrollDir == ScrollDir.Up)
				{
					m_scrollRect.verticalNormalizedPosition += num2;
					if (m_scrollRect.verticalNormalizedPosition > 1f)
					{
						m_scrollRect.verticalNormalizedPosition = 1f;
						m_scrollDir = ScrollDir.None;
					}
				}
				else if (m_scrollDir == ScrollDir.Down)
				{
					m_scrollRect.verticalNormalizedPosition -= num2;
					if (m_scrollRect.verticalNormalizedPosition < 0f)
					{
						m_scrollRect.verticalNormalizedPosition = 0f;
						m_scrollDir = ScrollDir.None;
					}
				}
				else if (m_scrollDir == ScrollDir.Left)
				{
					m_scrollRect.horizontalNormalizedPosition -= num4;
					if (m_scrollRect.horizontalNormalizedPosition < 0f)
					{
						m_scrollRect.horizontalNormalizedPosition = 0f;
						m_scrollDir = ScrollDir.None;
					}
				}
				if (m_scrollDir == ScrollDir.Right)
				{
					m_scrollRect.horizontalNormalizedPosition += num4;
					if (m_scrollRect.horizontalNormalizedPosition > 1f)
					{
						m_scrollRect.horizontalNormalizedPosition = 1f;
						m_scrollDir = ScrollDir.None;
					}
				}
			}
			if (Input.GetKeyDown(RemoveKey))
			{
				RemoveSelectedItems();
			}
			if (Input.GetKeyDown(SelectAllKey) && Input.GetKey(RangeselectKey))
			{
				SelectedItems = m_scrollRect.Items;
			}
			if (m_prevCanDrag != CanDrag)
			{
				OnCanDragChanged();
				m_prevCanDrag = CanDrag;
			}
			UpdateOverride();
		}

		private void OnEnable()
		{
			VirtualizingItemContainer.Selected += OnItemSelected;
			VirtualizingItemContainer.Unselected += OnItemUnselected;
			VirtualizingItemContainer.PointerUp += OnItemPointerUp;
			VirtualizingItemContainer.PointerDown += OnItemPointerDown;
			VirtualizingItemContainer.PointerEnter += OnItemPointerEnter;
			VirtualizingItemContainer.PointerExit += OnItemPointerExit;
			VirtualizingItemContainer.DoubleClick += OnItemDoubleClick;
			VirtualizingItemContainer.BeginEdit += OnItemBeginEdit;
			VirtualizingItemContainer.EndEdit += OnItemEndEdit;
			VirtualizingItemContainer.BeginDrag += OnItemBeginDrag;
			VirtualizingItemContainer.Drag += OnItemDrag;
			VirtualizingItemContainer.Drop += OnItemDrop;
			VirtualizingItemContainer.EndDrag += OnItemEndDrag;
			OnEnableOverride();
		}

		private void OnDisable()
		{
			VirtualizingItemContainer.Selected -= OnItemSelected;
			VirtualizingItemContainer.Unselected -= OnItemUnselected;
			VirtualizingItemContainer.PointerUp -= OnItemPointerUp;
			VirtualizingItemContainer.PointerDown -= OnItemPointerDown;
			VirtualizingItemContainer.PointerEnter -= OnItemPointerEnter;
			VirtualizingItemContainer.PointerExit -= OnItemPointerExit;
			VirtualizingItemContainer.DoubleClick -= OnItemDoubleClick;
			VirtualizingItemContainer.BeginEdit -= OnItemBeginEdit;
			VirtualizingItemContainer.EndEdit -= OnItemEndEdit;
			VirtualizingItemContainer.BeginDrag -= OnItemBeginDrag;
			VirtualizingItemContainer.Drag -= OnItemDrag;
			VirtualizingItemContainer.Drop -= OnItemDrop;
			VirtualizingItemContainer.EndDrag -= OnItemEndDrag;
			OnDisableOverride();
		}

		private void OnDestroy()
		{
			if (m_scrollRect != null)
			{
				m_scrollRect.ItemDataBinding -= OnScrollRectItemDataBinding;
			}
			if (m_rtcListener != null)
			{
				m_rtcListener.RectTransformChanged -= OnViewportRectTransformChanged;
			}
			OnDestroyOverride();
		}

		protected virtual void AwakeOverride()
		{
		}

		protected virtual void StartOverride()
		{
		}

		protected virtual void UpdateOverride()
		{
		}

		protected virtual void OnEnableOverride()
		{
		}

		protected virtual void OnDisableOverride()
		{
		}

		protected virtual void OnDestroyOverride()
		{
		}

		private void OnItemSelected(object sender, EventArgs e)
		{
			if (!m_selectionLocked && CanHandleEvent(sender))
			{
				VirtualizingItemContainer.Unselected -= OnItemUnselected;
				if (Input.GetKey(MultiselectKey))
				{
					IList list = ((m_selectedItems == null) ? new List<object>() : m_selectedItems.ToList());
					list.Add(((VirtualizingItemContainer)sender).Item);
					SelectedItems = list;
				}
				else if (Input.GetKey(RangeselectKey))
				{
					SelectRange((VirtualizingItemContainer)sender);
				}
				else
				{
					SelectedIndex = IndexOf(((VirtualizingItemContainer)sender).Item);
				}
				VirtualizingItemContainer.Unselected += OnItemUnselected;
			}
		}

		private void SelectRange(VirtualizingItemContainer itemContainer)
		{
			if (m_selectedItems != null && m_selectedItems.Count > 0)
			{
				List<object> list = new List<object>();
				int num = IndexOf(m_selectedItems[0]);
				object item = itemContainer.Item;
				int num2 = IndexOf(item);
				int num3 = Mathf.Min(num, num2);
				int num4 = Math.Max(num, num2);
				list.Add(m_selectedItems[0]);
				for (int i = num3; i < num; i++)
				{
					list.Add(m_scrollRect.Items[i]);
				}
				for (int j = num + 1; j <= num4; j++)
				{
					list.Add(m_scrollRect.Items[j]);
				}
				SelectedItems = list;
			}
			else
			{
				SelectedIndex = IndexOf(itemContainer.Item);
			}
		}

		private void OnItemUnselected(object sender, EventArgs e)
		{
			if (!m_selectionLocked && CanHandleEvent(sender))
			{
				IList list = ((m_selectedItems == null) ? new List<object>() : m_selectedItems.ToList());
				list.Remove(((VirtualizingItemContainer)sender).Item);
				SelectedItems = list;
			}
		}

		private void OnItemPointerDown(VirtualizingItemContainer sender, PointerEventData eventData)
		{
			if (!CanHandleEvent(sender) || m_externalDragOperation)
			{
				return;
			}
			m_dropMarker.SetTraget(null);
			m_dragItems = null;
			m_isDropInProgress = false;
			if (!SelectOnPointerUp)
			{
				if (Input.GetKey(RangeselectKey))
				{
					SelectRange(sender);
				}
				else if (Input.GetKey(MultiselectKey))
				{
					sender.IsSelected = !sender.IsSelected;
				}
				else
				{
					sender.IsSelected = true;
				}
			}
		}

		private void OnItemPointerUp(VirtualizingItemContainer sender, PointerEventData eventData)
		{
			if (!CanHandleEvent(sender) || m_externalDragOperation || m_dragItems != null)
			{
				return;
			}
			if (SelectOnPointerUp)
			{
				if (!m_isDropInProgress)
				{
					if (Input.GetKey(RangeselectKey))
					{
						SelectRange(sender);
					}
					else if (Input.GetKey(MultiselectKey))
					{
						sender.IsSelected = !sender.IsSelected;
					}
					else
					{
						sender.IsSelected = true;
					}
				}
			}
			else if (!Input.GetKey(MultiselectKey) && !Input.GetKey(RangeselectKey) && m_selectedItems != null && m_selectedItems.Count > 1)
			{
				if (SelectedItem == sender.Item)
				{
					SelectedItem = null;
				}
				SelectedItem = sender.Item;
			}
		}

		private void OnItemPointerEnter(VirtualizingItemContainer sender, PointerEventData eventData)
		{
			if (CanHandleEvent(sender))
			{
				m_dropTarget = sender;
				if ((m_dragItems != null || m_externalDragOperation) && m_scrollDir == ScrollDir.None)
				{
					m_dropMarker.SetTraget(m_dropTarget);
				}
			}
		}

		private void OnItemPointerExit(VirtualizingItemContainer sender, PointerEventData eventData)
		{
			if (CanHandleEvent(sender))
			{
				m_dropTarget = null;
				if (m_dragItems != null || m_externalDragOperation)
				{
					m_dropMarker.SetTraget(null);
				}
			}
		}

		private void OnItemDoubleClick(VirtualizingItemContainer sender, PointerEventData eventData)
		{
			if (CanHandleEvent(sender) && this.ItemDoubleClick != null)
			{
				this.ItemDoubleClick(this, new ItemArgs(new object[1] { sender.Item }));
			}
		}

		private void OnItemBeginDrag(VirtualizingItemContainer sender, PointerEventData eventData)
		{
			if (!CanHandleEvent(sender))
			{
				return;
			}
			if (m_dropTarget != null)
			{
				m_dropMarker.SetTraget(m_dropTarget);
				m_dropMarker.SetPosition(eventData.position);
			}
			if (m_selectedItems != null && m_selectedItems.Contains(sender.Item))
			{
				m_dragItems = GetDragItems();
			}
			else
			{
				m_dragItems = new ItemContainerData[1] { m_itemContainerData[sender.Item] };
			}
			if (this.ItemBeginDrag != null)
			{
				this.ItemBeginDrag(this, new ItemArgs(m_dragItems.Select((ItemContainerData di) => di.Item).ToArray()));
			}
		}

		private void OnItemDrag(VirtualizingItemContainer sender, PointerEventData eventData)
		{
			if (!CanHandleEvent(sender))
			{
				return;
			}
			ExternalItemDrag(eventData.position);
			float height = m_scrollRect.viewport.rect.height;
			float width = m_scrollRect.viewport.rect.width;
			Camera cam = null;
			if (m_canvas.renderMode == RenderMode.WorldSpace || m_canvas.renderMode == RenderMode.ScreenSpaceCamera)
			{
				cam = Camera;
			}
			if (CanScroll)
			{
				Vector2 localPoint;
				if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_scrollRect.viewport, eventData.position, cam, out localPoint))
				{
					if (localPoint.y >= 0f)
					{
						m_scrollDir = ScrollDir.Up;
						m_dropMarker.SetTraget(null);
					}
					else if (localPoint.y < 0f - height)
					{
						m_scrollDir = ScrollDir.Down;
						m_dropMarker.SetTraget(null);
					}
					else if (localPoint.x <= 0f)
					{
						m_scrollDir = ScrollDir.Left;
					}
					else if (localPoint.x >= width)
					{
						m_scrollDir = ScrollDir.Right;
					}
					else
					{
						m_scrollDir = ScrollDir.None;
					}
				}
			}
			else
			{
				m_scrollDir = ScrollDir.None;
			}
		}

		private void OnItemDrop(VirtualizingItemContainer sender, PointerEventData eventData)
		{
			if (!CanHandleEvent(sender) || m_dragItems == null)
			{
				return;
			}
			m_isDropInProgress = true;
			try
			{
				if (CanDrop(m_dragItems, GetItemContainerData(m_dropTarget.Item)))
				{
					bool flag = false;
					if (this.ItemBeginDrop != null)
					{
						ItemDropCancelArgs itemDropCancelArgs = new ItemDropCancelArgs(m_dragItems.Select((ItemContainerData di) => di.Item).ToArray(), m_dropTarget.Item, m_dropMarker.Action, false);
						if (this.ItemBeginDrop != null)
						{
							this.ItemBeginDrop(this, itemDropCancelArgs);
							flag = itemDropCancelArgs.Cancel;
						}
					}
					if (!flag)
					{
						object[] array = ((m_dragItems == null) ? null : m_dragItems.Select((ItemContainerData di) => di.Item).ToArray());
						object obj = ((!(m_dropTarget != null)) ? null : m_dropTarget.Item);
						ItemContainerData itemContainerData = GetItemContainerData(obj);
						Drop(m_dragItems, itemContainerData, m_dropMarker.Action);
						if (this.ItemDrop != null && array != null && obj != null && m_dropMarker != null)
						{
							this.ItemDrop(this, new ItemDropArgs(array, obj, m_dropMarker.Action, false));
						}
					}
				}
				RaiseEndDrag();
			}
			finally
			{
				m_isDropInProgress = false;
			}
		}

		private void OnItemEndDrag(VirtualizingItemContainer sender, PointerEventData eventData)
		{
			if (m_dropTarget != null)
			{
				OnItemDrop(sender, eventData);
			}
			else if (CanHandleEvent(sender))
			{
				RaiseEndDrag();
			}
		}

		private void RaiseEndDrag()
		{
			if (m_dragItems == null)
			{
				return;
			}
			if (this.ItemEndDrag != null)
			{
				this.ItemEndDrag(this, new ItemArgs(m_dragItems.Select((ItemContainerData di) => di.Item).ToArray()));
			}
			m_dropMarker.SetTraget(null);
			m_dragItems = null;
			m_scrollDir = ScrollDir.None;
		}

		private void OnViewportRectTransformChanged()
		{
			if (ExpandChildrenHeight || ExpandChildrenWidth)
			{
				Rect rect = m_scrollRect.viewport.rect;
				if (rect.width != m_width || rect.height != m_height)
				{
					m_width = rect.width;
					m_height = rect.height;
					SetContainersSize();
				}
			}
		}

		private void SetContainersSize()
		{
			m_scrollRect.ForEachContainer(delegate(RectTransform c)
			{
				VirtualizingItemContainer component = c.GetComponent<VirtualizingItemContainer>();
				UpdateContainerSize(component);
			});
		}

		public void UpdateContainerSize(VirtualizingItemContainer container)
		{
			if (container != null)
			{
				if (ExpandChildrenWidth)
				{
					container.LayoutElement.minWidth = m_width;
				}
				if (ExpandChildrenHeight)
				{
					container.LayoutElement.minHeight = m_height;
				}
			}
		}

		private void OnCanDragChanged()
		{
			m_scrollRect.ForEachContainer(delegate(RectTransform c)
			{
				VirtualizingItemContainer component = c.GetComponent<VirtualizingItemContainer>();
				if (component != null)
				{
					component.CanDrag = CanDrag;
				}
			});
		}

		protected bool CanHandleEvent(object sender)
		{
			VirtualizingItemContainer virtualizingItemContainer = sender as VirtualizingItemContainer;
			if (!virtualizingItemContainer)
			{
				return false;
			}
			return m_scrollRect.IsParentOf(virtualizingItemContainer.transform);
		}

		void IDropHandler.OnDrop(PointerEventData eventData)
		{
			if (!CanReorder)
			{
				return;
			}
			if (m_dragItems == null)
			{
				GameObject pointerDrag = eventData.pointerDrag;
				if (!(pointerDrag != null))
				{
					return;
				}
				ItemContainer component = pointerDrag.GetComponent<ItemContainer>();
				if (component != null && component.Item != null)
				{
					object item = component.Item;
					if (this.ItemDrop != null)
					{
						this.ItemDrop(this, new ItemDropArgs(new object[1] { item }, null, ItemDropAction.SetLastChild, true));
					}
				}
				return;
			}
			if (m_scrollRect.ItemsCount > 0)
			{
				RectTransform rectTransform = m_scrollRect.LastContainer();
				if (rectTransform != null)
				{
					m_dropTarget = rectTransform.GetComponent<VirtualizingItemContainer>();
					m_dropMarker.Action = ItemDropAction.SetNextSibling;
				}
			}
			m_isDropInProgress = true;
			try
			{
				ItemContainerData itemContainerData = GetItemContainerData(m_dropTarget.Item);
				if (CanDrop(m_dragItems, itemContainerData))
				{
					Drop(m_dragItems, itemContainerData, m_dropMarker.Action);
					if (this.ItemDrop != null)
					{
						this.ItemDrop(this, new ItemDropArgs(m_dragItems.Select((ItemContainerData di) => di.Item).ToArray(), m_dropTarget.Item, m_dropMarker.Action, false));
					}
				}
				m_dropMarker.SetTraget(null);
				m_dragItems = null;
			}
			finally
			{
				m_isDropInProgress = false;
			}
		}

		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
			if (CanUnselectAll)
			{
				SelectedIndex = -1;
			}
		}

		protected virtual void OnItemBeginEdit(object sender, EventArgs e)
		{
		}

		protected virtual void OnItemEndEdit(object sender, EventArgs e)
		{
		}

		public virtual void DataBindItem(object item, ItemContainerData itemContainerData, VirtualizingItemContainer itemContainer)
		{
		}

		private void OnScrollRectItemDataBinding(RectTransform container, object item)
		{
			VirtualizingItemContainer component = container.GetComponent<VirtualizingItemContainer>();
			component.Item = item;
			m_selectionLocked = true;
			ItemContainerData itemContainerData = m_itemContainerData[item];
			itemContainerData.IsSelected = IsItemSelected(item);
			component.IsSelected = itemContainerData.IsSelected;
			component.CanDrag = CanDrag;
			m_selectionLocked = false;
			DataBindItem(item, itemContainerData, component);
			if (m_scrollRect.ItemsCount == 1)
			{
				SetContainersSize();
			}
		}

		public int IndexOf(object obj)
		{
			if (m_scrollRect.Items == null)
			{
				return -1;
			}
			if (obj == null)
			{
				return -1;
			}
			return m_scrollRect.Items.IndexOf(obj);
		}

		public virtual void SetIndex(object obj, int newIndex)
		{
			int num = IndexOf(obj);
			if (num != -1)
			{
				if (num == m_selectedIndex)
				{
					m_selectedIndex = newIndex;
				}
				if (num < newIndex)
				{
					m_scrollRect.SetNextSibling(GetItemAt(newIndex), obj);
				}
				else
				{
					m_scrollRect.SetPrevSibling(GetItemAt(newIndex), obj);
				}
			}
		}

		public ItemContainerData LastItemContainerData()
		{
			if (m_scrollRect.Items == null || m_scrollRect.ItemsCount == 0)
			{
				return null;
			}
			return GetItemContainerData(m_scrollRect.Items[m_scrollRect.ItemsCount - 1]);
		}

		public VirtualizingItemContainer GetItemContainer(object item)
		{
			if (item == null)
			{
				return null;
			}
			RectTransform container = m_scrollRect.GetContainer(item);
			if (container == null)
			{
				return null;
			}
			return container.GetComponent<VirtualizingItemContainer>();
		}

		public ItemContainerData GetItemContainerData(object item)
		{
			if (item == null)
			{
				return null;
			}
			ItemContainerData value = null;
			m_itemContainerData.TryGetValue(item, out value);
			return value;
		}

		public ItemContainerData GetItemContainerData(int siblingIndex)
		{
			if (siblingIndex < 0 || m_scrollRect.Items.Count <= siblingIndex)
			{
				return null;
			}
			object key = m_scrollRect.Items[siblingIndex];
			return m_itemContainerData[key];
		}

		protected virtual bool CanDrop(ItemContainerData[] dragItems, ItemContainerData dropTarget)
		{
			if (dropTarget == null)
			{
				return true;
			}
			if (dragItems == null)
			{
				return false;
			}
			if (dragItems.Contains(dropTarget.Item))
			{
				return false;
			}
			return true;
		}

		protected ItemContainerData[] GetDragItems()
		{
			ItemContainerData[] array = new ItemContainerData[m_selectedItems.Count];
			if (m_selectedItems != null)
			{
				for (int i = 0; i < m_selectedItems.Count; i++)
				{
					array[i] = m_itemContainerData[m_selectedItems[i]];
				}
			}
			return array.OrderBy((ItemContainerData di) => IndexOf(di.Item)).ToArray();
		}

		protected virtual void Drop(ItemContainerData[] dragItems, ItemContainerData dropTargetData, ItemDropAction action)
		{
			switch (action)
			{
			case ItemDropAction.SetPrevSibling:
				foreach (ItemContainerData prevSibling in dragItems)
				{
					SetPrevSibling(dropTargetData, prevSibling);
				}
				break;
			case ItemDropAction.SetNextSibling:
				foreach (ItemContainerData nextSibling in dragItems)
				{
					SetNextSiblingInternal(dropTargetData, nextSibling);
				}
				break;
			}
			UpdateSelectedItemIndex();
		}

		protected virtual void SetNextSiblingInternal(ItemContainerData sibling, ItemContainerData nextSibling)
		{
			m_scrollRect.SetNextSibling(sibling.Item, nextSibling.Item);
		}

		protected virtual void SetPrevSibling(ItemContainerData sibling, ItemContainerData prevSibling)
		{
			m_scrollRect.SetPrevSibling(sibling.Item, prevSibling.Item);
		}

		protected void UpdateSelectedItemIndex()
		{
			m_selectedIndex = IndexOf(SelectedItem);
		}

		public void ExternalBeginDrag(Vector3 position)
		{
			if (CanDrag)
			{
				m_externalDragOperation = true;
				if (!(m_dropTarget == null) && (m_dragItems != null || m_externalDragOperation) && m_scrollDir == ScrollDir.None)
				{
					m_dropMarker.SetTraget(m_dropTarget);
				}
			}
		}

		public void ExternalItemDrag(Vector3 position)
		{
			if (CanDrag && m_dropTarget != null)
			{
				m_dropMarker.SetPosition(position);
			}
		}

		public void ExternalItemDrop()
		{
			if (CanDrag)
			{
				m_externalDragOperation = false;
				m_dropMarker.SetTraget(null);
			}
		}

		public void RemoveSelectedItems()
		{
			if (m_selectedItems == null)
			{
				return;
			}
			object[] array;
			if (this.ItemsRemoving != null)
			{
				ItemsCancelArgs itemsCancelArgs = new ItemsCancelArgs(m_selectedItems.ToList());
				this.ItemsRemoving(this, itemsCancelArgs);
				array = ((itemsCancelArgs.Items != null) ? itemsCancelArgs.Items.ToArray() : new object[0]);
			}
			else
			{
				array = m_selectedItems.ToArray();
			}
			if (array.Length != 0)
			{
				DestroyItems(array, true);
				if (this.ItemsRemoved != null)
				{
					this.ItemsRemoved(this, new ItemsRemovedArgs(array));
				}
			}
		}

		protected virtual void DestroyItems(object[] items, bool unselect)
		{
			if (unselect)
			{
				foreach (object item2 in items)
				{
					if (m_selectedItems != null && m_selectedItems.Contains(item2))
					{
						m_selectedItems.Remove(item2);
						m_selectedItemsHS.Remove(item2);
						if (m_selectedItems.Count == 0)
						{
							m_selectedIndex = -1;
						}
						else
						{
							m_selectedIndex = IndexOf(m_selectedItems[0]);
						}
					}
				}
			}
			m_scrollRect.RemoveItems(items.Select((object item) => IndexOf(item)).ToArray());
			foreach (object key in items)
			{
				m_itemContainerData.Remove(key);
			}
		}

		public ItemContainerData Add(object item)
		{
			return Insert(m_scrollRect.ItemsCount, item);
		}

		public virtual ItemContainerData Insert(int index, object item)
		{
			ItemContainerData itemContainerData = InstantiateItemContainerData(item);
			m_itemContainerData.Add(item, itemContainerData);
			m_scrollRect.InsertItem(index, item);
			return itemContainerData;
		}

		public void SetNextSibling(object sibling, object nextSibling)
		{
			ItemContainerData itemContainerData = GetItemContainerData(sibling);
			if (itemContainerData != null)
			{
				ItemContainerData itemContainerData2 = GetItemContainerData(nextSibling);
				if (itemContainerData2 != null)
				{
					Drop(new ItemContainerData[1] { itemContainerData2 }, itemContainerData, ItemDropAction.SetNextSibling);
				}
			}
		}

		public void SetPrevSibling(object sibling, object prevSibling)
		{
			ItemContainerData itemContainerData = GetItemContainerData(sibling);
			if (itemContainerData != null)
			{
				ItemContainerData itemContainerData2 = GetItemContainerData(prevSibling);
				if (itemContainerData2 != null)
				{
					Drop(new ItemContainerData[1] { itemContainerData2 }, itemContainerData, ItemDropAction.SetPrevSibling);
				}
			}
		}

		public virtual void Remove(object item)
		{
			DestroyItems(new object[1] { item }, true);
		}

		public object GetItemAt(int index)
		{
			if (index < 0 || index >= m_scrollRect.Items.Count)
			{
				return null;
			}
			return m_scrollRect.Items[index];
		}
	}
}
