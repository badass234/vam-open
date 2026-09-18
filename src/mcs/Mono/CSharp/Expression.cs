using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Text;
using Mono.CSharp.Nullable;

namespace Mono.CSharp
{
	public abstract class Expression
	{
		[Flags]
		public enum MemberLookupRestrictions
		{
			None = 0,
			InvocableOnly = 1,
			ExactArity = 4,
			ReadAccess = 8,
			EmptyArguments = 0x10,
			IgnoreArity = 0x20,
			IgnoreAmbiguity = 0x40,
			NameOfExcluded = 0x80
		}

		public ExprClass eclass;

		protected TypeSpec type;

		protected Location loc;

		public TypeSpec Type
		{
			get
			{
				return type;
			}
			set
			{
				type = value;
			}
		}

		public virtual bool IsSideEffectFree => false;

		public Location Location => loc;

		public virtual bool IsNull => false;

		public virtual Location StartLocation => loc;

		public ResolveFlags ExprClassToResolveFlags
		{
			get
			{
				switch (eclass)
				{
				case ExprClass.Namespace:
				case ExprClass.Type:
					return ResolveFlags.Type;
				case ExprClass.MethodGroup:
					return ResolveFlags.MethodGroup;
				case ExprClass.TypeParameter:
					return ResolveFlags.TypeParameter;
				case ExprClass.Value:
				case ExprClass.Variable:
				case ExprClass.PropertyAccess:
				case ExprClass.EventAccess:
				case ExprClass.IndexerAccess:
					return ResolveFlags.VariableOrValue;
				default:
					throw new InternalErrorException(string.Concat(loc.ToString(), " ", GetType(), " ExprClass is Invalid after resolve"));
				}
			}
		}

		public virtual string ExprClassName
		{
			get
			{
				switch (eclass)
				{
				case ExprClass.Unresolved:
					return "Unresolved";
				case ExprClass.Value:
					return "value";
				case ExprClass.Variable:
					return "variable";
				case ExprClass.Namespace:
					return "namespace";
				case ExprClass.Type:
					return "type";
				case ExprClass.MethodGroup:
					return "method group";
				case ExprClass.PropertyAccess:
					return "property access";
				case ExprClass.EventAccess:
					return "event access";
				case ExprClass.IndexerAccess:
					return "indexer access";
				case ExprClass.Nothing:
					return "null";
				case ExprClass.TypeParameter:
					return "type parameter";
				default:
					throw new Exception("Should not happen");
				}
			}
		}

		public virtual MethodGroupExpr CanReduceLambda(AnonymousMethodBody body)
		{
			return null;
		}

		public virtual bool ContainsEmitWithAwait()
		{
			return false;
		}

		protected abstract Expression DoResolve(ResolveContext rc);

		public virtual Expression DoResolveLValue(ResolveContext rc, Expression right_side)
		{
			return null;
		}

		public virtual TypeSpec ResolveAsType(IMemberContext mc, bool allowUnboundTypeArguments = false)
		{
			ResolveContext resolveContext = (mc as ResolveContext) ?? new ResolveContext(mc);
			Resolve(resolveContext)?.Error_UnexpectedKind(resolveContext, ResolveFlags.Type, loc);
			return null;
		}

		public static void ErrorIsInaccesible(IMemberContext rc, string member, Location loc)
		{
			rc.Module.Compiler.Report.Error(122, loc, "`{0}' is inaccessible due to its protection level", member);
		}

		public void Error_ExpressionMustBeConstant(ResolveContext rc, Location loc, string e_name)
		{
			rc.Report.Error(133, loc, "The expression being assigned to `{0}' must be constant", e_name);
		}

		public void Error_ConstantCanBeInitializedWithNullOnly(ResolveContext rc, TypeSpec type, Location loc, string name)
		{
			rc.Report.Error(134, loc, "A constant `{0}' of reference type `{1}' can only be initialized with null", name, type.GetSignatureForError());
		}

