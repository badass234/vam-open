using System.Reflection.Emit;
using Mono.CSharp.Nullable;

namespace Mono.CSharp
{
	public class As : Probe
	{
		protected override string OperatorName => "as";

		public As(Expression expr, Expression probe_type, Location l)
			: base(expr, probe_type, l)
		{
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Arguments args = Arguments.CreateForExpressionTree(ec, null, expr.CreateExpressionTree(ec), new TypeOf(probe_type_expr, loc));
			return CreateExpressionFactoryCall(ec, "TypeAs", args);
		}

		public override void Emit(EmitContext ec)
		{
			expr.Emit(ec);
			ec.Emit(OpCodes.Isinst, type);
			if (TypeManager.IsGenericParameter(type) || type.IsNullableType)
			{
				ec.Emit(OpCodes.Unbox_Any, type);
			}
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			if (ResolveCommon(ec) == null)
			{
				return null;
			}
			type = probe_type_expr;
			eclass = ExprClass.Value;
			TypeSpec typeSpec = expr.Type;
			if (!TypeSpec.IsReferenceType(type) && !type.IsNullableType)
			{
				if (TypeManager.IsGenericParameter(type))
				{
					ec.Report.Error(413, loc, "The `as' operator cannot be used with a non-reference type parameter `{0}'. Consider adding `class' or a reference type constraint", probe_type_expr.GetSignatureForError());
				}
				else
				{
					ec.Report.Error(77, loc, "The `as' operator cannot be used with a non-nullable value type `{0}'", type.GetSignatureForError());
				}
				return null;
			}
			if (expr.IsNull && type.IsNullableType)
			{
				return LiftedNull.CreateFromExpression(ec, this);
			}
			if (typeSpec.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				return this;
			}
			Expression expression = Convert.ImplicitConversionStandard(ec, expr, type, loc);
			if (expression != null)
			{
				expression = EmptyCast.Create(expression, type);
				return ReducedExpression.Create(expression, this).Resolve(ec);
			}
			if (Convert.ExplicitReferenceConversionExists(typeSpec, type))
			{
				if (TypeManager.IsGenericParameter(typeSpec))
				{
					expr = new BoxedCast(expr, typeSpec);
				}
				return this;
			}
			if (InflatedTypeSpec.ContainsTypeParameter(typeSpec) || InflatedTypeSpec.ContainsTypeParameter(type))
			{
				expr = new BoxedCast(expr, typeSpec);
				return this;
			}
			if (typeSpec != InternalType.ErrorType)
			{
				ec.Report.Error(39, loc, "Cannot convert type `{0}' to `{1}' via a built-in conversion", typeSpec.GetSignatureForError(), type.GetSignatureForError());
			}
			return null;
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
