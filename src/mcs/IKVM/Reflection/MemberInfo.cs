using System;
using System.Collections.Generic;

namespace IKVM.Reflection
{
	public abstract class MemberInfo : ICustomAttributeProvider
	{
		public abstract string Name { get; }

		public abstract Type DeclaringType { get; }

		public abstract MemberTypes MemberType { get; }

		public virtual Type ReflectedType => DeclaringType;

		public virtual int MetadataToken
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public abstract Module Module { get; }

		public virtual bool __IsMissing => false;

		public IEnumerable<CustomAttributeData> CustomAttributes => GetCustomAttributesData();

		internal abstract bool IsBaked { get; }

		internal MemberInfo()
		{
		}

		internal abstract MemberInfo SetReflectedType(Type type);

		public bool IsDefined(Type attributeType, bool inherit)
		{
			return CustomAttributeData.__GetCustomAttributes(this, attributeType, inherit).Count != 0;
		}

		public IList<CustomAttributeData> __GetCustomAttributes(Type attributeType, bool inherit)
		{
			return CustomAttributeData.__GetCustomAttributes(this, attributeType, inherit);
		}

		public IList<CustomAttributeData> GetCustomAttributesData()
		{
			return CustomAttributeData.GetCustomAttributes(this);
		}

		public static bool operator ==(MemberInfo m1, MemberInfo m2)
		{
			if ((object)m1 != m2)
			{
				return m1?.Equals(m2) ?? false;
			}
			return true;
		}

		public static bool operator !=(MemberInfo m1, MemberInfo m2)
		{
			return !(m1 == m2);
		}

		internal abstract int GetCurrentToken();

		internal abstract List<CustomAttributeData> GetPseudoCustomAttributes(Type attributeType);

		internal virtual bool BindingFlagsMatch(BindingFlags flags)
		{
			throw new InvalidOperationException();
		}

		internal virtual bool BindingFlagsMatchInherited(BindingFlags flags)
		{
			throw new InvalidOperationException();
		}

		protected static bool BindingFlagsMatch(bool state, BindingFlags flags, BindingFlags trueFlag, BindingFlags falseFlag)
		{
			if (!state || (flags & trueFlag) != trueFlag)
			{
				if (!state)
				{
					return (flags & falseFlag) == falseFlag;
				}
				return false;
			}
			return true;
		}

		protected static T SetReflectedType<T>(T member, Type type) where T : MemberInfo
		{
			if (!(member == null))
			{
				return (T)member.SetReflectedType(type);
			}
			return null;
		}

		protected static T[] SetReflectedType<T>(T[] members, Type type) where T : MemberInfo
		{
			for (int i = 0; i < members.Length; i++)
			{
				members[i] = SetReflectedType(members[i], type);
			}
			return members;
		}
	}
}
