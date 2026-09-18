using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class BaseThis : This
	{
		public override string Name => "base";

		public BaseThis(Location loc)
			: base(loc)
		{
		}

		public BaseThis(TypeSpec type, Location loc)
			: base(loc)
		{
			base.type = type;
			eclass = ExprClass.Variable;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			ec.Report.Error(831, loc, "An expression tree may not contain a base access");
			return base.CreateExpressionTree(ec);
		}

		public override void Emit(EmitContext ec)
		{
			base.Emit(ec);
			if (type == ec.Module.Compiler.BuiltinTypes.ValueType)
			{
				TypeSpec currentType = ec.CurrentType;
				ec.Emit(OpCodes.Ldobj, currentType);
				ec.Emit(OpCodes.Box, currentType);
			}
		}

		protected override void Error_ThisNotAvailable(ResolveContext ec)
		{
			if (ec.IsStatic)
			{
				ec.Report.Error(1511, loc, "Keyword `base' is not available in a static method");
			}
			else
			{
				ec.Report.Error(1512, loc, "Keyword `base' is not available in the current context");
			}
		}

		public override void ResolveBase(ResolveContext ec)
		{
			base.ResolveBase(ec);
			type = ec.CurrentType.BaseType;
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
