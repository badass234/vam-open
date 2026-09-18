using System.Collections.Generic;
using System.Linq;
using Mono.CSharp.Linq;

namespace Mono.CSharp
{
	public class CompletionMemberAccess : CompletingExpression
	{
		private Expression expr;

		private string partial_name;

		private TypeArguments targs;

		public CompletionMemberAccess(Expression e, string partial_name, Location l)
		{
			expr = e;
			loc = l;
			this.partial_name = partial_name;
		}

		public CompletionMemberAccess(Expression e, string partial_name, TypeArguments targs, Location l)
		{
			expr = e;
			loc = l;
			this.partial_name = partial_name;
			this.targs = targs;
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			SimpleName simpleName = expr as SimpleName;
			if (simpleName != null)
			{
				expr = simpleName.LookupNameExpression(rc, MemberLookupRestrictions.ExactArity | MemberLookupRestrictions.ReadAccess);
				if (expr is VariableReference || expr is ConstantExpr || expr is TransparentMemberAccess)
				{
					expr = expr.Resolve(rc);
				}
				else if (expr is TypeParameterExpr)
				{
					expr.Error_UnexpectedKind(rc, ResolveFlags.VariableOrValue | ResolveFlags.Type, simpleName.Location);
					expr = null;
				}
			}
			else
			{
				expr = expr.Resolve(rc, ResolveFlags.VariableOrValue | ResolveFlags.Type);
			}
			if (expr == null)
			{
				return null;
			}
			TypeSpec typeSpec = expr.Type;
			if (typeSpec.IsPointer || typeSpec.Kind == MemberKind.Void || typeSpec == InternalType.NullLiteral || typeSpec == InternalType.AnonymousMethod)
			{
				expr.Error_OperatorCannotBeApplied(rc, loc, ".", typeSpec);
				return null;
			}
			if (targs != null && !targs.Resolve(rc, true))
			{
				return null;
			}
			List<string> list = new List<string>();
			NamespaceExpression namespaceExpression = expr as NamespaceExpression;
			if (namespaceExpression != null)
			{
				string prefix = ((partial_name != null) ? (namespaceExpression.Namespace.Name + "." + partial_name) : namespaceExpression.Namespace.Name);
				rc.CurrentMemberDefinition.GetCompletionStartingWith(prefix, list);
				if (partial_name != null)
				{
					list = list.Select((string l) => l.Substring(partial_name.Length)).ToList();
				}
			}
			else
			{
				IEnumerable<string> names = from l in MemberCache.GetCompletitionMembers(rc, typeSpec, partial_name)
					select l.Name;
				CompletingExpression.AppendResults(list, partial_name, names);
			}
			throw new CompletionResult((partial_name == null) ? "" : partial_name, list.Distinct().ToArray());
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			CompletionMemberAccess completionMemberAccess = (CompletionMemberAccess)t;
			if (targs != null)
			{
				completionMemberAccess.targs = targs.Clone();
			}
			completionMemberAccess.expr = expr.Clone(clonectx);
		}
	}
}
