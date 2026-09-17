namespace MeshVR
{
	/// <summary>
	/// Who this build is, so that the version has one home instead of a literal in the version text.
	/// Kept out of the decompiled sources on purpose: nothing here came from the original assembly.
	/// Keep <see cref="Version"/> in step with CHANGELOG.md.
	/// </summary>
	public static class VaMOpenBuild
	{
		public const string Name = "VaMOpen";

		public const string Version = "0.5.0-alpha";

		public const string Label = Name + " " + Version;
	}
}
