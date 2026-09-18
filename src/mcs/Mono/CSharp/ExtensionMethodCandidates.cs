using System.Collections.Generic;

namespace Mono.CSharp
{
	public class ExtensionMethodCandidates
	{
		private readonly NamespaceContainer container;

		private readonly IList<MethodSpec> methods;

		private readonly int index;

		private readonly IMemberContext context;

		public NamespaceContainer Container => container;

		public IMemberContext Context => context;

		public int LookupIndex => index;

		public IList<MethodSpec> Methods => methods;

		public ExtensionMethodCandidates(IMemberContext context, IList<MethodSpec> methods, NamespaceContainer nsContainer, int lookupIndex)
		{
			this.context = context;
			this.methods = methods;
			container = nsContainer;
			index = lookupIndex;
		}
	}
}
