namespace Mono.CSharp
{
	public class Argument
	{
		public enum AType : byte
		{
			None = 0,
			Ref = 1,
			Out = 2,
			Default = 3,
			DynamicTypeName = 4,
			ExtensionType = 5,
			ExtensionTypeConditionalAccess = 133,
			ConditionalAccessFlag = 128
		}

		public readonly AType ArgType;

		public Expression Expr;

		public bool IsByRef
		{
			get
			{
				if (ArgType != AType.Ref)
				{
					return ArgType == AType.Out;
				}
				return true;
			}
		}

		public bool IsDefaultArgument => ArgType == AType.Default;

		public bool IsExtensionType => (ArgType & AType.ExtensionType) == AType.ExtensionType;

		public Parameter.Modifier Modifier
		{
			get
			{
				switch (ArgType)
				{
				case AType.Out:
					return Parameter.Modifier.OUT;
				case AType.Ref:
					return Parameter.Modifier.REF;
				default:
					return Parameter.Modifier.NONE;
				}
			}
		}

		public TypeSpec Type => Expr.Type;

		public Argument(Expression expr, AType type)
		{
			Expr = expr;
			ArgType = type;
		}

		public Argument(Expression expr)
		{
			Expr = expr;
		}

		public Argument Clone(Expression expr)
		{
			Argument obj = (Argument)MemberwiseClone();
			obj.Expr = expr;
			return obj;
		}

		public Argument Clone(CloneContext clonectx)
		{
			return Clone(Expr.Clone(clonectx));
		}

		public virtual Expression CreateExpressionTree(ResolveContext ec)
		{
			if (ArgType == AType.Default)
			{
				ec.Report.Error(854, Expr.Location, "An expression tree cannot contain an invocation which uses optional parameter");
			}
			return Expr.CreateExpressionTree(ec);
		}

		public virtual void Emit(EmitContext ec)
		{
			if (!IsByRef)
			{
				if (ArgType == AType.ExtensionTypeConditionalAccess)
				{
					new InstanceEmitter(Expr, false).Emit(ec, true);
				}
				else
				{
					Expr.Emit(ec);
				}
				return;
			}
			AddressOp addressOp = AddressOp.Store;
			if (ArgType == AType.Ref)
			{
				addressOp |= AddressOp.Load;
			}
			((IMemoryLocation)Expr).AddressOf(ec, addressOp);
		}

		public Argument EmitToField(EmitContext ec, bool cloneResult)
		{
			Expression expression = Expr.EmitToField(ec);
			if (cloneResult && expression != Expr)
			{
				return new Argument(expression, ArgType);
			}
			Expr = expression;
			return this;
		}

		public void FlowAnalysis(FlowAnalysisContext fc)
		{
			if (ArgType == AType.Out)
			{
				VariableReference variableReference = Expr as VariableReference;
				if (variableReference != null)
				{
					if (variableReference.VariableInfo != null)
					{
						fc.SetVariableAssigned(variableReference.VariableInfo);
					}
				}
				else
				{
					(Expr as FieldExpr)?.SetFieldAssigned(fc);
				}
			}
			else
			{
				Expr.FlowAnalysis(fc);
			}
		}

		public string GetSignatureForError()
		{
			if (Expr.eclass == ExprClass.MethodGroup)
			{
				return Expr.ExprClassName;
			}
			return Expr.Type.GetSignatureForError();
		}

		public bool ResolveMethodGroup(ResolveContext ec)
		{
			SimpleName simpleName = Expr as SimpleName;
			if (simpleName != null)
			{
				Expr = simpleName.GetMethodGroup();
			}
			Expr = Expr.Resolve(ec, ResolveFlags.VariableOrValue | ResolveFlags.MethodGroup);
			if (Expr == null)
			{
				return false;
			}
			return true;
		}

		public void Resolve(ResolveContext ec)
		{
			if (ArgType != AType.Out)
			{
				Expr = Expr.Resolve(ec);
			}
			if (Expr != null && IsByRef)
			{
				Expr = Expr.ResolveLValue(ec, EmptyExpression.OutAccess);
			}
			if (Expr == null)
			{
				Expr = ErrorExpression.Instance;
			}
		}
	}
}
