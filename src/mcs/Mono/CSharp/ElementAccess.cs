using System;
using System.Collections.Generic;

namespace Mono.CSharp
{
	public class ElementAccess : Expression
	{
		public Arguments Arguments;

		public Expression Expr;

		public bool ConditionalAccess { get; set; }

		public override Location StartLocation => Expr.StartLocation;

		public ElementAccess(Expression e, Arguments args, Location loc)
		{
			Expr = e;
			base.loc = loc;
			Arguments = args;
		}

		public override bool ContainsEmitWithAwait()
		{
			if (!Expr.ContainsEmitWithAwait())
			{
				return Arguments.ContainsEmitWithAwait();
			}
			return true;
		}

		private Expression CreateAccessExpression(ResolveContext ec, bool conditionalAccessReceiver)
		{
			Expr = Expr.Resolve(ec);
			if (Expr == null)
			{
				return null;
			}
			type = Expr.Type;
			if (ConditionalAccess && !Expression.IsNullPropagatingValid(type))
			{
				Error_OperatorCannotBeApplied(ec, loc, "?", type);
				return null;
			}
			if (type.IsArray)
			{
				return new ArrayAccess(this, loc)
				{
					ConditionalAccess = ConditionalAccess,
					ConditionalAccessReceiver = conditionalAccessReceiver
				};
			}
			if (type.IsPointer)
			{
				return Expr.MakePointerAccess(ec, type, Arguments);
			}
			FieldExpr fieldExpr = Expr as FieldExpr;
			if (fieldExpr != null)
			{
				FixedFieldSpec fixedFieldSpec = fieldExpr.Spec as FixedFieldSpec;
				if (fixedFieldSpec != null)
				{
					return Expr.MakePointerAccess(ec, fixedFieldSpec.ElementType, Arguments);
				}
			}
			IList<MemberSpec> list = MemberCache.FindMembers(type, MemberCache.IndexerNameAlias, false);
			if (list != null || type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				IndexerExpr indexerExpr = new IndexerExpr(list, type, this)
				{
					ConditionalAccess = ConditionalAccess
				};
				if (conditionalAccessReceiver)
				{
					indexerExpr.SetConditionalAccessReceiver();
				}
				return indexerExpr;
			}
			Error_CannotApplyIndexing(ec, type, loc);
			return null;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Arguments args = Arguments.CreateForExpressionTree(ec, Arguments, Expr.CreateExpressionTree(ec));
			return CreateExpressionFactoryCall(ec, "ArrayIndex", args);
		}

		public static void Error_CannotApplyIndexing(ResolveContext rc, TypeSpec type, Location loc)
		{
			if (type != InternalType.ErrorType)
			{
				rc.Report.Error(21, loc, "Cannot apply indexing with [] to an expression of type `{0}'", type.GetSignatureForError());
			}
		}

		public override bool HasConditionalAccess()
		{
			if (!ConditionalAccess)
			{
				return Expr.HasConditionalAccess();
			}
			return true;
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			if (!rc.HasSet(ResolveContext.Options.ConditionalAccessReceiver) && HasConditionalAccess())
			{
				using (rc.Set(ResolveContext.Options.ConditionalAccessReceiver))
				{
					return CreateAccessExpression(rc, true)?.Resolve(rc);
				}
			}
			return CreateAccessExpression(rc, false)?.Resolve(rc);
		}

		public override Expression DoResolveLValue(ResolveContext ec, Expression rhs)
		{
			return CreateAccessExpression(ec, false)?.ResolveLValue(ec, rhs);
		}

		public override void Emit(EmitContext ec)
		{
			throw new Exception("Should never be reached");
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			Expr.FlowAnalysis(fc);
			if (ConditionalAccess)
			{
				fc.BranchConditionalAccessDefiniteAssignment();
			}
			Arguments.FlowAnalysis(fc);
		}

		public override string GetSignatureForError()
		{
			return Expr.GetSignatureForError();
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			ElementAccess elementAccess = (ElementAccess)t;
			elementAccess.Expr = Expr.Clone(clonectx);
			if (Arguments != null)
			{
				elementAccess.Arguments = Arguments.Clone(clonectx);
			}
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
