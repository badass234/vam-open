using System;

namespace IKVM.Reflection
{
	internal sealed class FunctionPointerType : TypeInfo
	{
		private readonly Universe universe;

		private readonly __StandAloneMethodSig sig;

		public override __StandAloneMethodSig __MethodSignature => sig;

		public override Type BaseType => null;

		public override TypeAttributes Attributes => TypeAttributes.AnsiClass;

		public override string Name
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		public override string FullName
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		public override Module Module
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		internal override Universe Universe => universe;

		protected override bool ContainsMissingTypeImpl => sig.ContainsMissingType;

		internal override bool IsBaked => true;

		internal static Type Make(Universe universe, __StandAloneMethodSig sig)
		{
			return universe.CanonicalizeType(new FunctionPointerType(universe, sig));
		}

		private FunctionPointerType(Universe universe, __StandAloneMethodSig sig)
			: base(27)
		{
			this.universe = universe;
			this.sig = sig;
		}

		public override bool Equals(object obj)
		{
			FunctionPointerType functionPointerType = obj as FunctionPointerType;
			if (functionPointerType != null && functionPointerType.universe == universe)
			{
				return functionPointerType.sig.Equals(sig);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return sig.GetHashCode();
		}

		public override string ToString()
		{
			return "<FunctionPtr>";
		}
	}
}
