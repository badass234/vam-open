using System;

namespace IKVM.Reflection
{
	internal sealed class MarkerType : Type
	{
		internal static readonly Type ModOpt = new MarkerType(32);

		internal static readonly Type ModReq = new MarkerType(31);

		internal static readonly Type Sentinel = new MarkerType(65);

		internal static readonly Type Pinned = new MarkerType(69);

		public override Type BaseType
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		public override TypeAttributes Attributes
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

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

		internal override bool IsBaked
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		public override bool __IsMissing => false;

		private MarkerType(byte sigElementType)
			: base(sigElementType)
		{
		}
	}
}
