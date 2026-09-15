using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls
{
	public class ListBoxDemo : MonoBehaviour
	{
		public ListBox ListBox;

		public static bool IsPrefab(Transform This)
		{
			if (Application.isEditor && !Application.isPlaying)
			{
				throw new InvalidOperationException("Does not work in edit mode");
			}
			return This.gameObject.scene.buildIndex < 0;
		}

		private void Start()
		{
			if (!ListBox)
			{
				Debug.LogError("Set ListBox field");
				return;
			}
			ListBox.ItemDataBinding += OnItemDataBinding;
			ListBox.SelectionChanged += OnSelectionChanged;
			ListBox.ItemsRemoved += OnItemsRemoved;
			ListBox.ItemBeginDrag += OnItemBeginDrag;
			ListBox.ItemDrop += OnItemDrop;
			ListBox.ItemEndDrag += OnItemEndDrag;
			IEnumerable<GameObject> source = from go in Resources.FindObjectsOfTypeAll<GameObject>()
				where !IsPrefab(go.transform) && go.transform.parent == null
				select go;
			ListBox.Items = source.OrderBy((GameObject t) => t.transform.GetSiblingIndex());
		}

		private void OnDestroy()
		{
			if ((bool)ListBox)
			{
				ListBox.ItemDataBinding -= OnItemDataBinding;
				ListBox.SelectionChanged -= OnSelectionChanged;
				ListBox.ItemsRemoved -= OnItemsRemoved;
				ListBox.ItemBeginDrag -= OnItemBeginDrag;
				ListBox.ItemDrop -= OnItemDrop;
				ListBox.ItemEndDrag -= OnItemEndDrag;
			}
		}

		private void OnItemExpanding(object sender, ItemExpandingArgs e)
		{
			GameObject gameObject = (GameObject)e.Item;
			if (gameObject.transform.childCount > 0)
			{
				GameObject[] array = new GameObject[gameObject.transform.childCount];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = gameObject.transform.GetChild(i).gameObject;
				}
				e.Children = array;
			}
		}

		private void OnSelectionChanged(object sender, SelectionChangedArgs e)
		{
		}

		private void OnItemsRemoved(object sender, ItemsRemovedArgs e)
		{
			for (int i = 0; i < e.Items.Length; i++)
			{
				GameObject gameObject = (GameObject)e.Items[i];
				if (gameObject != null)
				{
					UnityEngine.Object.Destroy(gameObject);
				}
			}
		}

		private void OnItemDataBinding(object sender, ItemDataBindingArgs e)
		{
			GameObject gameObject = e.Item as GameObject;
			if (gameObject != null)
			{
				Text componentInChildren = e.ItemPresenter.GetComponentInChildren<Text>(true);
				componentInChildren.text = gameObject.name;
			}
		}

		private void OnItemBeginDrag(object sender, ItemArgs e)
		{
		}

		private void OnItemDrop(object sender, ItemDropArgs e)
		{
			Transform transform = ((GameObject)e.DropTarget).transform;
			if (e.Action == ItemDropAction.SetLastChild)
			{
				for (int i = 0; i < e.DragItems.Length; i++)
				{
					Transform transform2 = ((GameObject)e.DragItems[i]).transform;
					transform2.SetParent(transform, true);
					transform2.SetAsLastSibling();
				}
			}
			else if (e.Action == ItemDropAction.SetNextSibling)
			{
				for (int j = 0; j < e.DragItems.Length; j++)
				{
					Transform transform3 = ((GameObject)e.DragItems[j]).transform;
					if (transform3.parent != transform.parent)
					{
						transform3.SetParent(transform.parent, true);
					}
					int siblingIndex = transform.GetSiblingIndex();
					transform3.SetSiblingIndex(siblingIndex + 1);
				}
			}
			else
			{
				if (e.Action != ItemDropAction.SetPrevSibling)
				{
					return;
				}
				for (int k = 0; k < e.DragItems.Length; k++)
				{
					Transform transform4 = ((GameObject)e.DragItems[k]).transform;
					if (transform4.parent != transform.parent)
					{
						transform4.SetParent(transform.parent, true);
					}
					int siblingIndex2 = transform.GetSiblingIndex();
					transform4.SetSiblingIndex(siblingIndex2);
				}
			}
		}

		private void OnItemEndDrag(object sender, ItemArgs e)
		{
		}
	}
}
