namespace Mono.CSharp
{
	public class HoistedParameter : HoistedVariable
	{
		private sealed class HoistedFieldAssign : CompilerAssign
		{
			public HoistedFieldAssign(Expression target, Expression source)
				: base(target, source, target.Location)
			{
			}

			protected override Expression ResolveConversions(ResolveContext ec)
			{
				return this;
			}
		}

		private readonly ParameterReference parameter;

		public bool IsAssigned { get; set; }

		public ParameterReference Parameter => parameter;

		public HoistedParameter(AnonymousMethodStorey scope, ParameterReference par)
			: base(scope, par.Name, par.Type)
		{
			parameter = par;
		}

		public HoistedParameter(HoistedParameter hp, string name)
			: base(hp.storey, name, hp.parameter.Type)
		{
			parameter = hp.parameter;
		}

		public void EmitHoistingAssignment(EmitContext ec)
		{
			HoistedParameter hoistedVariant = parameter.Parameter.HoistedVariant;
			parameter.Parameter.HoistedVariant = null;
			new HoistedFieldAssign(GetFieldExpression(ec), parameter).EmitStatement(ec);
			parameter.Parameter.HoistedVariant = hoistedVariant;
		}
	}
}
