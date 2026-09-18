using System;

namespace IKVM.Reflection
{
	[Flags]
	public enum ExceptionHandlingClauseOptions
	{
		Clause = 0,
		Filter = 1,
		Finally = 2,
		Fault = 4
	}
}
