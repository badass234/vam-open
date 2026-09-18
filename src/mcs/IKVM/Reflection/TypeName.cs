using System;

namespace IKVM.Reflection
{
	internal struct TypeName : IEquatable<TypeName>
	{
		private readonly string ns;

		private readonly string name;

		internal string Name => name;

		internal string Namespace => ns;

		internal TypeName(string ns, string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			this.ns = ns;
			this.name = name;
		}

		public static bool operator ==(TypeName o1, TypeName o2)
		{
			if (o1.ns == o2.ns)
			{
				return o1.name == o2.name;
			}
			return false;
		}

		public static bool operator !=(TypeName o1, TypeName o2)
		{
			if (!(o1.ns != o2.ns))
			{
				return o1.name != o2.name;
			}
			return true;
		}

		public override int GetHashCode()
		{
			if (ns != null)
			{
				return ns.GetHashCode() * 37 + name.GetHashCode();
			}
			return name.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			TypeName? typeName = obj as TypeName?;
			if (typeName.HasValue)
			{
				return typeName.Value == this;
			}
			return false;
		}

		public override string ToString()
		{
			if (ns != null)
			{
				return ns + "." + name;
			}
			return name;
		}

		bool IEquatable<TypeName>.Equals(TypeName other)
		{
			return this == other;
		}

		internal bool Matches(string fullName)
		{
			if (ns == null)
			{
				return name == fullName;
			}
			if (ns.Length + 1 + name.Length == fullName.Length)
			{
				if (fullName.StartsWith(ns, StringComparison.Ordinal) && fullName[ns.Length] == '.')
				{
					return fullName.EndsWith(name, StringComparison.Ordinal);
				}
				return false;
			}
			return false;
		}

		internal TypeName ToLowerInvariant()
		{
			return new TypeName((ns == null) ? null : ns.ToLowerInvariant(), name.ToLowerInvariant());
		}

		internal static TypeName Split(string name)
		{
			int num = name.LastIndexOf('.');
			if (num == -1)
			{
				return new TypeName(null, name);
			}
			return new TypeName(name.Substring(0, num), name.Substring(num + 1));
		}
	}
}
