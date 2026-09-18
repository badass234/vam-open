using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class Throw : Statement
	{
		private Expression expr;

		public Expression Expr => expr;

		public Throw(Expression expr, Location l)
		{
			this.expr = expr;
			loc = l;
		}

		public override bool Resolve(BlockContext ec)
		{
			if (expr == null)
			{
				if (!ec.HasSet(ResolveContext.Options.CatchScope))
				{
					ec.Report.Error(156, loc, "A throw statement with no arguments is not allowed outside of a catch clause");
				}
				else if (ec.HasSet(ResolveContext.Options.FinallyScope))
				{
					Block block = ec.CurrentBlock;
					while (block != null && !block.IsCatchBlock)
					{
						if (block.IsFinallyBlock)
						{
							ec.Report.Error(724, loc, "A throw statement with no arguments is not allowed inside of a finally clause nested inside of the innermost catch clause");
							break;
						}
						block = block.Parent;
					}
				}
				return true;
			}
			expr = expr.Resolve(ec, ResolveFlags.VariableOrValue | ResolveFlags.Type);
			if (expr == null)
			{
				return false;
			}
			BuiltinTypeSpec exception = ec.BuiltinTypes.Exception;
			if (Convert.ImplicitConversionExists(ec, expr, exception))
			{
				expr = Convert.ImplicitConversion(ec, expr, exception, loc);
			}
			else
			{
				ec.Report.Error(155, expr.Location, "The type caught or thrown must be derived from System.Exception");
			}
			return true;
		}

		protected override void DoEmit(EmitContext ec)
		{
			if (expr == null)
			{
				LocalVariable asyncThrowVariable = ec.AsyncThrowVariable;
				if (asyncThrowVariable != null)
				{
					if (asyncThrowVariable.HoistedVariant != null)
					{
						asyncThrowVariable.HoistedVariant.Emit(ec);
					}
					else
					{
						asyncThrowVariable.Emit(ec);
					}
					ec.Emit(OpCodes.Throw);
				}
				else
				{
					ec.Emit(OpCodes.Rethrow);
				}
			}
			else
			{
				expr.Emit(ec);
				ec.Emit(OpCodes.Throw);
			}
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			if (expr != null)
			{
				expr.FlowAnalysis(fc);
			}
			return true;
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			base.MarkReachable(rc);
			return Reachability.CreateUnreachable();
		}

		protected override void CloneTo(CloneContext clonectx, Statement t)
		{
			Throw obj = (Throw)t;
			if (expr != null)
			{
				obj.expr = expr.Clone(clonectx);
			}
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
