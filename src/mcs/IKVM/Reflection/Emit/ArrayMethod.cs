using System;

namespace IKVM.Reflection.Emit
{
	internal class ArrayMethod : MethodInfo
	{
		private readonly Module module;

		private readonly Type arrayClass;

		private readonly string methodName;

		private readonly CallingConventions callingConvention;

		private readonly Type returnType;

		protected readonly Type[] parameterTypes;

		private MethodSignature methodSignature;

		public override int __MethodRVA
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		public override MethodAttributes Attributes
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override CallingConventions CallingConvention => callingConvention;

		public override Type DeclaringType => arrayClass;

		internal override MethodSignature MethodSignature
		{
			get
			{
				if (methodSignature == null)
				{
					methodSignature = MethodSignature.MakeFromBuilder(returnType, parameterTypes, default(PackedCustomModifiers), callingConvention, 0);
				}
				return methodSignature;
			}
		}

		public override Module Module => module;

		public override string Name => methodName;

		internal override int ParameterCount => parameterTypes.Length;

		public override ParameterInfo ReturnParameter
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override Type ReturnType => returnType;

		internal override bool HasThis => (callingConvention & (CallingConventions.HasThis | CallingConventions.ExplicitThis)) == CallingConventions.HasThis;

		internal override bool IsBaked => arrayClass.IsBaked;

		internal ArrayMethod(Module module, Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			this.module = module;
			this.arrayClass = arrayClass;
			this.methodName = methodName;
			this.callingConvention = callingConvention;
			this.returnType = returnType ?? module.universe.System_Void;
			this.parameterTypes = Util.Copy(parameterTypes);
		}

		public override MethodBody GetMethodBody()
		{
			throw new InvalidOperationException();
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			throw new NotSupportedException();
		}

		public override ParameterInfo[] GetParameters()
		{
			throw new NotSupportedException();
		}

		internal override int ImportTo(ModuleBuilder module)
		{
			return module.ImportMethodOrField(arrayClass, methodName, MethodSignature);
		}

		internal override int GetCurrentToken()
		{
			return MetadataToken;
		}
	}
}
