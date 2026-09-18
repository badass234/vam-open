namespace IKVM.Reflection
{
	internal sealed class GenericPropertyInfo : PropertyInfo
	{
		private readonly Type typeInstance;

		private readonly PropertyInfo property;

		public override PropertyAttributes Attributes => property.Attributes;

		public override bool CanRead => property.CanRead;

		public override bool CanWrite => property.CanWrite;

		internal override bool IsPublic => property.IsPublic;

		internal override bool IsNonPrivate => property.IsNonPrivate;

		internal override bool IsStatic => property.IsStatic;

		internal override PropertySignature PropertySignature => property.PropertySignature.ExpandTypeParameters(typeInstance);

		public override string Name => property.Name;

		public override Type DeclaringType => typeInstance;

		public override Module Module => typeInstance.Module;

		public override int MetadataToken => property.MetadataToken;

		internal override bool IsBaked => property.IsBaked;

		internal GenericPropertyInfo(Type typeInstance, PropertyInfo property)
		{
			this.typeInstance = typeInstance;
			this.property = property;
		}

		public override bool Equals(object obj)
		{
			GenericPropertyInfo genericPropertyInfo = obj as GenericPropertyInfo;
			if (genericPropertyInfo != null && genericPropertyInfo.typeInstance == typeInstance)
			{
				return genericPropertyInfo.property == property;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return typeInstance.GetHashCode() * 537 + property.GetHashCode();
		}

		private MethodInfo Wrap(MethodInfo method)
		{
			if (method == null)
			{
				return null;
			}
			return new GenericMethodInstance(typeInstance, method, null);
		}

		public override MethodInfo GetGetMethod(bool nonPublic)
		{
			return Wrap(property.GetGetMethod(nonPublic));
		}

		public override MethodInfo GetSetMethod(bool nonPublic)
		{
			return Wrap(property.GetSetMethod(nonPublic));
		}

		public override MethodInfo[] GetAccessors(bool nonPublic)
		{
			MethodInfo[] accessors = property.GetAccessors(nonPublic);
			for (int i = 0; i < accessors.Length; i++)
			{
				accessors[i] = Wrap(accessors[i]);
			}
			return accessors;
		}

		public override object GetRawConstantValue()
		{
			return property.GetRawConstantValue();
		}

		internal override PropertyInfo BindTypeParameters(Type type)
		{
			return new GenericPropertyInfo(typeInstance.BindTypeParameters(type), property);
		}

		internal override int GetCurrentToken()
		{
			return property.GetCurrentToken();
		}
	}
}
