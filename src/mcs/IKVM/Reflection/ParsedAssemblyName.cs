namespace IKVM.Reflection
{
	internal struct ParsedAssemblyName
	{
		internal string Name;

		internal string Version;

		internal string Culture;

		internal string PublicKeyToken;

		internal bool? Retargetable;

		internal ProcessorArchitecture ProcessorArchitecture;

		internal bool HasPublicKey;

		internal bool WindowsRuntime;
	}
}
