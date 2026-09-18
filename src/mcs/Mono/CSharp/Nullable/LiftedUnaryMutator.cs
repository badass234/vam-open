using System.Reflection.Emit;

namespace Mono.CSharp.Nullable
{
	internal class LiftedUnaryMutator : UnaryMutator
	{
		public LiftedUnaryMutator(Mode mode, Expression expr, Location loc)
			: base(mode, expr, loc)
		{
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			Expression expression = expr;
			expr = Unwrap.Create(expr);
			Expression result = DoResolveOperation(ec);
			expr = expression;
			type = expr.Type;
			return result;
		}

		protected override void EmitOperation(EmitContext ec)
		{
			Label label = ec.DefineLabel();
			Label label2 = ec.DefineLabel();
			LocalTemporary localTemporary = new LocalTemporary(type);
			localTemporary.Store(ec);
			CallEmitter callEmitter = default(CallEmitter);
			callEmitter.InstanceExpression = localTemporary;
			callEmitter.EmitPredefined(ec, NullableInfo.GetHasValue(expr.Type), null);
			ec.Emit(OpCodes.Brfalse, label);
			callEmitter = default(CallEmitter);
			callEmitter.InstanceExpression = localTemporary;
			callEmitter.EmitPredefined(ec, NullableInfo.GetGetValueOrDefault(expr.Type), null);
			localTemporary.Release(ec);
			base.EmitOperation(ec);
			ec.Emit(OpCodes.Newobj, NullableInfo.GetConstructor(type));
			ec.Emit(OpCodes.Br_S, label2);
			ec.MarkLabel(label);
			LiftedNull.Create(type, loc).Emit(ec);
			ec.MarkLabel(label2);
		}
	}
}
