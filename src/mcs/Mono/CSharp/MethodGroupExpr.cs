using System;
using System.Collections.Generic;
using System.Linq;

namespace Mono.CSharp
{
	public class MethodGroupExpr : MemberExpr, OverloadResolver.IBaseMembersProvider
	{
		private static readonly MemberSpec[] Excluded = new MemberSpec[0];

		protected IList<MemberSpec> Methods;

		private MethodSpec best_candidate;

		private TypeSpec best_candidate_return;

		protected TypeArguments type_arguments;

		private SimpleName simple_name;

		protected TypeSpec queried_type;

		public MethodSpec BestCandidate => best_candidate;

		public TypeSpec BestCandidateReturnType => best_candidate_return;

		public IList<MemberSpec> Candidates => Methods;

		protected override TypeSpec DeclaringType => queried_type;

		public bool IsConditionallyExcluded => Methods == Excluded;

		public override bool IsInstance
		{
			get
			{
				if (best_candidate != null)
				{
					return !best_candidate.IsStatic;
				}
				return false;
			}
		}

		public override bool IsSideEffectFree
		{
			get
			{
				if (InstanceExpression != null)
				{
					return InstanceExpression.IsSideEffectFree;
				}
				return true;
			}
		}

		public override bool IsStatic
		{
			get
			{
				if (best_candidate != null)
				{
					return best_candidate.IsStatic;
				}
				return false;
			}
		}

		public override string KindName => "method";

		public override string Name
		{
			get
			{
				if (best_candidate != null)
				{
					return best_candidate.Name;
				}
				return Methods.First().Name;
			}
		}

		public MethodGroupExpr(IList<MemberSpec> mi, TypeSpec type, Location loc)
		{
			Methods = mi;
			base.loc = loc;
			base.type = InternalType.MethodGroup;
			eclass = ExprClass.MethodGroup;
			queried_type = type;
		}

		public MethodGroupExpr(MethodSpec m, TypeSpec type, Location loc)
			: this(new MemberSpec[1] { m }, type, loc)
		{
		}

		public static MethodGroupExpr CreatePredefined(MethodSpec best, TypeSpec queriedType, Location loc)
		{
			return new MethodGroupExpr(best, queriedType, loc)
			{
				best_candidate = best,
				best_candidate_return = best.ReturnType
			};
		}

		public override string GetSignatureForError()
		{
			if (best_candidate != null)
			{
				return best_candidate.GetSignatureForError();
			}
			return Methods.First().GetSignatureForError();
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			if (best_candidate == null)
			{
				ec.Report.Error(1953, loc, "An expression tree cannot contain an expression with method group");
				return null;
			}
			if (IsConditionallyExcluded)
			{
				ec.Report.Error(765, loc, "Partial methods with only a defining declaration or removed conditional methods cannot be used in an expression tree");
			}
			if (base.ConditionalAccess)
			{
				Error_NullShortCircuitInsideExpressionTree(ec);
			}
			return new TypeOfMethod(best_candidate, loc);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			eclass = ExprClass.MethodGroup;
			if (InstanceExpression != null)
			{
				InstanceExpression = InstanceExpression.Resolve(ec);
				if (InstanceExpression == null)
				{
					return null;
				}
			}
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			throw new NotSupportedException();
		}

		public void EmitCall(EmitContext ec, Arguments arguments, bool statement)
		{
			CallEmitter callEmitter = new CallEmitter
			{
				InstanceExpression = InstanceExpression,
				ConditionalAccess = base.ConditionalAccess
			};
			if (statement)
			{
				callEmitter.EmitStatement(ec, best_candidate, arguments, loc);
			}
			else
			{
				callEmitter.Emit(ec, best_candidate, arguments, loc);
			}
		}

		public void EmitCall(EmitContext ec, Arguments arguments, TypeSpec conditionalAccessReceiver, bool statement)
		{
			ec.ConditionalAccess = new ConditionalAccessContext(conditionalAccessReceiver, ec.DefineLabel())
			{
				Statement = statement
			};
			EmitCall(ec, arguments, statement);
			ec.CloseConditionalAccess((!statement && best_candidate_return != conditionalAccessReceiver && conditionalAccessReceiver.IsNullableType) ? conditionalAccessReceiver : null);
		}

		public override void Error_ValueCannotBeConverted(ResolveContext ec, TypeSpec target, bool expl)
		{
			ec.Report.Error(428, loc, "Cannot convert method group `{0}' to non-delegate type `{1}'. Consider using parentheses to invoke the method", Name, target.GetSignatureForError());
		}

