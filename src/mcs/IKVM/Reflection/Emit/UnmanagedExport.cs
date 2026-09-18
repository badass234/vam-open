namespace IKVM.Reflection.Emit
{
	internal struct UnmanagedExport
	{
		internal string name;

		internal int ordinal;

		internal RelativeVirtualAddress rva;

		internal MethodBuilder mb;
	}
}
