using System.Linq.Expressions;
using System.Reflection.Emit;
using Mono.CSharp.Linq;

namespace Mono.CSharp
{
	internal abstract class PropertyOrIndexerExpr<T> : MemberExpr, IDynamicAssign, IAssignMethod where T : PropertySpec
	{
		private MethodSpec getter;

		private MethodSpec setter;

		protected T best_candidate;

		protected LocalTemporary temp;

		protected bool emitting_compound_assignment;

		protected bool has_await_arguments;

		protected abstract Arguments Arguments { get; set; }

		public MethodSpec Getter
		{
			get
			{
				return getter;
			}
			set
			{
				getter = value;
			}
		}

		public MethodSpec Setter
		{
			get
			{
				return setter;
			}
			set
			{
				setter = value;
			}
		}

		protected PropertyOrIndexerExpr(Location l)
		{
			loc = l;
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			if (eclass == ExprClass.Unresolved)
			{
				ResolveConditionalAccessReceiver(ec);
				Expression expression = OverloadResolve(ec, null);
				if (expression == null)
				{
					return null;
				}
				if (expression != this)
				{
					return expression.Resolve(ec);
				}
				if (conditional_access_receiver)
				{
					type = Expression.LiftMemberType(ec, type);
					ec.With(ResolveContext.Options.ConditionalAccessReceiver, false);
				}
			}
			if (!ResolveGetter(ec))
			{
				return null;
			}
			return this;
		}

		public override Expression DoResolveLValue(ResolveContext rc, Expression right_side)
		{
			if (HasConditionalAccess())
			{
				Error_NullPropagatingLValue(rc);
			}
			if (right_side == EmptyExpression.OutAccess)
			{
				INamedBlockVariable variable = null;
				if (best_candidate != null && rc.CurrentBlock.ParametersBlock.TopBlock.GetLocalName(best_candidate.Name, rc.CurrentBlock, ref variable) && variable is RangeVariable)
				{
					rc.Report.Error(1939, loc, "A range variable `{0}' may not be passes as `ref' or `out' parameter", best_candidate.Name);
				}
				else
				{
					right_side.DoResolveLValue(rc, this);
				}
				return null;
			}
			if (eclass == ExprClass.Unresolved)
			{
				Expression expression = OverloadResolve(rc, right_side);
				if (expression == null)
				{
					return null;
				}
				if (expression != this)
				{
					return expression.ResolveLValue(rc, right_side);
				}
			}
			else
			{
				ResolveInstanceExpression(rc, right_side);
			}
			if (!best_candidate.HasSet)
			{
				if (ResolveAutopropertyAssignment(rc, right_side))
				{
					return this;
				}
				rc.Report.Error(200, loc, "Property or indexer `{0}' cannot be assigned to (it is read-only)", GetSignatureForError());
				return null;
			}
			if (!best_candidate.Set.IsAccessible(rc) || !best_candidate.Set.DeclaringType.IsAccessible(rc))
			{
				if (best_candidate.HasDifferentAccessibility)
				{
					rc.Report.SymbolRelatedToPreviousError(best_candidate.Set);
					rc.Report.Error(272, loc, "The property or indexer `{0}' cannot be used in this context because the set accessor is inaccessible", GetSignatureForError());
				}
				else
				{
					rc.Report.SymbolRelatedToPreviousError(best_candidate.Set);
					Expression.ErrorIsInaccesible(rc, best_candidate.GetSignatureForError(), loc);
				}
			}
			if (best_candidate.HasDifferentAccessibility)
			{
				CheckProtectedMemberAccess(rc, best_candidate.Set);
			}
			setter = CandidateToBaseOverride(rc, best_candidate.Set);
			return this;
		}

		private void EmitConditionalAccess(EmitContext ec, ref CallEmitter call, MethodSpec method, Arguments arguments)
		{
			ec.ConditionalAccess = new ConditionalAccessContext(type, ec.DefineLabel());
			call.Emit(ec, method, arguments, loc);
			ec.CloseConditionalAccess((method.ReturnType != type && type.IsNullableType) ? type : null);
		}

		public virtual void Emit(EmitContext ec, bool leave_copy)
		{
			CallEmitter call = new CallEmitter
			{
				ConditionalAccess = base.ConditionalAccess,
				InstanceExpression = InstanceExpression
			};
			if (has_await_arguments)
			{
				call.HasAwaitArguments = true;
			}
			else
			{
				call.DuplicateArguments = emitting_compound_assignment;
			}
			if (conditional_access_receiver)
			{
				EmitConditionalAccess(ec, ref call, Getter, Arguments);
			}
			else
			{
				call.Emit(ec, Getter, Arguments, loc);
			}
			if (call.HasAwaitArguments)
			{
				InstanceExpression = call.InstanceExpression;
				Arguments = call.EmittedArguments;
				has_await_arguments = true;
			}
			if (leave_copy)
			{
				ec.Emit(OpCodes.Dup);
				temp = new LocalTemporary(base.Type);
				temp.Store(ec);
			}
		}

		public abstract void EmitAssign(EmitContext ec, Expression source, bool leave_copy, bool isCompound);

		public override void Emit(EmitContext ec)
		{
			Emit(ec, false);
		}

		protected override FieldExpr EmitToFieldSource(EmitContext ec)
		{
			has_await_arguments = true;
			Emit(ec, false);
			return null;
		}

		public abstract System.Linq.Expressions.Expression MakeAssignExpression(BuilderContext ctx, Expression source);

		protected abstract Expression OverloadResolve(ResolveContext rc, Expression right_side);

		private bool ResolveGetter(ResolveContext rc)
		{
			if (!best_candidate.HasGet)
			{
				if (InstanceExpression != EmptyExpression.Null)
				{
					rc.Report.SymbolRelatedToPreviousError(best_candidate);
					rc.Report.Error(154, loc, "The property or indexer `{0}' cannot be used in this context because it lacks the `get' accessor", best_candidate.GetSignatureForError());
					return false;
				}
			}
			else if (!best_candidate.Get.IsAccessible(rc) || !best_candidate.Get.DeclaringType.IsAccessible(rc))
			{
				if (best_candidate.HasDifferentAccessibility)
				{
					rc.Report.SymbolRelatedToPreviousError(best_candidate.Get);
					rc.Report.Error(271, loc, "The property or indexer `{0}' cannot be used in this context because the get accessor is inaccessible", TypeManager.CSharpSignature(best_candidate));
				}
				else
				{
					rc.Report.SymbolRelatedToPreviousError(best_candidate.Get);
					Expression.ErrorIsInaccesible(rc, best_candidate.Get.GetSignatureForError(), loc);
				}
			}
			if (best_candidate.HasDifferentAccessibility)
			{
				CheckProtectedMemberAccess(rc, best_candidate.Get);
			}
			getter = CandidateToBaseOverride(rc, best_candidate.Get);
			return true;
		}

		protected virtual bool ResolveAutopropertyAssignment(ResolveContext rc, Expression rhs)
		{
			return false;
		}
	}
}
