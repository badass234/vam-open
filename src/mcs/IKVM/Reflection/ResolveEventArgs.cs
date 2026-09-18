using System;

namespace IKVM.Reflection
{
	public sealed class ResolveEventArgs : EventArgs
	{
		private readonly string name;

		private readonly Assembly requestingAssembly;

		public string Name => name;

		public Assembly RequestingAssembly => requestingAssembly;

		public ResolveEventArgs(string name)
			: this(name, null)
		{
		}

		public ResolveEventArgs(string name, Assembly requestingAssembly)
		{
			this.name = name;
			this.requestingAssembly = requestingAssembly;
		}
	}
}
