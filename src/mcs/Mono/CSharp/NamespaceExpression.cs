using System;

namespace Mono.CSharp
{
	public class NamespaceExpression : FullNamedExpression
	{
		private readonly Namespace ns;

		public Namespace Namespace => ns;

		public NamespaceExpression(Namespace ns, Location loc)
		{
			this.ns = ns;
			base.Type = InternalType.Namespace;
			eclass = ExprClass.Namespace;
			base.loc = loc;
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			throw new NotImplementedException();
		}

		public override FullNamedExpression ResolveAsTypeOrNamespace(IMemberContext mc, bool allowUnboundTypeArguments)
		{
			return this;
		}

		public void Error_NamespaceDoesNotExist(IMemberContext ctx, string name, int arity, Location loc)
		{
			TypeSpec typeSpec = Namespace.LookupType(ctx, name, arity, LookupMode.IgnoreAccessibility, loc);
			if (typeSpec != null)
			{
				Expression.ErrorIsInaccesible(ctx, typeSpec.GetSignatureForError(), loc);
				return;
			}
			typeSpec = Namespace.LookupType(ctx, name, -System.Math.Max(1, arity), LookupMode.Probing, loc);
			if (typeSpec != null)
			{
				Error_TypeArgumentsCannotBeUsed(ctx, typeSpec, loc);
				return;
			}
			Namespace obj;
			if (arity > 0 && Namespace.TryGetNamespace(name, out obj))
			{
				Expression.Error_TypeArgumentsCannotBeUsed(ctx, ExprClassName, obj.GetSignatureForError(), loc);
				return;
			}
			string text = null;
			string text2 = Namespace.GetSignatureForError() + "." + name;
			switch (text2)
			{
			case "System.Drawing":
			case "System.Web.Services":
			case "System.Web":
			case "System.Data":
			case "System.Configuration":
			case "System.Data.Services":
			case "System.DirectoryServices":
			case "System.Json":
			case "System.Net.Http":
			case "System.Numerics":
			case "System.Runtime.Caching":
			case "System.ServiceModel":
			case "System.Transactions":
			case "System.Web.Routing":
			case "System.Xml.Linq":
			case "System.Xml":
				text = text2;
				break;
			case "System.Linq":
			case "System.Linq.Expressions":
				text = "System.Core";
				break;
			case "System.Windows.Forms":
			case "System.Windows.Forms.Layout":
				text = "System.Windows.Forms";
				break;
			}
			text = ((text == null) ? "an" : ("`" + text + "'"));
			if (Namespace is GlobalRootNamespace)
			{
				ctx.Module.Compiler.Report.Error(400, loc, "The type or namespace name `{0}' could not be found in the global namespace. Are you missing {1} assembly reference?", name, text);
				return;
			}
			ctx.Module.Compiler.Report.Error(234, loc, "The type or namespace name `{0}' does not exist in the namespace `{1}'. Are you missing {2} assembly reference?", name, GetSignatureForError(), text);
		}

		public override string GetSignatureForError()
		{
			return ns.GetSignatureForError();
		}

		public FullNamedExpression LookupTypeOrNamespace(IMemberContext ctx, string name, int arity, LookupMode mode, Location loc)
		{
			return ns.LookupTypeOrNamespace(ctx, name, arity, mode, loc);
		}

		public override string ToString()
		{
			return Namespace.Name;
		}
	}
}
