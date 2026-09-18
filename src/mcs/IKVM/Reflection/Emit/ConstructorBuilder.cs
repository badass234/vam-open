using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;

namespace IKVM.Reflection.Emit
{
	public sealed class ConstructorBuilder : ConstructorInfo
	{
		private readonly MethodBuilder methodBuilder;

		public Type ReturnType => methodBuilder.ReturnType;

		public bool InitLocals
		{
			get
			{
				return methodBuilder.InitLocals;
			}
			set
			{
				methodBuilder.InitLocals = value;
			}
		}

		internal ConstructorBuilder(MethodBuilder mb)
		{
			methodBuilder = mb;
		}

		public override bool Equals(object obj)
		{
			ConstructorBuilder constructorBuilder = obj as ConstructorBuilder;
			if (constructorBuilder != null)
			{
				return constructorBuilder.methodBuilder.Equals(methodBuilder);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return methodBuilder.GetHashCode();
		}

		public void __SetSignature(Type returnType, CustomModifiers returnTypeCustomModifiers, Type[] parameterTypes, CustomModifiers[] parameterTypeCustomModifiers)
		{
			methodBuilder.__SetSignature(returnType, returnTypeCustomModifiers, parameterTypes, parameterTypeCustomModifiers);
		}

		[Obsolete("Please use __SetSignature(Type, CustomModifiers, Type[], CustomModifiers[]) instead.")]
		public void __SetSignature(Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			methodBuilder.SetSignature(returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers);
		}

		public ParameterBuilder DefineParameter(int position, ParameterAttributes attributes, string strParamName)
		{
			return methodBuilder.DefineParameter(position, attributes, strParamName);
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			methodBuilder.SetCustomAttribute(customBuilder);
		}

		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			methodBuilder.SetCustomAttribute(con, binaryAttribute);
		}

		public void __AddDeclarativeSecurity(CustomAttributeBuilder customBuilder)
		{
			methodBuilder.__AddDeclarativeSecurity(customBuilder);
		}

		public void AddDeclarativeSecurity(SecurityAction securityAction, PermissionSet permissionSet)
		{
			methodBuilder.AddDeclarativeSecurity(securityAction, permissionSet);
		}

		public void SetImplementationFlags(MethodImplAttributes attributes)
		{
			methodBuilder.SetImplementationFlags(attributes);
		}

		public ILGenerator GetILGenerator()
		{
			return methodBuilder.GetILGenerator();
		}

		public ILGenerator GetILGenerator(int streamSize)
		{
			return methodBuilder.GetILGenerator(streamSize);
		}

		public void __ReleaseILGenerator()
		{
			methodBuilder.__ReleaseILGenerator();
		}

		public Module GetModule()
		{
			return methodBuilder.GetModule();
		}

		public MethodToken GetToken()
		{
			return methodBuilder.GetToken();
		}

		public void SetMethodBody(byte[] il, int maxStack, byte[] localSignature, IEnumerable<ExceptionHandler> exceptionHandlers, IEnumerable<int> tokenFixups)
		{
			methodBuilder.SetMethodBody(il, maxStack, localSignature, exceptionHandlers, tokenFixups);
		}

		internal override MethodInfo GetMethodInfo()
		{
			return methodBuilder;
		}

		internal override MethodInfo GetMethodOnTypeDefinition()
		{
			return methodBuilder;
		}
	}
}
