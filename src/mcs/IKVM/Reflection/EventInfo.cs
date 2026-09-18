using System.Collections.Generic;

namespace IKVM.Reflection
{
	public abstract class EventInfo : MemberInfo
	{
		public sealed override MemberTypes MemberType => MemberTypes.Event;

		public abstract EventAttributes Attributes { get; }

		public abstract Type EventHandlerType { get; }

		internal abstract bool IsPublic { get; }

		internal abstract bool IsNonPrivate { get; }

		internal abstract bool IsStatic { get; }

		public bool IsSpecialName => (Attributes & EventAttributes.SpecialName) != 0;

		public MethodInfo AddMethod => GetAddMethod(true);

		public MethodInfo RaiseMethod => GetRaiseMethod(true);

		public MethodInfo RemoveMethod => GetRemoveMethod(true);

		internal EventInfo()
		{
		}

		public abstract MethodInfo GetAddMethod(bool nonPublic);

		public abstract MethodInfo GetRaiseMethod(bool nonPublic);

		public abstract MethodInfo GetRemoveMethod(bool nonPublic);

		public abstract MethodInfo[] GetOtherMethods(bool nonPublic);

		public abstract MethodInfo[] __GetMethods();

		public MethodInfo GetAddMethod()
		{
			return GetAddMethod(false);
		}

		public MethodInfo GetRaiseMethod()
		{
			return GetRaiseMethod(false);
		}

		public MethodInfo GetRemoveMethod()
		{
			return GetRemoveMethod(false);
		}

		public MethodInfo[] GetOtherMethods()
		{
			return GetOtherMethods(false);
		}

		internal virtual EventInfo BindTypeParameters(Type type)
		{
			return new GenericEventInfo(DeclaringType.BindTypeParameters(type), this);
		}

		public override string ToString()
		{
			return DeclaringType.ToString() + " " + Name;
		}

		internal sealed override bool BindingFlagsMatch(BindingFlags flags)
		{
			if (MemberInfo.BindingFlagsMatch(IsPublic, flags, BindingFlags.Public, BindingFlags.NonPublic))
			{
				return MemberInfo.BindingFlagsMatch(IsStatic, flags, BindingFlags.Static, BindingFlags.Instance);
			}
			return false;
		}

		internal sealed override bool BindingFlagsMatchInherited(BindingFlags flags)
		{
			if (IsNonPrivate && MemberInfo.BindingFlagsMatch(IsPublic, flags, BindingFlags.Public, BindingFlags.NonPublic))
			{
				return MemberInfo.BindingFlagsMatch(IsStatic, flags, BindingFlags.Static | BindingFlags.FlattenHierarchy, BindingFlags.Instance);
			}
			return false;
		}

		internal sealed override MemberInfo SetReflectedType(Type type)
		{
			return new EventInfoWithReflectedType(type, this);
		}

		internal sealed override List<CustomAttributeData> GetPseudoCustomAttributes(Type attributeType)
		{
			return null;
		}
	}
}
