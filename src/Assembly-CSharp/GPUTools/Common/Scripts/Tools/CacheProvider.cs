using System.Collections.Generic;
using UnityEngine;

namespace GPUTools.Common.Scripts.Tools
{
	public class CacheProvider<T> where T : Component
	{
		private readonly List<GameObject> providers;

		private List<T> items;

		public List<T> Items
		{
			get
			{
				if (items == null)
				{
					items = GetItems();
				}
				return items;
			}
		}

		public CacheProvider(List<GameObject> providers)
		{
			this.providers = providers;
			items = GetItems();
		}

		public List<T> GetItems()
		{
			List<T> list = new List<T>();
			foreach (GameObject provider in providers)
			{
				if (provider != null)
				{
					list.AddRange(provider.GetComponentsInChildren<T>().ToList());
				}
			}
			return list;
		}

		public static bool Verify(List<GameObject> list)
		{
			if (list.Count == 0)
			{
				return false;
			}
			foreach (GameObject item in list)
			{
				if (item == null)
				{
					return false;
				}
			}
			return true;
		}
	}
}
