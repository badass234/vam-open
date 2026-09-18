namespace IKVM.Reflection
{
	internal struct PackedCustomModifiers
	{
		private readonly CustomModifiers[] customModifiers;

		internal bool ContainsMissingType
		{
			get
			{
				if (customModifiers != null)
				{
					for (int i = 0; i < customModifiers.Length; i++)
					{
						if (customModifiers[i].ContainsMissingType)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		private PackedCustomModifiers(CustomModifiers[] customModifiers)
		{
			this.customModifiers = customModifiers;
		}

		public override int GetHashCode()
		{
			return Util.GetHashCode(customModifiers);
		}

		public override bool Equals(object obj)
		{
			PackedCustomModifiers? packedCustomModifiers = obj as PackedCustomModifiers?;
			if (packedCustomModifiers.HasValue)
			{
				return Equals(packedCustomModifiers.Value);
			}
			return false;
		}

		internal bool Equals(PackedCustomModifiers other)
		{
			return Util.ArrayEquals(customModifiers, other.customModifiers);
		}

		internal CustomModifiers GetReturnTypeCustomModifiers()
		{
			if (customModifiers == null)
			{
				return default(CustomModifiers);
			}
			return customModifiers[0];
		}

		internal CustomModifiers GetParameterCustomModifiers(int index)
		{
			if (customModifiers == null)
			{
				return default(CustomModifiers);
			}
			return customModifiers[index + 1];
		}

		internal PackedCustomModifiers Bind(IGenericBinder binder)
		{
			if (customModifiers == null)
			{
				return default(PackedCustomModifiers);
			}
			CustomModifiers[] array = new CustomModifiers[customModifiers.Length];
			for (int i = 0; i < customModifiers.Length; i++)
			{
				array[i] = customModifiers[i].Bind(binder);
			}
			return new PackedCustomModifiers(array);
		}

		internal static PackedCustomModifiers CreateFromExternal(Type[] returnOptional, Type[] returnRequired, Type[][] parameterOptional, Type[][] parameterRequired, int parameterCount)
		{
			CustomModifiers[] array = null;
			Pack(ref array, 0, CustomModifiers.FromReqOpt(returnRequired, returnOptional), parameterCount + 1);
			for (int i = 0; i < parameterCount; i++)
			{
				Pack(ref array, i + 1, CustomModifiers.FromReqOpt(Util.NullSafeElementAt(parameterRequired, i), Util.NullSafeElementAt(parameterOptional, i)), parameterCount + 1);
			}
			return new PackedCustomModifiers(array);
		}

		internal static PackedCustomModifiers CreateFromExternal(CustomModifiers returnTypeCustomModifiers, CustomModifiers[] parameterTypeCustomModifiers, int parameterCount)
		{
			CustomModifiers[] array = null;
			Pack(ref array, 0, returnTypeCustomModifiers, parameterCount + 1);
			if (parameterTypeCustomModifiers != null)
			{
				for (int i = 0; i < parameterCount; i++)
				{
					Pack(ref array, i + 1, parameterTypeCustomModifiers[i], parameterCount + 1);
				}
			}
			return new PackedCustomModifiers(array);
		}

		internal static PackedCustomModifiers Wrap(CustomModifiers[] modifiers)
		{
			return new PackedCustomModifiers(modifiers);
		}

		internal static void Pack(ref CustomModifiers[] array, int index, CustomModifiers mods, int count)
		{
			if (!mods.IsEmpty)
			{
				if (array == null)
				{
					array = new CustomModifiers[count];
				}
				array[index] = mods;
			}
		}
	}
}
