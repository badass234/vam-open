using System;

namespace IKVM.Reflection
{
	public struct MissingGenericMethodBuilder
	{
		private readonly MissingMethod method;

		public MissingGenericMethodBuilder(Type declaringType, CallingConventions callingConvention, string name, int genericParameterCount)
		{
			method = new MissingMethod(declaringType, name, new MethodSignature(null, null, default(PackedCustomModifiers), callingConvention, genericParameterCount));
		}

		public Type[] GetGenericArguments()
		{
			return method.GetGenericArguments();
		}

		public void SetSignature(Type returnType, CustomModifiers returnTypeCustomModifiers, Type[] parameterTypes, CustomModifiers[] parameterTypeCustomModifiers)
		{
			method.signature = new MethodSignature(returnType ?? method.Module.universe.System_Void, Util.Copy(parameterTypes), PackedCustomModifiers.CreateFromExternal(returnTypeCustomModifiers, parameterTypeCustomModifiers, parameterTypes.Length), method.signature.CallingConvention, method.signature.GenericParameterCount);
		}

		[Obsolete("Please use SetSignature(Type, CustomModifiers, Type[], CustomModifiers[]) instead.")]
		public void SetSignature(Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			method.signature = new MethodSignature(returnType ?? method.Module.universe.System_Void, Util.Copy(parameterTypes), PackedCustomModifiers.CreateFromExternal(returnTypeOptionalCustomModifiers, returnTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers, parameterTypeRequiredCustomModifiers, parameterTypes.Length), method.signature.CallingConvention, method.signature.GenericParameterCount);
		}

		public MethodInfo Finish()
		{
			return method;
		}
	}
}
