using System.Runtime.InteropServices;

namespace IKVM.Reflection
{
	public sealed class __StandAloneMethodSig
	{
		private readonly bool unmanaged;

		private readonly CallingConvention unmanagedCallingConvention;

		private readonly CallingConventions callingConvention;

		private readonly Type returnType;

		private readonly Type[] parameterTypes;

		private readonly Type[] optionalParameterTypes;

		private readonly PackedCustomModifiers customModifiers;

		public bool IsUnmanaged => unmanaged;

		public CallingConventions CallingConvention => callingConvention;

		public CallingConvention UnmanagedCallingConvention => unmanagedCallingConvention;

		public Type ReturnType => returnType;

		public Type[] ParameterTypes => Util.Copy(parameterTypes);

		public Type[] OptionalParameterTypes => Util.Copy(optionalParameterTypes);

		public bool ContainsMissingType
		{
			get
			{
				if (!returnType.__ContainsMissingType && !Type.ContainsMissingType(parameterTypes) && !Type.ContainsMissingType(optionalParameterTypes))
				{
					return customModifiers.ContainsMissingType;
				}
				return true;
			}
		}

		internal int ParameterCount => parameterTypes.Length + optionalParameterTypes.Length;

		internal __StandAloneMethodSig(bool unmanaged, CallingConvention unmanagedCallingConvention, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Type[] optionalParameterTypes, PackedCustomModifiers customModifiers)
		{
			this.unmanaged = unmanaged;
			this.unmanagedCallingConvention = unmanagedCallingConvention;
			this.callingConvention = callingConvention;
			this.returnType = returnType;
			this.parameterTypes = parameterTypes;
			this.optionalParameterTypes = optionalParameterTypes;
			this.customModifiers = customModifiers;
		}

		public bool Equals(__StandAloneMethodSig other)
		{
			if (other != null && other.unmanaged == unmanaged && other.unmanagedCallingConvention == unmanagedCallingConvention && other.callingConvention == callingConvention && other.returnType == returnType && Util.ArrayEquals(other.parameterTypes, parameterTypes) && Util.ArrayEquals(other.optionalParameterTypes, optionalParameterTypes))
			{
				return other.customModifiers.Equals(customModifiers);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as __StandAloneMethodSig);
		}

		public override int GetHashCode()
		{
			return returnType.GetHashCode() ^ Util.GetHashCode(parameterTypes);
		}

		public CustomModifiers GetReturnTypeCustomModifiers()
		{
			return customModifiers.GetReturnTypeCustomModifiers();
		}

		public CustomModifiers GetParameterCustomModifiers(int index)
		{
			return customModifiers.GetParameterCustomModifiers(index);
		}
	}
}
