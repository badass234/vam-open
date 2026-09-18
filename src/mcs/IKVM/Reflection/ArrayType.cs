using System;
using System.Collections.Generic;

namespace IKVM.Reflection
{
	internal sealed class ArrayType : ElementHolderType
	{
		public override Type BaseType => elementType.Module.universe.System_Array;

		public override TypeAttributes Attributes => TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Serializable;

		internal static Type Make(Type type, CustomModifiers mods)
		{
			return type.Universe.CanonicalizeType(new ArrayType(type, mods));
		}

		private ArrayType(Type type, CustomModifiers mods)
			: base(type, mods, 29)
		{
		}

		public override Type[] __GetDeclaredInterfaces()
		{
			return new Type[3]
			{
				Module.universe.Import(typeof(IList<>)).MakeGenericType(elementType),
				Module.universe.Import(typeof(ICollection<>)).MakeGenericType(elementType),
				Module.universe.Import(typeof(IEnumerable<>)).MakeGenericType(elementType)
			};
		}

		public override MethodBase[] __GetDeclaredMethods()
		{
			Type[] array = new Type[1] { Module.universe.System_Int32 };
			List<MethodBase> list = new List<MethodBase>();
			list.Add(new BuiltinArrayMethod(Module, this, "Set", CallingConventions.Standard | CallingConventions.HasThis, Module.universe.System_Void, new Type[2]
			{
				Module.universe.System_Int32,
				elementType
			}));
			list.Add(new BuiltinArrayMethod(Module, this, "Address", CallingConventions.Standard | CallingConventions.HasThis, elementType.MakeByRefType(), array));
			list.Add(new BuiltinArrayMethod(Module, this, "Get", CallingConventions.Standard | CallingConventions.HasThis, elementType, array));
			list.Add(new ConstructorInfoImpl(new BuiltinArrayMethod(Module, this, ".ctor", CallingConventions.Standard | CallingConventions.HasThis, Module.universe.System_Void, array)));
			Type type = elementType;
			while (type.__IsVector)
			{
				Array.Resize(ref array, array.Length + 1);
				Type[] array2 = array;
				array2[array2.Length - 1] = array[0];
				list.Add(new ConstructorInfoImpl(new BuiltinArrayMethod(Module, this, ".ctor", CallingConventions.Standard | CallingConventions.HasThis, Module.universe.System_Void, array)));
				type = type.GetElementType();
			}
			return list.ToArray();
		}

		public override int GetArrayRank()
		{
			return 1;
		}

		public override bool Equals(object o)
		{
			return EqualsHelper(o as ArrayType);
		}

		public override int GetHashCode()
		{
			return elementType.GetHashCode() * 5;
		}

		internal override string GetSuffix()
		{
			return "[]";
		}

		protected override Type Wrap(Type type, CustomModifiers mods)
		{
			return Make(type, mods);
		}
	}
}
