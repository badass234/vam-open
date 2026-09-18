using System;

namespace Mono.CSharp
{
	public class This : VariableReference
	{
		private sealed class ThisVariable : ILocalVariable
		{
			public static readonly ILocalVariable Instance = new ThisVariable();

			public void Emit(EmitContext ec)
			{
				ec.EmitThis();
			}

			public void EmitAssign(EmitContext ec)
			{
				throw new InvalidOperationException();
			}

			public void EmitAddressOf(EmitContext ec)
			{
				ec.EmitThis();
			}
		}

		protected VariableInfo variable_info;

		public override string Name => "this";

		public override bool IsLockedByStatement
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public override bool IsRef => type.IsStruct;

		public override bool IsSideEffectFree => true;

		protected override ILocalVariable Variable => ThisVariable.Instance;

		public override VariableInfo VariableInfo => variable_info;

		public override bool IsFixed => false;

		public This(Location loc)
		{
			base.loc = loc;
		}

		private void CheckStructThisDefiniteAssignment(FlowAnalysisContext fc)
		{
			if (variable_info != null && !fc.IsDefinitelyAssigned(variable_info))
			{
				fc.Report.Error(188, loc, "The `this' object cannot be used before all of its fields are assigned to");
			}
		}

		protected virtual void Error_ThisNotAvailable(ResolveContext ec)
		{
			if (ec.IsStatic && !ec.HasSet(ResolveContext.Options.ConstantScope))
			{
				ec.Report.Error(26, loc, "Keyword `this' is not valid in a static property, static method, or static field initializer");
			}
			else if (ec.CurrentAnonymousMethod != null)
			{
				ec.Report.Error(1673, loc, "Anonymous methods inside structs cannot access instance members of `this'. Consider copying `this' to a local variable outside the anonymous method and using the local instead");
			}
			else
			{
				ec.Report.Error(27, loc, "Keyword `this' is not available in the current context");
			}
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			CheckStructThisDefiniteAssignment(fc);
		}

		public override HoistedVariable GetHoistedVariable(AnonymousExpression ae)
		{
			return ae?.Storey?.HoistedThis;
		}

		public static bool IsThisAvailable(ResolveContext ec, bool ignoreAnonymous)
		{
			if (ec.IsStatic || ec.HasAny(ResolveContext.Options.FieldInitializerScope | ResolveContext.Options.BaseInitializer | ResolveContext.Options.ConstantScope))
			{
				return false;
			}
			if (ignoreAnonymous || ec.CurrentAnonymousMethod == null)
			{
				return true;
			}
			if (ec.CurrentType.IsStruct && !(ec.CurrentAnonymousMethod is StateMachineInitializer))
			{
				return false;
			}
			return true;
		}

		public virtual void ResolveBase(ResolveContext ec)
		{
			eclass = ExprClass.Variable;
			type = ec.CurrentType;
			if (!IsThisAvailable(ec, false))
			{
				Error_ThisNotAvailable(ec);
				return;
			}
			Block currentBlock = ec.CurrentBlock;
			if (currentBlock != null)
			{
				ToplevelBlock topBlock = currentBlock.ParametersBlock.TopBlock;
				if (topBlock.ThisVariable != null)
				{
					variable_info = topBlock.ThisVariable.VariableInfo;
				}
				AnonymousExpression currentAnonymousMethod = ec.CurrentAnonymousMethod;
				if (currentAnonymousMethod != null && ec.IsVariableCapturingRequired && !currentBlock.Explicit.HasCapturedThis)
				{
					topBlock.AddThisReferenceFromChildrenBlock(currentBlock.Explicit);
					currentAnonymousMethod.SetHasThisAccess();
				}
			}
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			ResolveBase(ec);
			return this;
		}

		public override Expression DoResolveLValue(ResolveContext ec, Expression right_side)
		{
			if (eclass == ExprClass.Unresolved)
			{
				ResolveBase(ec);
			}
			if (type.IsClass)
			{
				if (right_side == EmptyExpression.UnaryAddress)
				{
					ec.Report.Error(459, loc, "Cannot take the address of `this' because it is read-only");
				}
				else if (right_side == EmptyExpression.OutAccess)
				{
					ec.Report.Error(1605, loc, "Cannot pass `this' as a ref or out argument because it is read-only");
				}
				else
				{
					ec.Report.Error(1604, loc, "Cannot assign to `this' because it is read-only");
				}
			}
			return this;
		}

		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is This))
			{
				return false;
			}
			return true;
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
		}

		public override void SetHasAddressTaken()
		{
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
