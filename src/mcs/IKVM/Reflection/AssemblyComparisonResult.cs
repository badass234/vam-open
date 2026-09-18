namespace IKVM.Reflection
{
	public enum AssemblyComparisonResult
	{
		Unknown = 0,
		EquivalentFullMatch = 1,
		EquivalentWeakNamed = 2,
		EquivalentFXUnified = 3,
		EquivalentUnified = 4,
		NonEquivalentVersion = 5,
		NonEquivalent = 6,
		EquivalentPartialMatch = 7,
		EquivalentPartialWeakNamed = 8,
		EquivalentPartialUnified = 9,
		EquivalentPartialFXUnified = 10,
		NonEquivalentPartialVersion = 11
	}
}
