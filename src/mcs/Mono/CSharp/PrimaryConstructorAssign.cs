namespace Mono.CSharp
{
	internal class PrimaryConstructorAssign : SimpleAssign
	{
		private readonly Field field;

		private readonly Parameter parameter;

		public PrimaryConstructorAssign(Field field, Parameter parameter)
			: base(null, null, parameter.Location)
		{
			this.field = field;
			this.parameter = parameter;
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			target = new FieldExpr(field, loc);
			source = rc.CurrentBlock.ParametersBlock.GetParameterInfo(parameter).CreateReferenceExpression(rc, loc);
			return base.DoResolve(rc);
		}

		public override void EmitStatement(EmitContext ec)
		{
			using (ec.With(BuilderContext.Options.OmitDebugInfo, true))
			{
				base.EmitStatement(ec);
			}
		}
	}
}
