using System.Reflection.Emit;

namespace Mono.CSharp.Nullable
{
	public class LiftedConversion : Expression, IMemoryLocation
	{
		private Expression expr;

		private Expression null_value;

		private Unwrap unwrap;

		public override bool IsNull => expr.IsNull;

		public LiftedConversion(Expression expr, Unwrap unwrap, TypeSpec type)
		{
			this.expr = expr;
			this.unwrap = unwrap;
			loc = expr.Location;
			base.type = type;
		}

		public LiftedConversion(Expression expr, Expression unwrap, TypeSpec type)
			: this(expr, unwrap as Unwrap, type)
		{
		}

		public override bool ContainsEmitWithAwait()
		{
			return unwrap.ContainsEmitWithAwait();
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			return expr.CreateExpressionTree(ec);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			if (unwrap == null)
			{
				if (type.IsNullableType)
				{
					return Wrap.Create(expr, type);
				}
				return expr;
			}
			if (type.IsNullableType)
			{
				if (!expr.Type.IsNullableType)
				{
					expr = Wrap.Create(expr, type);
					if (expr == null)
					{
						return null;
					}
				}
				null_value = LiftedNull.Create(type, loc);
			}
			else if (TypeSpec.IsValueType(type))
			{
				null_value = LiftedNull.Create(type, loc);
			}
			else
			{
				null_value = new NullConstant(type, loc);
			}
			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			Label label = ec.DefineLabel();
			Label label2 = ec.DefineLabel();
			unwrap.EmitCheck(ec);
			ec.Emit(OpCodes.Brfalse, label);
			expr.Emit(ec);
			ec.Emit(OpCodes.Br, label2);
			ec.MarkLabel(label);
			null_value.Emit(ec);
			ec.MarkLabel(label2);
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			expr.FlowAnalysis(fc);
		}

		public void AddressOf(EmitContext ec, AddressOp mode)
		{
			unwrap.AddressOf(ec, mode);
		}
	}
}
