using System;

namespace IKVM.Reflection
{
	[Flags]
	public enum UniverseOptions
	{
		None = 0,
		EnableFunctionPointers = 1,
		DisableFusion = 2,
		DisablePseudoCustomAttributeRetrieval = 4,
		DontProvideAutomaticDefaultConstructor = 8,
		MetadataOnly = 0x10,
		ResolveMissingMembers = 0x20,
		DisableWindowsRuntimeProjection = 0x40,
		DecodeVersionInfoAttributeBlobs = 0x80,
		DeterministicOutput = 0x100
	}
}
