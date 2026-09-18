using IKVM.Reflection.Emit;

namespace IKVM.Reflection
{
	public abstract class MethodBase : MemberInfo
	{
		internal abstract MethodSignature MethodSignature { get; }

		internal abstract int ParameterCount { get; }

		public abstract MethodAttributes Attributes { get; }

		public abstract CallingConventions CallingConvention { get; }

		public abstract int __MethodRVA { get; }

		public bool IsConstructor
		{
			get
			{
				if ((Attributes & MethodAttributes.RTSpecialName) != MethodAttributes.PrivateScope)
				{
					string name = Name;
					if (!(name == ConstructorInfo.ConstructorName))
					{
						return name == ConstructorInfo.TypeConstructorName;
					}
					return true;
				}
				return false;
			}
		}

		public bool IsStatic => (Attributes & MethodAttributes.Static) != 0;

		public bool IsVirtual => (Attributes & MethodAttributes.Virtual) != 0;

		public bool IsAbstract => (Attributes & MethodAttributes.Abstract) != 0;

		public bool IsFinal => (Attributes & MethodAttributes.Final) != 0;

		public bool IsPublic => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;

		public bool IsFamily => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Family;

		public bool IsFamilyOrAssembly => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamORAssem;

		public bool IsAssembly => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Assembly;

		public bool IsFamilyAndAssembly => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamANDAssem;

		public bool IsPrivate => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Private;

		public bool IsSpecialName => (Attributes & MethodAttributes.SpecialName) != 0;

		public bool IsHideBySig => (Attributes & MethodAttributes.HideBySig) != 0;

		public MethodImplAttributes MethodImplementationFlags => GetMethodImplementationFlags();

		public virtual bool IsGenericMethod => false;

		public virtual bool IsGenericMethodDefinition => false;

		public virtual bool ContainsGenericParameters => IsGenericMethodDefinition;

		internal MethodBase()
		{
		}

		public abstract ParameterInfo[] GetParameters();

		public abstract MethodImplAttributes GetMethodImplementationFlags();

		public abstract MethodBody GetMethodBody();

		public virtual Type[] GetGenericArguments()
		{
			return Type.EmptyTypes;
		}

		public virtual MethodBase __GetMethodOnTypeDefinition()
		{
			return this;
		}

		internal abstract MethodInfo GetMethodOnTypeDefinition();

		internal abstract int ImportTo(ModuleBuilder module);

		internal abstract MethodBase BindTypeParameters(Type type);

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
			if ((Attributes & MethodAttributes.MemberAccessMask) > MethodAttributes.Private && MemberInfo.BindingFlagsMatch(IsPublic, flags, BindingFlags.Public, BindingFlags.NonPublic))
			{
				return MemberInfo.BindingFlagsMatch(IsStatic, flags, BindingFlags.Static | BindingFlags.FlattenHierarchy, BindingFlags.Instance);
			}
			return false;
		}
	}
}
