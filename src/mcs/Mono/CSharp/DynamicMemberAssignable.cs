using System;

namespace Mono.CSharp
{
	internal abstract class DynamicMemberAssignable : DynamicExpressionStatement, IDynamicBinder, IAssignMethod
	{
		private Expression setter;

		private Arguments setter_args;

		protected DynamicMemberAssignable(Arguments args, Location loc)
			: base(null, args, loc)
		{
			binder = this;
		}

		public Expression CreateCallSiteBinder(ResolveContext ec, Arguments args)
		{
			return CreateCallSiteBinder(ec, args, false);
		}

		protected abstract Expression CreateCallSiteBinder(ResolveContext ec, Arguments args, bool isSet);

		protected virtual Arguments CreateSetterArguments(ResolveContext rc, Expression rhs)
		{
			Arguments obj = new Arguments(base.Arguments.Count + 1);
			obj.AddRange(base.Arguments);
			obj.Add(new Argument(rhs));
			return obj;
		}

		public override Expression DoResolveLValue(ResolveContext rc, Expression right_side)
		{
			if (right_side == EmptyExpression.OutAccess)
			{
				right_side.DoResolveLValue(rc, this);
				return null;
			}
			if (DoResolveCore(rc))
			{
				setter_args = CreateSetterArguments(rc, right_side);
				setter = CreateCallSiteBinder(rc, setter_args, true);
			}
			eclass = ExprClass.Variable;
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			if (binder_expr == null)
			{
				EmitCall(ec, setter, base.Arguments, false);
			}
			else
			{
				base.Emit(ec);
			}
		}

		public override void EmitStatement(EmitContext ec)
		{
			if (binder_expr == null)
			{
				EmitCall(ec, setter, base.Arguments, true);
			}
			else
			{
				base.EmitStatement(ec);
			}
		}

		public void Emit(EmitContext ec, bool leave_copy)
		{
			throw new NotImplementedException();
		}

		public void EmitAssign(EmitContext ec, Expression source, bool leave_copy, bool isCompound)
		{
			EmitCall(ec, setter, setter_args, !leave_copy);
		}
	}
}
