using IKVM.Reflection.Emit;

namespace IKVM.Reflection
{
	internal sealed class GenericFieldInstance : FieldInfo
	{
		private readonly Type declaringType;

		private readonly FieldInfo field;

		public override FieldAttributes Attributes => field.Attributes;

		public override string Name => field.Name;

		public override Type DeclaringType => declaringType;

		public override Module Module => declaringType.Module;

		public override int MetadataToken => field.MetadataToken;

		public override int __FieldRVA => field.__FieldRVA;

		internal override FieldSignature FieldSignature => field.FieldSignature.ExpandTypeParameters(declaringType);

		internal override bool IsBaked => field.IsBaked;

		internal GenericFieldInstance(Type declaringType, FieldInfo field)
		{
			this.declaringType = declaringType;
			this.field = field;
		}

		public override bool Equals(object obj)
		{
			GenericFieldInstance genericFieldInstance = obj as GenericFieldInstance;
			if (genericFieldInstance != null && genericFieldInstance.declaringType.Equals(declaringType))
			{
				return genericFieldInstance.field.Equals(field);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (declaringType.GetHashCode() * 3) ^ field.GetHashCode();
		}

		public override object GetRawConstantValue()
		{
			return field.GetRawConstantValue();
		}

		public override void __GetDataFromRVA(byte[] data, int offset, int length)
		{
			field.__GetDataFromRVA(data, offset, length);
		}

		public override bool __TryGetFieldOffset(out int offset)
		{
			return field.__TryGetFieldOffset(out offset);
		}

		public override FieldInfo __GetFieldOnTypeDefinition()
		{
			return field;
		}

		internal override int ImportTo(ModuleBuilder module)
		{
			return module.ImportMethodOrField(declaringType, field.Name, field.FieldSignature);
		}

		internal override FieldInfo BindTypeParameters(Type type)
		{
			return new GenericFieldInstance(declaringType.BindTypeParameters(type), field);
		}

		internal override int GetCurrentToken()
		{
			return field.GetCurrentToken();
		}
	}
}
