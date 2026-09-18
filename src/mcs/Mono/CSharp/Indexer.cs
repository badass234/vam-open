using System.Reflection;
using System.Text;

namespace Mono.CSharp
{
	public class Indexer : PropertyBase, IParametersMember, IInterfaceMemberSpec
	{
		public class GetIndexerMethod : GetMethod, IParametersMember, IInterfaceMemberSpec
		{
			private ParametersCompiled parameters;

			public override ParametersCompiled ParameterInfo => parameters;

			AParametersCollection IParametersMember.Parameters => parameters;

			TypeSpec IInterfaceMemberSpec.MemberType => ReturnType;

			public GetIndexerMethod(PropertyBase property, Modifiers modifiers, ParametersCompiled parameters, Attributes attrs, Location loc)
				: base(property, modifiers, attrs, loc)
			{
				this.parameters = parameters;
			}

			public override void Define(TypeContainer parent)
			{
				base.Report.DisableReporting();
				try
				{
					parameters.Resolve(this);
				}
				finally
				{
					base.Report.EnableReporting();
				}
				base.Define(parent);
			}
		}

		public class SetIndexerMethod : SetMethod, IParametersMember, IInterfaceMemberSpec
		{
			AParametersCollection IParametersMember.Parameters => parameters;

			TypeSpec IInterfaceMemberSpec.MemberType => ReturnType;

			public SetIndexerMethod(PropertyBase property, Modifiers modifiers, ParametersCompiled parameters, Attributes attrs, Location loc)
				: base(property, modifiers, parameters, attrs, loc)
			{
			}
		}

		private const Modifiers AllowedModifiers = Modifiers.AccessibilityMask | Modifiers.AllowedExplicitImplFlags | Modifiers.NEW | Modifiers.ABSTRACT | Modifiers.SEALED | Modifiers.VIRTUAL | Modifiers.OVERRIDE;

		private const Modifiers AllowedInterfaceModifiers = Modifiers.NEW;

		private readonly ParametersCompiled parameters;

		AParametersCollection IParametersMember.Parameters => parameters;

		public ParametersCompiled ParameterInfo => parameters;

		public Indexer(TypeDefinition parent, FullNamedExpression type, MemberName name, Modifiers mod, ParametersCompiled parameters, Attributes attrs)
			: base(parent, type, mod, (parent.PartialContainer.Kind == MemberKind.Interface) ? Modifiers.NEW : (Modifiers.AccessibilityMask | Modifiers.AllowedExplicitImplFlags | Modifiers.NEW | Modifiers.ABSTRACT | Modifiers.SEALED | Modifiers.VIRTUAL | Modifiers.OVERRIDE), name, attrs)
		{
			this.parameters = parameters;
		}

		public override void Accept(StructuralVisitor visitor)
		{
			visitor.Visit(this);
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (!(a.Type == pa.IndexerName))
			{
				base.ApplyAttributeBuilder(a, ctor, cdata, pa);
			}
		}

		protected override bool CheckForDuplications()
		{
			return Parent.MemberCache.CheckExistingMembersOverloads(this, parameters);
		}

		public override bool Define()
		{
			if (!base.Define())
			{
				return false;
			}
			if (!DefineParameters(parameters))
			{
				return false;
			}
			if (base.OptAttributes != null)
			{
				Attribute attribute = base.OptAttributes.Search(Module.PredefinedAttributes.IndexerName);
				if (attribute != null)
				{
					(attribute.Type.MemberDefinition as TypeContainer)?.Define();
					if (IsExplicitImpl)
					{
						base.Report.Error(415, attribute.Location, "The `{0}' attribute is valid only on an indexer that is not an explicit interface member declaration", attribute.Type.GetSignatureForError());
					}
					else if ((base.ModFlags & Modifiers.OVERRIDE) != 0)
					{
						base.Report.Error(609, attribute.Location, "Cannot set the `IndexerName' attribute on an indexer marked override");
					}
					else
					{
						string indexerAttributeValue = attribute.GetIndexerAttributeValue();
						if (!string.IsNullOrEmpty(indexerAttributeValue))
						{
							SetMemberName(new MemberName(base.MemberName.Left, indexerAttributeValue, base.Location));
						}
					}
				}
			}
			if (InterfaceType != null)
			{
				string attributeDefaultMember = InterfaceType.MemberDefinition.GetAttributeDefaultMember();
				if (attributeDefaultMember != base.ShortName)
				{
					SetMemberName(new MemberName(base.MemberName.Left, attributeDefaultMember, new TypeExpression(InterfaceType, base.Location), base.Location));
				}
			}
			Parent.AddNameToContainer(this, base.MemberName.Basename);
			flags |= MethodAttributes.HideBySig | MethodAttributes.SpecialName;
			if (!DefineAccessors())
			{
				return false;
			}
			if (!CheckBase())
			{
				return false;
			}
			DefineBuilders(MemberKind.Indexer, parameters);
			return true;
		}

		public override bool EnableOverloadChecks(MemberCore overload)
		{
			if (overload is Indexer)
			{
				caching_flags |= Flags.MethodOverloadsExist;
				return true;
			}
			return base.EnableOverloadChecks(overload);
		}

		public override void Emit()
		{
			parameters.CheckConstraints(this);
			base.Emit();
		}

		public override string GetSignatureForError()
		{
			StringBuilder stringBuilder = new StringBuilder(Parent.GetSignatureForError());
			if (base.MemberName.ExplicitInterface != null)
			{
				stringBuilder.Append(".");
				stringBuilder.Append(base.MemberName.ExplicitInterface.GetSignatureForError());
			}
			stringBuilder.Append(".this");
			stringBuilder.Append(parameters.GetSignatureForError("[", "]", parameters.Count));
			return stringBuilder.ToString();
		}

		public override string GetSignatureForDocumentation()
		{
			return base.GetSignatureForDocumentation() + parameters.GetSignatureForDocumentation();
		}

		public override void PrepareEmit()
		{
			parameters.ResolveDefaultValues(this);
			base.PrepareEmit();
		}

		protected override bool VerifyClsCompliance()
		{
			if (!base.VerifyClsCompliance())
			{
				return false;
			}
			parameters.VerifyClsCompliance(this);
			return true;
		}
	}
}
