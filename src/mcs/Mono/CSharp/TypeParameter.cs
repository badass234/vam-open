using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class TypeParameter : MemberCore, ITypeDefinition, IMemberDefinition
	{
		private static readonly string[] attribute_target = new string[1] { "type parameter" };

		private Constraints constraints;

		private GenericTypeParameterBuilder builder;

		private readonly TypeParameterSpec spec;

		public override AttributeTargets AttributeTargets => AttributeTargets.GenericParameter;

		public Constraints Constraints
		{
			get
			{
				return constraints;
			}
			set
			{
				constraints = value;
			}
		}

		public IAssemblyDefinition DeclaringAssembly => Module.DeclaringAssembly;

		public override string DocCommentHeader
		{
			get
			{
				throw new InvalidOperationException("Unexpected attempt to get doc comment from " + GetType());
			}
		}

		bool ITypeDefinition.IsComImport => false;

		bool ITypeDefinition.IsPartial => false;

		public bool IsMethodTypeParameter => spec.IsMethodOwned;

		bool ITypeDefinition.IsTypeForwarder => false;

		bool ITypeDefinition.IsCyclicTypeForwarder => false;

		public string Name => base.MemberName.Name;

		public string Namespace => null;

		public TypeParameterSpec Type => spec;

		public int TypeParametersCount => 0;

		public TypeParameterSpec[] TypeParameters => null;

		public override string[] ValidAttributeTargets => attribute_target;

		public Variance Variance => spec.Variance;

		public VarianceDecl VarianceDecl { get; private set; }

		public TypeParameter(int index, MemberName name, Constraints constraints, Attributes attrs, Variance Variance)
			: base(null, name, attrs)
		{
			this.constraints = constraints;
			spec = new TypeParameterSpec(null, index, this, SpecialConstraint.None, Variance, null);
		}

		public TypeParameter(MemberName name, Attributes attrs, VarianceDecl variance)
			: base(null, name, attrs)
		{
			spec = new TypeParameterSpec(null, -1, this, SpecialConstraint.None, variance?.Variance ?? Variance.None, null);
			VarianceDecl = variance;
		}

		public TypeParameter(TypeParameterSpec spec, TypeSpec parentSpec, MemberName name, Attributes attrs)
			: base(null, name, attrs)
		{
			this.spec = new TypeParameterSpec(parentSpec, spec.DeclaredPosition, spec.MemberDefinition, spec.SpecialConstraint, spec.Variance, null)
			{
				BaseType = spec.BaseType,
				InterfacesDefined = spec.InterfacesDefined,
				TypeArguments = spec.TypeArguments
			};
		}

		public bool AddPartialConstraints(TypeDefinition part, TypeParameter tp)
		{
			if (builder == null)
			{
				throw new InvalidOperationException();
			}
			if (tp.constraints == null)
			{
				return true;
			}
			tp.spec.DeclaringType = part.Definition;
			if (!tp.ResolveConstraints(part))
			{
				return false;
			}
			if (constraints != null)
			{
				return spec.HasSameConstraintsDefinition(tp.Type);
			}
			spec.SpecialConstraint = tp.spec.SpecialConstraint;
			spec.InterfacesDefined = tp.spec.InterfacesDefined;
			spec.TypeArguments = tp.spec.TypeArguments;
			spec.BaseType = tp.spec.BaseType;
			return true;
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			builder.SetCustomAttribute((ConstructorInfo)ctor.GetMetaInfo(), cdata);
		}

		public void CheckGenericConstraints(bool obsoleteCheck)
		{
			if (constraints != null)
			{
				constraints.CheckGenericConstraints(this, obsoleteCheck);
			}
		}

		public TypeParameter CreateHoistedCopy(TypeSpec declaringSpec)
		{
			return new TypeParameter(spec, declaringSpec, base.MemberName, null);
		}

		public override bool Define()
		{
			return true;
		}

		public void Create(TypeSpec declaringType, TypeContainer parent)
		{
			if (builder != null)
			{
				throw new InternalErrorException();
			}
			Parent = parent;
			spec.DeclaringType = declaringType;
		}

		public void Define(GenericTypeParameterBuilder type)
		{
			builder = type;
			spec.SetMetaInfo(type);
		}

		public void Define(TypeParameter tp)
		{
			builder = tp.builder;
		}

		public void EmitConstraints(GenericTypeParameterBuilder builder)
		{
			GenericParameterAttributes genericParameterAttributes = GenericParameterAttributes.None;
			if (spec.Variance == Variance.Contravariant)
			{
				genericParameterAttributes |= GenericParameterAttributes.Contravariant;
			}
			else if (spec.Variance == Variance.Covariant)
			{
				genericParameterAttributes |= GenericParameterAttributes.Covariant;
			}
			if (spec.HasSpecialClass)
			{
				genericParameterAttributes |= GenericParameterAttributes.ReferenceTypeConstraint;
			}
			else if (spec.HasSpecialStruct)
			{
				genericParameterAttributes |= GenericParameterAttributes.NotNullableValueTypeConstraint | GenericParameterAttributes.DefaultConstructorConstraint;
			}
			if (spec.HasSpecialConstructor)
			{
				genericParameterAttributes |= GenericParameterAttributes.DefaultConstructorConstraint;
			}
			if (spec.BaseType.BuiltinType != BuiltinTypeSpec.Type.Object)
			{
				builder.SetBaseTypeConstraint(spec.BaseType.GetMetaInfo());
			}
			if (spec.InterfacesDefined != null)
			{
				builder.SetInterfaceConstraints(spec.InterfacesDefined.Select((TypeSpec l) => l.GetMetaInfo()).ToArray());
			}
			if (spec.TypeArguments != null)
			{
				List<Type> list = new List<Type>(spec.TypeArguments.Length);
				TypeSpec[] typeArguments = spec.TypeArguments;
				foreach (TypeSpec typeSpec in typeArguments)
				{
					if (typeSpec.BuiltinType != BuiltinTypeSpec.Type.Object && typeSpec.BuiltinType != BuiltinTypeSpec.Type.ValueType)
					{
						list.Add(typeSpec.GetMetaInfo());
					}
				}
				builder.SetInterfaceConstraints(list.ToArray());
			}
			builder.SetGenericParameterAttributes(genericParameterAttributes);
		}

		public override void Emit()
		{
			EmitConstraints(builder);
			if (base.OptAttributes != null)
			{
				base.OptAttributes.Emit();
			}
			base.Emit();
		}

		public void ErrorInvalidVariance(IMemberContext mc, Variance expected)
		{
			base.Report.SymbolRelatedToPreviousError(mc.CurrentMemberDefinition);
			string text = ((Variance == Variance.Contravariant) ? "contravariant" : "covariant");
			string text2;
			switch (expected)
			{
			case Variance.Contravariant:
				text2 = "contravariantly";
				break;
			case Variance.Covariant:
				text2 = "covariantly";
				break;
			default:
				text2 = "invariantly";
				break;
			}
			Delegate obj = mc as Delegate;
			string text3 = ((obj != null) ? obj.Parameters.GetSignatureForError() : "");
			base.Report.Error(1961, base.Location, "The {2} type parameter `{0}' must be {3} valid on `{1}{4}'", GetSignatureForError(), mc.GetSignatureForError(), text, text2, text3);
		}

		public TypeSpec GetAttributeCoClass()
		{
			return null;
		}

		public string GetAttributeDefaultMember()
		{
			throw new NotSupportedException();
		}

		public AttributeUsageAttribute GetAttributeUsage(PredefinedAttribute pa)
		{
			throw new NotSupportedException();
		}

		public override string GetSignatureForDocumentation()
		{
			throw new NotImplementedException();
		}

		public override string GetSignatureForError()
		{
			return base.MemberName.Name;
		}

		bool ITypeDefinition.IsInternalAsPublic(IAssemblyDefinition assembly)
		{
			return spec.MemberDefinition.DeclaringAssembly == assembly;
		}

		public void LoadMembers(TypeSpec declaringType, bool onlyTypes, ref MemberCache cache)
		{
			throw new NotSupportedException("Not supported for compiled definition");
		}

		public bool ResolveConstraints(IMemberContext context)
		{
			if (constraints != null)
			{
				return constraints.Resolve(context, this);
			}
			if (spec.BaseType == null)
			{
				spec.BaseType = context.Module.Compiler.BuiltinTypes.Object;
			}
			return true;
		}

		public override bool IsClsComplianceRequired()
		{
			return false;
		}

		public new void VerifyClsCompliance()
		{
			if (constraints != null)
			{
				constraints.VerifyClsCompliance(base.Report);
			}
		}

		public void WarningParentNameConflict(TypeParameter conflict)
		{
			conflict.Report.SymbolRelatedToPreviousError(conflict.Location, null);
			conflict.Report.Warning(693, 3, base.Location, "Type parameter `{0}' has the same name as the type parameter from outer type `{1}'", GetSignatureForError(), conflict.CurrentType.GetSignatureForError());
		}
	}
}
