using System.Reflection.Emit;

namespace Mono.CSharp.Nullable
{
	public class LiftedNull : NullConstant, IMemoryLocation
	{
		private LiftedNull(TypeSpec nullable_type, Location loc)
			: base(nullable_type, loc)
		{
			eclass = ExprClass.Value;
		}

		public static Constant Create(TypeSpec nullable, Location loc)
		{
			return new LiftedNull(nullable, loc);
		}

		public static Constant CreateFromExpression(ResolveContext rc, Expression e)
		{
			if (!rc.HasSet(ResolveContext.Options.ExpressionTreeConversion))
			{
				rc.Report.Warning(458, 2, e.Location, "The result of the expression is always `null' of type `{0}'", e.Type.GetSignatureForError());
			}
			return ReducedExpression.Create(Create(e.Type, e.Location), e);
		}

		public override void Emit(EmitContext ec)
		{
			LocalTemporary localTemporary = new LocalTemporary(type);
			localTemporary.AddressOf(ec, AddressOp.Store);
			ec.Emit(OpCodes.Initobj, type);
			localTemporary.Emit(ec);
			localTemporary.Release(ec);
		}

		public void AddressOf(EmitContext ec, AddressOp Mode)
		{
			LocalTemporary localTemporary = new LocalTemporary(type);
			localTemporary.AddressOf(ec, AddressOp.Store);
			ec.Emit(OpCodes.Initobj, type);
			localTemporary.AddressOf(ec, Mode);
		}
	}
}
