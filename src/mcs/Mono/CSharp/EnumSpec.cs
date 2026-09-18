using System;

namespace Mono.CSharp
{
	internal class EnumSpec : TypeSpec
	{
		private TypeSpec underlying;

		public TypeSpec UnderlyingType
		{
			get
			{
				return underlying;
			}
			set
			{
				if (underlying != null)
				{
					throw new InternalErrorException("UnderlyingType reset");
				}
				underlying = value;
			}
		}

		public EnumSpec(TypeSpec declaringType, ITypeDefinition definition, TypeSpec underlyingType, Type info, Modifiers modifiers)
			: base(MemberKind.Enum, declaringType, definition, info, modifiers | Modifiers.SEALED)
		{
			underlying = underlyingType;
		}

		public static TypeSpec GetUnderlyingType(TypeSpec t)
		{
			return ((EnumSpec)t.GetDefinition()).UnderlyingType;
		}

		public static bool IsValidUnderlyingType(TypeSpec type)
		{
			switch (type.BuiltinType)
			{
			case BuiltinTypeSpec.Type.Byte:
			case BuiltinTypeSpec.Type.SByte:
			case BuiltinTypeSpec.Type.Short:
			case BuiltinTypeSpec.Type.UShort:
			case BuiltinTypeSpec.Type.Int:
			case BuiltinTypeSpec.Type.UInt:
			case BuiltinTypeSpec.Type.Long:
			case BuiltinTypeSpec.Type.ULong:
				return true;
			default:
				return false;
			}
		}
	}
}
