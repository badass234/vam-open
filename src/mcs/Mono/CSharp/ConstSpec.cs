using System.Reflection;

namespace Mono.CSharp
{
	public class ConstSpec : FieldSpec
	{
		private Expression value;

		public Expression Value => value;

		public ConstSpec(TypeSpec declaringType, IMemberDefinition definition, TypeSpec memberType, FieldInfo fi, Modifiers mod, Expression value)
			: base(declaringType, definition, memberType, fi, mod)
		{
			this.value = value;
		}

		public Constant GetConstant(ResolveContext rc)
		{
			if (value.eclass != ExprClass.Value)
			{
				value = value.Resolve(rc);
			}
			return (Constant)value;
		}
	}
}
