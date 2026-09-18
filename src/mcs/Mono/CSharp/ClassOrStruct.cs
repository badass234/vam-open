using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace Mono.CSharp
{
	public abstract class ClassOrStruct : TypeDefinition
	{
		public const TypeAttributes StaticClassAttribute = TypeAttributes.Abstract | TypeAttributes.Sealed;

		private Dictionary<SecurityAction, PermissionSet> declarative_security;

		protected Constructor generated_primary_constructor;

		public ToplevelBlock PrimaryConstructorBlock { get; set; }

		protected override TypeAttributes TypeAttr
		{
			get
			{
				TypeAttributes typeAttributes = base.TypeAttr;
				if (!has_static_constructor)
				{
					typeAttributes |= TypeAttributes.BeforeFieldInit;
				}
				if (Kind == MemberKind.Class)
				{
					typeAttributes |= TypeAttributes.NotPublic;
					if (base.IsStatic)
					{
						typeAttributes |= TypeAttributes.Abstract | TypeAttributes.Sealed;
					}
				}
				else
				{
					typeAttributes |= TypeAttributes.SequentialLayout;
				}
				return typeAttributes;
			}
		}

		protected ClassOrStruct(TypeContainer parent, MemberName name, Attributes attrs, MemberKind kind)
			: base(parent, name, attrs, kind)
		{
		}

		public override void AddNameToContainer(MemberCore symbol, string name)
		{
			if (!(symbol is Constructor) && symbol.MemberName.Name == base.MemberName.Name)
			{
				if (symbol is TypeParameter)
				{
					base.Report.Error(694, symbol.Location, "Type parameter `{0}' has same name as containing type, or method", symbol.GetSignatureForError());
					return;
				}
				InterfaceMemberBase interfaceMemberBase = symbol as InterfaceMemberBase;
				if (interfaceMemberBase == null || !interfaceMemberBase.IsExplicitImpl)
				{
					base.Report.SymbolRelatedToPreviousError(this);
					base.Report.Error(542, symbol.Location, "`{0}': member names cannot be the same as their enclosing type", symbol.GetSignatureForError());
					return;
				}
			}
			base.AddNameToContainer(symbol, name);
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.IsValidSecurityAttribute())
			{
				a.ExtractSecurityPermissionSet(ctor, ref declarative_security);
				return;
			}
			if (a.Type == pa.StructLayout)
			{
				base.PartialContainer.HasStructLayout = true;
				if (a.IsExplicitLayoutKind())
				{
					base.PartialContainer.HasExplicitLayout = true;
				}
			}
			if (a.Type == pa.Dynamic)
			{
				a.Error_MisusedDynamicAttribute();
			}
			else
			{
				base.ApplyAttributeBuilder(a, ctor, cdata, pa);
			}
		}

		protected virtual Constructor DefineDefaultConstructor(bool is_static)
		{
			ParametersCompiled parametersCompiled = null;
			Modifiers mod;
			if (is_static)
			{
				mod = Modifiers.PRIVATE | Modifiers.STATIC;
				parametersCompiled = ParametersCompiled.EmptyReadOnlyParameters;
			}
			else
			{
				mod = (((base.ModFlags & Modifiers.ABSTRACT) != 0) ? Modifiers.PROTECTED : Modifiers.PUBLIC);
				parametersCompiled = base.PrimaryConstructorParameters ?? ParametersCompiled.EmptyReadOnlyParameters;
			}
			Constructor constructor = new Constructor(this, base.MemberName.Name, mod, null, parametersCompiled, base.Location);
			if (Kind == MemberKind.Class)
			{
				constructor.Initializer = new GeneratedBaseInitializer(base.Location, base.PrimaryConstructorBaseArguments);
			}
			if (base.PrimaryConstructorParameters != null && !is_static)
			{
				constructor.IsPrimaryConstructor = true;
				constructor.caching_flags |= Flags.MethodOverloadsExist;
			}
			AddConstructor(constructor, true);
			if (PrimaryConstructorBlock == null)
			{
				constructor.Block = new ToplevelBlock(Compiler, parametersCompiled, base.Location)
				{
					IsCompilerGenerated = true
				};
			}
			else
			{
				constructor.Block = PrimaryConstructorBlock;
			}
			return constructor;
		}

		protected override bool DoDefineMembers()
		{
			CheckProtectedModifier();
			if (base.PrimaryConstructorParameters != null)
			{
				IParameterData[] fixedParameters = base.PrimaryConstructorParameters.FixedParameters;
				for (int i = 0; i < fixedParameters.Length; i++)
				{
					Parameter parameter = (Parameter)fixedParameters[i];
					if (parameter.Name == base.MemberName.Name)
					{
						base.Report.Error(8039, parameter.Location, "Primary constructor of type `{0}' has parameter of same name as containing type", GetSignatureForError());
					}
					if (CurrentTypeParameters == null)
					{
						continue;
					}
					for (int j = 0; j < CurrentTypeParameters.Count; j++)
					{
						TypeParameter typeParameter = CurrentTypeParameters[j];
						if (parameter.Name == typeParameter.Name)
						{
							base.Report.Error(8038, parameter.Location, "Primary constructor of type `{0}' has parameter of same name as type parameter `{1}'", GetSignatureForError(), parameter.GetSignatureForError());
						}
					}
				}
			}
			base.DoDefineMembers();
			return true;
		}

		public override void Emit()
		{
			if (!has_static_constructor && base.HasStaticFieldInitializer)
			{
				DefineDefaultConstructor(true).Define();
			}
			base.Emit();
			if (declarative_security == null)
			{
				return;
			}
			foreach (KeyValuePair<SecurityAction, PermissionSet> item in declarative_security)
			{
				TypeBuilder.AddDeclarativeSecurity(item.Key, item.Value);
			}
		}
	}
}
