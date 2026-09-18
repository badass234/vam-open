using System;

namespace Mono.CSharp
{
	[Flags]
	public enum MemberKind
	{
		Constructor = 1,
		Event = 2,
		Field = 4,
		Method = 8,
		Property = 0x10,
		Indexer = 0x20,
		Operator = 0x40,
		Destructor = 0x80,
		Class = 0x800,
		Struct = 0x1000,
		Delegate = 0x2000,
		Enum = 0x4000,
		Interface = 0x8000,
		TypeParameter = 0x10000,
		ArrayType = 0x80000,
		PointerType = 0x100000,
		InternalCompilerType = 0x200000,
		MissingType = 0x400000,
		Void = 0x800000,
		Namespace = 0x1000000,
		NestedMask = Class | Struct | Delegate | Enum | Interface,
		GenericMask = Method | Class | Struct | Delegate | Interface,
		MaskType = NestedMask | Constructor | Event | Field | Method | Property | Indexer | Operator | Destructor
	}
}
