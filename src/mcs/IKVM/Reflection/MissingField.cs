using IKVM.Reflection.Emit;

namespace IKVM.Reflection
{
	internal sealed class MissingField : FieldInfo
	{
		private readonly Type declaringType;

		private readonly string name;

		private readonly FieldSignature signature;

		private FieldInfo forwarder;

		private FieldInfo Forwarder
		{
			get
			{
				FieldInfo fieldInfo = TryGetForwarder();
				if (fieldInfo == null)
				{
					throw new MissingMemberException(this);
				}
				return fieldInfo;
			}
		}

		public override bool __IsMissing => TryGetForwarder() == null;

		public override FieldAttributes Attributes => Forwarder.Attributes;

		public override int __FieldRVA => Forwarder.__FieldRVA;

		internal override FieldSignature FieldSignature => signature;

		public override string Name => name;

		public override Type DeclaringType
		{
			get
			{
				if (!declaringType.IsModulePseudoType)
				{
					return declaringType;
				}
				return null;
			}
		}

		public override Module Module => declaringType.Module;

		public override int MetadataToken => Forwarder.MetadataToken;

		internal override bool IsBaked => Forwarder.IsBaked;

		internal MissingField(Type declaringType, string name, FieldSignature signature)
		{
			this.declaringType = declaringType;
			this.name = name;
			this.signature = signature;
		}

		private FieldInfo TryGetForwarder()
		{
			if (forwarder == null && !declaringType.__IsMissing)
			{
				forwarder = declaringType.FindField(name, signature);
			}
			return forwarder;
		}

		public override void __GetDataFromRVA(byte[] data, int offset, int length)
		{
			Forwarder.__GetDataFromRVA(data, offset, length);
		}

		public override bool __TryGetFieldOffset(out int offset)
		{
			return Forwarder.__TryGetFieldOffset(out offset);
		}

		public override object GetRawConstantValue()
		{
			return Forwarder.GetRawConstantValue();
		}

		internal override int ImportTo(ModuleBuilder module)
		{
			FieldInfo fieldInfo = TryGetForwarder();
			if (fieldInfo != null)
			{
				return fieldInfo.ImportTo(module);
			}
			return module.ImportMethodOrField(declaringType, Name, FieldSignature);
		}

		internal override FieldInfo BindTypeParameters(Type type)
		{
			FieldInfo fieldInfo = TryGetForwarder();
			if (fieldInfo != null)
			{
				return fieldInfo.BindTypeParameters(type);
			}
			return new GenericFieldInstance(type, this);
		}

		public override bool Equals(object obj)
		{
			MissingField missingField = obj as MissingField;
			if (missingField != null && missingField.declaringType == declaringType && missingField.name == name)
			{
				return missingField.signature.Equals(signature);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return declaringType.GetHashCode() ^ name.GetHashCode() ^ signature.GetHashCode();
		}

		public override string ToString()
		{
			return base.FieldType.Name + " " + Name;
		}

		internal override int GetCurrentToken()
		{
			return Forwarder.GetCurrentToken();
		}
	}
}
