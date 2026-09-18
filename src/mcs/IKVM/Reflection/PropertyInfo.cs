using System;
using System.Collections.Generic;

namespace IKVM.Reflection
{
	public abstract class PropertyInfo : MemberInfo
	{
		private sealed class ParameterInfoImpl : ParameterInfo
		{
			private readonly PropertyInfo property;

			private readonly int parameter;

			public override string Name => null;

			public override Type ParameterType => property.PropertySignature.GetParameter(parameter);

			public override ParameterAttributes Attributes => ParameterAttributes.None;

			public override int Position => parameter;

			public override object RawDefaultValue
			{
				get
				{
					throw new InvalidOperationException();
				}
			}

			public override MemberInfo Member => property;

			public override int MetadataToken => 134217728;

			internal override Module Module => property.Module;

			internal ParameterInfoImpl(PropertyInfo property, int parameter)
			{
				this.property = property;
				this.parameter = parameter;
			}

			public override CustomModifiers __GetCustomModifiers()
			{
				return property.PropertySignature.GetParameterCustomModifiers(parameter);
			}

			public override bool __TryGetFieldMarshal(out FieldMarshal fieldMarshal)
			{
				fieldMarshal = default(FieldMarshal);
				return false;
			}
		}

		public sealed override MemberTypes MemberType => MemberTypes.Property;

		public abstract PropertyAttributes Attributes { get; }

		public abstract bool CanRead { get; }

		public abstract bool CanWrite { get; }

		internal abstract bool IsPublic { get; }

		internal abstract bool IsNonPrivate { get; }

		internal abstract bool IsStatic { get; }

		internal abstract PropertySignature PropertySignature { get; }

		public Type PropertyType => PropertySignature.PropertyType;

		public bool IsSpecialName => (Attributes & PropertyAttributes.SpecialName) != 0;

		public MethodInfo GetMethod => GetGetMethod(true);

		public MethodInfo SetMethod => GetSetMethod(true);

		public CallingConventions __CallingConvention => PropertySignature.CallingConvention;

		internal PropertyInfo()
		{
		}

		public abstract MethodInfo GetGetMethod(bool nonPublic);

		public abstract MethodInfo GetSetMethod(bool nonPublic);

		public abstract MethodInfo[] GetAccessors(bool nonPublic);

		public abstract object GetRawConstantValue();

		public virtual ParameterInfo[] GetIndexParameters()
		{
			ParameterInfo[] array = new ParameterInfo[PropertySignature.ParameterCount];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new ParameterInfoImpl(this, i);
			}
			return array;
		}

		public CustomModifiers __GetCustomModifiers()
		{
			return PropertySignature.GetCustomModifiers();
		}

		public Type[] GetRequiredCustomModifiers()
		{
			return __GetCustomModifiers().GetRequired();
		}

		public Type[] GetOptionalCustomModifiers()
		{
			return __GetCustomModifiers().GetOptional();
		}

		public MethodInfo GetGetMethod()
		{
			return GetGetMethod(false);
		}

		public MethodInfo GetSetMethod()
		{
			return GetSetMethod(false);
		}

		public MethodInfo[] GetAccessors()
		{
			return GetAccessors(false);
		}

		internal virtual PropertyInfo BindTypeParameters(Type type)
		{
			return new GenericPropertyInfo(DeclaringType.BindTypeParameters(type), this);
		}

		public override string ToString()
		{
			return DeclaringType.ToString() + " " + Name;
		}

		internal sealed override bool BindingFlagsMatch(BindingFlags flags)
		{
			if (MemberInfo.BindingFlagsMatch(IsPublic, flags, BindingFlags.Public, BindingFlags.NonPublic))
			{
				return MemberInfo.BindingFlagsMatch(IsStatic, flags, BindingFlags.Static, BindingFlags.Instance);
			}
			return false;
		}

		internal sealed override bool BindingFlagsMatchInherited(BindingFlags flags)
		{
			if (IsNonPrivate && MemberInfo.BindingFlagsMatch(IsPublic, flags, BindingFlags.Public, BindingFlags.NonPublic))
			{
				return MemberInfo.BindingFlagsMatch(IsStatic, flags, BindingFlags.Static | BindingFlags.FlattenHierarchy, BindingFlags.Instance);
			}
			return false;
		}

		internal sealed override MemberInfo SetReflectedType(Type type)
		{
			return new PropertyInfoWithReflectedType(type, this);
		}

		internal sealed override List<CustomAttributeData> GetPseudoCustomAttributes(Type attributeType)
		{
			return null;
		}
	}
}
