using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class Lock : TryFinallyBlock
	{
		private Expression expr;

		private TemporaryVariableReference expr_copy;

		private TemporaryVariableReference lock_taken;

		public Expression Expr => expr;

		public Lock(Expression expr, Statement stmt, Location loc)
			: base(stmt, loc)
		{
			this.expr = expr;
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			expr.FlowAnalysis(fc);
			return base.DoFlowAnalysis(fc);
		}

		public override bool Resolve(BlockContext ec)
		{
			expr = expr.Resolve(ec);
			if (expr == null)
			{
				return false;
			}
			if (!TypeSpec.IsReferenceType(expr.Type) && expr.Type != InternalType.ErrorType)
			{
				ec.Report.Error(185, loc, "`{0}' is not a reference type as required by the lock statement", expr.Type.GetSignatureForError());
			}
			if (expr.Type.IsGenericParameter)
			{
				expr = Convert.ImplicitTypeParameterConversion(expr, (TypeParameterSpec)expr.Type, ec.BuiltinTypes.Object);
			}
			VariableReference variableReference = expr as VariableReference;
			bool isLockedByStatement;
			if (variableReference != null)
			{
				isLockedByStatement = variableReference.IsLockedByStatement;
				variableReference.IsLockedByStatement = true;
			}
			else
			{
				variableReference = null;
				isLockedByStatement = false;
			}
			expr_copy = TemporaryVariableReference.Create(ec.BuiltinTypes.Object, ec.CurrentBlock, loc);
			expr_copy.Resolve(ec);
			if (ResolvePredefinedMethods(ec) > 1)
			{
				lock_taken = TemporaryVariableReference.Create(ec.BuiltinTypes.Bool, ec.CurrentBlock, loc);
				lock_taken.Resolve(ec);
			}
			using (ec.Set(ResolveContext.Options.LockScope))
			{
				base.Resolve(ec);
			}
			if (variableReference != null)
			{
				variableReference.IsLockedByStatement = isLockedByStatement;
			}
			return true;
		}

		protected override void EmitTryBodyPrepare(EmitContext ec)
		{
			expr_copy.EmitAssign(ec, expr);
			if (lock_taken != null)
			{
				lock_taken.EmitAssign(ec, new BoolLiteral(ec.BuiltinTypes, false, loc));
			}
			else
			{
				expr_copy.Emit(ec);
				ec.Emit(OpCodes.Call, ec.Module.PredefinedMembers.MonitorEnter.Get());
			}
			base.EmitTryBodyPrepare(ec);
		}

		protected override void EmitTryBody(EmitContext ec)
		{
			if (lock_taken != null)
			{
				expr_copy.Emit(ec);
				lock_taken.LocalInfo.CreateBuilder(ec);
				lock_taken.AddressOf(ec, AddressOp.Load);
				ec.Emit(OpCodes.Call, ec.Module.PredefinedMembers.MonitorEnter_v4.Get());
			}
			base.Statement.Emit(ec);
		}

		public override void EmitFinallyBody(EmitContext ec)
		{
			Label label = ec.DefineLabel();
			if (lock_taken != null)
			{
				lock_taken.Emit(ec);
				ec.Emit(OpCodes.Brfalse_S, label);
			}
			expr_copy.Emit(ec);
			MethodSpec methodSpec = ec.Module.PredefinedMembers.MonitorExit.Resolve(loc);
			if (methodSpec != null)
			{
				ec.Emit(OpCodes.Call, methodSpec);
			}
			ec.MarkLabel(label);
		}

		private int ResolvePredefinedMethods(ResolveContext rc)
		{
			if (rc.Module.PredefinedMembers.MonitorEnter_v4.Get() != null)
			{
				return 4;
			}
			if (rc.Module.PredefinedMembers.MonitorEnter.Get() != null)
			{
				return 1;
			}
			rc.Module.PredefinedMembers.MonitorEnter_v4.Resolve(loc);
			return 0;
		}

		protected override void CloneTo(CloneContext clonectx, Statement t)
		{
			Lock obj = (Lock)t;
			obj.expr = expr.Clone(clonectx);
			obj.stmt = base.Statement.Clone(clonectx);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
