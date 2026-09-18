namespace IKVM.Reflection
{
	internal sealed class PropertyInfoWithReflectedType : PropertyInfo
	{
		private readonly Type reflectedType;

		private readonly PropertyInfo property;

		public override PropertyAttributes Attributes => property.Attributes;

		public override bool CanRead => property.CanRead;

		public override bool CanWrite => property.CanWrite;

		internal override bool IsPublic => property.IsPublic;

		internal override bool IsNonPrivate => property.IsNonPrivate;

		internal override bool IsStatic => property.IsStatic;

		internal override PropertySignature PropertySignature => property.PropertySignature;

		public override bool __IsMissing => property.__IsMissing;

		public override Type DeclaringType => property.DeclaringType;

		public override Type ReflectedType => reflectedType;

		public override int MetadataToken => property.MetadataToken;

		public override Module Module => property.Module;

		public override string Name => property.Name;

		internal override bool IsBaked => property.IsBaked;

		internal PropertyInfoWithReflectedType(Type reflectedType, PropertyInfo property)
		{
			this.reflectedType = reflectedType;
			this.property = property;
		}

		public override MethodInfo GetGetMethod(bool nonPublic)
		{
			return MemberInfo.SetReflectedType(property.GetGetMethod(nonPublic), reflectedType);
		}

		public override MethodInfo GetSetMethod(bool nonPublic)
		{
			return MemberInfo.SetReflectedType(property.GetSetMethod(nonPublic), reflectedType);
		}

		public override MethodInfo[] GetAccessors(bool nonPublic)
		{
			return MemberInfo.SetReflectedType(property.GetAccessors(nonPublic), reflectedType);
		}

		public override object GetRawConstantValue()
		{
			return property.GetRawConstantValue();
		}

		public override ParameterInfo[] GetIndexParameters()
		{
			ParameterInfo[] indexParameters = property.GetIndexParameters();
			for (int i = 0; i < indexParameters.Length; i++)
			{
				indexParameters[i] = new ParameterInfoWrapper(this, indexParameters[i]);
			}
			return indexParameters;
		}

		internal override PropertyInfo BindTypeParameters(Type type)
		{
			return property.BindTypeParameters(type);
		}

		public override string ToString()
		{
			return property.ToString();
		}

		public override bool Equals(object obj)
		{
			PropertyInfoWithReflectedType propertyInfoWithReflectedType = obj as PropertyInfoWithReflectedType;
			if (propertyInfoWithReflectedType != null && propertyInfoWithReflectedType.reflectedType == reflectedType)
			{
				return propertyInfoWithReflectedType.property == property;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return reflectedType.GetHashCode() ^ property.GetHashCode();
		}

		internal override int GetCurrentToken()
		{
			return property.GetCurrentToken();
		}
	}
}