		protected virtual void Error_InvalidExpressionStatement(Report report, Location loc)
		{
			report.Error(201, loc, "Only assignment, call, increment, decrement, await, and new object expressions can be used as a statement");
		}

		public void Error_InvalidExpressionStatement(BlockContext bc)
		{
			Error_InvalidExpressionStatement(bc.Report, loc);
		}

		public void Error_InvalidExpressionStatement(Report report)
		{
			Error_InvalidExpressionStatement(report, loc);
		}

		public static void Error_VoidInvalidInTheContext(Location loc, Report Report)
		{
			Report.Error(1547, loc, "Keyword `void' cannot be used in this context");
		}

		public virtual void Error_ValueCannotBeConverted(ResolveContext ec, TypeSpec target, bool expl)
		{
			Error_ValueCannotBeConvertedCore(ec, loc, target, expl);
		}

		protected void Error_ValueCannotBeConvertedCore(ResolveContext ec, Location loc, TypeSpec target, bool expl)
		{
			if (type == InternalType.AnonymousMethod || type == InternalType.ErrorType || target == InternalType.ErrorType)
			{
				return;
			}
			string text = type.GetSignatureForError();
			string text2 = target.GetSignatureForError();
			if (text == text2)
			{
				text = type.GetSignatureForErrorIncludingAssemblyName();
				text2 = target.GetSignatureForErrorIncludingAssemblyName();
			}
			if (expl)
			{
				ec.Report.Error(30, loc, "Cannot convert type `{0}' to `{1}'", text, text2);
				return;
			}
			ec.Report.DisableReporting();
			bool num = Convert.ExplicitConversion(ec, this, target, Location.Null) != null;
			ec.Report.EnableReporting();
			if (num)
			{
				ec.Report.Error(266, loc, "Cannot implicitly convert type `{0}' to `{1}'. An explicit conversion exists (are you missing a cast?)", text, text2);
			}
			else
			{
				ec.Report.Error(29, loc, "Cannot implicitly convert type `{0}' to `{1}'", text, text2);
			}
		}

		public void Error_TypeArgumentsCannotBeUsed(IMemberContext context, MemberSpec member, Location loc)
		{
			if (member != null && (member.Kind & MemberKind.GenericMask) != 0)
			{
				Report report = context.Module.Compiler.Report;
				report.SymbolRelatedToPreviousError(member);
				member = ((!(member is TypeSpec)) ? ((MemberSpec)((MethodSpec)member).GetGenericMethodDefinition()) : ((MemberSpec)((TypeSpec)member).GetDefinition()));
				string text = ((member.Kind == MemberKind.Method) ? "method" : "type");
				if (member.IsGeneric)
				{
					report.Error(305, loc, "Using the generic {0} `{1}' requires `{2}' type argument(s)", text, member.GetSignatureForError(), member.Arity.ToString());
				}
				else
				{
					report.Error(308, loc, "The non-generic {0} `{1}' cannot be used with the type arguments", text, member.GetSignatureForError());
				}
			}
			else
			{
				Error_TypeArgumentsCannotBeUsed(context, ExprClassName, GetSignatureForError(), loc);
			}
		}

		public static void Error_TypeArgumentsCannotBeUsed(IMemberContext context, string exprType, string name, Location loc)
		{
			context.Module.Compiler.Report.Error(307, loc, "The {0} `{1}' cannot be used with type arguments", exprType, name);
		}

		public virtual void Error_TypeDoesNotContainDefinition(ResolveContext ec, TypeSpec type, string name)
		{
			Error_TypeDoesNotContainDefinition(ec, loc, type, name);
		}

		public static void Error_TypeDoesNotContainDefinition(ResolveContext ec, Location loc, TypeSpec type, string name)
		{
			ec.Report.SymbolRelatedToPreviousError(type);
			ec.Report.Error(117, loc, "`{0}' does not contain a definition for `{1}'", type.GetSignatureForError(), name);
		}

