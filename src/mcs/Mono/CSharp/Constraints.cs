using System;
using System.Collections.Generic;

namespace Mono.CSharp
{
	public class Constraints
	{
		private readonly SimpleMemberName tparam;

		private readonly List<FullNamedExpression> constraints;

		private readonly Location loc;

		private bool resolved;

		private bool resolving;

		public List<FullNamedExpression> TypeExpressions => constraints;

		public Location Location => loc;

		public SimpleMemberName TypeParameter => tparam;

		public Constraints(SimpleMemberName tparam, List<FullNamedExpression> constraints, Location loc)
		{
			this.tparam = tparam;
			this.constraints = constraints;
			this.loc = loc;
		}

		public static bool CheckConflictingInheritedConstraint(TypeParameterSpec spec, TypeSpec bb, IMemberContext context, Location loc)
		{
			if (spec.HasSpecialClass && bb.IsStruct)
			{
				context.Module.Compiler.Report.Error(455, loc, "Type parameter `{0}' inherits conflicting constraints `{1}' and `{2}'", spec.Name, "class", bb.GetSignatureForError());
				return false;
			}
			return CheckConflictingInheritedConstraint(spec, spec.BaseType, bb, context, loc);
		}

		private static bool CheckConflictingInheritedConstraint(TypeParameterSpec spec, TypeSpec ba, TypeSpec bb, IMemberContext context, Location loc)
		{
			if (ba == bb)
			{
				return true;
			}
			if (TypeSpec.IsBaseClass(ba, bb, false) || TypeSpec.IsBaseClass(bb, ba, false))
			{
				return true;
			}
			Error_ConflictingConstraints(context, spec, ba, bb, loc);
			return false;
		}

		public static void Error_ConflictingConstraints(IMemberContext context, TypeParameterSpec tp, TypeSpec ba, TypeSpec bb, Location loc)
		{
			context.Module.Compiler.Report.Error(455, loc, "Type parameter `{0}' inherits conflicting constraints `{1}' and `{2}'", tp.Name, ba.GetSignatureForError(), bb.GetSignatureForError());
		}

		public void CheckGenericConstraints(IMemberContext context, bool obsoleteCheck)
		{
			foreach (FullNamedExpression constraint in constraints)
			{
				if (constraint == null)
				{
					continue;
				}
				TypeSpec type = constraint.Type;
				if (type == null)
				{
					continue;
				}
				if (obsoleteCheck)
				{
					ObsoleteAttribute attributeObsolete = type.GetAttributeObsolete();
					if (attributeObsolete != null)
					{
						AttributeTester.Report_ObsoleteMessage(attributeObsolete, type.GetSignatureForError(), constraint.Location, context.Module.Compiler.Report);
					}
				}
				ConstraintChecker.Check(context, type, constraint.Location);
			}
		}

