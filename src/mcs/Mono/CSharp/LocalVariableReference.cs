namespace Mono.CSharp
{
	public class LocalVariableReference : VariableReference
	{
		public LocalVariable local_info;

		public override VariableInfo VariableInfo => local_info.VariableInfo;

		public override bool IsFixed => true;

		public override bool IsLockedByStatement
		{
			get
			{
				return local_info.IsLocked;
			}
			set
			{
				local_info.IsLocked = value;
			}
		}

		public override bool IsRef => false;

		public override string Name => local_info.Name;

		protected override ILocalVariable Variable => local_info;

		public LocalVariableReference(LocalVariable li, Location l)
		{
			local_info = li;
			loc = l;
		}

		public override HoistedVariable GetHoistedVariable(AnonymousExpression ae)
		{
			return local_info.HoistedVariant;
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			VariableInfo variableInfo = VariableInfo;
			if (variableInfo != null && !fc.IsDefinitelyAssigned(variableInfo))
			{
				fc.Report.Error(165, loc, "Use of unassigned local variable `{0}'", Name);
				variableInfo.SetAssigned(fc.DefiniteAssignment, true);
			}
		}

		public override void SetHasAddressTaken()
		{
			local_info.SetHasAddressTaken();
		}

		private void DoResolveBase(ResolveContext ec)
		{
			eclass = ExprClass.Variable;
			type = local_info.Type;
			if (ec.MustCaptureVariable(local_info))
			{
				if (local_info.AddressTaken)
				{
					AnonymousMethodExpression.Error_AddressOfCapturedVar(ec, this, loc);
				}
				else if (local_info.IsFixed)
				{
					ec.Report.Error(1764, loc, "Cannot use fixed local `{0}' inside an anonymous method, lambda expression or query expression", GetSignatureForError());
				}
				if (ec.IsVariableCapturingRequired)
				{
					local_info.Block.Explicit.CreateAnonymousMethodStorey(ec).CaptureLocalVariable(ec, local_info);
				}
			}
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			local_info.SetIsUsed();
			DoResolveBase(ec);
			if (local_info.Type == InternalType.VarOutType)
			{
				ec.Report.Error(8048, loc, "Cannot use uninitialized variable `{0}'", GetSignatureForError());
				type = InternalType.ErrorType;
			}
			return this;
		}

		public override Expression DoResolveLValue(ResolveContext ec, Expression rhs)
		{
			if (rhs == EmptyExpression.OutAccess || rhs.eclass == ExprClass.PropertyAccess || rhs.eclass == ExprClass.IndexerAccess)
			{
				local_info.SetIsUsed();
			}
			if (local_info.IsReadonly && !ec.HasAny(ResolveContext.Options.FieldInitializerScope | ResolveContext.Options.UsingInitializerScope) && rhs != EmptyExpression.LValueMemberAccess)
			{
				int code;
				string format;
				if (rhs == EmptyExpression.OutAccess)
				{
					code = 1657;
					format = "Cannot pass `{0}' as a ref or out argument because it is a `{1}'";
				}
				else if (rhs == EmptyExpression.LValueMemberOutAccess)
				{
					code = 1655;
					format = "Cannot pass members of `{0}' as ref or out arguments because it is a `{1}'";
				}
				else if (rhs == EmptyExpression.UnaryAddress)
				{
					code = 459;
					format = "Cannot take the address of {1} `{0}'";
				}
				else
				{
					code = 1656;
					format = "Cannot assign to `{0}' because it is a `{1}'";
				}
				ec.Report.Error(code, loc, format, Name, local_info.GetReadOnlyContext());
			}
			if (eclass == ExprClass.Unresolved)
			{
				DoResolveBase(ec);
			}
			return base.DoResolveLValue(ec, rhs);
		}

		public override int GetHashCode()
		{
			return local_info.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			LocalVariableReference localVariableReference = obj as LocalVariableReference;
			if (localVariableReference == null)
			{
				return false;
			}
			return local_info == localVariableReference.local_info;
		}

		public override string ToString()
		{
			return $"{GetType()} ({Name}:{loc})";
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
		}
	}
}
