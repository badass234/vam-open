using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection
{
	internal sealed class FieldSignature : Signature
	{
		private readonly Type fieldType;

		private readonly CustomModifiers mods;

		internal Type FieldType => fieldType;

		internal static FieldSignature Create(Type fieldType, CustomModifiers customModifiers)
		{
			return new FieldSignature(fieldType, customModifiers);
		}

		private FieldSignature(Type fieldType, CustomModifiers mods)
		{
			this.fieldType = fieldType;
			this.mods = mods;
		}

		public override bool Equals(object obj)
		{
			FieldSignature fieldSignature = obj as FieldSignature;
			if (fieldSignature != null && fieldSignature.fieldType.Equals(fieldType))
			{
				return fieldSignature.mods.Equals(mods);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return fieldType.GetHashCode() ^ mods.GetHashCode();
		}

		internal CustomModifiers GetCustomModifiers()
		{
			return mods;
		}

		internal FieldSignature ExpandTypeParameters(Type declaringType)
		{
			return new FieldSignature(fieldType.BindTypeParameters(declaringType), mods.Bind(declaringType));
		}

		internal static FieldSignature ReadSig(ModuleReader module, ByteReader br, IGenericContext context)
		{
			if (br.ReadByte() != 6)
			{
				throw new BadImageFormatException();
			}
			CustomModifiers customModifiers = CustomModifiers.Read(module, br, context);
			return new FieldSignature(Signature.ReadType(module, br, context), customModifiers);
		}

		internal override void WriteSig(ModuleBuilder module, ByteBuffer bb)
		{
			bb.Write((byte)6);
			Signature.WriteCustomModifiers(module, bb, mods);
			Signature.WriteType(module, bb, fieldType);
		}
	}
}
