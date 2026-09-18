using IKVM.Reflection.Emit;

namespace IKVM.Reflection
{
	internal sealed class BuiltinArrayMethod : ArrayMethod
	{
		private sealed class ParameterInfoImpl : ParameterInfo
		{
			private readonly MethodInfo method;

			private readonly Type type;

			private readonly int pos;

			public override Type ParameterType => type;

			public override string Name => null;

			public override ParameterAttributes Attributes => ParameterAttributes.None;

			public override int Position => pos;

			public override object RawDefaultValue => null;

			public override MemberInfo Member
			{
				get
				{
					if (!method.IsConstructor)
					{
						return method;
					}
					return new ConstructorInfoImpl(method);
				}
			}

			public override int MetadataToken => 134217728;

			internal override Module Module => method.Module;

			internal ParameterInfoImpl(MethodInfo method, Type type, int pos)
			{
				this.method = method;
				this.type = type;
				this.pos = pos;
			}

			public override CustomModifiers __GetCustomModifiers()
			{
				return default(CustomModifiers);
			}

			public override bool __TryGetFieldMarshal(out FieldMarshal fieldMarshal)
			{
				fieldMarshal = default(FieldMarshal);
				return false;
			}
		}

		public override MethodAttributes Attributes
		{
			get
			{
				if (!(Name == ".ctor"))
				{
					return MethodAttributes.Public;
				}
				return MethodAttributes.Public | MethodAttributes.RTSpecialName;
			}
		}

		public override int MetadataToken => 100663296;

		public override ParameterInfo ReturnParameter => new ParameterInfoImpl(this, ReturnType, -1);

		internal BuiltinArrayMethod(Module module, Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
			: base(module, arrayClass, methodName, callingConvention, returnType, parameterTypes)
		{
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			return MethodImplAttributes.IL;
		}

		public override MethodBody GetMethodBody()
		{
			return null;
		}

		public override ParameterInfo[] GetParameters()
		{
			ParameterInfo[] array = new ParameterInfo[parameterTypes.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new ParameterInfoImpl(this, parameterTypes[i], i);
			}
			return array;
		}
	}
}
