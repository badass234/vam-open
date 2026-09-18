namespace Mono.CSharp.Linq
{
	public class Select : AQueryClause
	{
		protected override string MethodName => "Select";

		public Select(QueryBlock block, Expression expr, Location loc)
			: base(block, expr, loc)
		{
		}

		public bool IsRequired(Parameter parameter)
		{
			SimpleName simpleName = expr as SimpleName;
			if (simpleName == null)
			{
				return true;
			}
			return simpleName.Name != parameter.Name;
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
