using System;

namespace IKVM.Reflection
{
	[Flags]
	public enum TypeAttributes
	{
		AnsiClass = 0,
		Class = 0,
		AutoLayout = 0,
		NotPublic = 0,
		Public = 1,
		NestedPublic = 2,
		NestedPrivate = Public | NestedPublic,
		NestedFamily = 4,
		NestedAssembly = Public | NestedFamily,
		NestedFamANDAssem = NestedPublic | NestedFamily,
		VisibilityMask = NestedPrivate | NestedFamily,
		NestedFamORAssem = VisibilityMask,
		SequentialLayout = 8,
		ExplicitLayout = 0x10,
		LayoutMask = SequentialLayout | ExplicitLayout,
		ClassSemanticsMask = 0x20,
		Interface = ClassSemanticsMask,
		Abstract = 0x80,
		Sealed = 0x100,
		SpecialName = 0x400,
		RTSpecialName = 0x800,
		Import = 0x1000,
		Serializable = 0x2000,
		WindowsRuntime = 0x4000,
		UnicodeClass = 0x10000,
		AutoClass = 0x20000,
		CustomFormatClass = UnicodeClass | AutoClass,
		StringFormatMask = CustomFormatClass,
		HasSecurity = 0x40000,
		ReservedMask = RTSpecialName | HasSecurity,
		BeforeFieldInit = 0x100000,
		CustomFormatMask = 0xC00000
	}
}
