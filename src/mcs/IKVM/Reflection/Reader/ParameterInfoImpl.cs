using System.Collections.Generic;

namespace IKVM.Reflection.Reader
{
	internal sealed class ParameterInfoImpl : ParameterInfo
	{
		private readonly MethodDefImpl method;

		private readonly int position;

		private readonly int index;

		public override string Name
		{
			get
			{
				if (index != -1)
				{
					return ((ModuleReader)Module).GetString(Module.Param.records[index].Name);
				}
				return null;
			}
		}

		public override Type ParameterType
		{
			get
			{
				if (position != -1)
				{
					return method.MethodSignature.GetParameterType(method, position);
				}
				return method.MethodSignature.GetReturnType(method);
			}
		}

		public override ParameterAttributes Attributes
		{
			get
			{
				if (index != -1)
				{
					return (ParameterAttributes)Module.Param.records[index].Flags;
				}
				return ParameterAttributes.None;
			}
		}

		public override int Position => position;

		public override object RawDefaultValue
		{
			get
			{
				if ((Attributes & ParameterAttributes.HasDefault) != ParameterAttributes.None)
				{
					return Module.Constant.GetRawConstantValue(Module, MetadataToken);
				}
				Universe universe = Module.universe;
				if (ParameterType == universe.System_Decimal)
				{
					Type system_Runtime_CompilerServices_DecimalConstantAttribute = universe.System_Runtime_CompilerServices_DecimalConstantAttribute;
					if (system_Runtime_CompilerServices_DecimalConstantAttribute != null)
					{
						foreach (CustomAttributeData item in CustomAttributeData.__GetCustomAttributes(this, system_Runtime_CompilerServices_DecimalConstantAttribute, false))
						{
							IList<CustomAttributeTypedArgument> constructorArguments = item.ConstructorArguments;
							if (constructorArguments.Count == 5)
							{
								if (constructorArguments[0].ArgumentType == universe.System_Byte && constructorArguments[1].ArgumentType == universe.System_Byte && constructorArguments[2].ArgumentType == universe.System_Int32 && constructorArguments[3].ArgumentType == universe.System_Int32 && constructorArguments[4].ArgumentType == universe.System_Int32)
								{
									return new decimal((int)constructorArguments[4].Value, (int)constructorArguments[3].Value, (int)constructorArguments[2].Value, (byte)constructorArguments[1].Value != 0, (byte)constructorArguments[0].Value);
								}
								if (constructorArguments[0].ArgumentType == universe.System_Byte && constructorArguments[1].ArgumentType == universe.System_Byte && constructorArguments[2].ArgumentType == universe.System_UInt32 && constructorArguments[3].ArgumentType == universe.System_UInt32 && constructorArguments[4].ArgumentType == universe.System_UInt32)
								{
									return new decimal((int)(uint)constructorArguments[4].Value, (int)(uint)constructorArguments[3].Value, (int)(uint)constructorArguments[2].Value, (byte)constructorArguments[1].Value != 0, (byte)constructorArguments[0].Value);
								}
							}
						}
					}
				}
				if ((Attributes & ParameterAttributes.Optional) != ParameterAttributes.None)
				{
					return Missing.Value;
				}
				return null;
			}
		}

		public override MemberInfo Member => method.Module.ResolveMethod(method.MetadataToken);

		public override int MetadataToken => (8 << 24) + index + 1;

		internal override Module Module => method.Module;

		internal ParameterInfoImpl(MethodDefImpl method, int position, int index)
		{
			this.method = method;
			this.position = position;
			this.index = index;
		}

		public override CustomModifiers __GetCustomModifiers()
		{
			if (position != -1)
			{
				return method.MethodSignature.GetParameterCustomModifiers(method, position);
			}
			return method.MethodSignature.GetReturnTypeCustomModifiers(method);
		}

		public override bool __TryGetFieldMarshal(out FieldMarshal fieldMarshal)
		{
			return FieldMarshal.ReadFieldMarshal(Module, MetadataToken, out fieldMarshal);
		}
	}
}
