using System;
using Mono.Cecil;

namespace DynamicCSharp.Security
{
	[Serializable]
	public abstract class Restriction
	{
		public abstract string Message { get; }

		public abstract RestrictionMode Mode { get; }

		public abstract bool Verify(ModuleDefinition module);
	}
}
