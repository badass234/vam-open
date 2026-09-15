using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.UIControls
{
	[RequireComponent(typeof(RectTransform), typeof(LayoutElement))]
	public class VirtualizingItemContainer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IDropHandler, IEndDragHandler, IEventSystemHandler
	{
		public bool CanDrag = true;

		public bool CanEdit = true;

		public bool CanDrop = true;

		public GameObject ItemPresenter;

		public GameObject EditorPresenter;

		private LayoutElement m_layoutElement;

		private RectTransform m_rectTransform;

		protected bool m_isSelected;

		private bool m_isEditing;

		private VirtualizingItemsControl m_itemsControl;

		private object m_item;

		private bool m_canBeginEdit;

		private IEnumerator m_coBeginEdit;

		public LayoutElement LayoutElement => m_layoutElement;

		public RectTransform RectTransform => m_rectTransform;

		public virtual bool IsSelected
		{
			get
			{
				return m_isSelected;
			}
			set
			{
				if (m_isSelected == value)
				{
					return;
				}
				m_isSelected = value;
				if (m_isSelected)
				{
					if (VirtualizingItemContainer.Selected != null)
					{
						VirtualizingItemContainer.Selected(this, EventArgs.Empty);
					}
				}
				else if (VirtualizingItemContainer.Unselected != null)
				{
					VirtualizingItemContainer.Unselected(this, EventArgs.Empty);
				}
			}
		}

		public bool IsEditing
		{
			get
			{
				return m_isEditing;
			}
			set
			{
				if (m_isEditing == value || !m_isSelected)
				{
					return;
				}
				m_isEditing = value && m_isSelected;
				if (EditorPresenter != ItemPresenter)
				{
					if (EditorPresenter != null)
					{
						EditorPresenter.SetActive(m_isEditing);
					}
					if (ItemPresenter != null)
					{
						ItemPresenter.SetActive(!m_isEditing);
					}
				}
				if (m_isEditing)
				{
					if (VirtualizingItemContainer.BeginEdit != null)
					{
						VirtualizingItemContainer.BeginEdit(this, EventArgs.Empty);
					}
				}
				else if (VirtualizingItemContainer.EndEdit != null)
				{
					VirtualizingItemContainer.EndEdit(this, EventArgs.Empty);
				}
			}
		}

		private VirtualizingItemsControl ItemsControl
		{
			get
			{
				if (m_itemsControl == null)
				{
					m_itemsControl = GetComponentInParent<VirtualizingItemsControl>();
				}
				return m_itemsControl;
			}
		}

		public virtual object Item
		{
			get
			{
				return m_item;
			}
			set
			{
				m_item = value;
			}
		}

		public static event EventHandler Selected;

		public static event EventHandler Unselected;

		public static event VirtualizingItemEventHandler PointerDown;

		public static event VirtualizingItemEventHandler PointerUp;

		public static event VirtualizingItemEventHandler DoubleClick;

		public static event VirtualizingItemEventHandler PointerEnter;

		public static event VirtualizingItemEventHandler PointerExit;

		public static event VirtualizingItemEventHandler BeginDrag;

		public static event VirtualizingItemEventHandler Drag;

		public static event VirtualizingItemEventHandler Drop;

		public static event VirtualizingItemEventHandler EndDrag;

		public static event EventHandler BeginEdit;

		public static event EventHandler EndEdit;

		private void Awake()
		{
			m_rectTransform = GetComponent<RectTransform>();
			m_layoutElement = GetComponent<LayoutElement>();
			if (ItemPresenter == null)
			{
				ItemPresenter = base.gameObject;
			}
			if (EditorPresenter == null)
			{
				EditorPresenter = base.gameObject;
			}
			AwakeOverride();
		}

		private void Start()
		{
			StartOverride();
			ItemsControl.UpdateContainerSize(this);
		}

		private void OnDestroy()
		{
			StopAllCoroutines();
			m_coBeginEdit = null;
			OnDestroyOverride();
		}

		protected virtual void AwakeOverride()
		{
		}

		protected virtual void StartOverride()
		{
		}

		protected virtual void OnDestroyOverride()
		{
		}

		public virtual void Clear()
		{
			m_isEditing = false;
			if (EditorPresenter != ItemPresenter)
			{
				if (EditorPresenter != null)
				{
					EditorPresenter.SetActive(m_isEditing);
				}
				if (ItemPresenter != null)
				{
					ItemPresenter.SetActive(!m_isEditing);
				}
			}
		}

		private IEnumerator CoBeginEdit()
		{
			yield return new WaitForSeconds(0.5f);
			m_coBeginEdit = null;
			IsEditing = CanEdit;
		}

		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
			m_canBeginEdit = m_isSelected && ItemsControl != null && ItemsControl.SelectedItemsCount == 1 && ItemsControl.CanEdit;
			if (VirtualizingItemContainer.PointerDown != null)
			{
				VirtualizingItemContainer.PointerDown(this, eventData);
			}
		}

		void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
		{
			if (eventData.clickCount == 2)
			{
				if (VirtualizingItemContainer.DoubleClick != null)
				{
					VirtualizingItemContainer.DoubleClick(this, eventData);
				}
				if (CanEdit && m_coBeginEdit != null)
				{
					StopCoroutine(m_coBeginEdit);
					m_coBeginEdit = null;
				}
			}
			else
			{
				if (m_canBeginEdit && m_coBeginEdit == null)
				{
					m_coBeginEdit = CoBeginEdit();
					StartCoroutine(m_coBeginEdit);
				}
				if (VirtualizingItemContainer.PointerUp != null)
				{
					VirtualizingItemContainer.PointerUp(this, eventData);
				}
			}
		}

		void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
		{
			if (!CanDrag)
			{
				ExecuteEvents.ExecuteHierarchy(base.transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
				return;
			}
			m_canBeginEdit = false;
			if (VirtualizingItemContainer.BeginDrag != null)
			{
				VirtualizingItemContainer.BeginDrag(this, eventData);
			}
		}

		void IDropHandler.OnDrop(PointerEventData eventData)
		{
			if (VirtualizingItemContainer.Drop != null)
			{
				VirtualizingItemContainer.Drop(this, eventData);
			}
		}

		void IDragHandler.OnDrag(PointerEventData eventData)
		{
			if (!CanDrag)
			{
				ExecuteEvents.ExecuteHierarchy(base.transform.parent.gameObject, eventData, ExecuteEvents.dragHandler);
			}
			else if (VirtualizingItemContainer.Drag != null)
			{
				VirtualizingItemContainer.Drag(this, eventData);
			}
		}

		void IEndDragHandler.OnEndDrag(PointerEventData eventData)
		{
			if (!CanDrag)
			{
				ExecuteEvents.ExecuteHierarchy(base.transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
			}
			else if (VirtualizingItemContainer.EndDrag != null)
			{
				VirtualizingItemContainer.EndDrag(this, eventData);
			}
		}

		void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
		{
			if (VirtualizingItemContainer.PointerEnter != null)
			{
				VirtualizingItemContainer.PointerEnter(this, eventData);
			}
		}

		void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
		{
			if (VirtualizingItemContainer.PointerExit != null)
			{
				VirtualizingItemContainer.PointerExit(this, eventData);
			}
		}
	}
}
