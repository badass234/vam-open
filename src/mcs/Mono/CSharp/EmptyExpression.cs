using System;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class EmptyExpression : Expression
	{
		private sealed class OutAccessExpression : EmptyExpression
		{
			public OutAccessExpression(TypeSpec t)
				: base(t)
			{
			}

			public override Expression DoResolveLValue(ResolveContext rc, Expression right_side)
			{
				rc.Report.Error(206, right_side.Location, "A property, indexer or dynamic member access may not be passed as `ref' or `out' parameter");
				return null;
			}
		}

		public static readonly EmptyExpression LValueMemberAccess = new EmptyExpression(InternalType.FakeInternalType);

		public static readonly EmptyExpression LValueMemberOutAccess = new EmptyExpression(InternalType.FakeInternalType);

		public static readonly EmptyExpression UnaryAddress = new EmptyExpression(InternalType.FakeInternalType);

		public static readonly EmptyExpression EventAddition = new EmptyExpression(InternalType.FakeInternalType);

		public static readonly EmptyExpression EventSubtraction = new EmptyExpression(InternalType.FakeInternalType);

		public static readonly EmptyExpression MissingValue = new EmptyExpression(InternalType.FakeInternalType);

		public static readonly Expression Null = new EmptyExpression(InternalType.FakeInternalType);

		public static readonly EmptyExpression OutAccess = new OutAccessExpression(InternalType.FakeInternalType);

		public EmptyExpression(TypeSpec t)
		{
			type = t;
			eclass = ExprClass.Value;
			loc = Location.Null;
		}

		protected override void CloneTo(CloneContext clonectx, Expression target)
		{
		}

		public override bool ContainsEmitWithAwait()
		{
			return false;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			throw new NotSupportedException("ET");
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			return this;
		}

		public override void Emit(EmitContext ec)
		{
		}

		public override void EmitBranchable(EmitContext ec, Label target, bool on_true)
		{
		}

		public override void EmitSideEffect(EmitContext ec)
		{
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
