using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection
{
	internal sealed class PropertySignature : Signature
	{
		private CallingConventions callingConvention;

		private readonly Type propertyType;

		private readonly Type[] parameterTypes;

		private readonly PackedCustomModifiers customModifiers;

		internal int ParameterCount => parameterTypes.Length;

		internal bool HasThis
		{
			set
			{
				if (value)
				{
					callingConvention |= CallingConventions.HasThis;
				}
				else
				{
					callingConvention &= ~CallingConventions.HasThis;
				}
			}
		}

		internal Type PropertyType => propertyType;

		internal CallingConventions CallingConvention => callingConvention;

		internal static PropertySignature Create(CallingConventions callingConvention, Type propertyType, Type[] parameterTypes, PackedCustomModifiers customModifiers)
		{
			return new PropertySignature(callingConvention, propertyType, Util.Copy(parameterTypes), customModifiers);
		}

		private PropertySignature(CallingConventions callingConvention, Type propertyType, Type[] parameterTypes, PackedCustomModifiers customModifiers)
		{
			this.callingConvention = callingConvention;
			this.propertyType = propertyType;
			this.parameterTypes = parameterTypes;
			this.customModifiers = customModifiers;
		}

		public override bool Equals(object obj)
		{
			PropertySignature propertySignature = obj as PropertySignature;
			if (propertySignature != null && propertySignature.propertyType.Equals(propertyType))
			{
				return propertySignature.customModifiers.Equals(customModifiers);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return propertyType.GetHashCode() ^ customModifiers.GetHashCode();
		}

		internal CustomModifiers GetCustomModifiers()
		{
			return customModifiers.GetReturnTypeCustomModifiers();
		}

		internal PropertySignature ExpandTypeParameters(Type declaringType)
		{
			return new PropertySignature(callingConvention, propertyType.BindTypeParameters(declaringType), Signature.BindTypeParameters(declaringType, parameterTypes), customModifiers.Bind(declaringType));
		}

		internal override void WriteSig(ModuleBuilder module, ByteBuffer bb)
		{
			byte b = 8;
			if ((callingConvention & CallingConventions.HasThis) != 0)
			{
				b |= 0x20;
			}
			if ((callingConvention & CallingConventions.ExplicitThis) != 0)
			{
				b |= 0x40;
			}
			if ((callingConvention & CallingConventions.VarArgs) != 0)
			{
				b |= 5;
			}
			bb.Write(b);
			bb.WriteCompressedUInt((parameterTypes != null) ? parameterTypes.Length : 0);
			Signature.WriteCustomModifiers(module, bb, customModifiers.GetReturnTypeCustomModifiers());
			Signature.WriteType(module, bb, propertyType);
			if (parameterTypes != null)
			{
				for (int i = 0; i < parameterTypes.Length; i++)
				{
					Signature.WriteCustomModifiers(module, bb, customModifiers.GetParameterCustomModifiers(i));
					Signature.WriteType(module, bb, parameterTypes[i]);
				}
			}
		}

		internal Type GetParameter(int parameter)
		{
			return parameterTypes[parameter];
		}

		internal CustomModifiers GetParameterCustomModifiers(int parameter)
		{
			return customModifiers.GetParameterCustomModifiers(parameter);
		}

		internal bool MatchParameterTypes(Type[] types)
		{
			return Util.ArrayEquals(types, parameterTypes);
		}

		internal static PropertySignature ReadSig(ModuleReader module, ByteReader br, IGenericContext context)
		{
			byte num = br.ReadByte();
			if ((num & 8) == 0)
			{
				throw new BadImageFormatException();
			}
			CallingConventions callingConventions = CallingConventions.Standard;
			if ((num & 0x20) != 0)
			{
				callingConventions |= CallingConventions.HasThis;
			}
			if ((num & 0x40) != 0)
			{
				callingConventions |= CallingConventions.ExplicitThis;
			}
			int num2 = br.ReadCompressedUInt();
			CustomModifiers[] array = null;
			PackedCustomModifiers.Pack(ref array, 0, CustomModifiers.Read(module, br, context), num2 + 1);
			Type type = Signature.ReadRetType(module, br, context);
			Type[] array2 = new Type[num2];
			for (int i = 0; i < array2.Length; i++)
			{
				PackedCustomModifiers.Pack(ref array, i + 1, CustomModifiers.Read(module, br, context), num2 + 1);
				array2[i] = Signature.ReadParam(module, br, context);
			}
			return new PropertySignature(callingConventions, type, array2, PackedCustomModifiers.Wrap(array));
		}
	}
}
