using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Mono.CSharp
{
	public class Invocation : ExpressionStatement
	{
		public class Predefined : Invocation
		{
			public Predefined(MethodGroupExpr expr, Arguments arguments)
				: base(expr, arguments)
			{
				mg = expr;
			}

			protected override MethodGroupExpr DoResolveOverload(ResolveContext rc)
			{
				if (!rc.IsObsolete)
				{
					MethodSpec bestCandidate = mg.BestCandidate;
					ObsoleteAttribute attributeObsolete = bestCandidate.GetAttributeObsolete();
					if (attributeObsolete != null)
					{
						AttributeTester.Report_ObsoleteMessage(attributeObsolete, bestCandidate.GetSignatureForError(), loc, rc.Report);
					}
				}
				return mg;
			}
		}

		protected Arguments arguments;

		protected Expression expr;

		protected MethodGroupExpr mg;

		private bool conditional_access_receiver;

		public Arguments Arguments => arguments;

		public Expression Exp => expr;

		public MethodGroupExpr MethodGroup => mg;

		public override Location StartLocation => expr.StartLocation;

		public Invocation(Expression expr, Arguments arguments)
		{
			this.expr = expr;
			this.arguments = arguments;
			if (expr != null)
			{
				loc = expr.Location;
			}
		}

		public override MethodGroupExpr CanReduceLambda(AnonymousMethodBody body)
		{
			if (MethodGroup == null)
			{
				return null;
			}
			MethodSpec bestCandidate = MethodGroup.BestCandidate;
			if (bestCandidate == null || (!bestCandidate.IsStatic && !(Exp is This)))
			{
				return null;
			}
			int num = ((arguments != null) ? arguments.Count : 0);
			if (num != body.Parameters.Count)
			{
				return null;
			}
			IParameterData[] fixedParameters = body.Block.Parameters.FixedParameters;
			for (int i = 0; i < num; i++)
			{
				ParameterReference parameterReference = arguments[i].Expr as ParameterReference;
				if (parameterReference == null)
				{
					return null;
				}
				if (fixedParameters[i] != parameterReference.Parameter)
				{
					return null;
				}
				if ((fixedParameters[i].ModFlags & Parameter.Modifier.RefOutMask) != (parameterReference.Parameter.ModFlags & Parameter.Modifier.RefOutMask))
				{
					return null;
				}
			}
			if (MethodGroup is ExtensionMethodGroupExpr)
			{
				MethodGroupExpr methodGroupExpr = MethodGroupExpr.CreatePredefined(bestCandidate, bestCandidate.DeclaringType, MethodGroup.Location);
				if (bestCandidate.IsGeneric)
				{
					TypeExpression[] array = new TypeExpression[bestCandidate.Arity];
					for (int j = 0; j < array.Length; j++)
					{
						array[j] = new TypeExpression(bestCandidate.TypeArguments[j], MethodGroup.Location);
					}
					methodGroupExpr.SetTypeArguments(null, new TypeArguments(array));
				}
				return methodGroupExpr;
			}
			return MethodGroup;
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			Invocation invocation = (Invocation)t;
			if (arguments != null)
			{
				invocation.arguments = arguments.Clone(clonectx);
			}
			invocation.expr = expr.Clone(clonectx);
		}

		public override bool ContainsEmitWithAwait()
		{
			if (arguments != null && arguments.ContainsEmitWithAwait())
			{
				return true;
			}
			return mg.ContainsEmitWithAwait();
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			Expression expression = (mg.IsInstance ? mg.InstanceExpression.CreateExpressionTree(ec) : new NullLiteral(loc));
			Arguments args = Arguments.CreateForExpressionTree(ec, arguments, expression, mg.CreateExpressionTree(ec));
			return CreateExpressionFactoryCall(ec, "Call", args);
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			if (!rc.HasSet(ResolveContext.Options.ConditionalAccessReceiver) && expr.HasConditionalAccess())
			{
				conditional_access_receiver = true;
				using (rc.Set(ResolveContext.Options.ConditionalAccessReceiver))
				{
					return DoResolveInvocation(rc);
				}
			}
			return DoResolveInvocation(rc);
		}

		private Expression DoResolveInvocation(ResolveContext ec)
		{
			ATypeNameExpression aTypeNameExpression = expr as ATypeNameExpression;
			Expression expression;
			if (aTypeNameExpression != null)
			{
				expression = aTypeNameExpression.LookupNameExpression(ec, MemberLookupRestrictions.InvocableOnly | MemberLookupRestrictions.ReadAccess);
				if (expression != null)
				{
					NameOf nameOf = expression as NameOf;
					if (nameOf != null)
					{
						return nameOf.ResolveOverload(ec, arguments);
					}
					expression = expression.Resolve(ec);
				}
			}
			else
			{
				expression = expr.Resolve(ec);
			}
			if (expression == null)
			{
				return null;
			}
			bool dynamic = false;
			if (arguments != null)
			{
				arguments.Resolve(ec, out dynamic);
			}
			TypeSpec typeSpec = expression.Type;
			if (typeSpec.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				return DoResolveDynamic(ec, expression);
			}
			mg = expression as MethodGroupExpr;
			Expression expression2 = null;
			if (mg == null)
			{
				if (typeSpec == null || !typeSpec.IsDelegate)
				{
					if (expression is RuntimeValueExpression)
					{
						ec.Report.Error(10000, loc, "Cannot invoke a non-delegate type `{0}'", expression.Type.GetSignatureForError());
						return null;
					}
					if (!(expression is MemberExpr))
					{
						expression.Error_UnexpectedKind(ec, ResolveFlags.MethodGroup, loc);
						return null;
					}
					ec.Report.Error(1955, loc, "The member `{0}' cannot be used as method or delegate", expression.GetSignatureForError());
					return null;
				}
				expression2 = new DelegateInvocation(expression, arguments, conditional_access_receiver, loc);
				expression2 = expression2.Resolve(ec);
				if (expression2 == null || !dynamic)
				{
					return expression2;
				}
			}
			if (expression2 == null)
			{
				mg = DoResolveOverload(ec);
				if (mg == null)
				{
					return null;
				}
			}
			if (dynamic)
			{
				return DoResolveDynamic(ec, expression);
			}
			MethodSpec bestCandidate = mg.BestCandidate;
			type = mg.BestCandidateReturnType;
			if (conditional_access_receiver)
			{
				type = Expression.LiftMemberType(ec, type);
			}
			if (arguments == null && bestCandidate.DeclaringType.BuiltinType == BuiltinTypeSpec.Type.Object && bestCandidate.Name == Destructor.MetadataName)
			{
				if (mg.IsBase)
				{
					ec.Report.Error(250, loc, "Do not directly call your base class Finalize method. It is called automatically from your destructor");
				}
				else
				{
					ec.Report.Error(245, loc, "Destructors and object.Finalize cannot be called directly. Consider calling IDisposable.Dispose if available");
				}
				return null;
			}
			IsSpecialMethodInvocation(ec, bestCandidate, loc);
			eclass = ExprClass.Value;
			return this;
		}

		protected virtual Expression DoResolveDynamic(ResolveContext ec, Expression memberExpr)
		{
			DynamicMemberBinder dynamicMemberBinder = memberExpr as DynamicMemberBinder;
			Arguments arguments;
			if (dynamicMemberBinder != null)
			{
				arguments = dynamicMemberBinder.Arguments;
				if (this.arguments != null)
				{
					arguments.AddRange(this.arguments);
				}
			}
			else if (mg == null)
			{
				arguments = ((this.arguments != null) ? this.arguments : new Arguments(1));
				arguments.Insert(0, new Argument(memberExpr));
				expr = null;
			}
			else
			{
				if (mg.IsBase)
				{
					ec.Report.Error(1971, loc, "The base call to method `{0}' cannot be dynamically dispatched. Consider casting the dynamic arguments or eliminating the base access", mg.Name);
					return null;
				}
				arguments = ((this.arguments != null) ? this.arguments : new Arguments(1));
				if (expr is MemberAccess)
				{
					Expression instanceExpression = mg.InstanceExpression;
					TypeExpr typeExpr = instanceExpression as TypeExpr;
					if (typeExpr != null)
					{
						arguments.Insert(0, new Argument(new TypeOf(typeExpr.Type, loc).Resolve(ec), Argument.AType.DynamicTypeName));
					}
					else if (instanceExpression != null)
					{
						Argument.AType aType = ((instanceExpression is IMemoryLocation && TypeSpec.IsValueType(instanceExpression.Type)) ? Argument.AType.Ref : Argument.AType.None);
						arguments.Insert(0, new Argument(instanceExpression.Resolve(ec), aType));
					}
				}
				else if (ec.IsStatic)
				{
					arguments.Insert(0, new Argument(new TypeOf(ec.CurrentType, loc).Resolve(ec), Argument.AType.DynamicTypeName));
				}
				else
				{
					arguments.Insert(0, new Argument(new This(loc).Resolve(ec)));
				}
			}
			return new DynamicInvocation(expr as ATypeNameExpression, arguments, loc).Resolve(ec);
		}

		protected virtual MethodGroupExpr DoResolveOverload(ResolveContext ec)
		{
			return mg.OverloadResolve(ec, ref arguments, null, OverloadResolver.Restrictions.None);
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			if (!mg.IsConditionallyExcluded)
			{
				mg.FlowAnalysis(fc);
				if (arguments != null)
				{
					arguments.FlowAnalysis(fc);
				}
				if (conditional_access_receiver)
				{
					fc.ConditionalAccessEnd();
				}
			}
		}

		public override string GetSignatureForError()
		{
			return mg.GetSignatureForError();
		}

		public override bool HasConditionalAccess()
		{
			return expr.HasConditionalAccess();
		}

		public static bool IsMemberInvocable(MemberSpec member)
		{
			switch (member.Kind)
			{
			case MemberKind.Event:
				return true;
			case MemberKind.Field:
			case MemberKind.Property:
			{
				IInterfaceMemberSpec interfaceMemberSpec = member as IInterfaceMemberSpec;
				if (!interfaceMemberSpec.MemberType.IsDelegate)
				{
					return interfaceMemberSpec.MemberType.BuiltinType == BuiltinTypeSpec.Type.Dynamic;
				}
				return true;
			}
			default:
				return false;
			}
		}

		public static bool IsSpecialMethodInvocation(ResolveContext ec, MethodSpec method, Location loc)
		{
			if (!method.IsReservedMethod)
			{
				return false;
			}
			if (ec.HasSet(ResolveContext.Options.InvokeSpecialName) || ec.CurrentMemberDefinition.IsCompilerGenerated)
			{
				return false;
			}
			ec.Report.SymbolRelatedToPreviousError(method);
			ec.Report.Error(571, loc, "`{0}': cannot explicitly call operator or accessor", method.GetSignatureForError());
			return true;
		}

		public override void Emit(EmitContext ec)
		{
			if (!mg.IsConditionallyExcluded)
			{
				if (conditional_access_receiver)
				{
					mg.EmitCall(ec, arguments, type, false);
				}
				else
				{
					mg.EmitCall(ec, arguments, false);
				}
			}
		}

		public override void EmitStatement(EmitContext ec)
		{
			if (!mg.IsConditionallyExcluded)
			{
				if (conditional_access_receiver)
				{
					mg.EmitCall(ec, arguments, type, true);
				}
				else
				{
					mg.EmitCall(ec, arguments, true);
				}
			}
		}

		public override System.Linq.Expressions.Expression MakeExpression(BuilderContext ctx)
		{
			return MakeExpression(ctx, mg.InstanceExpression, mg.BestCandidate, arguments);
		}

		public static System.Linq.Expressions.Expression MakeExpression(BuilderContext ctx, Expression instance, MethodSpec mi, Arguments args)
		{
			return System.Linq.Expressions.Expression.Call(instance?.MakeExpression(ctx), (MethodInfo)mi.GetMetaInfo(), Arguments.MakeExpression(args, ctx));
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
