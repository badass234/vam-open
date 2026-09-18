namespace IKVM.Reflection.Emit
{
	internal enum KnownCA
	{
		Unknown = 0,
		DllImportAttribute = 1,
		ComImportAttribute = 2,
		SerializableAttribute = 3,
		NonSerializedAttribute = 4,
		MethodImplAttribute = 5,
		MarshalAsAttribute = 6,
		PreserveSigAttribute = 7,
		InAttribute = 8,
		OutAttribute = 9,
		OptionalAttribute = 10,
		StructLayoutAttribute = 11,
		FieldOffsetAttribute = 12,
		SpecialNameAttribute = 13,
		SuppressUnmanagedCodeSecurityAttribute = 14
	}
}
