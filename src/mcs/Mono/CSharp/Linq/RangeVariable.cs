namespace Mono.CSharp.Linq
{
	public sealed class RangeVariable : INamedBlockVariable
	{
		private Block block;

		public Block Block
		{
			get
			{
				return block;
			}
			set
			{
				block = value;
			}
		}

		public bool IsDeclared => true;

		public bool IsParameter => false;

		public Location Location { get; private set; }

		public string Name { get; private set; }

		public RangeVariable(string name, Location loc)
		{
			Name = name;
			Location = loc;
		}

		public Expression CreateReferenceExpression(ResolveContext rc, Location loc)
		{
			ParametersBlock parametersBlock = rc.CurrentBlock.ParametersBlock;
			while (true)
			{
				if (parametersBlock is QueryBlock)
				{
					for (int num = parametersBlock.Parameters.Count - 1; num >= 0; num--)
					{
						Parameter parameter = parametersBlock.Parameters[num];
						if (parameter.Name == Name)
						{
							return parametersBlock.GetParameterReference(num, loc);
						}
						Expression expression = null;
						for (QueryBlock.TransparentParameter transparentParameter = parameter as QueryBlock.TransparentParameter; transparentParameter != null; transparentParameter = transparentParameter.Parent as QueryBlock.TransparentParameter)
						{
							expression = ((expression != null) ? ((Expression)new TransparentMemberAccess(expression, transparentParameter.Name)) : ((Expression)parametersBlock.GetParameterReference(num, loc)));
							if (transparentParameter.Identifier == Name)
							{
								return new TransparentMemberAccess(expression, Name);
							}
							if (transparentParameter.Parent.Name == Name)
							{
								return new TransparentMemberAccess(expression, Name);
							}
						}
					}
				}
				if (parametersBlock == block)
				{
					break;
				}
				parametersBlock = parametersBlock.Parent.ParametersBlock;
			}
			return null;
		}
	}
}
