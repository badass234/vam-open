using System.Collections.Generic;

public static class IEnumerableExtension
{
	public static List<T> ToList<T>(this IEnumerable<T> collection)
	{
		List<T> list = new List<T>();
		foreach (T item in collection)
		{
			list.Add(item);
		}
		return list;
	}
}
