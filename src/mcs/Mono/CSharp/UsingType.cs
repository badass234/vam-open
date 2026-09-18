namespace Mono.CSharp
{
	public class UsingType : UsingClause
	{
		public UsingType(ATypeNameExpression expr, Location loc)
			: base(expr, loc)
		{
		}

		public override void Define(NamespaceContainer ctx)
		{
			base.Define(ctx);
			if (resolved != null)
			{
				NamespaceExpression namespaceExpression = resolved as NamespaceExpression;
				if (namespaceExpression != null)
				{
					ctx.Module.Compiler.Report.Error(7007, base.Location, "A 'using static' directive can only be applied to types but `{0}' denotes a namespace. Consider using a `using' directive instead", namespaceExpression.GetSignatureForError());
				}
			}
		}
	}
}
