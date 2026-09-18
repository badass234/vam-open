using System;

namespace Mono.CSharp
{
	public class EventExpr : MemberExpr, IAssignMethod
	{
		private readonly EventSpec spec;

		private MethodSpec op;

		protected override TypeSpec DeclaringType => spec.DeclaringType;

		public override string Name => spec.Name;

		public override bool IsInstance => !spec.IsStatic;

		public override bool IsStatic => spec.IsStatic;

		public override string KindName => "event";

		public MethodSpec Operator => op;

		public EventExpr(EventSpec spec, Location loc)
		{
			this.spec = spec;
			base.loc = loc;
		}

		public override MemberExpr ResolveMemberAccess(ResolveContext ec, Expression left, SimpleName original)
		{
			if (!ec.HasSet(ResolveContext.Options.CompoundAssignmentScope) && spec.BackingField != null && (spec.DeclaringType == ec.CurrentType || TypeManager.IsNestedChildOf(ec.CurrentType, spec.DeclaringType.MemberDefinition)))
			{
				spec.MemberDefinition.SetIsUsed();
				if (!ec.IsObsolete)
				{
					ObsoleteAttribute attributeObsolete = spec.GetAttributeObsolete();
					if (attributeObsolete != null)
					{
						AttributeTester.Report_ObsoleteMessage(attributeObsolete, spec.GetSignatureForError(), loc, ec.Report);
					}
				}
				if ((spec.Modifiers & (Modifiers.ABSTRACT | Modifiers.EXTERN)) != 0)
				{
					Error_AssignmentEventOnly(ec);
				}
				FieldExpr fieldExpr = new FieldExpr(spec.BackingField, loc);
				InstanceExpression = null;
				return fieldExpr.ResolveMemberAccess(ec, left, original);
			}
			return base.ResolveMemberAccess(ec, left, original);
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			throw new NotSupportedException("ET");
		}

		public override Expression DoResolveLValue(ResolveContext ec, Expression right_side)
		{
			if (right_side == EmptyExpression.EventAddition)
			{
				op = spec.AccessorAdd;
			}
			else if (right_side == EmptyExpression.EventSubtraction)
			{
				op = spec.AccessorRemove;
			}
			if (op == null)
			{
				Error_AssignmentEventOnly(ec);
				return null;
			}
			if (HasConditionalAccess())
			{
				Error_NullPropagatingLValue(ec);
			}
			op = CandidateToBaseOverride(ec, op);
			return this;
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			eclass = ExprClass.EventAccess;
			type = spec.MemberType;
			ResolveInstanceExpression(ec, null);
			if (!ec.HasSet(ResolveContext.Options.CompoundAssignmentScope))
			{
				Error_AssignmentEventOnly(ec);
			}
			DoBestMemberChecks(ec, spec);
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			throw new NotSupportedException();
		}

		public void Emit(EmitContext ec, bool leave_copy)
		{
			throw new NotImplementedException();
		}

		public void EmitAssign(EmitContext ec, Expression source, bool leave_copy, bool isCompound)
		{
			if (leave_copy || !isCompound)
			{
				throw new NotImplementedException("EventExpr::EmitAssign");
			}
			Arguments arguments = new Arguments(1);
			arguments.Add(new Argument(source));
			CallEmitter callEmitter = default(CallEmitter);
			callEmitter.InstanceExpression = InstanceExpression;
			callEmitter.ConditionalAccess = base.ConditionalAccess;
			callEmitter.EmitStatement(ec, op, arguments, loc);
		}

		private void Error_AssignmentEventOnly(ResolveContext ec)
		{
			if (spec.DeclaringType == ec.CurrentType || TypeManager.IsNestedChildOf(ec.CurrentType, spec.DeclaringType.MemberDefinition))
			{
				ec.Report.Error(79, loc, "The event `{0}' can only appear on the left hand side of `+=' or `-=' operator", GetSignatureForError());
			}
			else
			{
				ec.Report.Error(70, loc, "The event `{0}' can only appear on the left hand side of += or -= when used outside of the type `{1}'", GetSignatureForError(), spec.DeclaringType.GetSignatureForError());
			}
		}

		protected override void Error_CannotCallAbstractBase(ResolveContext rc, string name)
		{
			name = name.Substring(0, name.LastIndexOf('.'));
			base.Error_CannotCallAbstractBase(rc, name);
		}

		public override string GetSignatureForError()
		{
			return TypeManager.CSharpSignature(spec);
		}

		public override void SetTypeArguments(ResolveContext ec, TypeArguments ta)
		{
			Expression.Error_TypeArgumentsCannotBeUsed(ec, "event", GetSignatureForError(), loc);
		}
	}
}
