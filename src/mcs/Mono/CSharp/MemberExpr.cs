using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public abstract class MemberExpr : Expression, OverloadResolver.IInstanceQualifier
	{
		protected bool conditional_access_receiver;

		public Expression InstanceExpression;

		public abstract string Name { get; }

		public bool IsBase => InstanceExpression is BaseThis;

		public abstract bool IsInstance { get; }

		public abstract bool IsStatic { get; }

		public abstract string KindName { get; }

		public bool ConditionalAccess { get; set; }

		protected abstract TypeSpec DeclaringType { get; }

		TypeSpec OverloadResolver.IInstanceQualifier.InstanceType => InstanceExpression.Type;

		protected MethodSpec CandidateToBaseOverride(ResolveContext rc, MethodSpec method)
		{
			if (!IsBase)
			{
				return method;
			}
			if ((method.Modifiers & (Modifiers.ABSTRACT | Modifiers.VIRTUAL | Modifiers.OVERRIDE)) != 0)
			{
				TypeSpec[] array = null;
				if (method.DeclaringType != InstanceExpression.Type)
				{
					AParametersCollection aParametersCollection = method.Parameters;
					if (method.Arity > 0)
					{
						aParametersCollection = ((IParametersMember)method.MemberDefinition).Parameters;
						InflatedTypeSpec inflatedTypeSpec = method.DeclaringType as InflatedTypeSpec;
						if (inflatedTypeSpec != null)
						{
							aParametersCollection = aParametersCollection.Inflate(inflatedTypeSpec.CreateLocalInflator(rc));
						}
					}
					MethodSpec methodSpec = MemberCache.FindMember(filter: new MemberFilter(method.Name, method.Arity, MemberKind.Method, aParametersCollection, null), container: InstanceExpression.Type, restrictions: BindingRestriction.InstanceOnly | BindingRestriction.OverrideOnly) as MethodSpec;
					if (methodSpec != null && methodSpec.DeclaringType != method.DeclaringType)
					{
						if (methodSpec.IsGeneric)
						{
							array = method.TypeArguments;
						}
						method = methodSpec;
					}
				}
				if (rc.CurrentAnonymousMethod != null)
				{
					if (array == null && method.IsGeneric)
					{
						array = method.TypeArguments;
						method = method.GetGenericMethodDefinition();
					}
					if (method.Parameters.HasArglist)
					{
						throw new NotImplementedException("__arglist base call proxy");
					}
					method = rc.CurrentMemberDefinition.Parent.PartialContainer.CreateHoistedBaseCallProxy(rc, method);
					if (rc.CurrentType.IsStruct || rc.CurrentAnonymousMethod.Storey is AsyncTaskStorey)
					{
						InstanceExpression = new This(loc).Resolve(rc);
					}
				}
				if (array != null)
				{
					method = method.MakeGenericMethod(rc, array);
				}
			}
			if (method.IsAbstract)
			{
				rc.Report.SymbolRelatedToPreviousError(method);
				Error_CannotCallAbstractBase(rc, method.GetSignatureForError());
			}
			return method;
		}

		protected void CheckProtectedMemberAccess(ResolveContext rc, MemberSpec member)
		{
			if (InstanceExpression != null && (member.Modifiers & Modifiers.PROTECTED) != 0 && !(InstanceExpression is This) && !CheckProtectedMemberAccess(rc, member, InstanceExpression.Type))
			{
				Error_ProtectedMemberAccess(rc, member, InstanceExpression.Type, loc);
			}
		}

		bool OverloadResolver.IInstanceQualifier.CheckProtectedMemberAccess(ResolveContext rc, MemberSpec member)
		{
			if (InstanceExpression == null)
			{
				return true;
			}
			if (!(InstanceExpression is This))
			{
				return CheckProtectedMemberAccess(rc, member, InstanceExpression.Type);
			}
			return true;
		}

		public static bool CheckProtectedMemberAccess<T>(ResolveContext rc, T member, TypeSpec qualifier) where T : MemberSpec
		{
			TypeSpec currentType = rc.CurrentType;
			if (currentType == qualifier)
			{
				return true;
			}
			if ((member.Modifiers & Modifiers.INTERNAL) != 0 && member.DeclaringType.MemberDefinition.IsInternalAsPublic(currentType.MemberDefinition.DeclaringAssembly))
			{
				return true;
			}
			qualifier = qualifier.GetDefinition();
			if (currentType != qualifier && !IsSameOrBaseQualifier(currentType, qualifier))
			{
				return false;
			}
			return true;
		}

		public override bool ContainsEmitWithAwait()
		{
			if (InstanceExpression != null)
			{
				return InstanceExpression.ContainsEmitWithAwait();
			}
			return false;
		}

		public override bool HasConditionalAccess()
		{
			if (!ConditionalAccess)
			{
				if (InstanceExpression != null)
				{
					return InstanceExpression.HasConditionalAccess();
				}
				return false;
			}
			return true;
		}

		private static bool IsSameOrBaseQualifier(TypeSpec type, TypeSpec qtype)
		{
			do
			{
				type = type.GetDefinition();
				if (type == qtype || TypeManager.IsFamilyAccessible(qtype, type))
				{
					return true;
				}
				type = type.DeclaringType;
			}
			while (type != null);
			return false;
		}

		protected void DoBestMemberChecks<T>(ResolveContext rc, T member) where T : MemberSpec, IInterfaceMemberSpec
		{
			if (InstanceExpression != null)
			{
				InstanceExpression = InstanceExpression.Resolve(rc);
				CheckProtectedMemberAccess(rc, member);
			}
			if (member.MemberType.IsPointer && !rc.IsUnsafe)
			{
				Expression.UnsafeError(rc, loc);
			}
			List<MissingTypeSpecReference> missingDependencies = member.GetMissingDependencies();
			if (missingDependencies != null)
			{
				ImportedTypeDefinition.Error_MissingDependency(rc, missingDependencies, loc);
			}
			if (!rc.IsObsolete)
			{
				ObsoleteAttribute attributeObsolete = member.GetAttributeObsolete();
				if (attributeObsolete != null)
				{
					AttributeTester.Report_ObsoleteMessage(attributeObsolete, member.GetSignatureForError(), loc, rc.Report);
				}
			}
			if (!(member is FieldSpec))
			{
				member.MemberDefinition.SetIsUsed();
			}
		}

		protected virtual void Error_CannotCallAbstractBase(ResolveContext rc, string name)
		{
			rc.Report.Error(205, loc, "Cannot call an abstract base member `{0}'", name);
		}

		public static void Error_ProtectedMemberAccess(ResolveContext rc, MemberSpec member, TypeSpec qualifier, Location loc)
		{
			rc.Report.SymbolRelatedToPreviousError(member);
			rc.Report.Error(1540, loc, "Cannot access protected member `{0}' via a qualifier of type `{1}'. The qualifier must be of type `{2}' or derived from it", member.GetSignatureForError(), qualifier.GetSignatureForError(), rc.CurrentType.GetSignatureForError());
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			if (InstanceExpression != null)
			{
				InstanceExpression.FlowAnalysis(fc);
				if (ConditionalAccess)
				{
					fc.BranchConditionalAccessDefiniteAssignment();
				}
			}
		}

		protected void ResolveConditionalAccessReceiver(ResolveContext rc)
		{
			if (!rc.HasSet(ResolveContext.Options.ConditionalAccessReceiver) && HasConditionalAccess())
			{
				conditional_access_receiver = true;
				rc.Set(ResolveContext.Options.ConditionalAccessReceiver);
			}
		}

		public bool ResolveInstanceExpression(ResolveContext rc, Expression rhs)
		{
			if (!ResolveInstanceExpressionCore(rc, rhs))
			{
				return false;
			}
			if (rhs != null && TypeSpec.IsValueType(InstanceExpression.Type))
			{
				FieldExpr fieldExpr = InstanceExpression as FieldExpr;
				if (fieldExpr != null)
				{
					if (!fieldExpr.Spec.IsReadOnly || rc.HasAny(ResolveContext.Options.FieldInitializerScope | ResolveContext.Options.ConstructorScope))
					{
						return true;
					}
					if (fieldExpr.IsStatic)
					{
						rc.Report.Error(1650, loc, "Fields of static readonly field `{0}' cannot be assigned to (except in a static constructor or a variable initializer)", fieldExpr.GetSignatureForError());
					}
					else
					{
						rc.Report.Error(1648, loc, "Members of readonly field `{0}' cannot be modified (except in a constructor or a variable initializer)", fieldExpr.GetSignatureForError());
					}
					return true;
				}
				if (InstanceExpression is PropertyExpr || InstanceExpression is IndexerExpr || InstanceExpression is Invocation)
				{
					if (rc.CurrentInitializerVariable != null)
					{
						rc.Report.Error(1918, loc, "Members of value type `{0}' cannot be assigned using a property `{1}' object initializer", InstanceExpression.Type.GetSignatureForError(), InstanceExpression.GetSignatureForError());
					}
					else
					{
						rc.Report.Error(1612, loc, "Cannot modify a value type return value of `{0}'. Consider storing the value in a temporary variable", InstanceExpression.GetSignatureForError());
					}
					return true;
				}
				LocalVariableReference localVariableReference = InstanceExpression as LocalVariableReference;
				if (localVariableReference != null)
				{
					if (!localVariableReference.local_info.IsReadonly)
					{
						return true;
					}
					rc.Report.Error(1654, loc, "Cannot assign to members of `{0}' because it is a `{1}'", InstanceExpression.GetSignatureForError(), localVariableReference.local_info.GetReadOnlyContext());
				}
			}
			return true;
		}

		private bool ResolveInstanceExpressionCore(ResolveContext rc, Expression rhs)
		{
			if (IsStatic)
			{
				if (InstanceExpression != null)
				{
					if (InstanceExpression is TypeExpr)
					{
						TypeSpec declaringType = InstanceExpression.Type;
						do
						{
							ObsoleteAttribute attributeObsolete = declaringType.GetAttributeObsolete();
							if (attributeObsolete != null && !rc.IsObsolete)
							{
								AttributeTester.Report_ObsoleteMessage(attributeObsolete, declaringType.GetSignatureForError(), loc, rc.Report);
							}
							declaringType = declaringType.DeclaringType;
						}
						while (declaringType != null);
					}
					else
					{
						RuntimeValueExpression runtimeValueExpression = InstanceExpression as RuntimeValueExpression;
						if (runtimeValueExpression == null || !runtimeValueExpression.IsSuggestionOnly)
						{
							rc.Report.Error(176, loc, "Static member `{0}' cannot be accessed with an instance reference, qualify it with a type name instead", GetSignatureForError());
						}
					}
					InstanceExpression = null;
				}
				return false;
			}
			if (InstanceExpression == null || InstanceExpression is TypeExpr)
			{
				if (InstanceExpression != null || !This.IsThisAvailable(rc, true))
				{
					if (rc.HasSet(ResolveContext.Options.FieldInitializerScope))
					{
						rc.Report.Error(236, loc, "A field initializer cannot reference the nonstatic field, method, or property `{0}'", GetSignatureForError());
					}
					else
					{
						FieldExpr fieldExpr = this as FieldExpr;
						if (fieldExpr != null && fieldExpr.Spec.MemberDefinition is PrimaryConstructorField)
						{
							if (rc.HasSet(ResolveContext.Options.BaseInitializer))
							{
								rc.Report.Error(9005, loc, "Constructor initializer cannot access primary constructor parameters");
							}
							else
							{
								rc.Report.Error(9006, loc, "An object reference is required to access primary constructor parameter `{0}'", fieldExpr.Name);
							}
						}
						else
						{
							rc.Report.Error(120, loc, "An object reference is required to access non-static member `{0}'", GetSignatureForError());
						}
					}
					InstanceExpression = new CompilerGeneratedThis(rc.CurrentType, loc).Resolve(rc);
					return false;
				}
				if (!TypeManager.IsFamilyAccessible(rc.CurrentType, DeclaringType))
				{
					rc.Report.Error(38, loc, "Cannot access a nonstatic member of outer type `{0}' via nested type `{1}'", DeclaringType.GetSignatureForError(), rc.CurrentType.GetSignatureForError());
				}
				InstanceExpression = new This(loc).Resolve(rc);
				return false;
			}
			MemberExpr memberExpr = InstanceExpression as MemberExpr;
			if (memberExpr != null)
			{
				memberExpr.ResolveInstanceExpressionCore(rc, rhs);
				FieldExpr fieldExpr2 = memberExpr as FieldExpr;
				if (fieldExpr2 != null && fieldExpr2.IsMarshalByRefAccess(rc))
				{
					rc.Report.SymbolRelatedToPreviousError(memberExpr.DeclaringType);
					rc.Report.Warning(1690, 1, loc, "Cannot call methods, properties, or indexers on `{0}' because it is a value type member of a marshal-by-reference class", memberExpr.GetSignatureForError());
				}
				return true;
			}
			if (rhs != null && InstanceExpression is UnboxCast)
			{
				rc.Report.Error(445, InstanceExpression.Location, "Cannot modify the result of an unboxing conversion");
			}
			return true;
		}

		public virtual MemberExpr ResolveMemberAccess(ResolveContext ec, Expression left, SimpleName original)
		{
			if (left != null && !ConditionalAccess && left.IsNull && TypeSpec.IsReferenceType(left.Type))
			{
				ec.Report.Warning(1720, 1, left.Location, "Expression will always cause a `{0}'", "System.NullReferenceException");
			}
			InstanceExpression = left;
			return this;
		}

		protected void EmitInstance(EmitContext ec, bool prepare_for_load)
		{
			new InstanceEmitter(InstanceExpression, TypeSpec.IsValueType(InstanceExpression.Type)).Emit(ec, ConditionalAccess);
			if (prepare_for_load)
			{
				ec.Emit(OpCodes.Dup);
			}
		}

		public abstract void SetTypeArguments(ResolveContext ec, TypeArguments ta);
	}
}
