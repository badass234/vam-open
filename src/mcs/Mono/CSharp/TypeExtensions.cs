using System;

namespace Mono.CSharp
{
	public static class TypeExtensions
	{
		public static string GetNamespace(this Type t)
		{
			try
			{
				return t.Namespace;
			}
			catch
			{
				return null;
			}
		}
	}
}