		public bool HasAccessibleCandidate(ResolveContext rc)
		{
			foreach (MemberSpec candidate in Candidates)
			{
				if (candidate.IsAccessible(rc))
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsExtensionMethodArgument(Expression expr)
		{
			if (!(expr is TypeExpr))
			{
				return !(expr is BaseThis);
			}
			return false;
		}

		public virtual MethodGroupExpr OverloadResolve(ResolveContext ec, ref Arguments args, OverloadResolver.IErrorHandler cerrors, OverloadResolver.Restrictions restr)
		{
			if (best_candidate != null && best_candidate.Kind == MemberKind.Destructor)
			{
				return this;
			}
			OverloadResolver overloadResolver = new OverloadResolver(Methods, type_arguments, restr, loc);
			if ((restr & OverloadResolver.Restrictions.NoBaseMembers) == 0)
			{
				overloadResolver.BaseMembersProvider = this;
				overloadResolver.InstanceQualifier = this;
			}
			if (cerrors != null)
			{
				overloadResolver.CustomErrors = cerrors;
			}
			best_candidate = overloadResolver.ResolveMember<MethodSpec>(ec, ref args);
			if (best_candidate == null)
			{
				if (!overloadResolver.BestCandidateIsDynamic)
				{
					return null;
				}
				if (simple_name != null && ec.IsStatic)
				{
					InstanceExpression = ProbeIdenticalTypeName(ec, InstanceExpression, simple_name);
				}
				return this;
			}
			if (overloadResolver.BestCandidateNewMethodGroup != null)
			{
				return overloadResolver.BestCandidateNewMethodGroup;
			}
			if (best_candidate.Kind == MemberKind.Method && (restr & OverloadResolver.Restrictions.ProbingOnly) == 0)
			{
				if (InstanceExpression != null)
				{
					if (best_candidate.IsExtensionMethod && args[0].Expr == InstanceExpression)
					{
						InstanceExpression = null;
					}
					else
					{
						if (simple_name != null && best_candidate.IsStatic)
						{
							InstanceExpression = ProbeIdenticalTypeName(ec, InstanceExpression, simple_name);
						}
						InstanceExpression.Resolve(ec, ResolveFlags.VariableOrValue | ResolveFlags.Type | ResolveFlags.MethodGroup);
					}
				}
				ResolveInstanceExpression(ec, null);
			}
			MethodSpec methodSpec = CandidateToBaseOverride(ec, best_candidate);
			if (methodSpec == best_candidate)
			{
				best_candidate_return = overloadResolver.BestCandidateReturnType;
			}
			else
			{
				best_candidate = methodSpec;
				best_candidate_return = best_candidate.ReturnType;
			}
			if (best_candidate.IsGeneric && (restr & OverloadResolver.Restrictions.ProbingOnly) == 0 && TypeParameterSpec.HasAnyTypeParameterConstrained(best_candidate.GenericDefinition))
			{
				new ConstraintChecker(ec).CheckAll(best_candidate.GetGenericMethodDefinition(), best_candidate.TypeArguments, best_candidate.Constraints, loc);
			}
			if (best_candidate.IsVirtual && (best_candidate.DeclaringType.Modifiers & Modifiers.PROTECTED) != 0 && best_candidate.MemberDefinition.IsImported && !best_candidate.DeclaringType.IsAccessible(ec))
			{
				ec.Report.SymbolRelatedToPreviousError(best_candidate);
				Expression.ErrorIsInaccesible(ec, best_candidate.GetSignatureForError(), loc);
			}
			if (best_candidate_return.Kind == MemberKind.Void && best_candidate.IsConditionallyExcluded(ec))
			{
				Methods = Excluded;
			}
			return this;
		}

		public override MemberExpr ResolveMemberAccess(ResolveContext ec, Expression left, SimpleName original)
		{
			(left as FieldExpr)?.Spec.MemberDefinition.SetIsAssigned();
			simple_name = original;
			return base.ResolveMemberAccess(ec, left, original);
		}

		public override void SetTypeArguments(ResolveContext ec, TypeArguments ta)
		{
			type_arguments = ta;
		}

		public virtual IList<MemberSpec> GetBaseMembers(TypeSpec baseType)
		{
			if (baseType != null)
			{
				return MemberCache.FindMembers(baseType, Methods[0].Name, false);
			}
			return null;
		}

		public IParametersMember GetOverrideMemberParameters(MemberSpec member)
		{
			if (queried_type == member.DeclaringType)
			{
				return null;
			}
			return MemberCache.FindMember(queried_type, new MemberFilter((MethodSpec)member), BindingRestriction.InstanceOnly | BindingRestriction.OverrideOnly) as IParametersMember;
		}

		public virtual MethodGroupExpr LookupExtensionMethod(ResolveContext rc)
		{
			if (InstanceExpression == null || InstanceExpression.eclass == ExprClass.Type)
			{
				return null;
			}
			if (!IsExtensionMethodArgument(InstanceExpression))
			{
				return null;
			}
			int arity = ((type_arguments != null) ? type_arguments.Count : 0);
			ExtensionMethodCandidates extensionMethodCandidates = rc.LookupExtensionMethod(Methods[0].Name, arity);
			if (extensionMethodCandidates == null)
			{
				return null;
			}
			ExtensionMethodGroupExpr extensionMethodGroupExpr = new ExtensionMethodGroupExpr(extensionMethodCandidates, InstanceExpression, loc);
			extensionMethodGroupExpr.SetTypeArguments(rc, type_arguments);
			return extensionMethodGroupExpr;
		}
	}
}
