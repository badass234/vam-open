using System;

namespace Mono.CSharp
{
	public sealed class BuiltinTypeSpec : TypeSpec
	{
		public enum Type
		{
			None = 0,
			FirstPrimitive = 1,
			Bool = FirstPrimitive,
			Byte = 2,
			SByte = 3,
			Char = 4,
			Short = 5,
			UShort = 6,
			Int = 7,
			UInt = 8,
			Long = 9,
			ULong = 10,
			Float = 11,
			Double = 12,
			LastPrimitive = Double,
			Decimal = 13,
			IntPtr = 14,
			UIntPtr = 15,
			Object = 16,
			Dynamic = 17,
			String = 18,
			Type = 19,
			ValueType = 20,
			Enum = 21,
			Delegate = 22,
			MulticastDelegate = 23,
			Array = 24,
			IEnumerator = 25,
			IEnumerable = 26,
			IDisposable = 27,
			Exception = 28,
			Attribute = 29,
			Other = 30
		}

		private readonly Type type;

		private readonly string ns;

		private readonly string name;

		public override int Arity => 0;

		public override Type BuiltinType => type;

		public string FullName => ns + "." + name;

		public override string Name => name;

		public string Namespace => ns;

		public BuiltinTypeSpec(MemberKind kind, string ns, string name, Type builtinKind)
			: base(kind, null, null, null, Modifiers.PUBLIC)
		{
			type = builtinKind;
			this.ns = ns;
			this.name = name;
		}

		public BuiltinTypeSpec(string name, Type builtinKind)
			: this(MemberKind.InternalCompilerType, "", name, builtinKind)
		{
			state = (state & ~(StateFlags.Obsolete_Undetected | StateFlags.CLSCompliant_Undetected | StateFlags.MissingDependency_Undetected)) | StateFlags.CLSCompliant;
		}

		public static bool IsPrimitiveType(TypeSpec type)
		{
			if (type.BuiltinType >= Type.FirstPrimitive)
			{
				return type.BuiltinType <= Type.Double;
			}
			return false;
		}

		public static bool IsPrimitiveTypeOrDecimal(TypeSpec type)
		{
			if (type.BuiltinType >= Type.FirstPrimitive)
			{
				return type.BuiltinType <= Type.Decimal;
			}
			return false;
		}

		public override string GetSignatureForError()
		{
			switch (Name)
			{
			case "Int32":
				return "int";
			case "Int64":
				return "long";
			case "String":
				return "string";
			case "Boolean":
				return "bool";
			case "Void":
				return "void";
			case "Object":
				return "object";
			case "UInt32":
				return "uint";
			case "Int16":
				return "short";
			case "UInt16":
				return "ushort";
			case "UInt64":
				return "ulong";
			case "Single":
				return "float";
			case "Double":
				return "double";
			case "Decimal":
				return "decimal";
			case "Char":
				return "char";
			case "Byte":
				return "byte";
			case "SByte":
				return "sbyte";
			default:
				if (ns.Length == 0)
				{
					return name;
				}
				return FullName;
			}
		}

		public static int GetSize(TypeSpec type)
		{
			switch (type.BuiltinType)
			{
			case Type.Int:
			case Type.UInt:
			case Type.Float:
				return 4;
			case Type.Long:
			case Type.ULong:
			case Type.Double:
				return 8;
			case Type.FirstPrimitive:
			case Type.Byte:
			case Type.SByte:
				return 1;
			case Type.Char:
			case Type.Short:
			case Type.UShort:
				return 2;
			case Type.Decimal:
				return 16;
			default:
				return 0;
			}
		}

		public void SetDefinition(ITypeDefinition td, System.Type type, Modifiers mod)
		{
			definition = td;
			info = type;
			modifiers |= mod & ~Modifiers.AccessibilityMask;
		}

		public void SetDefinition(TypeSpec ts)
		{
			definition = ts.MemberDefinition;
			info = ts.GetMetaInfo();
			BaseType = ts.BaseType;
			Interfaces = ts.Interfaces;
			modifiers = ts.Modifiers;
		}
	}
}
