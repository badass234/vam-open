using System.Collections.Generic;

public static class DictionaryExt
{
	public static U Get<T, U>(this Dictionary<T, U> dict, T key)
	{
		U value;
		if (dict.TryGetValue(key, out value))
		{
			return value;
		}
		return default(U);
	}
}
