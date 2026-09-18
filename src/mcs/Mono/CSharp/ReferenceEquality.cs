using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Mono.CSharp
{
	internal sealed class ReferenceEquality<T> : IEqualityComparer<T> where T : class
	{
		public static readonly IEqualityComparer<T> Default = new ReferenceEquality<T>();

		private ReferenceEquality()
		{
		}

		public bool Equals(T x, T y)
		{
			return x == y;
		}

		public int GetHashCode(T obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}
}