		public virtual void Error_ValueAssignment(ResolveContext rc, Expression rhs)
		{
			if (rhs != EmptyExpression.LValueMemberAccess && rhs != EmptyExpression.LValueMemberOutAccess)
			{
				if (rhs == EmptyExpression.OutAccess)
				{
					rc.Report.Error(1510, loc, "A ref or out argument must be an assignable variable");
				}
				else
				{
					rc.Report.Error(131, loc, "The left-hand side of an assignment must be a variable, a property or an indexer");
				}
			}
		}

		protected void Error_VoidPointerOperation(ResolveContext rc)
		{
			rc.Report.Error(242, loc, "The operation in question is undefined on void pointers");
		}

		public static void Warning_UnreachableExpression(ResolveContext rc, Location loc)
		{
			rc.Report.Warning(429, 4, loc, "Unreachable expression code detected");
		}

		public Expression ProbeIdenticalTypeName(ResolveContext rc, Expression left, SimpleName name)
		{
			TypeSpec typeSpec = left.Type;
			if (typeSpec.Kind == MemberKind.InternalCompilerType || typeSpec is ElementTypeSpec || typeSpec.Arity > 0)
			{
				return left;
			}
			if (left is MemberExpr || left is VariableReference)
			{
				TypeExpr typeExpr = rc.LookupNamespaceOrType(name.Name, 0, LookupMode.Probing, loc) as TypeExpr;
				if (typeExpr != null && typeExpr.Type == left.Type)
				{
					return typeExpr;
				}
			}
			return left;
		}

		public virtual string GetSignatureForError()
		{
			return type.GetDefinition().GetSignatureForError();
		}

		public static bool IsNeverNull(Expression expr)
		{
			if (expr is This || expr is New || expr is ArrayCreation || expr is DelegateCreation || expr is ConditionalMemberAccess)
			{
				return true;
			}
			Constant constant = expr as Constant;
			if (constant != null)
			{
				return !constant.IsNull;
			}
			TypeCast typeCast = expr as TypeCast;
			if (typeCast != null)
			{
				return IsNeverNull(typeCast.Child);
			}
			return false;
		}

		protected static bool IsNullPropagatingValid(TypeSpec type)
		{
			switch (type.Kind)
			{
			case MemberKind.Struct:
				return type.IsNullableType;
			case MemberKind.Enum:
			case MemberKind.PointerType:
			case MemberKind.Void:
				return false;
			case MemberKind.InternalCompilerType:
				return type.BuiltinType == BuiltinTypeSpec.Type.Dynamic;
			case MemberKind.TypeParameter:
				return !((TypeParameterSpec)type).IsValueType;
			default:
				return true;
			}
		}

		public virtual bool HasConditionalAccess()
		{
			return false;
		}

		protected static TypeSpec LiftMemberType(ResolveContext rc, TypeSpec type)
		{
			if (!TypeSpec.IsValueType(type) || type.IsNullableType)
			{
				return type;
			}
			return NullableInfo.MakeType(rc.Module, type);
		}

		public Expression Resolve(ResolveContext ec, ResolveFlags flags)
		{
			if (eclass != ExprClass.Unresolved)
			{
				if ((flags & ExprClassToResolveFlags) == 0)
				{
					Error_UnexpectedKind(ec, flags, loc);
					return null;
				}
				return this;
			}
			try
			{
				Expression expression = DoResolve(ec);
				if (expression == null)
				{
					return null;
				}
				if ((flags & expression.ExprClassToResolveFlags) == 0)
				{
					expression.Error_UnexpectedKind(ec, flags, loc);
					return null;
				}
				if (expression.type == null)
				{
					throw new InternalErrorException("Expression `{0}' didn't set its type in DoResolve", expression.GetType());
				}
				return expression;
			}
			catch (Exception ex)
			{
				if (loc.IsNull || ec.Module.Compiler.Settings.BreakOnInternalError || ex is CompletionResult || ec.Report.IsDisabled || ex is FatalException || ec.Report.Printer is NullReportPrinter)
				{
					throw;
				}
				ec.Report.Error(584, loc, "Internal compiler error: {0}", ex.Message);
				return ErrorExpression.Instance;
			}
		}

