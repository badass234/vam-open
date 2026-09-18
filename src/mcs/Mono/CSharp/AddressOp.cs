using System;

namespace Mono.CSharp
{
	[Flags]
	public enum AddressOp
	{
		Store = 1,
		Load = 2,
		LoadStore = Store | Load
	}
}
