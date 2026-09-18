using System;

namespace Mono.CSharp
{
	[Flags]
	public enum ResolveFlags
	{
		VariableOrValue = 1,
		Type = 2,
		MethodGroup = 4,
		TypeParameter = 8,
		MaskExprClass = VariableOrValue | Type | MethodGroup | TypeParameter
	}
}
