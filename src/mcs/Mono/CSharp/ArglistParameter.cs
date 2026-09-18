using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class ArglistParameter : Parameter
	{
		public ArglistParameter(Location loc)
			: base(null, string.Empty, Modifier.NONE, null, loc)
		{
			parameter_type = InternalType.Arglist;
		}

		public override void ApplyAttributes(MethodBuilder mb, ConstructorBuilder cb, int index, PredefinedAttributes pa)
		{
		}

		public override bool CheckAccessibility(InterfaceMemberBase member)
		{
			return true;
		}

		public override TypeSpec Resolve(IMemberContext ec, int index)
		{
			return parameter_type;
		}
	}
}