		public Expression Resolve(ResolveContext rc)
		{
			return Resolve(rc, ResolveFlags.VariableOrValue | ResolveFlags.MethodGroup);
		}

		public Expression ResolveLValue(ResolveContext ec, Expression right_side)
		{
			int errors = ec.Report.Errors;
			bool flag = right_side == EmptyExpression.OutAccess;
			Expression expression = DoResolveLValue(ec, right_side);
			if (((expression != null) & flag) && !(expression is IMemoryLocation))
			{
				expression = null;
			}
			if (expression == null)
			{
				if (errors == ec.Report.Errors)
				{
					Error_ValueAssignment(ec, right_side);
				}
				return null;
			}
			if (expression.eclass == ExprClass.Unresolved)
			{
				throw new Exception(string.Concat("Expression ", expression, " ExprClass is Invalid after resolve"));
			}
			if (expression.type == null && !(expression is GenericTypeExpr))
			{
				throw new Exception(string.Concat("Expression ", expression, " did not set its type after Resolve"));
			}
			return expression;
		}

		public Constant ResolveLabelConstant(ResolveContext rc)
		{
			Expression expression = Resolve(rc);
			if (expression == null)
			{
				return null;
			}
			Constant constant = expression as Constant;
			if (constant == null)
			{
				if (expression.type != InternalType.ErrorType)
				{
					rc.Report.Error(150, expression.StartLocation, "A constant value is expected");
				}
				return null;
			}
			return constant;
		}

		public virtual void EncodeAttributeValue(IMemberContext rc, AttributeEncoder enc, TypeSpec targetType, TypeSpec parameterType)
		{
			if (Attribute.IsValidArgumentType(parameterType))
			{
				rc.Module.Compiler.Report.Error(182, loc, "An attribute argument must be a constant expression, typeof expression or array creation expression");
			}
			else
			{
				rc.Module.Compiler.Report.Error(181, loc, "Attribute constructor parameter has type `{0}', which is not a valid attribute parameter type", targetType.GetSignatureForError());
			}
		}

		public abstract void Emit(EmitContext ec);

		public virtual void EmitBranchable(EmitContext ec, Label target, bool on_true)
		{
			Emit(ec);
			ec.Emit(on_true ? OpCodes.Brtrue : OpCodes.Brfalse, target);
		}

		public virtual void EmitSideEffect(EmitContext ec)
		{
			Emit(ec);
			ec.Emit(OpCodes.Pop);
		}

		public virtual Expression EmitToField(EmitContext ec)
		{
			if (IsSideEffectFree)
			{
				return this;
			}
			bool flag = ContainsEmitWithAwait();
			if (!flag)
			{
				ec.EmitThis();
			}
			FieldExpr fieldExpr = EmitToFieldSource(ec);
			if (fieldExpr == null)
			{
				fieldExpr = ec.GetTemporaryField(type);
				if (flag)
				{
					LocalBuilder temporaryLocal = ec.GetTemporaryLocal(type);
					ec.Emit(OpCodes.Stloc, temporaryLocal);
					ec.EmitThis();
					ec.Emit(OpCodes.Ldloc, temporaryLocal);
					fieldExpr.EmitAssignFromStack(ec);
					ec.FreeTemporaryLocal(temporaryLocal, type);
				}
				else
				{
					fieldExpr.EmitAssignFromStack(ec);
				}
			}
			return fieldExpr;
		}

		protected virtual FieldExpr EmitToFieldSource(EmitContext ec)
		{
			Emit(ec);
			return null;
		}

		protected static void EmitExpressionsList(EmitContext ec, List<Expression> expressions)
		{
			if (ec.HasSet(BuilderContext.Options.AsyncBody))
			{
				bool flag = false;
				for (int i = 1; i < expressions.Count; i++)
				{
					if (expressions[i].ContainsEmitWithAwait())
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					for (int j = 0; j < expressions.Count; j++)
					{
						expressions[j] = expressions[j].EmitToField(ec);
					}
				}
			}
			for (int k = 0; k < expressions.Count; k++)
			{
				expressions[k].Emit(ec);
			}
		}

