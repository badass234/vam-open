namespace Mono.CSharp
{
	public class UsingExternAlias : UsingAliasNamespace
	{
		public UsingExternAlias(SimpleMemberName alias, Location loc)
			: base(alias, null, loc)
		{
		}

		public override void Define(NamespaceContainer ctx)
		{
			RootNamespace rootNamespace = ctx.Module.GetRootNamespace(Alias.Value);
			if (rootNamespace == null)
			{
				ctx.Module.Compiler.Report.Error(430, base.Location, "The extern alias `{0}' was not specified in -reference option", Alias.Value);
			}
			else
			{
				resolved = new NamespaceExpression(rootNamespace, base.Location);
			}
		}
	}
}
