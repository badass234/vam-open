using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class ParamsParameter : Parameter
	{
		public ParamsParameter(FullNamedExpression type, string name, Attributes attrs, Location loc)
			: base(type, name, Modifier.PARAMS, attrs, loc)
		{
		}

		public override TypeSpec Resolve(IMemberContext ec, int index)
		{
			if (base.Resolve(ec, index) == null)
			{
				return null;
			}
			ArrayContainer arrayContainer = parameter_type as ArrayContainer;
			if (arrayContainer == null || arrayContainer.Rank != 1)
			{
				ec.Module.Compiler.Report.Error(225, base.Location, "The params parameter must be a single dimensional array");
				return null;
			}
			return parameter_type;
		}

		public override void ApplyAttributes(MethodBuilder mb, ConstructorBuilder cb, int index, PredefinedAttributes pa)
		{
			base.ApplyAttributes(mb, cb, index, pa);
			pa.ParamArray.EmitAttribute(builder);
		}
	}
}