		private static Expression ExprClassFromMemberInfo(MemberSpec spec, Location loc)
		{
			if (spec is EventSpec)
			{
				return new EventExpr((EventSpec)spec, loc);
			}
			if (spec is ConstSpec)
			{
				return new ConstantExpr((ConstSpec)spec, loc);
			}
			if (spec is FieldSpec)
			{
				return new FieldExpr((FieldSpec)spec, loc);
			}
			if (spec is PropertySpec)
			{
				return new PropertyExpr((PropertySpec)spec, loc);
			}
			if (spec is TypeSpec)
			{
				return new TypeExpression((TypeSpec)spec, loc);
			}
			return null;
		}

		public static MethodSpec ConstructorLookup(ResolveContext rc, TypeSpec type, ref Arguments args, Location loc)
		{
			IList<MemberSpec> list = MemberCache.FindMembers(type, Constructor.ConstructorName, true);
			if (list == null)
			{
				switch (type.Kind)
				{
				case MemberKind.Struct:
					if (args == null)
					{
						return null;
					}
					rc.Report.SymbolRelatedToPreviousError(type);
					OverloadResolver.Error_ConstructorMismatch(rc, type, (args != null) ? args.Count : 0, loc);
					break;
				default:
					rc.Report.SymbolRelatedToPreviousError(type);
					rc.Report.Error(143, loc, "The class `{0}' has no constructors defined", type.GetSignatureForError());
					break;
				case MemberKind.InternalCompilerType:
				case MemberKind.MissingType:
					break;
				}
				return null;
			}
			if (args == null && type.IsStruct)
			{
				bool flag = false;
				foreach (MethodSpec item in list)
				{
					if (item.Parameters.IsEmpty)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					return null;
				}
			}
			OverloadResolver overloadResolver = new OverloadResolver(list, OverloadResolver.Restrictions.NoBaseMembers, loc);
			if (!rc.HasSet(ResolveContext.Options.BaseInitializer))
			{
				overloadResolver.InstanceQualifier = new ConstructorInstanceQualifier(type);
			}
			return overloadResolver.ResolveMember<MethodSpec>(rc, ref args);
		}

		public static Expression MemberLookup(IMemberContext rc, bool errorMode, TypeSpec queried_type, string name, int arity, MemberLookupRestrictions restrictions, Location loc)
		{
			IList<MemberSpec> list = MemberCache.FindMembers(queried_type, name, false);
			if (list == null)
			{
				return null;
			}
			Expression expression;
			do
			{
				expression = MemberLookupToExpression(rc, list, errorMode, queried_type, name, arity, restrictions, loc);
				if (expression != null)
				{
					return expression;
				}
				list = ((list[0].DeclaringType.BaseType != null) ? MemberCache.FindMembers(list[0].DeclaringType.BaseType, name, false) : null);
			}
			while (list != null);
			return expression;
		}

