using System;

namespace Mono.CSharp
{
	public class Destructor : MethodOrOperator
	{
		private const Modifiers AllowedModifiers = Modifiers.AllowedExplicitImplFlags;

		private static readonly string[] attribute_targets = new string[1] { "method" };

		public static readonly string MetadataName = "Finalize";

		public override string[] ValidAttributeTargets => attribute_targets;

		public Destructor(TypeDefinition parent, Modifiers mod, ParametersCompiled parameters, Attributes attrs, Location l)
			: base(parent, null, mod, Modifiers.AllowedExplicitImplFlags, new MemberName(MetadataName, l), attrs, parameters)
		{
			base.ModFlags &= ~Modifiers.PRIVATE;
			base.ModFlags |= Modifiers.PROTECTED | Modifiers.OVERRIDE;
		}

		public override void Accept(StructuralVisitor visitor)
		{
			visitor.Visit(this);
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Type == pa.Conditional)
			{
				Error_ConditionalAttributeIsNotValid();
			}
			else
			{
				base.ApplyAttributeBuilder(a, ctor, cdata, pa);
			}
		}

		protected override bool CheckBase()
		{
			if ((caching_flags & Flags.MethodOverloadsExist) != 0)
			{
				CheckForDuplications();
			}
			return true;
		}

		public override bool Define()
		{
			base.Define();
			if (Compiler.Settings.WriteMetadataOnly)
			{
				block = null;
			}
			return true;
		}

		public override void Emit()
		{
			TypeSpec baseType = Parent.PartialContainer.BaseType;
			if (baseType != null && base.Block != null)
			{
				MethodSpec obj = MemberCache.FindMember(baseType, new MemberFilter(MetadataName, 0, MemberKind.Destructor, null, null), BindingRestriction.InstanceOnly) as MethodSpec;
				if (obj == null)
				{
					throw new NotImplementedException();
				}
				MethodGroupExpr methodGroupExpr = MethodGroupExpr.CreatePredefined(obj, baseType, base.Location);
				methodGroupExpr.InstanceExpression = new BaseThis(baseType, base.Location);
				ExplicitBlock explicitBlock = new ExplicitBlock(block, block.StartLocation, block.EndLocation)
				{
					IsCompilerGenerated = true
				};
				ExplicitBlock explicitBlock2 = new ExplicitBlock(block, base.Location, base.Location)
				{
					IsCompilerGenerated = true
				};
				explicitBlock2.AddStatement(new StatementExpression(new Invocation(methodGroupExpr, new Arguments(0)), Location.Null));
				TryFinally tf = new TryFinally(explicitBlock, explicitBlock2, base.Location);
				block.WrapIntoDestructor(tf, explicitBlock);
			}
			base.Emit();
		}

		public override string GetSignatureForError()
		{
			return Parent.GetSignatureForError() + ".~" + Parent.MemberName.Name + "()";
		}

		protected override bool ResolveMemberType()
		{
			member_type = Compiler.BuiltinTypes.Void;
			return true;
		}
	}
}
