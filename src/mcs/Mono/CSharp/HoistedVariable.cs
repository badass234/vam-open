using System.Collections.Generic;

namespace Mono.CSharp
{
	public abstract class HoistedVariable
	{
		private class ExpressionTreeVariableReference : Expression
		{
			private readonly HoistedVariable hv;

			public ExpressionTreeVariableReference(HoistedVariable hv)
			{
				this.hv = hv;
			}

			public override bool ContainsEmitWithAwait()
			{
				return false;
			}

			public override Expression CreateExpressionTree(ResolveContext ec)
			{
				return hv.CreateExpressionTree();
			}

			protected override Expression DoResolve(ResolveContext ec)
			{
				eclass = ExprClass.Value;
				type = ec.Module.PredefinedTypes.Expression.Resolve();
				return this;
			}

			public override void Emit(EmitContext ec)
			{
				ResolveContext resolveContext = new ResolveContext(ec.MemberContext);
				Expression expression = hv.GetFieldExpression(ec).CreateExpressionTree(resolveContext, false);
				expression.Resolve(resolveContext)?.Emit(ec);
			}
		}

		protected readonly AnonymousMethodStorey storey;

		protected Field field;

		private Dictionary<AnonymousExpression, FieldExpr> cached_inner_access;

		private FieldExpr cached_outer_access;

		public Field Field => field;

		public AnonymousMethodStorey Storey => storey;

		protected HoistedVariable(AnonymousMethodStorey storey, string name, TypeSpec type)
			: this(storey, storey.AddCapturedVariable(name, type))
		{
		}

		protected HoistedVariable(AnonymousMethodStorey storey, Field field)
		{
			this.storey = storey;
			this.field = field;
		}

		public void AddressOf(EmitContext ec, AddressOp mode)
		{
			GetFieldExpression(ec).AddressOf(ec, mode);
		}

		public Expression CreateExpressionTree()
		{
			return new ExpressionTreeVariableReference(this);
		}

		public void Emit(EmitContext ec)
		{
			GetFieldExpression(ec).Emit(ec);
		}

		public Expression EmitToField(EmitContext ec)
		{
			return GetFieldExpression(ec);
		}

		protected virtual FieldExpr GetFieldExpression(EmitContext ec)
		{
			if (ec.CurrentAnonymousMethod == null || ec.CurrentAnonymousMethod.Storey == null)
			{
				if (cached_outer_access != null)
				{
					return cached_outer_access;
				}
				if (storey.Instance.Type.IsGenericOrParentIsGeneric)
				{
					FieldSpec member = MemberCache.GetMember(storey.Instance.Type, field.Spec);
					cached_outer_access = new FieldExpr(member, field.Location);
				}
				else
				{
					cached_outer_access = new FieldExpr(field, field.Location);
				}
				cached_outer_access.InstanceExpression = storey.GetStoreyInstanceExpression(ec);
				return cached_outer_access;
			}
			FieldExpr value;
			if (cached_inner_access != null)
			{
				if (!cached_inner_access.TryGetValue(ec.CurrentAnonymousMethod, out value))
				{
					value = null;
				}
			}
			else
			{
				value = null;
				cached_inner_access = new Dictionary<AnonymousExpression, FieldExpr>(4);
			}
			if (value == null)
			{
				value = ((!field.Parent.IsGenericOrParentIsGeneric) ? new FieldExpr(field, field.Location) : new FieldExpr(MemberCache.GetMember(field.Parent.CurrentType, field.Spec), field.Location));
				value.InstanceExpression = storey.GetStoreyInstanceExpression(ec);
				cached_inner_access.Add(ec.CurrentAnonymousMethod, value);
			}
			return value;
		}

		public void Emit(EmitContext ec, bool leave_copy)
		{
			GetFieldExpression(ec).Emit(ec, leave_copy);
		}

		public void EmitAssign(EmitContext ec, Expression source, bool leave_copy, bool isCompound)
		{
			GetFieldExpression(ec).EmitAssign(ec, source, leave_copy, false);
		}
	}
}
