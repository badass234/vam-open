using System;
using System.Runtime.Serialization;

namespace IKVM.Reflection
{
	[Serializable]
	public sealed class MissingAssemblyException : InvalidOperationException
	{
		[NonSerialized]
		private readonly MissingAssembly assembly;

		public Assembly Assembly => assembly;

		internal MissingAssemblyException(MissingAssembly assembly)
			: base("Assembly '" + assembly.FullName + "' is a missing assembly and does not support the requested operation.")
		{
			this.assembly = assembly;
		}

		private MissingAssemblyException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
