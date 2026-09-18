using IKVM.Reflection.Reader;

namespace IKVM.Reflection
{
	public sealed class ExceptionHandlingClause
	{
		private readonly int flags;

		private readonly int tryOffset;

		private readonly int tryLength;

		private readonly int handlerOffset;

		private readonly int handlerLength;

		private readonly Type catchType;

		private readonly int filterOffset;

		public Type CatchType => catchType;

		public int FilterOffset => filterOffset;

		public ExceptionHandlingClauseOptions Flags => (ExceptionHandlingClauseOptions)flags;

		public int HandlerLength => handlerLength;

		public int HandlerOffset => handlerOffset;

		public int TryLength => tryLength;

		public int TryOffset => tryOffset;

		internal ExceptionHandlingClause(ModuleReader module, int flags, int tryOffset, int tryLength, int handlerOffset, int handlerLength, int classTokenOrfilterOffset, IGenericContext context)
		{
			this.flags = flags;
			this.tryOffset = tryOffset;
			this.tryLength = tryLength;
			this.handlerOffset = handlerOffset;
			this.handlerLength = handlerLength;
			catchType = ((flags == 0 && classTokenOrfilterOffset != 0) ? module.ResolveType(classTokenOrfilterOffset, context) : null);
			filterOffset = ((flags == 1) ? classTokenOrfilterOffset : 0);
		}
	}
}