		public static Expression MemberLookupToExpression(IMemberContext rc, IList<MemberSpec> members, bool errorMode, TypeSpec queried_type, string name, int arity, MemberLookupRestrictions restrictions, Location loc)
		{
			MemberSpec memberSpec = null;
			MemberSpec memberSpec2 = null;
			for (int i = 0; i < members.Count; i++)
			{
				MemberSpec memberSpec3 = members[i];
				if (((memberSpec3.Modifiers & Modifiers.OVERRIDE) != 0 && memberSpec3.Kind != MemberKind.Event) || (memberSpec3.Modifiers & Modifiers.BACKING_FIELD) != 0 || memberSpec3.Kind == MemberKind.Operator || ((arity > 0 || (restrictions & MemberLookupRestrictions.ExactArity) != MemberLookupRestrictions.None) && memberSpec3.Arity != arity) || (!errorMode && (!memberSpec3.IsAccessible(rc) || (rc.Module.Compiler.IsRuntimeBinder && !memberSpec3.DeclaringType.IsAccessible(rc)))))
				{
					continue;
				}
				if ((restrictions & MemberLookupRestrictions.InvocableOnly) != MemberLookupRestrictions.None)
				{
					if (memberSpec3 is MethodSpec)
					{
						TypeParameterSpec typeParameterSpec = queried_type as TypeParameterSpec;
						if (typeParameterSpec != null && typeParameterSpec.HasTypeConstraint)
						{
							members = RemoveHiddenTypeParameterMethods(members);
						}
						return new MethodGroupExpr(members, queried_type, loc);
					}
					if (!Invocation.IsMemberInvocable(memberSpec3))
					{
						continue;
					}
				}
				if (memberSpec == null || memberSpec3 is MethodSpec || memberSpec.IsNotCSharpCompatible)
				{
					memberSpec = memberSpec3;
				}
				else
				{
					if (errorMode || memberSpec3.IsNotCSharpCompatible)
					{
						continue;
					}
					TypeParameterSpec typeParameterSpec2 = queried_type as TypeParameterSpec;
					if (typeParameterSpec2 != null && typeParameterSpec2.HasTypeConstraint)
					{
						if (memberSpec.DeclaringType.IsClass && memberSpec3.DeclaringType.IsInterface)
						{
							continue;
						}
						if (memberSpec.DeclaringType.IsInterface && memberSpec3.DeclaringType.IsInterface)
						{
							memberSpec = memberSpec3;
							continue;
						}
					}
					memberSpec2 = memberSpec3;
				}
			}
			if (memberSpec != null)
			{
				if (memberSpec2 != null && rc != null && (restrictions & MemberLookupRestrictions.IgnoreAmbiguity) == 0)
				{
					Report report = rc.Module.Compiler.Report;
					report.SymbolRelatedToPreviousError(memberSpec);
					report.SymbolRelatedToPreviousError(memberSpec2);
					report.Error(229, loc, "Ambiguity between `{0}' and `{1}'", memberSpec.GetSignatureForError(), memberSpec2.GetSignatureForError());
				}
				if (memberSpec is MethodSpec)
				{
					return new MethodGroupExpr(members, queried_type, loc);
				}
				return ExprClassFromMemberInfo(memberSpec, loc);
			}
			return null;
		}

		private static IList<MemberSpec> RemoveHiddenTypeParameterMethods(IList<MemberSpec> members)
		{
			if (members.Count < 2)
			{
				return members;
			}
			bool flag = false;
			for (int i = 0; i < members.Count; i++)
			{
				MethodSpec methodSpec = members[i] as MethodSpec;
				if (methodSpec == null)
				{
					if (!flag)
					{
						flag = true;
						members = new List<MemberSpec>(members);
					}
					members.RemoveAt(i--);
				}
				else
				{
					if (!methodSpec.DeclaringType.IsInterface)
					{
						continue;
					}
					for (int j = 0; j < members.Count; j++)
					{
						MethodSpec methodSpec2 = members[j] as MethodSpec;
						if (methodSpec2 != null && methodSpec2.DeclaringType.IsClass && TypeSpecComparer.Override.IsEqual(methodSpec2.Parameters, methodSpec.Parameters) && AParametersCollection.HasSameParameterDefaults(methodSpec2.Parameters, methodSpec.Parameters))
						{
							if (!flag)
							{
								flag = true;
								members = new List<MemberSpec>(members);
							}
							members.RemoveAt(i--);
							break;
						}
					}
				}
			}
			return members;
		}

		protected static void Error_NamedArgument(NamedArgument na, Report Report)
		{
			Report.Error(1742, na.Location, "An element access expression cannot use named argument");
		}

		protected virtual void Error_NegativeArrayIndex(ResolveContext ec, Location loc)
		{
			throw new NotImplementedException();
		}

		public virtual void Error_OperatorCannotBeApplied(ResolveContext rc, Location loc, string oper, TypeSpec t)
		{
			if (t != InternalType.ErrorType)
			{
				rc.Report.Error(23, loc, "The `{0}' operator cannot be applied to operand of type `{1}'", oper, t.GetSignatureForError());
			}
		}

