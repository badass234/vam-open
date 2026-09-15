using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls
{
	public class VirtualizingScrollRect : ScrollRect
	{
		public RectTransform ContainerPrefab;

		[SerializeField]
		private RectTransform m_virtualContent;

		private RectTransformChangeListener m_virtualContentTransformChangeListener;

		[SerializeField]
		private VirtualizingMode m_mode = VirtualizingMode.Vertical;

		private LinkedList<RectTransform> m_containers = new LinkedList<RectTransform>();

		private IList m_items;

		private float m_normalizedIndex;

		public IList Items
		{
			get
			{
				return m_items;
			}
			set
			{
				if (m_items != value)
				{
					m_items = value;
					DataBind(Index);
					UpdateContentSize();
				}
			}
		}

		public int ItemsCount
		{
			get
			{
				if (Items == null)
				{
					return 0;
				}
				return Items.Count;
			}
		}

		private float NormalizedIndex
		{
			get
			{
				return m_normalizedIndex;
			}
			set
			{
				if (value != m_normalizedIndex)
				{
					OnNormalizedIndexChanged(value);
				}
			}
		}

		private int Index
		{
			get
			{
				if (ItemsCount == 0)
				{
					return 0;
				}
				return Mathf.RoundToInt(NormalizedIndex * (float)Mathf.Max(ItemsCount - VisibleItemsCount, 0));
			}
			set
			{
				if (value >= 0 && value < ItemsCount)
				{
					NormalizedIndex = EvalNormalizedIndex(value);
				}
			}
		}

		private int VisibleItemsCount => Mathf.Min(ItemsCount, PossibleItemsCount);

		private int PossibleItemsCount
		{
			get
			{
				if (ContainerSize < 1E-05f)
				{
					Debug.LogWarning("ContainerSize is too small");
					return 0;
				}
				return Mathf.FloorToInt(Size / ContainerSize);
			}
		}

		private float ContainerSize
		{
			get
			{
				if (m_mode == VirtualizingMode.Horizontal)
				{
					return Mathf.Max(0f, ContainerPrefab.rect.width);
				}
				if (m_mode == VirtualizingMode.Vertical)
				{
					return Mathf.Max(0f, ContainerPrefab.rect.height);
				}
				throw new InvalidOperationException("Unable to eval container size in non-virtualizing mode");
			}
		}

		private float Size
		{
			get
			{
				if (m_mode == VirtualizingMode.Horizontal)
				{
					return Mathf.Max(0f, m_virtualContent.rect.width);
				}
				return Mathf.Max(0f, m_virtualContent.rect.height);
			}
		}

		public event DataBindAction ItemDataBinding;

		private float EvalNormalizedIndex(int index)
		{
			int num = ItemsCount - VisibleItemsCount;
			if (num <= 0)
			{
				return 0f;
			}
			return (float)index / (float)num;
		}

		protected override void Awake()
		{
			base.Awake();
			if (m_virtualContent == null)
			{
				return;
			}
			m_virtualContentTransformChangeListener = m_virtualContent.GetComponent<RectTransformChangeListener>();
			m_virtualContentTransformChangeListener.RectTransformChanged += OnVirtualContentTransformChaged;
			UpdateVirtualContentPosition();
			if (m_mode == VirtualizingMode.Horizontal)
			{
				VerticalLayoutGroup component = m_virtualContent.GetComponent<VerticalLayoutGroup>();
				if (component != null)
				{
					UnityEngine.Object.DestroyImmediate(component);
				}
				HorizontalLayoutGroup horizontalLayoutGroup = m_virtualContent.GetComponent<HorizontalLayoutGroup>();
				if (horizontalLayoutGroup == null)
				{
					horizontalLayoutGroup = m_virtualContent.gameObject.AddComponent<HorizontalLayoutGroup>();
				}
				horizontalLayoutGroup.childControlHeight = true;
				horizontalLayoutGroup.childControlWidth = false;
				horizontalLayoutGroup.childForceExpandWidth = false;
			}
			else
			{
				HorizontalLayoutGroup component2 = m_virtualContent.GetComponent<HorizontalLayoutGroup>();
				if (component2 != null)
				{
					UnityEngine.Object.DestroyImmediate(component2);
				}
				VerticalLayoutGroup verticalLayoutGroup = m_virtualContent.GetComponent<VerticalLayoutGroup>();
				if (verticalLayoutGroup == null)
				{
					verticalLayoutGroup = m_virtualContent.gameObject.AddComponent<VerticalLayoutGroup>();
				}
				verticalLayoutGroup.childControlWidth = true;
				verticalLayoutGroup.childControlHeight = false;
				verticalLayoutGroup.childForceExpandHeight = false;
			}
			base.scrollSensitivity = ContainerSize;
		}

		protected override void Start()
		{
			base.Start();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (m_virtualContentTransformChangeListener != null)
			{
				m_virtualContentTransformChangeListener.RectTransformChanged -= OnVirtualContentTransformChaged;
			}
		}

		private void OnVirtualContentTransformChaged()
		{
			if (m_mode == VirtualizingMode.Horizontal)
			{
				base.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_virtualContent.rect.height);
			}
			else if (m_mode == VirtualizingMode.Vertical)
			{
				base.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_virtualContent.rect.width);
			}
		}

		protected override void SetNormalizedPosition(float value, int axis)
		{
			base.SetNormalizedPosition(value, axis);
			UpdateVirtualContentPosition();
			if (m_mode == VirtualizingMode.Vertical && axis == 1)
			{
				NormalizedIndex = 1f - value;
			}
			else if (m_mode == VirtualizingMode.Horizontal && axis == 0)
			{
				NormalizedIndex = value;
			}
		}

		protected override void SetContentAnchoredPosition(Vector2 position)
		{
			base.SetContentAnchoredPosition(position);
			UpdateVirtualContentPosition();
			if (m_mode == VirtualizingMode.Vertical)
			{
				NormalizedIndex = 1f - base.verticalNormalizedPosition;
			}
			else if (m_mode == VirtualizingMode.Horizontal)
			{
				NormalizedIndex = base.horizontalNormalizedPosition;
			}
		}

		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange();
			StartCoroutine(CoRectTransformDimensionsChange());
		}

		private IEnumerator CoRectTransformDimensionsChange()
		{
			yield return new WaitForEndOfFrame();
			if (VisibleItemsCount != m_containers.Count)
			{
				DataBind(Index);
			}
			OnVirtualContentTransformChaged();
		}

		private void UpdateVirtualContentPosition()
		{
			if (m_virtualContent != null)
			{
				if (m_mode == VirtualizingMode.Horizontal)
				{
					m_virtualContent.anchoredPosition = new Vector2(0f, base.content.anchoredPosition.y);
				}
				else if (m_mode == VirtualizingMode.Vertical)
				{
					m_virtualContent.anchoredPosition = new Vector2(base.content.anchoredPosition.x, 0f);
				}
			}
		}

		private void UpdateContentSize()
		{
			if (m_mode == VirtualizingMode.Horizontal)
			{
				base.content.sizeDelta = new Vector2((float)ItemsCount * ContainerSize, base.content.sizeDelta.y);
			}
			else if (m_mode == VirtualizingMode.Vertical)
			{
				base.content.sizeDelta = new Vector2(base.content.sizeDelta.x, (float)ItemsCount * ContainerSize);
			}
		}

		private void OnNormalizedIndexChanged(float newValue)
		{
			newValue = Mathf.Clamp01(newValue);
			int num = Index;
			float normalizedIndex = m_normalizedIndex;
			m_normalizedIndex = newValue;
			int index = Index;
			if (index < 0 || index >= ItemsCount)
			{
				m_normalizedIndex = normalizedIndex;
			}
			else
			{
				if (num == index)
				{
					return;
				}
				int num2 = index - num;
				bool flag = num2 > 0;
				num2 = Mathf.Abs(num2);
				if (num2 > VisibleItemsCount)
				{
					DataBind(index);
					return;
				}
				if (flag)
				{
					for (int i = 0; i < num2; i++)
					{
						LinkedListNode<RectTransform> first = m_containers.First;
						m_containers.RemoveFirst();
						int siblingIndex = m_containers.Last.Value.transform.GetSiblingIndex();
						m_containers.AddLast(first);
						RectTransform value = first.Value;
						value.SetSiblingIndex(siblingIndex + 1);
						if (this.ItemDataBinding != null && Items != null)
						{
							object item = Items[num + VisibleItemsCount];
							this.ItemDataBinding(value, item);
						}
						num++;
					}
					return;
				}
				for (int j = 0; j < num2; j++)
				{
					LinkedListNode<RectTransform> last = m_containers.Last;
					m_containers.RemoveLast();
					int siblingIndex2 = m_containers.First.Value.transform.GetSiblingIndex();
					m_containers.AddFirst(last);
					RectTransform value2 = last.Value;
					value2.SetSiblingIndex(siblingIndex2);
					num--;
					if (this.ItemDataBinding != null && Items != null)
					{
						object item2 = Items[num];
						this.ItemDataBinding(value2, item2);
					}
				}
			}
		}

		private void DataBind(int firstItemIndex)
		{
			int num = VisibleItemsCount - m_containers.Count;
			if (num < 0)
			{
				for (int i = 0; i < -num; i++)
				{
					UnityEngine.Object.Destroy(m_containers.Last.Value.gameObject);
					m_containers.RemoveLast();
				}
			}
			else
			{
				for (int j = 0; j < num; j++)
				{
					RectTransform value = UnityEngine.Object.Instantiate(ContainerPrefab, m_virtualContent);
					m_containers.AddLast(value);
				}
			}
			if (this.ItemDataBinding == null || Items == null)
			{
				return;
			}
			int num2 = 0;
			foreach (RectTransform container in m_containers)
			{
				this.ItemDataBinding(container, Items[firstItemIndex + num2]);
				num2++;
			}
		}

		public bool IsParentOf(Transform child)
		{
			if (m_virtualContent == null)
			{
				return false;
			}
			return child.IsChildOf(m_virtualContent);
		}

		public void InsertItem(int index, object item, bool raiseItemDataBindingEvent = true)
		{
			int index2 = Index;
			int num = index2 + VisibleItemsCount - 1;
			m_items.Insert(index, item);
			UpdateContentSize();
			UpdateScrollbar(index2);
			if (PossibleItemsCount >= m_items.Count && m_containers.Count < VisibleItemsCount)
			{
				RectTransform value = UnityEngine.Object.Instantiate(ContainerPrefab, m_virtualContent);
				m_containers.AddLast(value);
				num++;
			}
			if (index2 <= index && index <= num)
			{
				RectTransform value2 = m_containers.Last.Value;
				m_containers.RemoveLast();
				if (index == index2)
				{
					m_containers.AddFirst(value2);
					value2.SetSiblingIndex(0);
				}
				else
				{
					RectTransform value3 = m_containers.ElementAt(index - index2 - 1);
					LinkedListNode<RectTransform> node = m_containers.Find(value3);
					m_containers.AddAfter(node, value2);
					value2.SetSiblingIndex(index - index2);
				}
				if (raiseItemDataBindingEvent && this.ItemDataBinding != null)
				{
					this.ItemDataBinding(value2, item);
				}
			}
		}

		public void RemoveItems(int[] indices, bool raiseItemDataBindingEvent = true)
		{
			int num = Index;
			indices = indices.OrderBy((int i) => i).ToArray();
			for (int num2 = indices.Length - 1; num2 >= 0; num2--)
			{
				int num3 = indices[num2];
				if (num3 >= 0 && num3 < m_items.Count)
				{
					m_items.RemoveAt(num3);
				}
			}
			if (num + VisibleItemsCount >= ItemsCount)
			{
				num = Mathf.Max(0, ItemsCount - VisibleItemsCount);
			}
			UpdateContentSize();
			UpdateScrollbar(num);
			DataBind(num);
			OnVirtualContentTransformChaged();
		}

		public void SetNextSibling(object sibling, object nextSibling)
		{
			if (sibling == nextSibling)
			{
				return;
			}
			int num = m_items.IndexOf(sibling);
			int num2 = m_items.IndexOf(nextSibling);
			int index = Index;
			int num3 = index + VisibleItemsCount - 1;
			bool flag = index <= num2 && num2 <= num3;
			int num4 = num;
			if (num2 > num)
			{
				num4++;
			}
			int num5 = num2 - index;
			int num6 = num4 - index;
			bool flag2 = index <= num4 && ((num5 < 0) ? (num4 < num3) : (num4 <= num3));
			m_items.RemoveAt(num2);
			m_items.Insert(num4, nextSibling);
			if (flag2)
			{
				if (flag)
				{
					RectTransform rectTransform = m_containers.ElementAt(num5);
					m_containers.Remove(rectTransform);
					if (num6 == 0)
					{
						m_containers.AddFirst(rectTransform);
						rectTransform.SetSiblingIndex(0);
					}
					else
					{
						RectTransform value = m_containers.ElementAt(num6 - 1);
						LinkedListNode<RectTransform> node = m_containers.Find(value);
						m_containers.AddAfter(node, rectTransform);
					}
					rectTransform.SetSiblingIndex(num6);
					if (this.ItemDataBinding != null)
					{
						this.ItemDataBinding(rectTransform, nextSibling);
					}
					return;
				}
				RectTransform value2 = m_containers.Last.Value;
				m_containers.RemoveLast();
				if (num6 == 0)
				{
					m_containers.AddFirst(value2);
				}
				else
				{
					RectTransform value3 = ((num5 >= 0) ? m_containers.ElementAt(num6 - 1) : m_containers.ElementAt(num6));
					LinkedListNode<RectTransform> node2 = m_containers.Find(value3);
					m_containers.AddAfter(node2, value2);
				}
				if (num5 < 0)
				{
					UpdateScrollbar(index - 1);
					value2.SetSiblingIndex(num6 + 1);
				}
				else
				{
					value2.SetSiblingIndex(num6);
				}
				if (this.ItemDataBinding != null)
				{
					this.ItemDataBinding(value2, nextSibling);
				}
			}
			else if (flag)
			{
				if (num4 < index)
				{
					RectTransform rectTransform2 = m_containers.ElementAt(num5);
					m_containers.Remove(rectTransform2);
					m_containers.AddFirst(rectTransform2);
					rectTransform2.SetSiblingIndex(0);
					if (this.ItemDataBinding != null)
					{
						this.ItemDataBinding(rectTransform2, m_items[index]);
					}
				}
				else if (num4 > num3)
				{
					RectTransform rectTransform3 = m_containers.ElementAt(num5);
					m_containers.Remove(rectTransform3);
					m_containers.AddLast(rectTransform3);
					rectTransform3.SetSiblingIndex(m_containers.Count - 1);
					if (this.ItemDataBinding != null)
					{
						this.ItemDataBinding(rectTransform3, m_items[num3]);
					}
				}
			}
			else if (num5 < 0)
			{
				UpdateScrollbar(index - 1);
			}
		}

		public void SetPrevSibling(object sibling, object prevSibling)
		{
			int num = m_items.IndexOf(sibling);
			num--;
			if (num >= 0)
			{
				sibling = m_items[num];
				SetNextSibling(sibling, prevSibling);
				return;
			}
			int index = m_items.IndexOf(prevSibling);
			m_items.RemoveAt(index);
			m_items.Insert(0, prevSibling);
			RectTransform value = m_containers.Last.Value;
			m_containers.RemoveLast();
			m_containers.AddFirst(value);
			value.SetSiblingIndex(0);
			if (this.ItemDataBinding != null)
			{
				this.ItemDataBinding(value, prevSibling);
			}
		}

		public RectTransform GetContainer(object obj)
		{
			if (m_items == null)
			{
				return null;
			}
			int num = m_items.IndexOf(obj);
			if (num < 0)
			{
				return null;
			}
			int index = Index;
			int num2 = index + VisibleItemsCount - 1;
			if (index <= num && num <= num2)
			{
				return m_containers.ElementAt(num - index);
			}
			return null;
		}

		public RectTransform FirstContainer()
		{
			if (m_containers.Count == 0)
			{
				return null;
			}
			return m_containers.First.Value;
		}

		public void ForEachContainer(Action<RectTransform> action)
		{
			if (action == null)
			{
				return;
			}
			foreach (RectTransform container in m_containers)
			{
				action(container);
			}
		}

		public RectTransform LastContainer()
		{
			if (m_containers.Count == 0)
			{
				return null;
			}
			return m_containers.Last.Value;
		}

		private void UpdateScrollbar(int index)
		{
			m_normalizedIndex = EvalNormalizedIndex(index);
			if (m_mode == VirtualizingMode.Vertical)
			{
				base.verticalNormalizedPosition = 1f - m_normalizedIndex;
			}
			else if (m_mode == VirtualizingMode.Horizontal)
			{
				base.horizontalNormalizedPosition = m_normalizedIndex;
			}
		}
	}
}
