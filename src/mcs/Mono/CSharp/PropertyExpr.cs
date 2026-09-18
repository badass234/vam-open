using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	internal sealed class PropertyExpr : PropertyOrIndexerExpr<PropertySpec>
	{
		private Arguments arguments;

		private FieldExpr backing_field;

		protected override Arguments Arguments
		{
			get
			{
				return arguments;
			}
			set
			{
				arguments = value;
			}
		}

		protected override TypeSpec DeclaringType => best_candidate.DeclaringType;

		public override string Name => best_candidate.Name;

		public bool IsAutoPropertyAccess
		{
			get
			{
				Property property = best_candidate.MemberDefinition as Property;
				if (property != null)
				{
					return property.BackingField != null;
				}
				return false;
			}
		}

		public override bool IsInstance => !IsStatic;

		public override bool IsStatic => best_candidate.IsStatic;

		public override string KindName => "property";

		public PropertySpec PropertyInfo => best_candidate;

		public PropertyExpr(PropertySpec spec, Location l)
			: base(l)
		{
			best_candidate = spec;
			type = spec.MemberType;
		}

		public override MethodGroupExpr CanReduceLambda(AnonymousMethodBody body)
		{
			if (best_candidate == null || (!best_candidate.IsStatic && !(InstanceExpression is This)))
			{
				return null;
			}
			int num = ((arguments != null) ? arguments.Count : 0);
			if (num != body.Parameters.Count && num == 0)
			{
				return null;
			}
			MethodGroupExpr methodGroupExpr = MethodGroupExpr.CreatePredefined(best_candidate.Get, DeclaringType, loc);
			methodGroupExpr.InstanceExpression = InstanceExpression;
			return methodGroupExpr;
		}

		public static PropertyExpr CreatePredefined(PropertySpec spec, Location loc)
		{
			return new PropertyExpr(spec, loc)
			{
				Getter = spec.Get,
				Setter = spec.Set
			};
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			if (base.ConditionalAccess)
			{
				Error_NullShortCircuitInsideExpressionTree(ec);
			}
			Arguments arguments;
			if (IsSingleDimensionalArrayLength())
			{
				arguments = new Arguments(1);
				arguments.Add(new Argument(InstanceExpression.CreateExpressionTree(ec)));
				return CreateExpressionFactoryCall(ec, "ArrayLength", arguments);
			}
			arguments = new Arguments(2);
			if (InstanceExpression == null)
			{
				arguments.Add(new Argument(new NullLiteral(loc)));
			}
			else
			{
				arguments.Add(new Argument(InstanceExpression.CreateExpressionTree(ec)));
			}
			arguments.Add(new Argument(new TypeOfMethod(base.Getter, loc)));
			return CreateExpressionFactoryCall(ec, "Property", arguments);
		}

		public Expression CreateSetterTypeOfExpression(ResolveContext rc)
		{
			DoResolveLValue(rc, null);
			return new TypeOfMethod(base.Setter, loc);
		}

		public override string GetSignatureForError()
		{
			return best_candidate.GetSignatureForError();
		}

		public override System.Linq.Expressions.Expression MakeAssignExpression(BuilderContext ctx, Expression source)
		{
			return System.Linq.Expressions.Expression.Property(InstanceExpression.MakeExpression(ctx), (MethodInfo)base.Setter.GetMetaInfo());
		}

		public override System.Linq.Expressions.Expression MakeExpression(BuilderContext ctx)
		{
			return System.Linq.Expressions.Expression.Property(InstanceExpression.MakeExpression(ctx), (MethodInfo)base.Getter.GetMetaInfo());
		}

		private void Error_PropertyNotValid(ResolveContext ec)
		{
			ec.Report.SymbolRelatedToPreviousError(best_candidate);
			ec.Report.Error(1546, loc, "Property or event `{0}' is not supported by the C# language", GetSignatureForError());
		}

		private bool IsSingleDimensionalArrayLength()
		{
			if (best_candidate.DeclaringType.BuiltinType != BuiltinTypeSpec.Type.Array || !best_candidate.HasGet || Name != "Length")
			{
				return false;
			}
			ArrayContainer arrayContainer = InstanceExpression.Type as ArrayContainer;
			if (arrayContainer != null)
			{
				return arrayContainer.Rank == 1;
			}
			return false;
		}

		public override void Emit(EmitContext ec, bool leave_copy)
		{
			if (IsSingleDimensionalArrayLength())
			{
				if (conditional_access_receiver)
				{
					ec.ConditionalAccess = new ConditionalAccessContext(type, ec.DefineLabel());
				}
				EmitInstance(ec, false);
				ec.Emit(OpCodes.Ldlen);
				ec.Emit(OpCodes.Conv_I4);
				if (conditional_access_receiver)
				{
					ec.CloseConditionalAccess(type);
				}
			}
			else
			{
				base.Emit(ec, leave_copy);
			}
		}

		public override void EmitAssign(EmitContext ec, Expression source, bool leave_copy, bool isCompound)
		{
			if (backing_field != null)
			{
				backing_field.EmitAssign(ec, source, false, false);
				return;
			}
			LocalTemporary localTemporary = null;
			Arguments arguments;
			if (isCompound && !(source is DynamicExpressionStatement))
			{
				emitting_compound_assignment = true;
				source.Emit(ec);
				if (has_await_arguments)
				{
					localTemporary = new LocalTemporary(base.Type);
					localTemporary.Store(ec);
					arguments = new Arguments(1);
					arguments.Add(new Argument(localTemporary));
					if (leave_copy)
					{
						temp = localTemporary;
					}
					has_await_arguments = false;
				}
				else
				{
					arguments = null;
					if (leave_copy)
					{
						ec.Emit(OpCodes.Dup);
						temp = new LocalTemporary(base.Type);
						temp.Store(ec);
					}
				}
			}
			else
			{
				arguments = this.arguments ?? new Arguments(1);
				if (leave_copy)
				{
					source.Emit(ec);
					temp = new LocalTemporary(base.Type);
					temp.Store(ec);
					arguments.Add(new Argument(temp));
				}
				else
				{
					arguments.Add(new Argument(source));
				}
			}
			emitting_compound_assignment = false;
			CallEmitter callEmitter = new CallEmitter
			{
				InstanceExpression = InstanceExpression
			};
			if (arguments == null)
			{
				callEmitter.InstanceExpressionOnStack = true;
			}
			if (base.ConditionalAccess)
			{
				callEmitter.ConditionalAccess = true;
			}
			if (leave_copy)
			{
				callEmitter.Emit(ec, base.Setter, arguments, loc);
			}
			else
			{
				callEmitter.EmitStatement(ec, base.Setter, arguments, loc);
			}
			if (temp != null)
			{
				temp.Emit(ec);
				temp.Release(ec);
			}
			localTemporary?.Release(ec);
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			Property property = best_candidate.MemberDefinition as Property;
			if (property != null && property.BackingField != null)
			{
				IVariableReference variableReference = InstanceExpression as IVariableReference;
				if (variableReference != null)
				{
					VariableInfo variableInfo = variableReference.VariableInfo;
					if (variableInfo != null && !fc.IsStructFieldDefinitelyAssigned(variableInfo, property.BackingField.Name))
					{
						fc.Report.Error(8079, loc, "Use of possibly unassigned auto-implemented property `{0}'", Name);
						return;
					}
					if (TypeSpec.IsValueType(InstanceExpression.Type) && InstanceExpression is VariableReference)
					{
						return;
					}
				}
			}
			base.FlowAnalysis(fc);
			if (conditional_access_receiver)
			{
				fc.ConditionalAccessEnd();
			}
		}

		protected override Expression OverloadResolve(ResolveContext rc, Expression right_side)
		{
			eclass = ExprClass.PropertyAccess;
			if (best_candidate.IsNotCSharpCompatible)
			{
				Error_PropertyNotValid(rc);
			}
			ResolveInstanceExpression(rc, right_side);
			if ((best_candidate.Modifiers & (Modifiers.ABSTRACT | Modifiers.VIRTUAL)) != 0 && best_candidate.DeclaringType != InstanceExpression.Type)
			{
				PropertySpec propertySpec = MemberCache.FindMember(filter: new MemberFilter(best_candidate.Name, 0, MemberKind.Property, null, null), container: InstanceExpression.Type, restrictions: BindingRestriction.InstanceOnly | BindingRestriction.OverrideOnly) as PropertySpec;
				if (propertySpec != null)
				{
					type = propertySpec.MemberType;
				}
			}
			DoBestMemberChecks(rc, best_candidate);
			if (best_candidate.HasGet && !best_candidate.Get.Parameters.IsEmpty)
			{
				AParametersCollection parameters = best_candidate.Get.Parameters;
				arguments = new Arguments(parameters.Count);
				for (int i = 0; i < parameters.Count; i++)
				{
					arguments.Add(new Argument(OverloadResolver.ResolveDefaultValueArgument(rc, parameters.Types[i], parameters.FixedParameters[i].DefaultValue, loc)));
				}
			}
			else if (best_candidate.HasSet && best_candidate.Set.Parameters.Count > 1)
			{
				AParametersCollection parameters2 = best_candidate.Set.Parameters;
				arguments = new Arguments(parameters2.Count - 1);
				for (int j = 0; j < parameters2.Count - 1; j++)
				{
					arguments.Add(new Argument(OverloadResolver.ResolveDefaultValueArgument(rc, parameters2.Types[j], parameters2.FixedParameters[j].DefaultValue, loc)));
				}
			}
			return this;
		}

		protected override bool ResolveAutopropertyAssignment(ResolveContext rc, Expression rhs)
		{
			Property property = best_candidate.MemberDefinition as Property;
			if (property == null)
			{
				return false;
			}
			if (!rc.HasSet(ResolveContext.Options.ConstructorScope))
			{
				return false;
			}
			if (property.Parent.PartialContainer != rc.CurrentMemberDefinition.Parent.PartialContainer)
			{
				PropertySpec propertySpec = MemberCache.FindMember(rc.CurrentType, MemberFilter.Property(property.ShortName, property.MemberType), BindingRestriction.DeclaredOnly) as PropertySpec;
				if (propertySpec == null)
				{
					return false;
				}
				property = (Property)propertySpec.MemberDefinition;
			}
			Property.BackingFieldDeclaration backingField = property.BackingField;
			if (backingField == null)
			{
				return false;
			}
			if (rc.IsStatic != backingField.IsStatic)
			{
				return false;
			}
			if (!backingField.IsStatic && (!(InstanceExpression is This) || InstanceExpression is BaseThis))
			{
				return false;
			}
			backing_field = new FieldExpr(property.BackingField, loc);
			backing_field.ResolveLValue(rc, rhs);
			return true;
		}

		public void SetBackingFieldAssigned(FlowAnalysisContext fc)
		{
			if (backing_field != null)
			{
				backing_field.SetFieldAssigned(fc);
			}
			else
			{
				if (!IsAutoPropertyAccess)
				{
					return;
				}
				Property property = best_candidate.MemberDefinition as Property;
				if (property != null && property.BackingField != null && best_candidate.DeclaringType.IsStruct)
				{
					IVariableReference variableReference = InstanceExpression as IVariableReference;
					if (variableReference != null && variableReference.VariableInfo != null)
					{
						fc.SetStructFieldAssigned(variableReference.VariableInfo, property.BackingField.Name);
					}
				}
			}
		}

		public override void SetTypeArguments(ResolveContext ec, TypeArguments ta)
		{
			Expression.Error_TypeArgumentsCannotBeUsed(ec, "property", GetSignatureForError(), loc);
		}
	}
}
