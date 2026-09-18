namespace Mono.CSharp.Nullable
{
	public class UnwrapCall : CompositeExpression
	{
		public UnwrapCall(Expression expr)
			: base(expr)
		{
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			base.DoResolve(rc);
			if (type != null)
			{
				type = NullableInfo.GetUnderlyingType(type);
			}
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			CallEmitter callEmitter = default(CallEmitter);
			callEmitter.InstanceExpression = base.Child;
			callEmitter.EmitPredefined(ec, NullableInfo.GetValue(base.Child.Type), null);
		}
	}
}
