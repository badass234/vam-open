using System;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class FieldExpr : MemberExpr, IDynamicAssign, IAssignMethod, IMemoryLocation, IVariableReference, IFixedExpression
	{
		protected FieldSpec spec;

		private VariableInfo variable_info;

		private LocalTemporary temp;

		private bool prepared;

		public override string Name => spec.Name;

		public bool IsHoisted => (InstanceExpression as IVariableReference)?.IsHoisted ?? false;

		public override bool IsInstance => !spec.IsStatic;

		public override bool IsStatic => spec.IsStatic;

		public override string KindName => "field";

		public FieldSpec Spec => spec;

		protected override TypeSpec DeclaringType => spec.DeclaringType;

		public VariableInfo VariableInfo => variable_info;

		public bool IsFixed
		{
			get
			{
				IVariableReference variableReference = InstanceExpression as IVariableReference;
				if (variableReference != null)
				{
					if (InstanceExpression.Type.IsStruct)
					{
						return variableReference.IsFixed;
					}
					return false;
				}
				return (InstanceExpression as IFixedExpression)?.IsFixed ?? false;
			}
		}

		protected FieldExpr(Location l)
		{
			loc = l;
		}

		public FieldExpr(FieldSpec spec, Location loc)
		{
			this.spec = spec;
			base.loc = loc;
			type = spec.MemberType;
		}

		public FieldExpr(FieldBase fi, Location l)
			: this(fi.Spec, l)
		{
		}

		public override string GetSignatureForError()
		{
			return spec.GetSignatureForError();
		}

		public bool IsMarshalByRefAccess(ResolveContext rc)
		{
			if (!spec.IsStatic && TypeSpec.IsValueType(spec.MemberType) && !(InstanceExpression is This) && rc.Module.PredefinedTypes.MarshalByRefObject.Define())
			{
				return TypeSpec.IsBaseClass(spec.DeclaringType, rc.Module.PredefinedTypes.MarshalByRefObject.TypeSpec, false);
			}
			return false;
		}

		public void SetHasAddressTaken()
		{
			(InstanceExpression as IVariableReference)?.SetHasAddressTaken();
		}

		protected override void CloneTo(CloneContext clonectx, Expression target)
		{
			FieldExpr fieldExpr = (FieldExpr)target;
			if (InstanceExpression != null)
			{
				fieldExpr.InstanceExpression = InstanceExpression.Clone(clonectx);
			}
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			if (base.ConditionalAccess)
			{
				Error_NullShortCircuitInsideExpressionTree(ec);
			}
			return CreateExpressionTree(ec, true);
		}

		public Expression CreateExpressionTree(ResolveContext ec, bool convertInstance)
		{
			Expression expression;
			Arguments arguments;
			if (InstanceExpression == null)
			{
				expression = new NullLiteral(loc);
			}
			else if (convertInstance)
			{
				expression = InstanceExpression.CreateExpressionTree(ec);
			}
			else
			{
				arguments = new Arguments(1);
				arguments.Add(new Argument(InstanceExpression));
				expression = CreateExpressionFactoryCall(ec, "Constant", arguments);
			}
			arguments = Arguments.CreateForExpressionTree(ec, null, expression, CreateTypeOfExpression());
			return CreateExpressionFactoryCall(ec, "Field", arguments);
		}

		public Expression CreateTypeOfExpression()
		{
			return new TypeOfField(spec, loc);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			spec.MemberDefinition.SetIsUsed();
			return DoResolve(ec, null);
		}

		private Expression DoResolve(ResolveContext ec, Expression rhs)
		{
			bool flag = rhs != null && IsInstance && spec.DeclaringType.IsStruct;
			if (rhs != this)
			{
				ResolveConditionalAccessReceiver(ec);
				if (ResolveInstanceExpression(ec, rhs))
				{
					if (flag)
					{
						Expression right_side = ((rhs == EmptyExpression.OutAccess || rhs == EmptyExpression.LValueMemberOutAccess) ? EmptyExpression.LValueMemberOutAccess : EmptyExpression.LValueMemberAccess);
						InstanceExpression = InstanceExpression.ResolveLValue(ec, right_side);
					}
					else
					{
						InstanceExpression = InstanceExpression.Resolve(ec, ResolveFlags.VariableOrValue);
					}
					if (InstanceExpression == null)
					{
						return null;
					}
				}
				DoBestMemberChecks(ec, spec);
				if (conditional_access_receiver)
				{
					ec.With(ResolveContext.Options.ConditionalAccessReceiver, false);
				}
			}
			FixedFieldSpec fixedFieldSpec = spec as FixedFieldSpec;
			IVariableReference variableReference = InstanceExpression as IVariableReference;
			if (fixedFieldSpec != null)
			{
				IFixedExpression fixedExpression = InstanceExpression as IFixedExpression;
				if (!ec.HasSet(ResolveContext.Options.FixedInitializerScope) && (fixedExpression == null || !fixedExpression.IsFixed))
				{
					ec.Report.Error(1666, loc, "You cannot use fixed size buffers contained in unfixed expressions. Try using the fixed statement");
				}
				if (InstanceExpression.eclass != ExprClass.Variable)
				{
					ec.Report.SymbolRelatedToPreviousError(spec);
					ec.Report.Error(1708, loc, "`{0}': Fixed size buffers can only be accessed through locals or fields", TypeManager.GetFullNameSignature(spec));
				}
				else if (variableReference != null && variableReference.IsHoisted)
				{
					AnonymousMethodExpression.Error_AddressOfCapturedVar(ec, variableReference, loc);
				}
				return new FixedBufferPtr(this, fixedFieldSpec.ElementType, loc).Resolve(ec);
			}
			if (variableReference != null && variableReference.VariableInfo != null && InstanceExpression.Type.IsStruct)
			{
				variable_info = variableReference.VariableInfo.GetStructFieldInfo(Name);
			}
			if (base.ConditionalAccess)
			{
				if (conditional_access_receiver)
				{
					type = Expression.LiftMemberType(ec, type);
				}
				if (InstanceExpression.IsNull)
				{
					return Constant.CreateConstantFromValue(type, null, loc);
				}
			}
			eclass = ExprClass.Variable;
			return this;
		}

		public void SetFieldAssigned(FlowAnalysisContext fc)
		{
			if (!IsInstance)
			{
				return;
			}
			if (spec.DeclaringType.IsStruct)
			{
				IVariableReference variableReference = InstanceExpression as IVariableReference;
				if (variableReference != null && variableReference.VariableInfo != null)
				{
					fc.SetStructFieldAssigned(variableReference.VariableInfo, Name);
				}
			}
			FieldExpr fieldExpr = InstanceExpression as FieldExpr;
			if (fieldExpr != null)
			{
				Expression instanceExpression;
				while (true)
				{
					instanceExpression = fieldExpr.InstanceExpression;
					FieldExpr fieldExpr2 = instanceExpression as FieldExpr;
					if ((fieldExpr2 == null || fieldExpr2.IsStatic) && !(instanceExpression is LocalVariableReference))
					{
						break;
					}
					if (TypeSpec.IsReferenceType(fieldExpr.Type) && instanceExpression.Type.IsStruct)
					{
						IVariableReference variableReference2 = InstanceExpression as IVariableReference;
						if (variableReference2 != null && variableReference2.VariableInfo == null)
						{
							IVariableReference variableReference3 = instanceExpression as IVariableReference;
							if (variableReference3 == null || (variableReference3.VariableInfo != null && !fc.IsDefinitelyAssigned(variableReference3.VariableInfo)))
							{
								fc.Report.Warning(1060, 1, fieldExpr.loc, "Use of possibly unassigned field `{0}'", fieldExpr.Name);
							}
						}
					}
					if (fieldExpr2 == null)
					{
						break;
					}
					fieldExpr = fieldExpr2;
				}
				if (instanceExpression != null && TypeSpec.IsReferenceType(instanceExpression.Type))
				{
					instanceExpression.FlowAnalysis(fc);
				}
			}
			else if (TypeSpec.IsReferenceType(InstanceExpression.Type))
			{
				InstanceExpression.FlowAnalysis(fc);
			}
		}

		private Expression Error_AssignToReadonly(ResolveContext rc, Expression right_side)
		{
			if (right_side == EmptyExpression.OutAccess)
			{
				if (IsStatic)
				{
					rc.Report.Error(199, loc, "A static readonly field `{0}' cannot be passed ref or out (except in a static constructor)", GetSignatureForError());
				}
				else
				{
					rc.Report.Error(192, loc, "A readonly field `{0}' cannot be passed ref or out (except in a constructor)", GetSignatureForError());
				}
				return null;
			}
			if (right_side == EmptyExpression.LValueMemberAccess)
			{
				return null;
			}
			if (right_side == EmptyExpression.LValueMemberOutAccess)
			{
				if (IsStatic)
				{
					rc.Report.Error(1651, loc, "Fields of static readonly field `{0}' cannot be passed ref or out (except in a static constructor)", GetSignatureForError());
				}
				else
				{
					rc.Report.Error(1649, loc, "Members of readonly field `{0}' cannot be passed ref or out (except in a constructor)", GetSignatureForError());
				}
				return null;
			}
			if (IsStatic)
			{
				rc.Report.Error(198, loc, "A static readonly field `{0}' cannot be assigned to (except in a static constructor or a variable initializer)", GetSignatureForError());
			}
			else
			{
				rc.Report.Error(191, loc, "A readonly field `{0}' cannot be assigned to (except in a constructor or a variable initializer)", GetSignatureForError());
			}
			return null;
		}

		public override Expression DoResolveLValue(ResolveContext ec, Expression right_side)
		{
			if (HasConditionalAccess())
			{
				Error_NullPropagatingLValue(ec);
			}
			if (spec is FixedFieldSpec)
			{
				Error_ValueAssignment(ec, right_side);
			}
			if (DoResolve(ec, right_side) == null)
			{
				return null;
			}
			spec.MemberDefinition.SetIsAssigned();
			if ((right_side == EmptyExpression.UnaryAddress || right_side == EmptyExpression.OutAccess) && (spec.Modifiers & Modifiers.VOLATILE) != 0)
			{
				ec.Report.Warning(420, 1, loc, "`{0}': A volatile field references will not be treated as volatile", spec.GetSignatureForError());
			}
			if (spec.IsReadOnly)
			{
				if (!ec.HasAny(ResolveContext.Options.FieldInitializerScope | ResolveContext.Options.ConstructorScope))
				{
					return Error_AssignToReadonly(ec, right_side);
				}
				if (ec.HasSet(ResolveContext.Options.ConstructorScope))
				{
					if (ec.CurrentMemberDefinition.Parent.PartialContainer.Definition != spec.DeclaringType.GetDefinition())
					{
						return Error_AssignToReadonly(ec, right_side);
					}
					if (IsStatic && !ec.IsStatic)
					{
						return Error_AssignToReadonly(ec, right_side);
					}
					if (!IsStatic && !(InstanceExpression is This))
					{
						return Error_AssignToReadonly(ec, right_side);
					}
				}
			}
			if (right_side == EmptyExpression.OutAccess && IsMarshalByRefAccess(ec))
			{
				ec.Report.SymbolRelatedToPreviousError(spec.DeclaringType);
				ec.Report.Warning(197, 1, loc, "Passing `{0}' as ref or out or taking its address may cause a runtime exception because it is a field of a marshal-by-reference class", GetSignatureForError());
			}
			eclass = ExprClass.Variable;
			return this;
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			IVariableReference variableReference = InstanceExpression as IVariableReference;
			if (variableReference != null)
			{
				VariableInfo variableInfo = variableReference.VariableInfo;
				if (variableInfo != null && !fc.IsStructFieldDefinitelyAssigned(variableInfo, Name))
				{
					fc.Report.Error(170, loc, "Use of possibly unassigned field `{0}'", Name);
					return;
				}
				if (TypeSpec.IsValueType(InstanceExpression.Type))
				{
					SkipLeftValueTypeAccess(InstanceExpression)?.FlowAnalysis(fc);
					return;
				}
			}
			base.FlowAnalysis(fc);
			if (conditional_access_receiver)
			{
				fc.ConditionalAccessEnd();
			}
		}

		private static Expression SkipLeftValueTypeAccess(Expression expr)
		{
			if (!TypeSpec.IsValueType(expr.Type))
			{
				return expr;
			}
			if (expr is VariableReference)
			{
				return null;
			}
			FieldExpr fieldExpr = expr as FieldExpr;
			if (fieldExpr == null)
			{
				return expr;
			}
			if (fieldExpr.InstanceExpression == null)
			{
				return expr;
			}
			return SkipLeftValueTypeAccess(fieldExpr.InstanceExpression);
		}

		public override int GetHashCode()
		{
			return spec.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			FieldExpr fieldExpr = obj as FieldExpr;
			if (fieldExpr == null)
			{
				return false;
			}
			if (spec != fieldExpr.spec)
			{
				return false;
			}
			if (InstanceExpression == null || fieldExpr.InstanceExpression == null)
			{
				return true;
			}
			return InstanceExpression.Equals(fieldExpr.InstanceExpression);
		}

		public void Emit(EmitContext ec, bool leave_copy)
		{
			bool flag = (spec.Modifiers & Modifiers.VOLATILE) != 0;
			if (IsStatic)
			{
				if (flag)
				{
					ec.Emit(OpCodes.Volatile);
				}
				ec.Emit(OpCodes.Ldsfld, spec);
			}
			else
			{
				if (!prepared)
				{
					if (conditional_access_receiver)
					{
						ec.ConditionalAccess = new ConditionalAccessContext(type, ec.DefineLabel());
					}
					EmitInstance(ec, false);
				}
				if (type.IsStruct && type == ec.CurrentType && InstanceExpression.Type == type)
				{
					ec.EmitLoadFromPtr(type);
				}
				else
				{
					FixedFieldSpec fixedFieldSpec = spec as FixedFieldSpec;
					if (fixedFieldSpec != null)
					{
						ec.Emit(OpCodes.Ldflda, spec);
						ec.Emit(OpCodes.Ldflda, fixedFieldSpec.Element);
					}
					else
					{
						if (flag)
						{
							ec.Emit(OpCodes.Volatile);
						}
						ec.Emit(OpCodes.Ldfld, spec);
					}
				}
				if (conditional_access_receiver)
				{
					ec.CloseConditionalAccess((type.IsNullableType && type != spec.MemberType) ? type : null);
				}
			}
			if (leave_copy)
			{
				ec.Emit(OpCodes.Dup);
				if (!IsStatic)
				{
					temp = new LocalTemporary(base.Type);
					temp.Store(ec);
				}
			}
		}

		public void EmitAssign(EmitContext ec, Expression source, bool leave_copy, bool isCompound)
		{
			bool flag = ec.HasSet(BuilderContext.Options.AsyncBody) && source.ContainsEmitWithAwait();
			if (isCompound && !(source is DynamicExpressionStatement) && !flag)
			{
				prepared = true;
			}
			if (IsInstance)
			{
				if (base.ConditionalAccess)
				{
					throw new NotImplementedException("null operator assignment");
				}
				if (flag)
				{
					source = source.EmitToField(ec);
				}
				EmitInstance(ec, prepared);
			}
			source.Emit(ec);
			if (leave_copy || ec.NotifyEvaluatorOnStore)
			{
				ec.Emit(OpCodes.Dup);
				if (!IsStatic)
				{
					temp = new LocalTemporary(base.Type);
					temp.Store(ec);
				}
			}
			if ((spec.Modifiers & Modifiers.VOLATILE) != 0)
			{
				ec.Emit(OpCodes.Volatile);
			}
			spec.MemberDefinition.SetIsAssigned();
			if (IsStatic)
			{
				ec.Emit(OpCodes.Stsfld, spec);
			}
			else
			{
				ec.Emit(OpCodes.Stfld, spec);
			}
			if (ec.NotifyEvaluatorOnStore)
			{
				if (!IsStatic)
				{
					throw new NotImplementedException("instance field write");
				}
				if (leave_copy)
				{
					ec.Emit(OpCodes.Dup);
				}
				ec.Module.Evaluator.EmitValueChangedCallback(ec, Name, type, loc);
			}
			if (temp != null)
			{
				temp.Emit(ec);
				temp.Release(ec);
				temp = null;
			}
		}

		public void EmitAssignFromStack(EmitContext ec)
		{
			if (IsStatic)
			{
				ec.Emit(OpCodes.Stsfld, spec);
			}
			else
			{
				ec.Emit(OpCodes.Stfld, spec);
			}
		}

		public override void Emit(EmitContext ec)
		{
			Emit(ec, false);
		}

		public override void EmitSideEffect(EmitContext ec)
		{
			if ((spec.Modifiers & Modifiers.VOLATILE) != 0)
			{
				base.EmitSideEffect(ec);
			}
		}

		public virtual void AddressOf(EmitContext ec, AddressOp mode)
		{
			if ((mode & AddressOp.Store) != 0)
			{
				spec.MemberDefinition.SetIsAssigned();
			}
			if ((mode & AddressOp.Load) != 0)
			{
				spec.MemberDefinition.SetIsUsed();
			}
			bool flag;
			if (spec.IsReadOnly)
			{
				flag = true;
				if (ec.HasSet(BuilderContext.Options.ConstructorScope) && spec.DeclaringType == ec.CurrentType)
				{
					if (IsStatic)
					{
						if (ec.IsStatic)
						{
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
				}
			}
			else
			{
				flag = false;
			}
			if (flag)
			{
				Emit(ec);
				LocalBuilder temporaryLocal = ec.GetTemporaryLocal(type);
				ec.Emit(OpCodes.Stloc, temporaryLocal);
				ec.Emit(OpCodes.Ldloca, temporaryLocal);
			}
			else if (IsStatic)
			{
				ec.Emit(OpCodes.Ldsflda, spec);
			}
			else
			{
				if (!prepared)
				{
					EmitInstance(ec, false);
				}
				ec.Emit(OpCodes.Ldflda, spec);
			}
		}

		public System.Linq.Expressions.Expression MakeAssignExpression(BuilderContext ctx, Expression source)
		{
			return MakeExpression(ctx);
		}

		public override System.Linq.Expressions.Expression MakeExpression(BuilderContext ctx)
		{
			return System.Linq.Expressions.Expression.Field(IsStatic ? null : InstanceExpression.MakeExpression(ctx), spec.GetMetaInfo());
		}

		public override void SetTypeArguments(ResolveContext ec, TypeArguments ta)
		{
			Expression.Error_TypeArgumentsCannotBeUsed(ec, "field", GetSignatureForError(), loc);
		}
	}
}
