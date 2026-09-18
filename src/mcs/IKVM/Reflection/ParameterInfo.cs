using System.Collections.Generic;

namespace IKVM.Reflection
{
	public abstract class ParameterInfo : ICustomAttributeProvider
	{
		public abstract string Name { get; }

		public abstract Type ParameterType { get; }

		public abstract ParameterAttributes Attributes { get; }

		public abstract int Position { get; }

		public abstract object RawDefaultValue { get; }

		public abstract MemberInfo Member { get; }

		public abstract int MetadataToken { get; }

		internal abstract Module Module { get; }

		public bool IsIn => (Attributes & ParameterAttributes.In) != 0;

		public bool IsOut => (Attributes & ParameterAttributes.Out) != 0;

		public bool IsLcid => (Attributes & ParameterAttributes.Lcid) != 0;

		public bool IsRetval => (Attributes & ParameterAttributes.Retval) != 0;

		public bool IsOptional => (Attributes & ParameterAttributes.Optional) != 0;

		public bool HasDefaultValue => (Attributes & ParameterAttributes.HasDefault) != 0;

		public IEnumerable<CustomAttributeData> CustomAttributes => GetCustomAttributesData();

		internal ParameterInfo()
		{
		}

		public sealed override bool Equals(object obj)
		{
			ParameterInfo parameterInfo = obj as ParameterInfo;
			if (parameterInfo != null && parameterInfo.Member == Member)
			{
				return parameterInfo.Position == Position;
			}
			return false;
		}

		public sealed override int GetHashCode()
		{
			return Member.GetHashCode() * 1777 + Position;
		}

		public static bool operator ==(ParameterInfo p1, ParameterInfo p2)
		{
			if ((object)p1 != p2)
			{
				return p1?.Equals(p2) ?? false;
			}
			return true;
		}

		public static bool operator !=(ParameterInfo p1, ParameterInfo p2)
		{
			return !(p1 == p2);
		}

		public abstract CustomModifiers __GetCustomModifiers();

		public abstract bool __TryGetFieldMarshal(out FieldMarshal fieldMarshal);

		public Type[] GetOptionalCustomModifiers()
		{
			return __GetCustomModifiers().GetOptional();
		}

		public Type[] GetRequiredCustomModifiers()
		{
			return __GetCustomModifiers().GetRequired();
		}

		public bool IsDefined(Type attributeType, bool inherit)
		{
			return CustomAttributeData.__GetCustomAttributes(this, attributeType, inherit).Count != 0;
		}

		public IList<CustomAttributeData> __GetCustomAttributes(Type attributeType, bool inherit)
		{
			return CustomAttributeData.__GetCustomAttributes(this, attributeType, inherit);
		}

		public IList<CustomAttributeData> GetCustomAttributesData()
		{
			return CustomAttributeData.GetCustomAttributes(this);
		}
	}
}
