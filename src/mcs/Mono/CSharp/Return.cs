using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class Return : ExitStatement
	{
		private Expression expr;

		public Expression Expr
		{
			get
			{
				return expr;
			}
			protected set
			{
				expr = value;
			}
		}

		protected override bool IsLocalExit => false;

		public Return(Expression expr, Location l)
		{
			this.expr = expr;
			loc = l;
		}

		protected override bool DoResolve(BlockContext ec)
		{
			TypeSpec typeSpec = ec.ReturnType;
			if (expr == null)
			{
				if (typeSpec.Kind == MemberKind.Void || typeSpec == InternalType.ErrorType)
				{
					return true;
				}
				if (ec.CurrentAnonymousMethod is AsyncInitializer)
				{
					AsyncTaskStorey asyncTaskStorey = (AsyncTaskStorey)ec.CurrentAnonymousMethod.Storey;
					if (asyncTaskStorey.ReturnType == ec.Module.PredefinedTypes.Task.TypeSpec)
					{
						expr = EmptyExpression.Null;
						return true;
					}
					if (asyncTaskStorey.ReturnType.IsGenericTask)
					{
						typeSpec = asyncTaskStorey.ReturnType.TypeArguments[0];
					}
				}
				if (ec.CurrentIterator != null)
				{
					Error_ReturnFromIterator(ec);
				}
				else if (typeSpec != InternalType.ErrorType)
				{
					ec.Report.Error(126, loc, "An object of a type convertible to `{0}' is required for the return statement", typeSpec.GetSignatureForError());
				}
				return false;
			}
			expr = expr.Resolve(ec);
			AnonymousExpression currentAnonymousMethod = ec.CurrentAnonymousMethod;
			if (currentAnonymousMethod == null)
			{
				if (typeSpec.Kind == MemberKind.Void)
				{
					ec.Report.Error(127, loc, "`{0}': A return keyword must not be followed by any expression when method returns void", ec.GetSignatureForError());
					return false;
				}
			}
			else
			{
				if (currentAnonymousMethod.IsIterator)
				{
					Error_ReturnFromIterator(ec);
					return false;
				}
				AsyncInitializer asyncInitializer = currentAnonymousMethod as AsyncInitializer;
				if (asyncInitializer != null)
				{
					if (expr != null)
					{
						TypeSpec returnType = ((AsyncTaskStorey)currentAnonymousMethod.Storey).ReturnType;
						if (returnType == null && asyncInitializer.ReturnTypeInference != null)
						{
							if (expr.Type.Kind == MemberKind.Void && !(this is ContextualReturn))
							{
								ec.Report.Error(4029, loc, "Cannot return an expression of type `void'");
							}
							else
							{
								asyncInitializer.ReturnTypeInference.AddCommonTypeBoundAsync(expr.Type);
							}
							return true;
						}
						if (returnType.Kind == MemberKind.Void)
						{
							ec.Report.Error(8030, loc, "Anonymous function or lambda expression converted to a void returning delegate cannot return a value");
							return false;
						}
						if (!returnType.IsGenericTask)
						{
							if (this is ContextualReturn)
							{
								return true;
							}
							if (asyncInitializer.DelegateType != null)
							{
								ec.Report.Error(8031, loc, "Async lambda expression or anonymous method converted to a `Task' cannot return a value. Consider returning `Task<T>'");
							}
							else
							{
								ec.Report.Error(1997, loc, "`{0}': A return keyword must not be followed by an expression when async method returns `Task'. Consider using `Task<T>' return type", ec.GetSignatureForError());
							}
							return false;
						}
						if (expr.Type == returnType)
						{
							ec.Report.Error(4016, loc, "`{0}': The return expression type of async method must be `{1}' rather than `Task<{1}>'", ec.GetSignatureForError(), returnType.TypeArguments[0].GetSignatureForError());
						}
						else
						{
							typeSpec = returnType.TypeArguments[0];
						}
					}
				}
				else
				{
					if (typeSpec.Kind == MemberKind.Void)
					{
						ec.Report.Error(8030, loc, "Anonymous function or lambda expression converted to a void returning delegate cannot return a value");
						return false;
					}
					AnonymousMethodBody anonymousMethodBody = currentAnonymousMethod as AnonymousMethodBody;
					if (anonymousMethodBody != null && expr != null)
					{
						if (anonymousMethodBody.ReturnTypeInference != null)
						{
							anonymousMethodBody.ReturnTypeInference.AddCommonTypeBound(expr.Type);
							return true;
						}
						if (this is ContextualReturn && !ec.IsInProbingMode && ec.Module.Compiler.Settings.Optimize)
						{
							anonymousMethodBody.DirectMethodGroupConversion = expr.CanReduceLambda(anonymousMethodBody);
						}
					}
				}
			}
			if (expr == null)
			{
				return false;
			}
			if (expr.Type != typeSpec && expr.Type != InternalType.ErrorType)
			{
				expr = Convert.ImplicitConversionRequired(ec, expr, typeSpec, loc);
				if (expr == null)
				{
					if (currentAnonymousMethod != null && typeSpec == ec.ReturnType)
					{
						ec.Report.Error(1662, loc, "Cannot convert `{0}' to delegate type `{1}' because some of the return types in the block are not implicitly convertible to the delegate return type", currentAnonymousMethod.ContainerType, currentAnonymousMethod.GetSignatureForError());
					}
					return false;
				}
			}
			return true;
		}

		protected override void DoEmit(EmitContext ec)
		{
			if (expr != null)
			{
				AsyncInitializer asyncInitializer = ec.CurrentAnonymousMethod as AsyncInitializer;
				if (asyncInitializer != null)
				{
					AsyncTaskStorey asyncTaskStorey = (AsyncTaskStorey)asyncInitializer.Storey;
					Label label = asyncInitializer.BodyEnd;
					if (asyncTaskStorey.HoistedReturnValue != null)
					{
						if (ec.TryFinallyUnwind != null)
						{
							if (asyncTaskStorey.HoistedReturnValue is VariableReference)
							{
								asyncTaskStorey.HoistedReturnValue = ec.GetTemporaryField(asyncTaskStorey.HoistedReturnValue.Type);
							}
							label = TryFinally.EmitRedirectedReturn(ec, asyncInitializer);
						}
						((IAssignMethod)asyncTaskStorey.HoistedReturnValue).EmitAssign(ec, expr, false, false);
						ec.EmitEpilogue();
					}
					else
					{
						expr.Emit(ec);
						if (ec.TryFinallyUnwind != null)
						{
							label = TryFinally.EmitRedirectedReturn(ec, asyncInitializer);
						}
					}
					ec.Emit(OpCodes.Leave, label);
					return;
				}
				expr.Emit(ec);
				ec.EmitEpilogue();
				if (unwind_protect || ec.EmitAccurateDebugInfo)
				{
					ec.Emit(OpCodes.Stloc, ec.TemporaryReturn());
				}
			}
			if (unwind_protect)
			{
				ec.Emit(OpCodes.Leave, ec.CreateReturnLabel());
			}
			else if (ec.EmitAccurateDebugInfo)
			{
				ec.Emit(OpCodes.Br, ec.CreateReturnLabel());
			}
			else
			{
				ec.Emit(OpCodes.Ret);
			}
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			if (expr != null)
			{
				expr.FlowAnalysis(fc);
			}
			base.DoFlowAnalysis(fc);
			return true;
		}

		private void Error_ReturnFromIterator(ResolveContext rc)
		{
			rc.Report.Error(1622, loc, "Cannot return a value from iterators. Use the yield return statement to return a value, or yield break to end the iteration");
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			base.MarkReachable(rc);
			return Reachability.CreateUnreachable();
		}

		protected override void CloneTo(CloneContext clonectx, Statement t)
		{
			Return obj = (Return)t;
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
