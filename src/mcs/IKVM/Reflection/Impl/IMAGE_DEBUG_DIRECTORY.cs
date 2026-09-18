namespace IKVM.Reflection.Impl
{
	internal struct IMAGE_DEBUG_DIRECTORY
	{
		public uint Characteristics;

		public uint TimeDateStamp;

		public ushort MajorVersion;

		public ushort MinorVersion;

		public uint Type;

		public uint SizeOfData;

		public uint AddressOfRawData;

		public uint PointerToRawData;
	}
}
