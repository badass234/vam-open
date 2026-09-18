using System.Reflection.Emit;

namespace Mono.CSharp
{
	public abstract class DelegateCreation : Expression, OverloadResolver.IErrorHandler
	{
		private bool conditional_access_receiver;

		protected MethodSpec constructor_method;

		protected MethodGroupExpr method_group;

		public bool AllowSpecialMethodsInvocation { get; set; }

		public override bool ContainsEmitWithAwait()
		{
			return method_group.InstanceExpression?.ContainsEmitWithAwait() ?? false;
		}

		public static Arguments CreateDelegateMethodArguments(ResolveContext rc, AParametersCollection pd, TypeSpec[] types, Location loc)
		{
			Arguments arguments = new Arguments(pd.Count);
			for (int i = 0; i < pd.Count; i++)
			{
				Argument.AType aType;
				switch (pd.FixedParameters[i].ModFlags & Parameter.Modifier.RefOutMask)
				{
				case Parameter.Modifier.REF:
					aType = Argument.AType.Ref;
					break;
				case Parameter.Modifier.OUT:
					aType = Argument.AType.Out;
					break;
				default:
					aType = Argument.AType.None;
					break;
				}
				TypeSpec typeSpec = types[i];
				if (typeSpec.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
				{
					typeSpec = rc.BuiltinTypes.Object;
				}
				arguments.Add(new Argument(new TypeExpression(typeSpec, loc), aType));
			}
			return arguments;
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			MemberAccess expr = new MemberAccess(new MemberAccess(new QualifiedAliasMember("global", "System", loc), "Delegate", loc), "CreateDelegate", loc);
			Arguments arguments = new Arguments(3);
			arguments.Add(new Argument(new TypeOf(type, loc)));
			if (method_group.InstanceExpression == null)
			{
				arguments.Add(new Argument(new NullLiteral(loc)));
			}
			else
			{
				arguments.Add(new Argument(method_group.InstanceExpression));
			}
			arguments.Add(new Argument(method_group.CreateExpressionTree(ec)));
			Expression expression = new Invocation(expr, arguments).Resolve(ec);
			if (expression == null)
			{
				return null;
			}
			return Convert.ExplicitConversion(ec, expression, type, loc)?.CreateExpressionTree(ec);
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			constructor_method = Delegate.GetConstructor(type);
			MethodSpec invokeMethod = Delegate.GetInvokeMethod(type);
			if (!ec.HasSet(ResolveContext.Options.ConditionalAccessReceiver) && method_group.HasConditionalAccess())
			{
				conditional_access_receiver = true;
				ec.Set(ResolveContext.Options.ConditionalAccessReceiver);
			}
			Arguments args = CreateDelegateMethodArguments(ec, invokeMethod.Parameters, invokeMethod.Parameters.Types, loc);
			method_group = method_group.OverloadResolve(ec, ref args, this, OverloadResolver.Restrictions.CovariantDelegate);
			if (conditional_access_receiver)
			{
				ec.With(ResolveContext.Options.ConditionalAccessReceiver, false);
			}
			if (method_group == null)
			{
				return null;
			}
			MethodSpec bestCandidate = method_group.BestCandidate;
			if (bestCandidate.DeclaringType.IsNullableType)
			{
				ec.Report.Error(1728, loc, "Cannot create delegate from method `{0}' because it is a member of System.Nullable<T> type", bestCandidate.GetSignatureForError());
				return null;
			}
			if (!AllowSpecialMethodsInvocation)
			{
				Invocation.IsSpecialMethodInvocation(ec, bestCandidate, loc);
			}
			ExtensionMethodGroupExpr extensionMethodGroupExpr = method_group as ExtensionMethodGroupExpr;
			if (extensionMethodGroupExpr != null)
			{
				method_group.InstanceExpression = extensionMethodGroupExpr.ExtensionExpression;
				TypeSpec typeSpec = extensionMethodGroupExpr.ExtensionExpression.Type;
				if (TypeSpec.IsValueType(typeSpec))
				{
					ec.Report.Error(1113, loc, "Extension method `{0}' of value type `{1}' cannot be used to create delegates", bestCandidate.GetSignatureForError(), typeSpec.GetSignatureForError());
				}
			}
			TypeSpec typeSpec2 = method_group.BestCandidateReturnType;
			if (typeSpec2.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				typeSpec2 = ec.BuiltinTypes.Object;
			}
			if (!Delegate.IsTypeCovariant(ec, typeSpec2, invokeMethod.ReturnType))
			{
				Expression return_type = new TypeExpression(bestCandidate.ReturnType, loc);
				Error_ConversionFailed(ec, bestCandidate, return_type);
			}
			if (method_group.IsConditionallyExcluded)
			{
				ec.Report.SymbolRelatedToPreviousError(bestCandidate);
				MethodOrOperator methodOrOperator = bestCandidate.MemberDefinition as MethodOrOperator;
				if (methodOrOperator != null && methodOrOperator.IsPartialDefinition)
				{
					ec.Report.Error(762, loc, "Cannot create delegate from partial method declaration `{0}'", bestCandidate.GetSignatureForError());
				}
				else
				{
					ec.Report.Error(1618, loc, "Cannot create delegate with `{0}' because it has a Conditional attribute", TypeManager.CSharpSignature(bestCandidate));
				}
			}
			Expression instanceExpression = method_group.InstanceExpression;
			if (instanceExpression != null && (instanceExpression.Type.IsGenericParameter || !TypeSpec.IsReferenceType(instanceExpression.Type)))
			{
				method_group.InstanceExpression = new BoxedCast(instanceExpression, ec.BuiltinTypes.Object);
			}
			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			if (conditional_access_receiver)
			{
				ec.ConditionalAccess = new ConditionalAccessContext(type, ec.DefineLabel());
			}
			if (method_group.InstanceExpression == null)
			{
				ec.EmitNull();
			}
			else
			{
				new InstanceEmitter(method_group.InstanceExpression, false).Emit(ec, method_group.ConditionalAccess);
			}
			MethodSpec bestCandidate = method_group.BestCandidate;
			if (!bestCandidate.DeclaringType.IsDelegate && bestCandidate.IsVirtual && !method_group.IsBase)
			{
				ec.Emit(OpCodes.Dup);
				ec.Emit(OpCodes.Ldvirtftn, bestCandidate);
			}
			else
			{
				ec.Emit(OpCodes.Ldftn, bestCandidate);
			}
			ec.Emit(OpCodes.Newobj, constructor_method);
			if (conditional_access_receiver)
			{
				ec.CloseConditionalAccess(null);
			}
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			base.FlowAnalysis(fc);
			method_group.FlowAnalysis(fc);
			if (conditional_access_receiver)
			{
				fc.ConditionalAccessEnd();
			}
		}

		private void Error_ConversionFailed(ResolveContext ec, MethodSpec method, Expression return_type)
		{
			MethodSpec invokeMethod = Delegate.GetInvokeMethod(type);
			string text = ((method_group.InstanceExpression != null) ? Delegate.FullDelegateDesc(method) : TypeManager.GetFullNameSignature(method));
			ec.Report.SymbolRelatedToPreviousError(type);
			ec.Report.SymbolRelatedToPreviousError(method);
			if (ec.Module.Compiler.Settings.Version == LanguageVersion.ISO_1)
			{
				ec.Report.Error(410, loc, "A method or delegate `{0} {1}' parameters and return type must be same as delegate `{2} {3}' parameters and return type", method.ReturnType.GetSignatureForError(), text, invokeMethod.ReturnType.GetSignatureForError(), Delegate.FullDelegateDesc(invokeMethod));
			}
			else if (return_type == null)
			{
				ec.Report.Error(123, loc, "A method or delegate `{0}' parameters do not match delegate `{1}' parameters", text, Delegate.FullDelegateDesc(invokeMethod));
			}
			else
			{
				ec.Report.Error(407, loc, "A method or delegate `{0} {1}' return type does not match delegate `{2} {3}' return type", return_type.GetSignatureForError(), text, invokeMethod.ReturnType.GetSignatureForError(), Delegate.FullDelegateDesc(invokeMethod));
			}
		}

		public static bool ImplicitStandardConversionExists(ResolveContext ec, MethodGroupExpr mg, TypeSpec target_type)
		{
			MethodSpec invokeMethod = Delegate.GetInvokeMethod(target_type);
			Arguments args = CreateDelegateMethodArguments(ec, invokeMethod.Parameters, invokeMethod.Parameters.Types, mg.Location);
			mg = mg.OverloadResolve(ec, ref args, null, OverloadResolver.Restrictions.ProbingOnly | OverloadResolver.Restrictions.CovariantDelegate);
			if (mg != null)
			{
				return Delegate.IsTypeCovariant(ec, mg.BestCandidateReturnType, invokeMethod.ReturnType);
			}
			return false;
		}

		bool OverloadResolver.IErrorHandler.AmbiguousCandidates(ResolveContext ec, MemberSpec best, MemberSpec ambiguous)
		{
			return false;
		}

		bool OverloadResolver.IErrorHandler.ArgumentMismatch(ResolveContext rc, MemberSpec best, Argument arg, int index)
		{
			Error_ConversionFailed(rc, best as MethodSpec, null);
			return true;
		}

		bool OverloadResolver.IErrorHandler.NoArgumentMatch(ResolveContext rc, MemberSpec best)
		{
			Error_ConversionFailed(rc, best as MethodSpec, null);
			return true;
		}

		bool OverloadResolver.IErrorHandler.TypeInferenceFailed(ResolveContext rc, MemberSpec best)
		{
			return false;
		}
	}
}
