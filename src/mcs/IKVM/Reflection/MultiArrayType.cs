namespace IKVM.Reflection
{
	internal sealed class MultiArrayType : ElementHolderType
	{
		private readonly int rank;

		private readonly int[] sizes;

		private readonly int[] lobounds;

		public override Type BaseType => elementType.Module.universe.System_Array;

		public override TypeAttributes Attributes => TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Serializable;

		internal static Type Make(Type type, int rank, int[] sizes, int[] lobounds, CustomModifiers mods)
		{
			return type.Universe.CanonicalizeType(new MultiArrayType(type, rank, sizes, lobounds, mods));
		}

		private MultiArrayType(Type type, int rank, int[] sizes, int[] lobounds, CustomModifiers mods)
			: base(type, mods, 20)
		{
			this.rank = rank;
			this.sizes = sizes;
			this.lobounds = lobounds;
		}

		public override MethodBase[] __GetDeclaredMethods()
		{
			Type system_Int = Module.universe.System_Int32;
			Type[] array = new Type[rank + 1];
			Type[] array2 = new Type[rank];
			Type[] array3 = new Type[rank * 2];
			for (int i = 0; i < rank; i++)
			{
				array[i] = system_Int;
				array2[i] = system_Int;
				array3[i * 2 + 0] = system_Int;
				array3[i * 2 + 1] = system_Int;
			}
			array[rank] = elementType;
			return new MethodBase[5]
			{
				new ConstructorInfoImpl(new BuiltinArrayMethod(Module, this, ".ctor", CallingConventions.Standard | CallingConventions.HasThis, Module.universe.System_Void, array2)),
				new ConstructorInfoImpl(new BuiltinArrayMethod(Module, this, ".ctor", CallingConventions.Standard | CallingConventions.HasThis, Module.universe.System_Void, array3)),
				new BuiltinArrayMethod(Module, this, "Set", CallingConventions.Standard | CallingConventions.HasThis, Module.universe.System_Void, array),
				new BuiltinArrayMethod(Module, this, "Address", CallingConventions.Standard | CallingConventions.HasThis, elementType.MakeByRefType(), array2),
				new BuiltinArrayMethod(Module, this, "Get", CallingConventions.Standard | CallingConventions.HasThis, elementType, array2)
			};
		}

		public override int GetArrayRank()
		{
			return rank;
		}

		public override int[] __GetArraySizes()
		{
			return Util.Copy(sizes);
		}

		public override int[] __GetArrayLowerBounds()
		{
			return Util.Copy(lobounds);
		}

		public override bool Equals(object o)
		{
			MultiArrayType multiArrayType = o as MultiArrayType;
			if (EqualsHelper(multiArrayType) && multiArrayType.rank == rank && ArrayEquals(multiArrayType.sizes, sizes))
			{
				return ArrayEquals(multiArrayType.lobounds, lobounds);
			}
			return false;
		}

		private static bool ArrayEquals(int[] i1, int[] i2)
		{
			if (i1.Length == i2.Length)
			{
				for (int j = 0; j < i1.Length; j++)
				{
					if (i1[j] != i2[j])
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return elementType.GetHashCode() * 9 + rank;
		}

		internal override string GetSuffix()
		{
			if (rank == 1)
			{
				return "[*]";
			}
			return "[" + new string(',', rank - 1) + "]";
		}

		protected override Type Wrap(Type type, CustomModifiers mods)
		{
			return Make(type, rank, sizes, lobounds, mods);
		}
	}
}
