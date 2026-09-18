using System.Collections.Generic;

namespace Mono.CSharp
{
	internal static class ArrayComparer
	{
		public static bool IsEqual<T>(T[] array1, T[] array2)
		{
			if (array1 == null || array2 == null)
			{
				return array1 == array2;
			}
			EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
			for (int i = 0; i < array1.Length; i++)
			{
				if (!equalityComparer.Equals(array1[i], array2[i]))
				{
					return false;
				}
			}
			return true;
		}
	}
}
