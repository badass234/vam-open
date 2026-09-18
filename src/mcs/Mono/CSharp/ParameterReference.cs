namespace Mono.CSharp
{
	public class ParameterReference : VariableReference
	{
		protected ParametersBlock.ParameterInfo pi;

		public override bool IsLockedByStatement
		{
			get
			{
				return pi.IsLocked;
			}
			set
			{
				pi.IsLocked = value;
			}
		}

		public override bool IsRef => (pi.Parameter.ModFlags & Parameter.Modifier.RefOutMask) != 0;

		private bool HasOutModifier => (pi.Parameter.ModFlags & Parameter.Modifier.OUT) != 0;

		public override bool IsFixed => !IsRef;

		public override string Name => Parameter.Name;

		public Parameter Parameter => pi.Parameter;

		public override VariableInfo VariableInfo => pi.VariableInfo;

		protected override ILocalVariable Variable => Parameter;

		public ParameterReference(ParametersBlock.ParameterInfo pi, Location loc)
		{
			this.pi = pi;
			base.loc = loc;
		}

		public override HoistedVariable GetHoistedVariable(AnonymousExpression ae)
		{
			return pi.Parameter.HoistedVariant;
		}

		public override void AddressOf(EmitContext ec, AddressOp mode)
		{
			if (IsRef)
			{
				EmitLoad(ec);
			}
			else
			{
				base.AddressOf(ec, mode);
			}
		}

		public override void SetHasAddressTaken()
		{
			Parameter.HasAddressTaken = true;
		}

		private bool DoResolveBase(ResolveContext ec)
		{
			if (eclass != ExprClass.Unresolved)
			{
				return true;
			}
			type = pi.ParameterType;
			eclass = ExprClass.Variable;
			if (ec.MustCaptureVariable(pi))
			{
				if (Parameter.HasAddressTaken)
				{
					AnonymousMethodExpression.Error_AddressOfCapturedVar(ec, this, loc);
				}
				if (IsRef)
				{
					ec.Report.Error(1628, loc, "Parameter `{0}' cannot be used inside `{1}' when using `ref' or `out' modifier", Name, ec.CurrentAnonymousMethod.ContainerType);
				}
				if (ec.IsVariableCapturingRequired && !pi.Block.ParametersBlock.IsExpressionTree)
				{
					pi.Block.Explicit.CreateAnonymousMethodStorey(ec).CaptureParameter(ec, pi, this);
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			ParameterReference parameterReference = obj as ParameterReference;
			if (parameterReference == null)
			{
				return false;
			}
			return Name == parameterReference.Name;
		}

		protected override void CloneTo(CloneContext clonectx, Expression target)
		{
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			HoistedVariable hoistedVariable = GetHoistedVariable(ec);
			if (hoistedVariable != null)
			{
				return hoistedVariable.CreateExpressionTree();
			}
			return Parameter.ExpressionTreeVariableReference();
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			if (!DoResolveBase(ec))
			{
				return null;
			}
			return this;
		}

		public override Expression DoResolveLValue(ResolveContext ec, Expression right_side)
		{
			if (!DoResolveBase(ec))
			{
				return null;
			}
			if (Parameter.HoistedVariant != null)
			{
				Parameter.HoistedVariant.IsAssigned = true;
			}
			return base.DoResolveLValue(ec, right_side);
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			VariableInfo variableInfo = VariableInfo;
			if (variableInfo != null && !fc.IsDefinitelyAssigned(variableInfo))
			{
				fc.Report.Error(269, loc, "Use of unassigned out parameter `{0}'", Name);
				fc.SetVariableAssigned(variableInfo);
			}
		}
	}
}
