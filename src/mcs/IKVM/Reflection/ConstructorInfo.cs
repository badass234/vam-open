using System.Collections.Generic;
using IKVM.Reflection.Emit;

namespace IKVM.Reflection
{
	public abstract class ConstructorInfo : MethodBase
	{
		public static readonly string ConstructorName = ".ctor";

		public static readonly string TypeConstructorName = ".cctor";

		public sealed override MemberTypes MemberType => MemberTypes.Constructor;

		public sealed override int __MethodRVA => GetMethodInfo().__MethodRVA;

		public sealed override bool ContainsGenericParameters => GetMethodInfo().ContainsGenericParameters;

		public ParameterInfo __ReturnParameter => new ParameterInfoWrapper(this, GetMethodInfo().ReturnParameter);

		public sealed override CallingConventions CallingConvention => GetMethodInfo().CallingConvention;

		public sealed override MethodAttributes Attributes => GetMethodInfo().Attributes;

		public sealed override Type DeclaringType => GetMethodInfo().DeclaringType;

		public sealed override string Name => GetMethodInfo().Name;

		public sealed override int MetadataToken => GetMethodInfo().MetadataToken;

		public sealed override Module Module => GetMethodInfo().Module;

		public sealed override bool __IsMissing => GetMethodInfo().__IsMissing;

		internal sealed override int ParameterCount => GetMethodInfo().ParameterCount;

		internal sealed override bool IsBaked => GetMethodInfo().IsBaked;

		internal sealed override MethodSignature MethodSignature => GetMethodInfo().MethodSignature;

		internal ConstructorInfo()
		{
		}

		public sealed override string ToString()
		{
			return GetMethodInfo().ToString();
		}

		internal abstract MethodInfo GetMethodInfo();

		internal override MethodBase BindTypeParameters(Type type)
		{
			return new ConstructorInfoImpl((MethodInfo)GetMethodInfo().BindTypeParameters(type));
		}

		public sealed override MethodBase __GetMethodOnTypeDefinition()
		{
			return new ConstructorInfoImpl((MethodInfo)GetMethodInfo().__GetMethodOnTypeDefinition());
		}

		public sealed override ParameterInfo[] GetParameters()
		{
			ParameterInfo[] parameters = GetMethodInfo().GetParameters();
			for (int i = 0; i < parameters.Length; i++)
			{
				parameters[i] = new ParameterInfoWrapper(this, parameters[i]);
			}
			return parameters;
		}

		public sealed override MethodImplAttributes GetMethodImplementationFlags()
		{
			return GetMethodInfo().GetMethodImplementationFlags();
		}

		public sealed override MethodBody GetMethodBody()
		{
			return GetMethodInfo().GetMethodBody();
		}

		internal sealed override MemberInfo SetReflectedType(Type type)
		{
			return new ConstructorInfoWithReflectedType(type, this);
		}

		internal sealed override int GetCurrentToken()
		{
			return GetMethodInfo().GetCurrentToken();
		}

		internal sealed override List<CustomAttributeData> GetPseudoCustomAttributes(Type attributeType)
		{
			return GetMethodInfo().GetPseudoCustomAttributes(attributeType);
		}

		internal sealed override int ImportTo(ModuleBuilder module)
		{
			return GetMethodInfo().ImportTo(module);
		}
	}
}
