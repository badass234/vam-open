using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	internal class IndexerExpr : PropertyOrIndexerExpr<IndexerSpec>, OverloadResolver.IBaseMembersProvider
	{
		private IList<MemberSpec> indexers;

		private Arguments arguments;

		private TypeSpec queried_type;

		protected override Arguments Arguments
		{
			get
			{
				return arguments;
			}
			set
			{
				arguments = value;
			}
		}

		protected override TypeSpec DeclaringType => best_candidate.DeclaringType;

		public override bool IsInstance => true;

		public override bool IsStatic => false;

		public override string KindName => "indexer";

		public override string Name => "this";

		public IndexerExpr(IList<MemberSpec> indexers, TypeSpec queriedType, ElementAccess ea)
			: this(indexers, queriedType, ea.Expr, ea.Arguments, ea.Location)
		{
		}

		public IndexerExpr(IList<MemberSpec> indexers, TypeSpec queriedType, Expression instance, Arguments args, Location loc)
			: base(loc)
		{
			this.indexers = indexers;
			queried_type = queriedType;
			InstanceExpression = instance;
			arguments = args;
		}

		public override bool ContainsEmitWithAwait()
		{
			if (!base.ContainsEmitWithAwait())
			{
				return arguments.ContainsEmitWithAwait();
			}
			return true;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			if (base.ConditionalAccess)
			{
				Error_NullShortCircuitInsideExpressionTree(ec);
			}
			Arguments args = Arguments.CreateForExpressionTree(ec, arguments, InstanceExpression.CreateExpressionTree(ec), new TypeOfMethod(base.Getter, loc));
			return CreateExpressionFactoryCall(ec, "Call", args);
		}

		public override void EmitAssign(EmitContext ec, Expression source, bool leave_copy, bool isCompound)
		{
			LocalTemporary localTemporary = null;
			if (isCompound)
			{
				emitting_compound_assignment = true;
				if (source is DynamicExpressionStatement)
				{
					Emit(ec, false);
				}
				else
				{
					source.Emit(ec);
				}
				emitting_compound_assignment = false;
				if (has_await_arguments)
				{
					localTemporary = new LocalTemporary(base.Type);
					localTemporary.Store(ec);
					arguments.Add(new Argument(localTemporary));
					if (leave_copy)
					{
						temp = localTemporary;
					}
					has_await_arguments = false;
				}
				else
				{
					arguments = null;
					if (leave_copy)
					{
						ec.Emit(OpCodes.Dup);
						temp = new LocalTemporary(base.Type);
						temp.Store(ec);
					}
				}
			}
			else
			{
				if (leave_copy)
				{
					if (ec.HasSet(BuilderContext.Options.AsyncBody) && (arguments.ContainsEmitWithAwait() || source.ContainsEmitWithAwait()))
					{
						source = source.EmitToField(ec);
					}
					else
					{
						temp = new LocalTemporary(base.Type);
						source.Emit(ec);
						temp.Store(ec);
						source = temp;
					}
				}
				arguments.Add(new Argument(source));
			}
			CallEmitter callEmitter = new CallEmitter
			{
				InstanceExpression = InstanceExpression
			};
			if (arguments == null)
			{
				callEmitter.InstanceExpressionOnStack = true;
			}
			callEmitter.Emit(ec, base.Setter, arguments, loc);
			if (temp != null)
			{
				temp.Emit(ec);
				temp.Release(ec);
			}
			else if (leave_copy)
			{
				source.Emit(ec);
			}
			localTemporary?.Release(ec);
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			base.FlowAnalysis(fc);
			arguments.FlowAnalysis(fc);
			if (conditional_access_receiver)
			{
				fc.ConditionalAccessEnd();
			}
		}

		public override string GetSignatureForError()
		{
			return best_candidate.GetSignatureForError();
		}

		public override System.Linq.Expressions.Expression MakeAssignExpression(BuilderContext ctx, Expression source)
		{
			System.Linq.Expressions.Expression[] second = new System.Linq.Expressions.Expression[1] { source.MakeExpression(ctx) };
			return Arguments.MakeExpression(arguments, ctx).Concat(second).First();
		}

		public override System.Linq.Expressions.Expression MakeExpression(BuilderContext ctx)
		{
			System.Linq.Expressions.Expression[] array = Arguments.MakeExpression(arguments, ctx);
			return System.Linq.Expressions.Expression.Call(InstanceExpression.MakeExpression(ctx), (MethodInfo)base.Getter.GetMetaInfo(), array);
		}

		protected override Expression OverloadResolve(ResolveContext rc, Expression right_side)
		{
			if (best_candidate != null)
			{
				return this;
			}
			eclass = ExprClass.IndexerAccess;
			bool dynamic;
			this.arguments.Resolve(rc, out dynamic);
			if (indexers == null && InstanceExpression.Type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				dynamic = true;
			}
			else
			{
				OverloadResolver overloadResolver = new OverloadResolver(indexers, OverloadResolver.Restrictions.None, loc);
				overloadResolver.BaseMembersProvider = this;
				overloadResolver.InstanceQualifier = this;
				best_candidate = overloadResolver.ResolveMember<IndexerSpec>(rc, ref this.arguments);
				if (best_candidate != null)
				{
					type = overloadResolver.BestCandidateReturnType;
				}
				else if (!overloadResolver.BestCandidateIsDynamic)
				{
					return null;
				}
			}
			if (dynamic)
			{
				Arguments arguments = new Arguments(this.arguments.Count + 1);
				if (base.IsBase)
				{
					rc.Report.Error(1972, loc, "The indexer base access cannot be dynamically dispatched. Consider casting the dynamic arguments or eliminating the base access");
				}
				else
				{
					arguments.Add(new Argument(InstanceExpression));
				}
				arguments.AddRange(this.arguments);
				best_candidate = null;
				return new DynamicIndexBinder(arguments, loc);
			}
			if (right_side != null)
			{
				ResolveInstanceExpression(rc, right_side);
			}
			return this;
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			IndexerExpr indexerExpr = (IndexerExpr)t;
			if (arguments != null)
			{
				indexerExpr.arguments = arguments.Clone(clonectx);
			}
		}

		public void SetConditionalAccessReceiver()
		{
			conditional_access_receiver = true;
		}

		public override void SetTypeArguments(ResolveContext ec, TypeArguments ta)
		{
			Expression.Error_TypeArgumentsCannotBeUsed(ec, "indexer", GetSignatureForError(), loc);
		}

		IList<MemberSpec> OverloadResolver.IBaseMembersProvider.GetBaseMembers(TypeSpec baseType)
		{
			if (baseType != null)
			{
				return MemberCache.FindMembers(baseType, MemberCache.IndexerNameAlias, false);
			}
			return null;
		}

		IParametersMember OverloadResolver.IBaseMembersProvider.GetOverrideMemberParameters(MemberSpec member)
		{
			if (queried_type == member.DeclaringType)
			{
				return null;
			}
			return MemberCache.FindMember(filter: new MemberFilter(MemberCache.IndexerNameAlias, 0, MemberKind.Indexer, ((IndexerSpec)member).Parameters, null), container: queried_type, restrictions: BindingRestriction.InstanceOnly | BindingRestriction.OverrideOnly) as IParametersMember;
		}

		MethodGroupExpr OverloadResolver.IBaseMembersProvider.LookupExtensionMethod(ResolveContext rc)
		{
			return null;
		}
	}
}
