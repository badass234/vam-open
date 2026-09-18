using System;

namespace Mono.CSharp
{
	public class DeclarationExpression : Expression, IMemoryLocation
	{
		private LocalVariableReference lvr;

		public LocalVariable Variable { get; set; }

		public Expression Initializer { get; set; }

		public FullNamedExpression VariableType { get; set; }

		public DeclarationExpression(FullNamedExpression variableType, LocalVariable variable)
		{
			VariableType = variableType;
			Variable = variable;
			loc = variable.Location;
		}

		public void AddressOf(EmitContext ec, AddressOp mode)
		{
			Variable.CreateBuilder(ec);
			if (Initializer != null)
			{
				lvr.EmitAssign(ec, Initializer, false, false);
			}
			lvr.AddressOf(ec, mode);
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			DeclarationExpression declarationExpression = (DeclarationExpression)t;
			declarationExpression.VariableType = (FullNamedExpression)VariableType.Clone(clonectx);
			if (Initializer != null)
			{
				declarationExpression.Initializer = Initializer.Clone(clonectx);
			}
		}

		public override Expression CreateExpressionTree(ResolveContext rc)
		{
			rc.Report.Error(8046, loc, "An expression tree cannot contain a declaration expression");
			return null;
		}

		private bool DoResolveCommon(ResolveContext rc)
		{
			VarExpr varExpr = VariableType as VarExpr;
			if (varExpr != null)
			{
				type = InternalType.VarOutType;
			}
			else
			{
				type = VariableType.ResolveAsType(rc);
				if (type == null)
				{
					return false;
				}
			}
			if (Initializer != null)
			{
				Initializer = Initializer.Resolve(rc);
				if (varExpr != null && Initializer != null && varExpr.InferType(rc, Initializer))
				{
					type = varExpr.Type;
				}
			}
			Variable.Type = type;
			lvr = new LocalVariableReference(Variable, loc);
			eclass = ExprClass.Variable;
			return true;
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			if (DoResolveCommon(rc))
			{
				lvr.Resolve(rc);
			}
			return this;
		}

		public override Expression DoResolveLValue(ResolveContext rc, Expression right_side)
		{
			if (lvr == null && DoResolveCommon(rc))
			{
				lvr.ResolveLValue(rc, right_side);
			}
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			throw new NotImplementedException();
		}
	}
}
