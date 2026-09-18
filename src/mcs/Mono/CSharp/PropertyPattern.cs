using System.Collections.Generic;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	internal class PropertyPattern : ComplexPatternExpression
	{
		private LocalTemporary instance;

		public List<PropertyPatternMember> Members { get; private set; }

		public PropertyPattern(ATypeNameExpression typeExpresion, List<PropertyPatternMember> members, Location loc)
			: base(typeExpresion, loc)
		{
			Members = members;
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			type = base.TypeExpression.ResolveAsType(rc);
			if (type == null)
			{
				return null;
			}
			comparisons = new Expression[Members.Count];
			instance = new LocalTemporary(type);
			for (int i = 0; i < Members.Count; i++)
			{
				PropertyPatternMember propertyPatternMember = Members[i];
				Expression expression = Expression.MemberLookup(rc, false, type, propertyPatternMember.Name, 0, MemberLookupRestrictions.ExactArity, loc);
				if (expression == null)
				{
					expression = Expression.MemberLookup(rc, true, type, propertyPatternMember.Name, 0, MemberLookupRestrictions.ExactArity, loc);
					if (expression != null)
					{
						Expression.ErrorIsInaccesible(rc, expression.GetSignatureForError(), loc);
						continue;
					}
				}
				if (expression == null)
				{
					Expression.Error_TypeDoesNotContainDefinition(rc, base.Location, base.Type, propertyPatternMember.Name);
					continue;
				}
				PropertyExpr propertyExpr = expression as PropertyExpr;
				if (propertyExpr == null || expression is FieldExpr)
				{
					rc.Report.Error(-2001, propertyPatternMember.Location, "`{0}' is not a valid pattern member", propertyPatternMember.Name);
					continue;
				}
				if (propertyExpr != null && !propertyExpr.PropertyInfo.HasGet)
				{
					rc.Report.Error(-2002, propertyPatternMember.Location, "Property `{0}.get' accessor is required", propertyExpr.GetSignatureForError());
					continue;
				}
				Expression expression2 = propertyPatternMember.Expr.Resolve(rc);
				if (expression2 != null)
				{
					MemberExpr memberExpr = (MemberExpr)expression;
					memberExpr.InstanceExpression = instance;
					comparisons[i] = ResolveComparison(rc, expression2, memberExpr);
				}
			}
			eclass = ExprClass.Value;
			return this;
		}

		private static Expression ResolveComparison(ResolveContext rc, Expression expr, Expression instance)
		{
			if (expr is WildcardPattern)
			{
				return new EmptyExpression(expr.Type);
			}
			return new Is(instance, expr, expr.Location).Resolve(rc);
		}

		public override void EmitBranchable(EmitContext ec, Label target, bool on_true)
		{
			instance.Store(ec);
			base.EmitBranchable(ec, target, on_true);
		}
	}
}
