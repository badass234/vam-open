using System;
using System.Collections.Generic;
using System.Text;
using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp
{
	public abstract class TypeContainer : MemberCore
	{
		public readonly MemberKind Kind;

		protected List<TypeContainer> containers;

		private TypeDefinition main_container;

		protected Dictionary<string, MemberCore> defined_names;

		protected bool is_defined;

		public int CounterAnonymousMethods { get; set; }

		public int CounterAnonymousContainers { get; set; }

		public int CounterSwitchTypes { get; set; }

		public override TypeSpec CurrentType => null;

		public Dictionary<string, MemberCore> DefinedNames => defined_names;

		public TypeDefinition PartialContainer
		{
			get
			{
				return main_container;
			}
			protected set
			{
				main_container = value;
			}
		}

		public IList<TypeContainer> Containers => containers;

		public Attributes UnattachedAttributes { get; set; }

		protected TypeContainer(TypeContainer parent, MemberName name, Attributes attrs, MemberKind kind)
			: base(parent, name, attrs)
		{
			Kind = kind;
			defined_names = new Dictionary<string, MemberCore>();
		}

		public void AddCompilerGeneratedClass(CompilerGeneratedContainer c)
		{
			AddTypeContainerMember(c);
		}

		public virtual void AddPartial(TypeDefinition next_part)
		{
			MemberCore value;
			(PartialContainer ?? this).defined_names.TryGetValue(next_part.MemberName.Basename, out value);
			AddPartial(next_part, value as TypeDefinition);
		}

		protected void AddPartial(TypeDefinition next_part, TypeDefinition existing)
		{
			next_part.ModFlags |= Modifiers.PARTIAL;
			if (existing == null)
			{
				AddTypeContainer(next_part);
				return;
			}
			if ((existing.ModFlags & Modifiers.PARTIAL) == 0)
			{
				if (existing.Kind != next_part.Kind)
				{
					AddTypeContainer(next_part);
					return;
				}
				base.Report.SymbolRelatedToPreviousError(next_part);
				Error_MissingPartialModifier(existing);
				return;
			}
			if (existing.Kind != next_part.Kind)
			{
				base.Report.SymbolRelatedToPreviousError(existing);
				base.Report.Error(261, next_part.Location, "Partial declarations of `{0}' must be all classes, all structs or all interfaces", next_part.GetSignatureForError());
			}
			if ((existing.ModFlags & Modifiers.AccessibilityMask) != (next_part.ModFlags & Modifiers.AccessibilityMask) && (existing.ModFlags & Modifiers.DEFAULT_ACCESS_MODIFIER) == 0 && (next_part.ModFlags & Modifiers.DEFAULT_ACCESS_MODIFIER) == 0)
			{
				base.Report.SymbolRelatedToPreviousError(existing);
				base.Report.Error(262, next_part.Location, "Partial declarations of `{0}' have conflicting accessibility modifiers", next_part.GetSignatureForError());
			}
			TypeParameters currentTypeParameters = existing.CurrentTypeParameters;
			if (currentTypeParameters != null)
			{
				for (int i = 0; i < currentTypeParameters.Count; i++)
				{
					TypeParameter typeParameter = next_part.MemberName.TypeParameters[i];
					if (currentTypeParameters[i].MemberName.Name != typeParameter.MemberName.Name)
					{
						base.Report.SymbolRelatedToPreviousError(existing.Location, "");
						base.Report.Error(264, next_part.Location, "Partial declarations of `{0}' must have the same type parameter names in the same order", next_part.GetSignatureForError());
						break;
					}
					if (currentTypeParameters[i].Variance != typeParameter.Variance)
					{
						base.Report.SymbolRelatedToPreviousError(existing.Location, "");
						base.Report.Error(1067, next_part.Location, "Partial declarations of `{0}' must have the same type parameter variance modifiers", next_part.GetSignatureForError());
						break;
					}
				}
			}
			if ((next_part.ModFlags & Modifiers.DEFAULT_ACCESS_MODIFIER) != 0)
			{
				existing.ModFlags |= next_part.ModFlags & ~(Modifiers.AccessibilityMask | Modifiers.DEFAULT_ACCESS_MODIFIER);
			}
			else if ((existing.ModFlags & Modifiers.DEFAULT_ACCESS_MODIFIER) != 0)
			{
				existing.ModFlags &= ~(Modifiers.AccessibilityMask | Modifiers.DEFAULT_ACCESS_MODIFIER);
				existing.ModFlags |= next_part.ModFlags;
			}
			else
			{
				existing.ModFlags |= next_part.ModFlags;
			}
			existing.Definition.Modifiers = existing.ModFlags;
			if (next_part.attributes != null)
			{
				if (existing.attributes == null)
				{
					existing.attributes = next_part.attributes;
				}
				else
				{
					existing.attributes.AddAttributes(next_part.attributes.Attrs);
				}
			}
			next_part.PartialContainer = existing;
			existing.AddPartialPart(next_part);
			AddTypeContainerMember(next_part);
		}

		public virtual void AddTypeContainer(TypeContainer tc)
		{
			AddTypeContainerMember(tc);
			TypeParameters typeParameters = tc.MemberName.TypeParameters;
			if (typeParameters == null || tc.PartialContainer == null)
			{
				return;
			}
			TypeDefinition typeDefinition = (TypeDefinition)tc;
			for (int i = 0; i < typeParameters.Count; i++)
			{
				TypeParameter typeParameter = typeParameters[i];
				if (typeParameter.MemberName != null)
				{
					typeDefinition.AddNameToContainer(typeParameter, typeParameter.Name);
				}
			}
		}

		protected virtual void AddTypeContainerMember(TypeContainer tc)
		{
			containers.Add(tc);
		}

		public virtual void CloseContainer()
		{
			if (containers == null)
			{
				return;
			}
			foreach (TypeContainer container in containers)
			{
				container.CloseContainer();
			}
		}

		public virtual void CreateMetadataName(StringBuilder sb)
		{
			if (Parent != null && Parent.MemberName != null)
			{
				Parent.CreateMetadataName(sb);
			}
			base.MemberName.CreateMetadataName(sb);
		}

		public virtual bool CreateContainer()
		{
			if (containers != null)
			{
				foreach (TypeContainer container in containers)
				{
					container.CreateContainer();
				}
			}
			return true;
		}

		public override bool Define()
		{
			if (containers != null)
			{
				foreach (TypeContainer container in containers)
				{
					container.Define();
				}
			}
			if (Module.Evaluator == null)
			{
				defined_names = null;
			}
			else
			{
				defined_names.Clear();
			}
			return true;
		}

		public virtual void PrepareEmit()
		{
			if (containers == null)
			{
				return;
			}
			foreach (TypeContainer container in containers)
			{
				try
				{
					container.PrepareEmit();
				}
				catch (Exception e)
				{
					if (base.MemberName == MemberName.Null)
					{
						throw;
					}
					throw new InternalErrorException(container, e);
				}
			}
		}

		public virtual bool DefineContainer()
		{
			if (is_defined)
			{
				return true;
			}
			is_defined = true;
			DoDefineContainer();
			if (containers != null)
			{
				foreach (TypeContainer container in containers)
				{
					try
					{
						container.DefineContainer();
					}
					catch (Exception e)
					{
						if (base.MemberName == MemberName.Null)
						{
							throw;
						}
						throw new InternalErrorException(container, e);
					}
				}
			}
			return true;
		}

		public virtual void ExpandBaseInterfaces()
		{
			if (containers == null)
			{
				return;
			}
			foreach (TypeContainer container in containers)
			{
				container.ExpandBaseInterfaces();
			}
		}

		protected virtual void DefineNamespace()
		{
			if (containers == null)
			{
				return;
			}
			foreach (TypeContainer container in containers)
			{
				try
				{
					container.DefineNamespace();
				}
				catch (Exception e)
				{
					throw new InternalErrorException(container, e);
				}
			}
		}

		protected virtual void DoDefineContainer()
		{
		}

		public virtual void EmitContainer()
		{
			if (containers != null)
			{
				for (int i = 0; i < containers.Count; i++)
				{
					containers[i].EmitContainer();
				}
			}
		}

		protected void Error_MissingPartialModifier(MemberCore type)
		{
			base.Report.Error(260, type.Location, "Missing partial modifier on declaration of type `{0}'. Another partial declaration of this type exists", type.GetSignatureForError());
		}

		public override string GetSignatureForDocumentation()
		{
			if (Parent != null && Parent.MemberName != null)
			{
				return Parent.GetSignatureForDocumentation() + "." + base.MemberName.GetSignatureForDocumentation();
			}
			return base.MemberName.GetSignatureForDocumentation();
		}

		public override string GetSignatureForError()
		{
			if (Parent != null && Parent.MemberName != null)
			{
				return Parent.GetSignatureForError() + "." + base.MemberName.GetSignatureForError();
			}
			return base.MemberName.GetSignatureForError();
		}

		public virtual string GetSignatureForMetadata()
		{
			StringBuilder stringBuilder = new StringBuilder();
			CreateMetadataName(stringBuilder);
			return stringBuilder.ToString();
		}

		public virtual void RemoveContainer(TypeContainer cont)
		{
			if (containers != null)
			{
				containers.Remove(cont);
			}
			((Parent == Module) ? Module : this).defined_names.Remove(cont.MemberName.Basename);
		}

		public virtual void VerifyMembers()
		{
			if (containers == null)
			{
				return;
			}
			foreach (TypeContainer container in containers)
			{
				container.VerifyMembers();
			}
		}

		public override void WriteDebugSymbol(MonoSymbolFile file)
		{
			if (containers == null)
			{
				return;
			}
			foreach (TypeContainer container in containers)
			{
				container.WriteDebugSymbol(file);
			}
		}
	}
}
