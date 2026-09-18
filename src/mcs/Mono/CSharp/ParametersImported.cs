namespace Mono.CSharp
{
	public class ParametersImported : AParametersCollection
	{
		public ParametersImported(IParameterData[] parameters, TypeSpec[] types, bool hasArglist, bool hasParams)
		{
			base.parameters = parameters;
			base.types = types;
			has_arglist = hasArglist;
			has_params = hasParams;
		}

		public ParametersImported(IParameterData[] param, TypeSpec[] types, bool hasParams)
		{
			parameters = param;
			base.types = types;
			has_params = hasParams;
		}
	}
}
