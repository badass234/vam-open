namespace Mono.CSharp
{
	public class BuiltinTypes
	{
		public readonly BuiltinTypeSpec Object;

		public readonly BuiltinTypeSpec ValueType;

		public readonly BuiltinTypeSpec Attribute;

		public readonly BuiltinTypeSpec Int;

		public readonly BuiltinTypeSpec UInt;

		public readonly BuiltinTypeSpec Long;

		public readonly BuiltinTypeSpec ULong;

		public readonly BuiltinTypeSpec Float;

		public readonly BuiltinTypeSpec Double;

		public readonly BuiltinTypeSpec Char;

		public readonly BuiltinTypeSpec Short;

		public readonly BuiltinTypeSpec Decimal;

		public readonly BuiltinTypeSpec Bool;

		public readonly BuiltinTypeSpec SByte;

		public readonly BuiltinTypeSpec Byte;

		public readonly BuiltinTypeSpec UShort;

		public readonly BuiltinTypeSpec String;

		public readonly BuiltinTypeSpec Enum;

		public readonly BuiltinTypeSpec Delegate;

		public readonly BuiltinTypeSpec MulticastDelegate;

		public readonly BuiltinTypeSpec Void;

		public readonly BuiltinTypeSpec Array;

		public readonly BuiltinTypeSpec Type;

		public readonly BuiltinTypeSpec IEnumerator;

		public readonly BuiltinTypeSpec IEnumerable;

		public readonly BuiltinTypeSpec IDisposable;

		public readonly BuiltinTypeSpec IntPtr;

		public readonly BuiltinTypeSpec UIntPtr;

		public readonly BuiltinTypeSpec RuntimeFieldHandle;

		public readonly BuiltinTypeSpec RuntimeTypeHandle;

		public readonly BuiltinTypeSpec Exception;

		public readonly BuiltinTypeSpec Dynamic;

		public readonly Binary.PredefinedOperator[] OperatorsBinaryStandard;

		public readonly Binary.PredefinedOperator[] OperatorsBinaryEquality;

		public readonly Binary.PredefinedOperator[] OperatorsBinaryUnsafe;

		public readonly TypeSpec[][] OperatorsUnary;

		public readonly TypeSpec[] OperatorsUnaryMutator;

		public readonly TypeSpec[] BinaryPromotionsTypes;

		private readonly BuiltinTypeSpec[] types;

		public BuiltinTypeSpec[] AllTypes => types;

		public BuiltinTypes()
		{
			Object = new BuiltinTypeSpec(MemberKind.Class, "System", "Object", BuiltinTypeSpec.Type.Object);
			ValueType = new BuiltinTypeSpec(MemberKind.Class, "System", "ValueType", BuiltinTypeSpec.Type.ValueType);
			Attribute = new BuiltinTypeSpec(MemberKind.Class, "System", "Attribute", BuiltinTypeSpec.Type.Attribute);
			Int = new BuiltinTypeSpec(MemberKind.Struct, "System", "Int32", BuiltinTypeSpec.Type.Int);
			Long = new BuiltinTypeSpec(MemberKind.Struct, "System", "Int64", BuiltinTypeSpec.Type.Long);
			UInt = new BuiltinTypeSpec(MemberKind.Struct, "System", "UInt32", BuiltinTypeSpec.Type.UInt);
			ULong = new BuiltinTypeSpec(MemberKind.Struct, "System", "UInt64", BuiltinTypeSpec.Type.ULong);
			Byte = new BuiltinTypeSpec(MemberKind.Struct, "System", "Byte", BuiltinTypeSpec.Type.Byte);
			SByte = new BuiltinTypeSpec(MemberKind.Struct, "System", "SByte", BuiltinTypeSpec.Type.SByte);
			Short = new BuiltinTypeSpec(MemberKind.Struct, "System", "Int16", BuiltinTypeSpec.Type.Short);
			UShort = new BuiltinTypeSpec(MemberKind.Struct, "System", "UInt16", BuiltinTypeSpec.Type.UShort);
			IEnumerator = new BuiltinTypeSpec(MemberKind.Interface, "System.Collections", "IEnumerator", BuiltinTypeSpec.Type.IEnumerator);
			IEnumerable = new BuiltinTypeSpec(MemberKind.Interface, "System.Collections", "IEnumerable", BuiltinTypeSpec.Type.IEnumerable);
			IDisposable = new BuiltinTypeSpec(MemberKind.Interface, "System", "IDisposable", BuiltinTypeSpec.Type.IDisposable);
			Char = new BuiltinTypeSpec(MemberKind.Struct, "System", "Char", BuiltinTypeSpec.Type.Char);
			String = new BuiltinTypeSpec(MemberKind.Class, "System", "String", BuiltinTypeSpec.Type.String);
			Float = new BuiltinTypeSpec(MemberKind.Struct, "System", "Single", BuiltinTypeSpec.Type.Float);
			Double = new BuiltinTypeSpec(MemberKind.Struct, "System", "Double", BuiltinTypeSpec.Type.Double);
			Decimal = new BuiltinTypeSpec(MemberKind.Struct, "System", "Decimal", BuiltinTypeSpec.Type.Decimal);
			Bool = new BuiltinTypeSpec(MemberKind.Struct, "System", "Boolean", BuiltinTypeSpec.Type.FirstPrimitive);
			IntPtr = new BuiltinTypeSpec(MemberKind.Struct, "System", "IntPtr", BuiltinTypeSpec.Type.IntPtr);
			UIntPtr = new BuiltinTypeSpec(MemberKind.Struct, "System", "UIntPtr", BuiltinTypeSpec.Type.UIntPtr);
			MulticastDelegate = new BuiltinTypeSpec(MemberKind.Class, "System", "MulticastDelegate", BuiltinTypeSpec.Type.MulticastDelegate);
			Delegate = new BuiltinTypeSpec(MemberKind.Class, "System", "Delegate", BuiltinTypeSpec.Type.Delegate);
			Enum = new BuiltinTypeSpec(MemberKind.Class, "System", "Enum", BuiltinTypeSpec.Type.Enum);
			Array = new BuiltinTypeSpec(MemberKind.Class, "System", "Array", BuiltinTypeSpec.Type.Array);
			Void = new BuiltinTypeSpec(MemberKind.Void, "System", "Void", BuiltinTypeSpec.Type.Other);
			Type = new BuiltinTypeSpec(MemberKind.Class, "System", "Type", BuiltinTypeSpec.Type.Type);
			Exception = new BuiltinTypeSpec(MemberKind.Class, "System", "Exception", BuiltinTypeSpec.Type.Exception);
			RuntimeFieldHandle = new BuiltinTypeSpec(MemberKind.Struct, "System", "RuntimeFieldHandle", BuiltinTypeSpec.Type.Other);
			RuntimeTypeHandle = new BuiltinTypeSpec(MemberKind.Struct, "System", "RuntimeTypeHandle", BuiltinTypeSpec.Type.Other);
			Dynamic = new BuiltinTypeSpec("dynamic", BuiltinTypeSpec.Type.Dynamic);
			OperatorsBinaryStandard = Binary.CreateStandardOperatorsTable(this);
			OperatorsBinaryEquality = Binary.CreateEqualityOperatorsTable(this);
			OperatorsBinaryUnsafe = Binary.CreatePointerOperatorsTable(this);
			OperatorsUnary = Unary.CreatePredefinedOperatorsTable(this);
			OperatorsUnaryMutator = UnaryMutator.CreatePredefinedOperatorsTable(this);
			BinaryPromotionsTypes = ConstantFold.CreateBinaryPromotionsTypes(this);
			types = new BuiltinTypeSpec[31]
			{
				Object, ValueType, Attribute, Int, UInt, Long, ULong, Float, Double, Char,
				Short, Decimal, Bool, SByte, Byte, UShort, String, Enum, Delegate, MulticastDelegate,
				Void, Array, Type, IEnumerator, IEnumerable, IDisposable, IntPtr, UIntPtr, RuntimeFieldHandle, RuntimeTypeHandle,
				Exception
			};
		}

		public bool CheckDefinitions(ModuleContainer module)
		{
			CompilerContext compiler = module.Compiler;
			BuiltinTypeSpec[] array = types;
			foreach (BuiltinTypeSpec builtinTypeSpec in array)
			{
				TypeSpec typeSpec = PredefinedType.Resolve(module, builtinTypeSpec.Kind, builtinTypeSpec.Namespace, builtinTypeSpec.Name, builtinTypeSpec.Arity, true, true);
				if (typeSpec != null && typeSpec != builtinTypeSpec)
				{
					TypeDefinition typeDefinition = typeSpec.MemberDefinition as TypeDefinition;
					if (typeDefinition != null)
					{
						module.GlobalRootNamespace.GetNamespace(builtinTypeSpec.Namespace, false).SetBuiltinType(builtinTypeSpec);
						typeDefinition.SetPredefinedSpec(builtinTypeSpec);
						builtinTypeSpec.SetDefinition(typeSpec);
					}
				}
			}
			if (compiler.Report.Errors != 0)
			{
				return false;
			}
			Dynamic.SetDefinition(Object);
			return true;
		}
	}
}
