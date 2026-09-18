using System;

namespace IKVM.Reflection.Writer
{
	internal struct OrdinalOrName
	{
		internal readonly ushort Ordinal;

		internal readonly string Name;

		internal OrdinalOrName(ushort value)
		{
			Ordinal = value;
			Name = null;
		}

		internal OrdinalOrName(string value)
		{
			Ordinal = ushort.MaxValue;
			Name = value;
		}

		internal bool IsGreaterThan(OrdinalOrName other)
		{
			if (Name != null)
			{
				return string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase) > 0;
			}
			return Ordinal > other.Ordinal;
		}

		internal bool IsEqual(OrdinalOrName other)
		{
			if (Name != null)
			{
				return string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase) == 0;
			}
			return Ordinal == other.Ordinal;
		}
	}
}
