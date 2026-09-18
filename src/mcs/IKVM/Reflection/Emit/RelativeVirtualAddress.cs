namespace IKVM.Reflection.Emit
{
	public struct RelativeVirtualAddress
	{
		internal readonly uint initializedDataOffset;

		internal RelativeVirtualAddress(uint initializedDataOffset)
		{
			this.initializedDataOffset = initializedDataOffset;
		}

		public static RelativeVirtualAddress operator +(RelativeVirtualAddress rva, int offset)
		{
			return new RelativeVirtualAddress(rva.initializedDataOffset + (uint)offset);
		}
	}
}
