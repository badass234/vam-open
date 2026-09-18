namespace IKVM.Reflection.Reader
{
	internal sealed class PropertyInfoImpl : PropertyInfo
	{
		private readonly ModuleReader module;

		private readonly Type declaringType;

		private readonly int index;

		private PropertySignature sig;

		private bool isPublic;

		private bool isNonPrivate;

		private bool isStatic;

		private bool flagsCached;

		internal override PropertySignature PropertySignature
		{
			get
			{
				if (sig == null)
				{
					sig = PropertySignature.ReadSig(module, module.GetBlob(module.Property.records[index].Type), declaringType);
				}
				return sig;
			}
		}

		public override PropertyAttributes Attributes => (PropertyAttributes)module.Property.records[index].Flags;

		public override bool CanRead => GetGetMethod(true) != null;

		public override bool CanWrite => GetSetMethod(true) != null;

		public override Type DeclaringType => declaringType;

		public override Module Module => module;

		public override int MetadataToken => (23 << 24) + index + 1;

		public override string Name => module.GetString(module.Property.records[index].Name);

		internal override bool IsPublic
		{
			get
			{
				if (!flagsCached)
				{
					ComputeFlags();
				}
				return isPublic;
			}
		}

		internal override bool IsNonPrivate
		{
			get
			{
				if (!flagsCached)
				{
					ComputeFlags();
				}
				return isNonPrivate;
			}
		}

		internal override bool IsStatic
		{
			get
			{
				if (!flagsCached)
				{
					ComputeFlags();
				}
				return isStatic;
			}
		}

		internal override bool IsBaked => true;

		internal PropertyInfoImpl(ModuleReader module, Type declaringType, int index)
		{
			this.module = module;
			this.declaringType = declaringType;
			this.index = index;
		}

		public override bool Equals(object obj)
		{
			PropertyInfoImpl propertyInfoImpl = obj as PropertyInfoImpl;
			if (propertyInfoImpl != null && propertyInfoImpl.DeclaringType == declaringType)
			{
				return propertyInfoImpl.index == index;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return declaringType.GetHashCode() * 77 + index;
		}

		public override object GetRawConstantValue()
		{
			return module.Constant.GetRawConstantValue(module, MetadataToken);
		}

		public override MethodInfo GetGetMethod(bool nonPublic)
		{
			return module.MethodSemantics.GetMethod(module, MetadataToken, nonPublic, 2);
		}

		public override MethodInfo GetSetMethod(bool nonPublic)
		{
			return module.MethodSemantics.GetMethod(module, MetadataToken, nonPublic, 1);
		}

		public override MethodInfo[] GetAccessors(bool nonPublic)
		{
			return module.MethodSemantics.GetMethods(module, MetadataToken, nonPublic, 7);
		}

		private void ComputeFlags()
		{
			module.MethodSemantics.ComputeFlags(module, MetadataToken, out isPublic, out isNonPrivate, out isStatic);
			flagsCached = true;
		}

		internal override int GetCurrentToken()
		{
			return MetadataToken;
		}
	}
}