		protected void Error_PointerInsideExpressionTree(ResolveContext ec)
		{
			ec.Report.Error(1944, loc, "An expression tree cannot contain an unsafe pointer operation");
		}

		protected void Error_NullShortCircuitInsideExpressionTree(ResolveContext rc)
		{
			rc.Report.Error(8072, loc, "An expression tree cannot contain a null propagating operator");
		}

		protected void Error_NullPropagatingLValue(ResolveContext rc)
		{
			rc.Report.Error(-1030, loc, "The left-hand side of an assignment cannot contain a null propagating operator");
		}

		public virtual void FlowAnalysis(FlowAnalysisContext fc)
		{
		}

		public virtual void FlowAnalysisConditional(FlowAnalysisContext fc)
		{
			FlowAnalysis(fc);
			DefiniteAssignmentBitSet definiteAssignmentOnTrue = (fc.DefiniteAssignmentOnFalse = fc.DefiniteAssignment);
			fc.DefiniteAssignmentOnTrue = definiteAssignmentOnTrue;
		}

		protected static Expression GetOperatorTrue(ResolveContext ec, Expression e, Location loc)
		{
			return GetOperatorTrueOrFalse(ec, e, true, loc);
		}

		protected static Expression GetOperatorFalse(ResolveContext ec, Expression e, Location loc)
		{
			return GetOperatorTrueOrFalse(ec, e, false, loc);
		}

		private static Expression GetOperatorTrueOrFalse(ResolveContext ec, Expression e, bool is_true, Location loc)
		{
			Operator.OpType op = (is_true ? Operator.OpType.True : Operator.OpType.False);
			TypeSpec underlyingType = e.type;
			if (underlyingType.IsNullableType)
			{
				underlyingType = NullableInfo.GetUnderlyingType(underlyingType);
			}
			IList<MemberSpec> userOperator = MemberCache.GetUserOperator(underlyingType, op, false);
			if (userOperator == null)
			{
				return null;
			}
			Arguments args = new Arguments(1);
			args.Add(new Argument(e));
			MethodSpec methodSpec = new OverloadResolver(userOperator, OverloadResolver.Restrictions.NoBaseMembers | OverloadResolver.Restrictions.BaseMembersIncluded, loc).ResolveOperator(ec, ref args);
			if (methodSpec == null)
			{
				return null;
			}
			return new UserOperatorCall(methodSpec, args, null, loc);
		}

		public static void Error_UnexpectedKind(IMemberContext ctx, Expression memberExpr, string expected, string was, Location loc)
		{
			string signatureForError = memberExpr.GetSignatureForError();
			ctx.Module.Compiler.Report.Error(118, loc, "`{0}' is a `{1}' but a `{2}' was expected", signatureForError, was, expected);
		}

		public virtual void Error_UnexpectedKind(ResolveContext ec, ResolveFlags flags, Location loc)
		{
			string[] array = new string[4];
			int num = 0;
			if ((flags & ResolveFlags.VariableOrValue) != 0)
			{
				array[num++] = "variable";
				array[num++] = "value";
			}
			if ((flags & ResolveFlags.Type) != 0)
			{
				array[num++] = "type";
			}
			if ((flags & ResolveFlags.MethodGroup) != 0)
			{
				array[num++] = "method group";
			}
			if (num == 0)
			{
				array[num++] = "unknown";
			}
			StringBuilder stringBuilder = new StringBuilder(array[0]);
			for (int i = 1; i < num - 1; i++)
			{
				stringBuilder.Append("', `");
				stringBuilder.Append(array[i]);
			}
			if (num > 1)
			{
				stringBuilder.Append("' or `");
				stringBuilder.Append(array[num - 1]);
			}
			ec.Report.Error(119, loc, "Expression denotes a `{0}', where a `{1}' was expected", ExprClassName, stringBuilder.ToString());
		}

		public static void UnsafeError(ResolveContext ec, Location loc)
		{
			UnsafeError(ec.Report, loc);
		}

		public static void UnsafeError(Report Report, Location loc)
		{
			Report.Error(214, loc, "Pointers and fixed size buffers may only be used in an unsafe context");
		}

