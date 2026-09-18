using System.Linq.Expressions;

namespace Mono.CSharp.Nullable
{
	public class Unwrap : Expression, IMemoryLocation
	{
		private Expression expr;

		private LocalTemporary temp;

		private Expression temp_field;

		private readonly bool useDefaultValue;

		public Expression Original => expr;

		public override bool IsNull => expr.IsNull;

		private LocalTemporary LocalVariable
		{
			get
			{
				if (temp == null && temp_field == null)
				{
					temp = new LocalTemporary(expr.Type);
				}
				return temp;
			}
		}

		public Unwrap(Expression expr, bool useDefaultValue = true)
		{
			this.expr = expr;
			loc = expr.Location;
			this.useDefaultValue = useDefaultValue;
			type = NullableInfo.GetUnderlyingType(expr.Type);
			eclass = expr.eclass;
		}

		public override bool ContainsEmitWithAwait()
		{
			return expr.ContainsEmitWithAwait();
		}

		public static Expression Create(Expression expr)
		{
			Wrap wrap = expr as Wrap;
			if (wrap != null)
			{
				return wrap.Child;
			}
			return Create(expr, false);
		}

		public static Expression CreateUnwrapped(Expression expr)
		{
			Wrap wrap = expr as Wrap;
			if (wrap != null)
			{
				return wrap.Child;
			}
			return Create(expr, true);
		}

		public static Unwrap Create(Expression expr, bool useDefaultValue)
		{
			return new Unwrap(expr, useDefaultValue);
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			return expr.CreateExpressionTree(ec);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			return this;
		}

		public override Expression DoResolveLValue(ResolveContext ec, Expression right_side)
		{
			expr = expr.DoResolveLValue(ec, right_side);
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			Store(ec);
			CallEmitter callEmitter = new CallEmitter
			{
				InstanceExpression = this
			};
			if (useDefaultValue)
			{
				callEmitter.EmitPredefined(ec, NullableInfo.GetGetValueOrDefault(expr.Type), null);
			}
			else
			{
				callEmitter.EmitPredefined(ec, NullableInfo.GetValue(expr.Type), null);
			}
		}

		public void EmitCheck(EmitContext ec)
		{
			Store(ec);
			CallEmitter callEmitter = default(CallEmitter);
			callEmitter.InstanceExpression = this;
			callEmitter.EmitPredefined(ec, NullableInfo.GetHasValue(expr.Type), null);
		}

		public override void EmitSideEffect(EmitContext ec)
		{
			expr.EmitSideEffect(ec);
		}

		public override Expression EmitToField(EmitContext ec)
		{
			if (temp_field == null)
			{
				temp_field = expr.EmitToField(ec);
			}
			return this;
		}

		public override bool Equals(object obj)
		{
			Unwrap unwrap = obj as Unwrap;
			if (unwrap != null)
			{
				return expr.Equals(unwrap.expr);
			}
			return false;
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			expr.FlowAnalysis(fc);
		}

		public override int GetHashCode()
		{
			return expr.GetHashCode();
		}

		public void Store(EmitContext ec)
		{
			if (temp == null && temp_field == null && !(expr is VariableReference))
			{
				expr.Emit(ec);
				LocalVariable.Store(ec);
			}
		}

		public void Load(EmitContext ec)
		{
			if (temp_field != null)
			{
				temp_field.Emit(ec);
			}
			else if (expr is VariableReference)
			{
				expr.Emit(ec);
			}
			else
			{
				LocalVariable.Emit(ec);
			}
		}

		public override System.Linq.Expressions.Expression MakeExpression(BuilderContext ctx)
		{
			return expr.MakeExpression(ctx);
		}

		public void AddressOf(EmitContext ec, AddressOp mode)
		{
			IMemoryLocation memoryLocation;
			if (temp_field != null)
			{
				memoryLocation = temp_field as IMemoryLocation;
				if (memoryLocation == null)
				{
					LocalTemporary localTemporary = new LocalTemporary(temp_field.Type);
					temp_field.Emit(ec);
					localTemporary.Store(ec);
					memoryLocation = localTemporary;
				}
			}
			else
			{
				memoryLocation = expr as VariableReference;
			}
			if (memoryLocation != null)
			{
				memoryLocation.AddressOf(ec, mode);
			}
			else
			{
				LocalVariable.AddressOf(ec, mode);
			}
		}
	}
}
