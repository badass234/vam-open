using System.Collections.Generic;

namespace GPUTools.Common.Scripts.PL.Tools
{
	public class GroupedData<T> where T : IGroupItem
	{
		public List<GroupData> GroupsData = new List<GroupData>();

		public List<List<T>> Groups = new List<List<T>>();

		public T[] Data
		{
			get
			{
				List<T> list = new List<T>();
				foreach (List<T> group in Groups)
				{
					GroupsData.Add(new GroupData(list.Count, group.Count));
					list.AddRange(group);
				}
				return list.ToArray();
			}
		}

		public void AddGroup(List<T> list)
		{
			Groups.Add(list);
		}

		public void Add(T item)
		{
			for (int i = 0; i < Groups.Count; i++)
			{
				List<T> list = Groups[i];
				bool flag = false;
				for (int j = 0; j < list.Count; j++)
				{
					T val = list[j];
					if (item.HasConflict(val))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Add(item);
					return;
				}
			}
			List<T> list2 = new List<T>();
			list2.Add(item);
			List<T> item2 = list2;
			Groups.Add(item2);
		}

		public void Dispose()
		{
		}
	}
}
