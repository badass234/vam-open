using System;

namespace IKVM.Reflection
{
	[Flags]
	public enum FieldAttributes
	{
		PrivateScope = 0,
		Private = 1,
		FamANDAssem = 2,
		Assembly = Private | FamANDAssem,
		Family = 4,
		FamORAssem = Private | Family,
		Public = FamANDAssem | Family,
		FieldAccessMask = Assembly | Family,
		Static = 0x10,
		InitOnly = 0x20,
		Literal = 0x40,
		NotSerialized = 0x80,
		HasFieldRVA = 0x100,
		SpecialName = 0x200,
		RTSpecialName = 0x400,
		HasFieldMarshal = 0x1000,
		PinvokeImpl = 0x2000,
		HasDefault = 0x8000,
		ReservedMask = HasFieldRVA | RTSpecialName | HasFieldMarshal | HasDefault
	}
}
