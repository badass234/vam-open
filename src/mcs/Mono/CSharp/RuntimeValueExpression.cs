using System;
using System.Linq.Expressions;

namespace Mono.CSharp
{
	public class RuntimeValueExpression : Expression, IDynamicAssign, IAssignMethod, IMemoryLocation
	{
		public class DynamicMetaObject
		{
			public TypeSpec RuntimeType;

			public TypeSpec LimitType;

			public System.Linq.Expressions.Expression Expression;
		}

		private readonly DynamicMetaObject obj;

		public bool IsSuggestionOnly { get; set; }

		public DynamicMetaObject MetaObject => obj;

		public RuntimeValueExpression(DynamicMetaObject obj, TypeSpec type)
		{
			this.obj = obj;
			base.type = type;
			eclass = ExprClass.Variable;
		}

		public void AddressOf(EmitContext ec, AddressOp mode)
		{
			throw new NotImplementedException();
		}

		public override bool ContainsEmitWithAwait()
		{
			throw new NotSupportedException();
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			throw new NotSupportedException();
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			return this;
		}

		public override Expression DoResolveLValue(ResolveContext ec, Expression right_side)
		{
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			throw new NotImplementedException();
		}

		public void Emit(EmitContext ec, bool leave_copy)
		{
			throw new NotImplementedException();
		}

		public void EmitAssign(EmitContext ec, Expression source, bool leave_copy, bool isCompound)
		{
			throw new NotImplementedException();
		}

		public System.Linq.Expressions.Expression MakeAssignExpression(BuilderContext ctx, Expression source)
		{
			return obj.Expression;
		}

		public override System.Linq.Expressions.Expression MakeExpression(BuilderContext ctx)
		{
			return System.Linq.Expressions.Expression.Convert(obj.Expression, type.GetMetaInfo());
		}
	}
}
