namespace Mono.CSharp.Linq
{
	public class QueryBlock : ParametersBlock
	{
		public sealed class TransparentParameter : ImplicitLambdaParameter
		{
			public static int Counter;

			private const string ParameterNamePrefix = "<>__TranspIdent";

			public readonly Parameter Parent;

			public readonly string Identifier;

			public TransparentParameter(Parameter parent, RangeVariable identifier)
				: base("<>__TranspIdent" + Counter++, identifier.Location)
			{
				Parent = parent;
				Identifier = identifier.Name;
			}

			public static void Reset()
			{
				Counter = 0;
			}
		}

		public QueryBlock(Block parent, Location start)
			: base(parent, ParametersCompiled.EmptyReadOnlyParameters, start, Flags.CompilerGenerated)
		{
		}

		public void AddRangeVariable(RangeVariable variable)
		{
			variable.Block = this;
			base.TopBlock.AddLocalName(variable.Name, variable, true);
		}

		public override void Error_AlreadyDeclared(string name, INamedBlockVariable variable, string reason)
		{
			base.TopBlock.Report.Error(1931, variable.Location, "A range variable `{0}' conflicts with a previous declaration of `{0}'", name);
		}

		public override void Error_AlreadyDeclared(string name, INamedBlockVariable variable)
		{
			base.TopBlock.Report.Error(1930, variable.Location, "A range variable `{0}' has already been declared in this scope", name);
		}

		public override void Error_AlreadyDeclaredTypeParameter(string name, Location loc)
		{
			base.TopBlock.Report.Error(1948, loc, "A range variable `{0}' conflicts with a method type parameter", name);
		}

		public void SetParameter(Parameter parameter)
		{
			parameters = new ParametersCompiled(parameter);
			parameter_info = new ParameterInfo[1]
			{
				new ParameterInfo(this, 0)
			};
		}

		public void SetParameters(Parameter first, Parameter second)
		{
			parameters = new ParametersCompiled(first, second);
			parameter_info = new ParameterInfo[2]
			{
				new ParameterInfo(this, 0),
				new ParameterInfo(this, 1)
			};
		}
	}
}
