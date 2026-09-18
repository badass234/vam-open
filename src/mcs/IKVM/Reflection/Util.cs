using System;
using System.Collections.Generic;

namespace IKVM.Reflection
{
	internal static class Util
	{
		internal static int[] Copy(int[] array)
		{
			if (array == null || array.Length == 0)
			{
				return Empty<int>.Array;
			}
			int[] array2 = new int[array.Length];
			Array.Copy(array, array2, array.Length);
			return array2;
		}

		internal static Type[] Copy(Type[] array)
		{
			if (array == null || array.Length == 0)
			{
				return Type.EmptyTypes;
			}
			Type[] array2 = new Type[array.Length];
			Array.Copy(array, array2, array.Length);
			return array2;
		}

		internal static T[] ToArray<T, V>(List<V> list, T[] empty) where V : T
		{
			if (list == null || list.Count == 0)
			{
				return empty;
			}
			T[] array = new T[list.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = (T)(object)list[i];
			}
			return array;
		}

		internal static T[] ToArray<T>(IEnumerable<T> values)
		{
			if (values != null)
			{
				return new List<T>(values).ToArray();
			}
			return Empty<T>.Array;
		}

		internal static bool ArrayEquals(Type[] t1, Type[] t2)
		{
			if (t1 == t2)
			{
				return true;
			}
			if (t1 == null)
			{
				return t2.Length == 0;
			}
			if (t2 == null)
			{
				return t1.Length == 0;
			}
			if (t1.Length == t2.Length)
			{
				for (int i = 0; i < t1.Length; i++)
				{
					if (!TypeEquals(t1[i], t2[i]))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		internal static bool TypeEquals(Type t1, Type t2)
		{
			if (t1 == t2)
			{
				return true;
			}
			if (t1 == null)
			{
				return false;
			}
			return t1.Equals(t2);
		}

		internal static int GetHashCode(Type[] types)
		{
			if (types == null)
			{
				return 0;
			}
			int num = 0;
			foreach (Type type in types)
			{
				if (type != null)
				{
					num *= 3;
					num ^= type.GetHashCode();
				}
			}
			return num;
		}

		internal static bool ArrayEquals(CustomModifiers[] m1, CustomModifiers[] m2)
		{
			if (m1 == null || m2 == null)
			{
				return m1 == m2;
			}
			if (m1.Length != m2.Length)
			{
				return false;
			}
			for (int i = 0; i < m1.Length; i++)
			{
				if (!m1[i].Equals(m2[i]))
				{
					return false;
				}
			}
			return true;
		}

		internal static int GetHashCode(CustomModifiers[] mods)
		{
			int num = 0;
			if (mods != null)
			{
				for (int i = 0; i < mods.Length; i++)
				{
					CustomModifiers customModifiers = mods[i];
					num ^= customModifiers.GetHashCode();
				}
			}
			return num;
		}

		internal static T NullSafeElementAt<T>(T[] array, int index)
		{
			if (array != null)
			{
				return array[index];
			}
			return default(T);
		}

		internal static int NullSafeLength<T>(T[] array)
		{
			if (array != null)
			{
				return array.Length;
			}
			return 0;
		}
	}
}
