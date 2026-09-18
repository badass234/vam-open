namespace Mono.CSharp
{
	public sealed class EmptyExpressionStatement : ExpressionStatement
	{
		public static readonly EmptyExpressionStatement Instance = new EmptyExpressionStatement();

		private EmptyExpressionStatement()
		{
			loc = Location.Null;
		}

		public override bool ContainsEmitWithAwait()
		{
			return false;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			return null;
		}

		public override void EmitStatement(EmitContext ec)
		{
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			eclass = ExprClass.Value;
			type = ec.BuiltinTypes.Object;
			return this;
		}

		public override void Emit(EmitContext ec)
		{
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
