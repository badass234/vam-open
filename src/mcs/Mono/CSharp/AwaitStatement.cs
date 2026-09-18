using System.Reflection.Emit;
using Mono.CSharp.Linq;

namespace Mono.CSharp
{
	public class AwaitStatement : YieldStatement<AsyncInitializer>
	{
		public sealed class AwaitableMemberAccess : MemberAccess
		{
			public bool ProbingMode { get; set; }

			public AwaitableMemberAccess(Expression expr)
				: base(expr, "GetAwaiter")
			{
			}

			public override void Error_TypeDoesNotContainDefinition(ResolveContext rc, TypeSpec type, string name)
			{
				Error_OperatorCannotBeApplied(rc, type);
			}

			protected override void Error_OperatorCannotBeApplied(ResolveContext rc, TypeSpec type)
			{
				if (!ProbingMode)
				{
					Invocation invocation = base.LeftExpression as Invocation;
					if (invocation != null && invocation.MethodGroup != null && (invocation.MethodGroup.BestCandidate.Modifiers & Modifiers.ASYNC) != 0)
					{
						rc.Report.Error(4008, loc, "Cannot await void method `{0}'. Consider changing method return type to `Task'", invocation.GetSignatureForError());
					}
					else if (type != InternalType.ErrorType)
					{
						rc.Report.Error(4001, loc, "Cannot await `{0}' expression", type.GetSignatureForError());
					}
				}
			}
		}

		private sealed class GetResultInvocation : Invocation
		{
			public GetResultInvocation(MethodGroupExpr mge, Arguments arguments)
				: base(null, arguments)
			{
				mg = mge;
				type = mg.BestCandidateReturnType;
			}

			public override Expression EmitToField(EmitContext ec)
			{
				return this;
			}
		}

		private Field awaiter;

		private AwaiterDefinition awaiter_definition;

		private TypeSpec type;

		private TypeSpec result_type;

		private bool IsDynamic => awaiter_definition == null;

		public TypeSpec ResultType => result_type;

		public AwaitStatement(Expression expr, Location loc)
			: base(expr, loc)
		{
			unwind_protect = true;
		}

		protected override void DoEmit(EmitContext ec)
		{
			using (ec.With(BuilderContext.Options.OmitDebugInfo, true))
			{
				GetResultExpression(ec).Emit(ec);
			}
		}

		public Expression GetResultExpression(EmitContext ec)
		{
			FieldExpr fieldExpr = new FieldExpr(awaiter, loc);
			fieldExpr.InstanceExpression = new CompilerGeneratedThis(ec.CurrentType, loc);
			if (IsDynamic)
			{
				ResolveContext rc = new ResolveContext(ec.MemberContext);
				return new Invocation(new MemberAccess(fieldExpr, "GetResult"), new Arguments(0)).Resolve(rc);
			}
			MethodGroupExpr methodGroupExpr = MethodGroupExpr.CreatePredefined(awaiter_definition.GetResult, fieldExpr.Type, loc);
			methodGroupExpr.InstanceExpression = fieldExpr;
			return new GetResultInvocation(methodGroupExpr, new Arguments(0));
		}

		public void EmitPrologue(EmitContext ec)
		{
			awaiter = ((AsyncTaskStorey)machine_initializer.Storey).AddAwaiter(expr.Type);
			FieldExpr fieldExpr = new FieldExpr(awaiter, loc);
			fieldExpr.InstanceExpression = new CompilerGeneratedThis(ec.CurrentType, loc);
			Label label = ec.DefineLabel();
			using (ec.With(BuilderContext.Options.OmitDebugInfo, true))
			{
				fieldExpr.EmitAssign(ec, expr, false, false);
				Expression expression;
				if (IsDynamic)
				{
					ResolveContext rc = new ResolveContext(ec.MemberContext);
					Arguments arguments = new Arguments(1);
					arguments.Add(new Argument(fieldExpr));
					expression = new DynamicMemberBinder("IsCompleted", arguments, loc).Resolve(rc);
					arguments = new Arguments(1);
					arguments.Add(new Argument(expression));
					expression = new DynamicConversion(ec.Module.Compiler.BuiltinTypes.Bool, CSharpBinderFlags.None, arguments, loc).Resolve(rc);
				}
				else
				{
					PropertyExpr propertyExpr = PropertyExpr.CreatePredefined(awaiter_definition.IsCompleted, loc);
					propertyExpr.InstanceExpression = fieldExpr;
					expression = propertyExpr;
				}
				expression.EmitBranchable(ec, label, true);
			}
			base.DoEmit(ec);
			ec.AssertEmptyStack();
			AsyncTaskStorey asyncTaskStorey = (AsyncTaskStorey)machine_initializer.Storey;
			if (IsDynamic)
			{
				asyncTaskStorey.EmitAwaitOnCompletedDynamic(ec, fieldExpr);
			}
			else
			{
				asyncTaskStorey.EmitAwaitOnCompleted(ec, fieldExpr);
			}
			machine_initializer.EmitLeave(ec, unwind_protect);
			ec.MarkLabel(resume_point);
			ec.MarkLabel(label);
		}

		public void EmitStatement(EmitContext ec)
		{
			EmitPrologue(ec);
			DoEmit(ec);
			awaiter.IsAvailableForReuse = true;
			if (ResultType.Kind != MemberKind.Void)
			{
				ec.Emit(OpCodes.Pop);
			}
		}

		private void Error_WrongAwaiterPattern(ResolveContext rc, TypeSpec awaiter)
		{
			rc.Report.Error(4011, loc, "The awaiter type `{0}' must have suitable IsCompleted and GetResult members", awaiter.GetSignatureForError());
		}

		public override bool Resolve(BlockContext bc)
		{
			if (bc.CurrentBlock is QueryBlock)
			{
				bc.Report.Error(1995, loc, "The `await' operator may only be used in a query expression within the first collection expression of the initial `from' clause or within the collection expression of a `join' clause");
				return false;
			}
			if (!base.Resolve(bc))
			{
				return false;
			}
			type = expr.Type;
			Arguments arguments = new Arguments(0);
			if (type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				result_type = type;
				expr = new Invocation(new MemberAccess(expr, "GetAwaiter"), arguments).Resolve(bc);
				return true;
			}
			Expression expression = new AwaitableMemberAccess(expr).Resolve(bc);
			if (expression == null)
			{
				return false;
			}
			SessionReportPrinter sessionReportPrinter = new SessionReportPrinter();
			ReportPrinter printer = bc.Report.SetPrinter(sessionReportPrinter);
			expression = new Invocation(expression, arguments).Resolve(bc);
			bc.Report.SetPrinter(printer);
			if (sessionReportPrinter.ErrorsCount > 0 || !MemberAccess.IsValidDotExpression(expression.Type))
			{
				bc.Report.Error(1986, expr.Location, "The `await' operand type `{0}' must have suitable GetAwaiter method", expr.Type.GetSignatureForError());
				return false;
			}
			TypeSpec typeSpec = expression.Type;
			awaiter_definition = bc.Module.GetAwaiter(typeSpec);
			if (!awaiter_definition.IsValidPattern)
			{
				Error_WrongAwaiterPattern(bc, typeSpec);
				return false;
			}
			if (!awaiter_definition.INotifyCompletion)
			{
				bc.Report.Error(4027, loc, "The awaiter type `{0}' must implement interface `{1}'", typeSpec.GetSignatureForError(), bc.Module.PredefinedTypes.INotifyCompletion.GetSignatureForError());
				return false;
			}
			expr = expression;
			result_type = awaiter_definition.GetResult.ReturnType;
			return true;
		}
	}
}
