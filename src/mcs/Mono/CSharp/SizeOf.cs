using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class SizeOf : Expression
	{
		private readonly Expression texpr;

		private TypeSpec type_queried;

		public override bool IsSideEffectFree => true;

		public Expression TypeExpression => texpr;

		public SizeOf(Expression queried_type, Location l)
		{
			texpr = queried_type;
			loc = l;
		}

		public override bool ContainsEmitWithAwait()
		{
			return false;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Error_PointerInsideExpressionTree(ec);
			return null;
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			type_queried = texpr.ResolveAsType(ec);
			if (type_queried == null)
			{
				return null;
			}
			if (type_queried.IsEnum)
			{
				type_queried = EnumSpec.GetUnderlyingType(type_queried);
			}
			int size = BuiltinTypeSpec.GetSize(type_queried);
			if (size > 0)
			{
				return new IntConstant(ec.BuiltinTypes, size, loc);
			}
			if (!TypeManager.VerifyUnmanaged(ec.Module, type_queried, loc))
			{
				return null;
			}
			if (!ec.IsUnsafe)
			{
				ec.Report.Error(233, loc, "`{0}' does not have a predefined size, therefore sizeof can only be used in an unsafe context (consider using System.Runtime.InteropServices.Marshal.SizeOf)", type_queried.GetSignatureForError());
			}
			type = ec.BuiltinTypes.Int;
			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			ec.Emit(OpCodes.Sizeof, type_queried);
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
