namespace Mono.CSharp
{
	public class UsingNamespace : UsingClause
	{
		public UsingNamespace(ATypeNameExpression expr, Location loc)
			: base(expr, loc)
		{
		}

		public override void Define(NamespaceContainer ctx)
		{
			base.Define(ctx);
			if (!(resolved is NamespaceExpression) && resolved != null)
			{
				CompilerContext compiler = ctx.Module.Compiler;
				TypeSpec type = resolved.Type;
				compiler.Report.SymbolRelatedToPreviousError(type);
				compiler.Report.Error(138, base.Location, "A `using' directive can only be applied to namespaces but `{0}' denotes a type. Consider using a `using static' instead", type.GetSignatureForError());
			}
		}
	}
}
