using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Permissions;
using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp
{
	public class Constructor : MethodCore, IMethodData, IMemberContext, IModuleContext, IMethodDefinition, IMemberDefinition
	{
		public ConstructorBuilder ConstructorBuilder;

		public ConstructorInitializer Initializer;

		private Dictionary<SecurityAction, PermissionSet> declarative_security;

		private bool has_compliant_args;

		private SourceMethodBuilder debug_builder;

		public const Modifiers AllowedModifiers = Modifiers.AccessibilityMask | Modifiers.AllowedExplicitImplFlags | Modifiers.STATIC;

		private static readonly string[] attribute_targets = new string[1] { "method" };

		public static readonly string ConstructorName = ".ctor";

		public static readonly string TypeConstructorName = ".cctor";

		public bool HasCompliantArgs => has_compliant_args;

		public override AttributeTargets AttributeTargets => AttributeTargets.Constructor;

		bool IMethodData.IsAccessor => false;

		public bool IsPrimaryConstructor { get; set; }

		MethodBase IMethodDefinition.Metadata => ConstructorBuilder;

		public override string[] ValidAttributeTargets => attribute_targets;

		public MemberName MethodName => base.MemberName;

		public TypeSpec ReturnType => base.MemberType;

		public Constructor(TypeDefinition parent, string name, Modifiers mod, Attributes attrs, ParametersCompiled args, Location loc)
			: base(parent, null, mod, Modifiers.AccessibilityMask | Modifiers.AllowedExplicitImplFlags | Modifiers.STATIC, new MemberName(name, loc), attrs, args)
		{
		}

		public bool IsDefault()
		{
			if ((base.ModFlags & Modifiers.STATIC) != 0)
			{
				return parameters.IsEmpty;
			}
			if (parameters.IsEmpty && Initializer is ConstructorBaseInitializer)
			{
				return Initializer.Arguments == null;
			}
			return false;
		}

		public override void Accept(StructuralVisitor visitor)
		{
			visitor.Visit(this);
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.IsValidSecurityAttribute())
			{
				a.ExtractSecurityPermissionSet(ctor, ref declarative_security);
				return;
			}
			if (a.Type == pa.MethodImpl)
			{
				is_external_implementation = a.IsInternalCall();
			}
			ConstructorBuilder.SetCustomAttribute((ConstructorInfo)ctor.GetMetaInfo(), cdata);
		}

		protected override bool CheckBase()
		{
			if ((base.ModFlags & Modifiers.STATIC) != 0)
			{
				if ((caching_flags & Flags.MethodOverloadsExist) != 0)
				{
					Parent.MemberCache.CheckExistingMembersOverloads(this, parameters);
				}
				return true;
			}
			if (!DefineParameters(parameters))
			{
				return false;
			}
			if ((caching_flags & Flags.MethodOverloadsExist) != 0)
			{
				Parent.MemberCache.CheckExistingMembersOverloads(this, parameters);
			}
			CheckProtectedModifier();
			return true;
		}

		public override bool Define()
		{
			if (ConstructorBuilder != null)
			{
				return true;
			}
			if (!CheckAbstractAndExtern(block != null))
			{
				return false;
			}
			if (!CheckBase())
			{
				return false;
			}
			if (Parent.PrimaryConstructorParameters != null && !IsPrimaryConstructor && !base.IsStatic)
			{
				if (Parent.Kind == MemberKind.Struct && Initializer is ConstructorThisInitializer && Initializer.Arguments == null)
				{
					base.Report.Error(8043, base.Location, "`{0}': Structs with primary constructor cannot specify default constructor initializer", GetSignatureForError());
				}
				else if (Initializer == null || Initializer is ConstructorBaseInitializer)
				{
					base.Report.Error(8037, base.Location, "`{0}': Instance constructor of type with primary constructor must specify `this' constructor initializer", GetSignatureForError());
				}
			}
			if ((base.ModFlags & Modifiers.EXTERN) != 0 && Initializer != null)
			{
				base.Report.Error(8091, base.Location, "`{0}': Contructors cannot be extern and have a constructor initializer", GetSignatureForError());
			}
			MethodAttributes methodAttributes = ModifiersExtensions.MethodAttr(base.ModFlags) | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName;
			ConstructorBuilder = Parent.TypeBuilder.DefineConstructor(methodAttributes, base.CallingConventions, parameters.GetMetaInfo());
			spec = new MethodSpec(MemberKind.Constructor, Parent.Definition, this, Compiler.BuiltinTypes.Void, parameters, base.ModFlags);
			Parent.MemberCache.AddMember(spec);
			if (block != null)
			{
				if (block.IsIterator)
				{
					member_type = Compiler.BuiltinTypes.Void;
					Iterator.CreateIterator(this, Parent.PartialContainer, base.ModFlags);
				}
				if (Compiler.Settings.WriteMetadataOnly)
				{
					block = null;
				}
			}
			return true;
		}

		public override void Emit()
		{
			if (Parent.PartialContainer.IsComImport)
			{
				if (!IsDefault())
				{
					base.Report.Error(669, base.Location, "`{0}': A class with the ComImport attribute cannot have a user-defined constructor", Parent.GetSignatureForError());
				}
				ConstructorBuilder.SetImplementationFlags(MethodImplAttributes.InternalCall);
				block = null;
			}
			if ((base.ModFlags & Modifiers.DEBUGGER_HIDDEN) != 0)
			{
				Module.PredefinedAttributes.DebuggerHidden.EmitAttribute(ConstructorBuilder);
			}
			if (base.OptAttributes != null)
			{
				base.OptAttributes.Emit();
			}
			base.Emit();
			parameters.ApplyAttributes(this, ConstructorBuilder);
			BlockContext blockContext = new BlockContext(this, block, Compiler.BuiltinTypes.Void);
			blockContext.Set(ResolveContext.Options.ConstructorScope);
			if (block != null)
			{
				if (!base.IsStatic && Initializer == null && Parent.PartialContainer.Kind == MemberKind.Struct)
				{
					block.AddThisVariable(blockContext);
				}
				if (!(Initializer is ConstructorThisInitializer))
				{
					Parent.PartialContainer.ResolveFieldInitializers(blockContext);
				}
				if (!base.IsStatic)
				{
					if (Initializer == null && Parent.PartialContainer.Kind == MemberKind.Class)
					{
						Initializer = new GeneratedBaseInitializer(base.Location, null);
					}
					if (Initializer != null)
					{
						block.AddScopeStatement(new StatementExpression(Initializer));
					}
				}
				if (block.Resolve(blockContext, this))
				{
					debug_builder = Parent.CreateMethodSymbolEntry();
					EmitContext emitContext = new EmitContext(this, ConstructorBuilder.GetILGenerator(), blockContext.ReturnType, debug_builder);
					emitContext.With(BuilderContext.Options.ConstructorScope, true);
					block.Emit(emitContext);
				}
			}
			if (declarative_security != null)
			{
				foreach (KeyValuePair<SecurityAction, PermissionSet> item in declarative_security)
				{
					ConstructorBuilder.AddDeclarativeSecurity(item.Key, item.Value);
				}
			}
			block = null;
		}

		protected override MemberSpec FindBaseMember(out MemberSpec bestCandidate, ref bool overrides)
		{
			bestCandidate = null;
			return null;
		}

		public override string GetCallerMemberName()
		{
			if (!base.IsStatic)
			{
				return ConstructorName;
			}
			return TypeConstructorName;
		}

		public override string GetSignatureForDocumentation()
		{
			return Parent.GetSignatureForDocumentation() + ".#ctor" + parameters.GetSignatureForDocumentation();
		}

		public override string GetSignatureForError()
		{
			return base.GetSignatureForError() + parameters.GetSignatureForError();
		}

		protected override bool VerifyClsCompliance()
		{
			if (!base.VerifyClsCompliance() || !IsExposedFromAssembly())
			{
				return false;
			}
			if (!parameters.IsEmpty && Parent.Definition.IsAttribute)
			{
				TypeSpec[] types = parameters.Types;
				for (int i = 0; i < types.Length; i++)
				{
					if (types[i].IsArray)
					{
						return true;
					}
				}
			}
			has_compliant_args = true;
			return true;
		}

		public override void WriteDebugSymbol(MonoSymbolFile file)
		{
			if (debug_builder != null)
			{
				int token = ConstructorBuilder.GetToken().Token;
				debug_builder.DefineMethod(file, token);
			}
		}

		EmitContext IMethodData.CreateEmitContext(ILGenerator ig, SourceMethodBuilder sourceMethod)
		{
			throw new NotImplementedException();
		}
	}
}
