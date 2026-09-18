using System;
using System.Runtime.Serialization;

namespace IKVM.Reflection
{
	[Serializable]
	public sealed class MissingModuleException : InvalidOperationException
	{
		[NonSerialized]
		private readonly MissingModule module;

		public Module Module => module;

		internal MissingModuleException(MissingModule module)
			: base("Module from missing assembly '" + module.Assembly.FullName + "' does not support the requested operation.")
		{
			this.module = module;
		}

		private MissingModuleException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