		public bool Resolve(IMemberContext context, TypeParameter tp)
		{
			if (resolved)
			{
				return true;
			}
			if (resolving)
			{
				return false;
			}
			resolving = true;
			TypeParameterSpec type = tp.Type;
			List<TypeParameterSpec> list = null;
			bool flag = false;
			type.BaseType = context.Module.Compiler.BuiltinTypes.Object;
			for (int i = 0; i < constraints.Count; i++)
			{
				FullNamedExpression fullNamedExpression = constraints[i];
				if (fullNamedExpression is SpecialContraintExpr)
				{
					type.SpecialConstraint |= ((SpecialContraintExpr)fullNamedExpression).Constraint;
					if (type.HasSpecialStruct)
					{
						type.BaseType = context.Module.Compiler.BuiltinTypes.ValueType;
					}
					constraints[i] = null;
					continue;
				}
				TypeSpec typeSpec = fullNamedExpression.ResolveAsType(context);
				if (typeSpec == null)
				{
					continue;
				}
				if (typeSpec.Arity > 0 && ((InflatedTypeSpec)typeSpec).HasDynamicArgument())
				{
					context.Module.Compiler.Report.Error(1968, fullNamedExpression.Location, "A constraint cannot be the dynamic type `{0}'", typeSpec.GetSignatureForError());
					continue;
				}
				if (!context.CurrentMemberDefinition.IsAccessibleAs(typeSpec))
				{
					context.Module.Compiler.Report.SymbolRelatedToPreviousError(typeSpec);
					context.Module.Compiler.Report.Error(703, loc, "Inconsistent accessibility: constraint type `{0}' is less accessible than `{1}'", typeSpec.GetSignatureForError(), context.GetSignatureForError());
				}
				if (typeSpec.IsInterface)
				{
					if (!type.AddInterface(typeSpec))
					{
						context.Module.Compiler.Report.Error(405, fullNamedExpression.Location, "Duplicate constraint `{0}' for type parameter `{1}'", typeSpec.GetSignatureForError(), tparam.Value);
					}
					flag = true;
					continue;
				}
				TypeParameterSpec typeParameterSpec = typeSpec as TypeParameterSpec;
				if (typeParameterSpec != null)
				{
					if (list == null)
					{
						list = new List<TypeParameterSpec>(2);
					}
					else if (list.Contains(typeParameterSpec))
					{
						context.Module.Compiler.Report.Error(405, fullNamedExpression.Location, "Duplicate constraint `{0}' for type parameter `{1}'", typeSpec.GetSignatureForError(), tparam.Value);
						continue;
					}
					if (tp.IsMethodTypeParameter)
					{
						VarianceDecl.CheckTypeVariance(typeSpec, Variance.Contravariant, context);
					}
					TypeParameter typeParameter = typeParameterSpec.MemberDefinition as TypeParameter;
					if (typeParameter != null && !typeParameter.ResolveConstraints(context))
					{
						context.Module.Compiler.Report.Error(454, fullNamedExpression.Location, "Circular constraint dependency involving `{0}' and `{1}'", typeParameterSpec.GetSignatureForError(), tp.GetSignatureForError());
						continue;
					}
					if (typeParameterSpec.HasTypeConstraint)
					{
						if (type.HasTypeConstraint || type.HasSpecialStruct)
						{
							if (!CheckConflictingInheritedConstraint(type, typeParameterSpec.BaseType, context, fullNamedExpression.Location))
							{
								continue;
							}
						}
						else
						{
							for (int j = 0; j < list.Count && (!list[j].HasTypeConstraint || CheckConflictingInheritedConstraint(type, list[j].BaseType, typeParameterSpec.BaseType, context, fullNamedExpression.Location)); j++)
							{
							}
						}
					}
					if (typeParameterSpec.TypeArguments != null)
					{
						TypeSpec effectiveBase = typeParameterSpec.GetEffectiveBase();
						if (effectiveBase != null && !CheckConflictingInheritedConstraint(type, effectiveBase, type.BaseType, context, fullNamedExpression.Location))
						{
							break;
						}
					}
					if (typeParameterSpec.HasSpecialStruct)
					{
						context.Module.Compiler.Report.Error(456, fullNamedExpression.Location, "Type parameter `{0}' has the `struct' constraint, so it cannot be used as a constraint for `{1}'", typeParameterSpec.GetSignatureForError(), tp.GetSignatureForError());
					}
					else
					{
						list.Add(typeParameterSpec);
					}
					continue;
				}
				if (flag || type.HasTypeConstraint)
				{
					context.Module.Compiler.Report.Error(406, fullNamedExpression.Location, "The class type constraint `{0}' must be listed before any other constraints. Consider moving type constraint to the beginning of the constraint list", typeSpec.GetSignatureForError());
				}
				if (type.HasSpecialStruct || type.HasSpecialClass)
				{
					context.Module.Compiler.Report.Error(450, fullNamedExpression.Location, "`{0}': cannot specify both a constraint class and the `class' or `struct' constraint", typeSpec.GetSignatureForError());
				}
				switch (typeSpec.BuiltinType)
				{
				case BuiltinTypeSpec.Type.Object:
				case BuiltinTypeSpec.Type.ValueType:
				case BuiltinTypeSpec.Type.Enum:
				case BuiltinTypeSpec.Type.Delegate:
				case BuiltinTypeSpec.Type.MulticastDelegate:
				case BuiltinTypeSpec.Type.Array:
					context.Module.Compiler.Report.Error(702, fullNamedExpression.Location, "A constraint cannot be special class `{0}'", typeSpec.GetSignatureForError());
					continue;
				case BuiltinTypeSpec.Type.Dynamic:
					context.Module.Compiler.Report.Error(1967, fullNamedExpression.Location, "A constraint cannot be the dynamic type");
					continue;
				}
				if (typeSpec.IsSealed || !typeSpec.IsClass)
				{
					context.Module.Compiler.Report.Error(701, loc, "`{0}' is not a valid constraint. A constraint must be an interface, a non-sealed class or a type parameter", typeSpec.GetSignatureForError());
					continue;
				}
				if (typeSpec.IsStatic)
				{
					context.Module.Compiler.Report.Error(717, fullNamedExpression.Location, "`{0}' is not a valid constraint. Static classes cannot be used as constraints", typeSpec.GetSignatureForError());
				}
				type.BaseType = typeSpec;
			}
			if (list != null)
			{
				type.TypeArguments = list.ToArray();
			}
			resolving = false;
			resolved = true;
			return true;
		}

		public void VerifyClsCompliance(Report report)
		{
			foreach (FullNamedExpression constraint in constraints)
			{
				if (constraint != null && !constraint.Type.IsCLSCompliant())
				{
					report.SymbolRelatedToPreviousError(constraint.Type);
					report.Warning(3024, 1, loc, "Constraint type `{0}' is not CLS-compliant", constraint.Type.GetSignatureForError());
				}
			}
		}
	}
}
