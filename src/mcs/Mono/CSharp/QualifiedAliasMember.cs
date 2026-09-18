namespace Mono.CSharp
{
	public class QualifiedAliasMember : MemberAccess
	{
		private readonly string alias;

		public static readonly string GlobalAlias = "global";

		public string Alias => alias;

		public QualifiedAliasMember(string alias, string identifier, Location l)
			: base(null, identifier, l)
		{
			this.alias = alias;
		}

		public QualifiedAliasMember(string alias, string identifier, TypeArguments targs, Location l)
			: base(null, identifier, targs, l)
		{
			this.alias = alias;
		}

		public QualifiedAliasMember(string alias, string identifier, int arity, Location l)
			: base(null, identifier, arity, l)
		{
			this.alias = alias;
		}

		public FullNamedExpression CreateExpressionFromAlias(IMemberContext mc)
		{
			if (alias == GlobalAlias)
			{
				return new NamespaceExpression(mc.Module.GlobalRootNamespace, loc);
			}
			int errors = mc.Module.Compiler.Report.Errors;
			FullNamedExpression fullNamedExpression = mc.LookupNamespaceAlias(alias);
			if (fullNamedExpression == null)
			{
				if (errors == mc.Module.Compiler.Report.Errors)
				{
					mc.Module.Compiler.Report.Error(432, loc, "Alias `{0}' not found", alias);
				}
				return null;
			}
			return fullNamedExpression;
		}

		public override FullNamedExpression ResolveAsTypeOrNamespace(IMemberContext mc, bool allowUnboundTypeArguments)
		{
			expr = CreateExpressionFromAlias(mc);
			if (expr == null)
			{
				return null;
			}
			return base.ResolveAsTypeOrNamespace(mc, allowUnboundTypeArguments);
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			return ResolveAsTypeOrNamespace(rc, false);
		}

		public override string GetSignatureForError()
		{
			string text = base.Name;
			if (targs != null)
			{
				text = base.Name + "<" + targs.GetSignatureForError() + ">";
			}
			return alias + "::" + text;
		}

		public override bool HasConditionalAccess()
		{
			return false;
		}

		public override Expression LookupNameExpression(ResolveContext rc, MemberLookupRestrictions restrictions)
		{
			if ((restrictions & MemberLookupRestrictions.InvocableOnly) != MemberLookupRestrictions.None)
			{
				rc.Module.Compiler.Report.Error(687, loc, "The namespace alias qualifier `::' cannot be used to invoke a method. Consider using `.' instead", GetSignatureForError());
				return null;
			}
			return DoResolve(rc);
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
