using IKVM.Reflection.Emit;

namespace IKVM.Reflection
{
	internal sealed class FieldInfoWithReflectedType : FieldInfo
	{
		private readonly Type reflectedType;

		private readonly FieldInfo field;

		public override FieldAttributes Attributes => field.Attributes;

		public override int __FieldRVA => field.__FieldRVA;

		internal override FieldSignature FieldSignature => field.FieldSignature;

		public override bool __IsMissing => field.__IsMissing;

		public override Type DeclaringType => field.DeclaringType;

		public override Type ReflectedType => reflectedType;

		public override int MetadataToken => field.MetadataToken;

		public override Module Module => field.Module;

		public override string Name => field.Name;

		internal override bool IsBaked => field.IsBaked;

		internal FieldInfoWithReflectedType(Type reflectedType, FieldInfo field)
		{
			this.reflectedType = reflectedType;
			this.field = field;
		}

		public override void __GetDataFromRVA(byte[] data, int offset, int length)
		{
			field.__GetDataFromRVA(data, offset, length);
		}

		public override bool __TryGetFieldOffset(out int offset)
		{
			return field.__TryGetFieldOffset(out offset);
		}

		public override object GetRawConstantValue()
		{
			return field.GetRawConstantValue();
		}

		public override FieldInfo __GetFieldOnTypeDefinition()
		{
			return field.__GetFieldOnTypeDefinition();
		}

		internal override int ImportTo(ModuleBuilder module)
		{
			return field.ImportTo(module);
		}

		internal override FieldInfo BindTypeParameters(Type type)
		{
			return field.BindTypeParameters(type);
		}

		public override bool Equals(object obj)
		{
			FieldInfoWithReflectedType fieldInfoWithReflectedType = obj as FieldInfoWithReflectedType;
			if (fieldInfoWithReflectedType != null && fieldInfoWithReflectedType.reflectedType == reflectedType)
			{
				return fieldInfoWithReflectedType.field == field;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return reflectedType.GetHashCode() ^ field.GetHashCode();
		}

		public override string ToString()
		{
			return field.ToString();
		}

		internal override int GetCurrentToken()
		{
			return field.GetCurrentToken();
		}
	}
}