		protected Expression ConvertExpressionToArrayIndex(ResolveContext ec, Expression source, bool pointerArray = false)
		{
			BuiltinTypes builtinTypes = ec.BuiltinTypes;
			if (source.type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				Arguments arguments = new Arguments(1);
				arguments.Add(new Argument(source));
				return new DynamicConversion(builtinTypes.Int, CSharpBinderFlags.ConvertArrayIndex, arguments, source.loc).Resolve(ec);
			}
			Expression expression;
			using (ec.Set(ResolveContext.Options.CheckedScope))
			{
				expression = Convert.ImplicitConversion(ec, source, builtinTypes.Int, source.loc);
				if (expression == null)
				{
					expression = Convert.ImplicitConversion(ec, source, builtinTypes.UInt, source.loc);
				}
				if (expression == null)
				{
					expression = Convert.ImplicitConversion(ec, source, builtinTypes.Long, source.loc);
				}
				if (expression == null)
				{
					expression = Convert.ImplicitConversion(ec, source, builtinTypes.ULong, source.loc);
				}
				if (expression == null)
				{
					source.Error_ValueCannotBeConverted(ec, builtinTypes.Int, false);
					return null;
				}
			}
			if (pointerArray)
			{
				return expression;
			}
			Constant constant = expression as Constant;
			if (constant != null && constant.IsNegative)
			{
				Error_NegativeArrayIndex(ec, source.loc);
			}
			if (expression.Type.BuiltinType == BuiltinTypeSpec.Type.Int)
			{
				return expression;
			}
			return new ArrayIndexCast(expression, builtinTypes.Int).Resolve(ec);
		}

		public Expression MakePointerAccess(ResolveContext rc, TypeSpec type, Arguments args)
		{
			if (args.Count != 1)
			{
				rc.Report.Error(196, loc, "A pointer must be indexed by only one value");
				return null;
			}
			Argument argument = args[0];
			if (argument is NamedArgument)
			{
				Error_NamedArgument((NamedArgument)argument, rc.Report);
			}
			Expression expression = argument.Expr.Resolve(rc);
			if (expression == null)
			{
				return null;
			}
			expression = ConvertExpressionToArrayIndex(rc, expression, true);
			return new Indirection(new PointerArithmetic(Binary.Operator.Addition, this, expression, type, loc), loc);
		}

		protected virtual void CloneTo(CloneContext clonectx, Expression target)
		{
			throw new NotImplementedException($"CloneTo not implemented for expression {GetType()}");
		}

		public virtual Expression Clone(CloneContext clonectx)
		{
			Expression expression = (Expression)MemberwiseClone();
			CloneTo(clonectx, expression);
			return expression;
		}

		public abstract Expression CreateExpressionTree(ResolveContext ec);

		protected Expression CreateExpressionFactoryCall(ResolveContext ec, string name, Arguments args)
		{
			return CreateExpressionFactoryCall(ec, name, null, args, loc);
		}

		protected Expression CreateExpressionFactoryCall(ResolveContext ec, string name, TypeArguments typeArguments, Arguments args)
		{
			return CreateExpressionFactoryCall(ec, name, typeArguments, args, loc);
		}

		public static Expression CreateExpressionFactoryCall(ResolveContext ec, string name, TypeArguments typeArguments, Arguments args, Location loc)
		{
			return new Invocation(new MemberAccess(CreateExpressionTypeExpression(ec, loc), name, typeArguments, loc), args);
		}

		protected static TypeExpr CreateExpressionTypeExpression(ResolveContext ec, Location loc)
		{
			TypeSpec typeSpec = ec.Module.PredefinedTypes.Expression.Resolve();
			if (typeSpec == null)
			{
				return null;
			}
			return new TypeExpression(typeSpec, loc);
		}

		public virtual System.Linq.Expressions.Expression MakeExpression(BuilderContext ctx)
		{
			throw new NotImplementedException("MakeExpression for " + GetType());
		}

		public virtual object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
