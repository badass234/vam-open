using System.Collections.Generic;
using IKVM.Reflection.Emit;

namespace IKVM.Reflection
{
	public abstract class FieldInfo : MemberInfo
	{
		public sealed override MemberTypes MemberType => MemberTypes.Field;

		public abstract FieldAttributes Attributes { get; }

		public abstract int __FieldRVA { get; }

		internal abstract FieldSignature FieldSignature { get; }

		public Type FieldType => FieldSignature.FieldType;

		public bool IsStatic => (Attributes & FieldAttributes.Static) != 0;

		public bool IsLiteral => (Attributes & FieldAttributes.Literal) != 0;

		public bool IsInitOnly => (Attributes & FieldAttributes.InitOnly) != 0;

		public bool IsNotSerialized => (Attributes & FieldAttributes.NotSerialized) != 0;

		public bool IsSpecialName => (Attributes & FieldAttributes.SpecialName) != 0;

		public bool IsPublic => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public;

		public bool IsPrivate => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Private;

		public bool IsFamily => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Family;

		public bool IsFamilyOrAssembly => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamORAssem;

		public bool IsAssembly => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Assembly;

		public bool IsFamilyAndAssembly => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamANDAssem;

		public bool IsPinvokeImpl => (Attributes & FieldAttributes.PinvokeImpl) != 0;

		internal FieldInfo()
		{
		}

		public abstract void __GetDataFromRVA(byte[] data, int offset, int length);

		public abstract object GetRawConstantValue();

		public CustomModifiers __GetCustomModifiers()
		{
			return FieldSignature.GetCustomModifiers();
		}

		public Type[] GetOptionalCustomModifiers()
		{
			return __GetCustomModifiers().GetOptional();
		}

		public Type[] GetRequiredCustomModifiers()
		{
			return __GetCustomModifiers().GetRequired();
		}

		public virtual FieldInfo __GetFieldOnTypeDefinition()
		{
			return this;
		}

		public abstract bool __TryGetFieldOffset(out int offset);

		public bool __TryGetFieldMarshal(out FieldMarshal fieldMarshal)
		{
			return FieldMarshal.ReadFieldMarshal(Module, GetCurrentToken(), out fieldMarshal);
		}

		internal abstract int ImportTo(ModuleBuilder module);

		internal virtual FieldInfo BindTypeParameters(Type type)
		{
			return new GenericFieldInstance(DeclaringType.BindTypeParameters(type), this);
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
			if ((Attributes & FieldAttributes.FieldAccessMask) > FieldAttributes.Private && MemberInfo.BindingFlagsMatch(IsPublic, flags, BindingFlags.Public, BindingFlags.NonPublic))
			{
				return MemberInfo.BindingFlagsMatch(IsStatic, flags, BindingFlags.Static | BindingFlags.FlattenHierarchy, BindingFlags.Instance);
			}
			return false;
		}

		internal sealed override MemberInfo SetReflectedType(Type type)
		{
			return new FieldInfoWithReflectedType(type, this);
		}

		internal sealed override List<CustomAttributeData> GetPseudoCustomAttributes(Type attributeType)
		{
			Module module = Module;
			List<CustomAttributeData> list = new List<CustomAttributeData>();
			FieldMarshal fieldMarshal;
			if ((attributeType == null || attributeType.IsAssignableFrom(module.universe.System_Runtime_InteropServices_MarshalAsAttribute)) && __TryGetFieldMarshal(out fieldMarshal))
			{
				list.Add(CustomAttributeData.CreateMarshalAsPseudoCustomAttribute(module, fieldMarshal));
			}
			int offset;
			if ((attributeType == null || attributeType.IsAssignableFrom(module.universe.System_Runtime_InteropServices_FieldOffsetAttribute)) && __TryGetFieldOffset(out offset))
			{
				list.Add(CustomAttributeData.CreateFieldOffsetPseudoCustomAttribute(module, offset));
			}
			return list;
		}
	}
}
