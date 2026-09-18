using System;

namespace Mono.CSharp
{
	public class BlockVariableDeclarator
	{
		private LocalVariable li;

		private Expression initializer;

		public LocalVariable Variable => li;

		public Expression Initializer
		{
			get
			{
				return initializer;
			}
			set
			{
				initializer = value;
			}
		}

		public BlockVariableDeclarator(LocalVariable li, Expression initializer)
		{
			if (li.Type != null)
			{
				throw new ArgumentException("Expected null variable type");
			}
			this.li = li;
			this.initializer = initializer;
		}

		public virtual BlockVariableDeclarator Clone(CloneContext cloneCtx)
		{
			BlockVariableDeclarator blockVariableDeclarator = (BlockVariableDeclarator)MemberwiseClone();
			if (initializer != null)
			{
				blockVariableDeclarator.initializer = initializer.Clone(cloneCtx);
			}
			return blockVariableDeclarator;
		}
	}
}
