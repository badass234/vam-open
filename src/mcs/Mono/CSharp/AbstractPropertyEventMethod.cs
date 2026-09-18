using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Permissions;
using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp
{
	public abstract class AbstractPropertyEventMethod : MemberCore, IMethodData, IMemberContext, IModuleContext, IMethodDefinition, IMemberDefinition
	{
		protected MethodData method_data;

		protected ToplevelBlock block;

		protected Dictionary<SecurityAction, PermissionSet> declarative_security;

		protected readonly string prefix;

		private ReturnParameter return_attributes;

		public ToplevelBlock Block
		{
			get
			{
				return block;
			}
			set
			{
				block = value;
			}
		}

		public CallingConventions CallingConventions => CallingConventions.Standard;

		public bool IsAccessor => true;

		public MemberName MethodName => base.MemberName;

		public TypeSpec[] ParameterTypes => ParameterInfo.Types;

		MethodBase IMethodDefinition.Metadata => method_data.MethodBuilder;

		public abstract ParametersCompiled ParameterInfo { get; }

		public abstract TypeSpec ReturnType { get; }

		public MethodSpec Spec { get; protected set; }

		public override string DocCommentHeader
		{
			get
			{
				throw new InvalidOperationException(string.Concat("Unexpected attempt to get doc comment from ", GetType(), "."));
			}
		}

		protected AbstractPropertyEventMethod(InterfaceMemberBase member, string prefix, Attributes attrs, Location loc)
			: base(member.Parent, SetupName(prefix, member, loc), attrs)
		{
			this.prefix = prefix;
		}

		private static MemberName SetupName(string prefix, InterfaceMemberBase member, Location loc)
		{
			return new MemberName(member.MemberName.Left, prefix + member.ShortName, member.MemberName.ExplicitInterface, loc);
		}

		public void UpdateName(InterfaceMemberBase member)
		{
			SetMemberName(SetupName(prefix, member, base.Location));
		}

		public EmitContext CreateEmitContext(ILGenerator ig, SourceMethodBuilder sourceMethod)
		{
			return new EmitContext(this, ig, ReturnType, sourceMethod);
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Type == pa.CLSCompliant || a.Type == pa.Obsolete || a.Type == pa.Conditional)
			{
				base.Report.Error(1667, a.Location, "Attribute `{0}' is not valid on property or event accessors. It is valid on `{1}' declarations only", a.Type.GetSignatureForError(), a.GetValidTargets());
			}
			else if (a.IsValidSecurityAttribute())
			{
				a.ExtractSecurityPermissionSet(ctor, ref declarative_security);
			}
			else if (a.Target == AttributeTargets.Method)
			{
				method_data.MethodBuilder.SetCustomAttribute((ConstructorInfo)ctor.GetMetaInfo(), cdata);
			}
			else if (a.Target == AttributeTargets.ReturnValue)
			{
				if (return_attributes == null)
				{
					return_attributes = new ReturnParameter(this, method_data.MethodBuilder, base.Location);
				}
				return_attributes.ApplyAttributeBuilder(a, ctor, cdata, pa);
			}
			else
			{
				ApplyToExtraTarget(a, ctor, cdata, pa);
			}
		}

		protected virtual void ApplyToExtraTarget(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			throw new NotSupportedException("You forgot to define special attribute target handling");
		}

		public sealed override bool Define()
		{
			throw new NotSupportedException();
		}

		public virtual void Emit(TypeDefinition parent)
		{
			method_data.Emit(parent);
			if ((base.ModFlags & Modifiers.COMPILER_GENERATED) != 0 && !Parent.IsCompilerGenerated)
			{
				Module.PredefinedAttributes.CompilerGenerated.EmitAttribute(method_data.MethodBuilder);
			}
			if ((base.ModFlags & Modifiers.DEBUGGER_HIDDEN) != 0)
			{
				Module.PredefinedAttributes.DebuggerHidden.EmitAttribute(method_data.MethodBuilder);
			}
			if (ReturnType.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				return_attributes = new ReturnParameter(this, method_data.MethodBuilder, base.Location);
				Module.PredefinedAttributes.Dynamic.EmitAttribute(return_attributes.Builder);
			}
			else if (ReturnType.HasDynamicElement)
			{
				return_attributes = new ReturnParameter(this, method_data.MethodBuilder, base.Location);
				Module.PredefinedAttributes.Dynamic.EmitAttribute(return_attributes.Builder, ReturnType, base.Location);
			}
			if (base.OptAttributes != null)
			{
				base.OptAttributes.Emit();
			}
			if (declarative_security != null)
			{
				foreach (KeyValuePair<SecurityAction, PermissionSet> item in declarative_security)
				{
					method_data.MethodBuilder.AddDeclarativeSecurity(item.Key, item.Value);
				}
			}
			block = null;
		}

		public override bool EnableOverloadChecks(MemberCore overload)
		{
			if (overload is MethodCore)
			{
				caching_flags |= Flags.MethodOverloadsExist;
				return true;
			}
			if (overload is AbstractPropertyEventMethod)
			{
				return true;
			}
			return false;
		}

		public override string GetCallerMemberName()
		{
			return base.GetCallerMemberName().Substring(prefix.Length);
		}

		public override string GetSignatureForDocumentation()
		{
			throw new NotSupportedException();
		}

		public override bool IsClsComplianceRequired()
		{
			return false;
		}

		public void PrepareEmit()
		{
			method_data.DefineMethodBuilder(Parent.PartialContainer, ParameterInfo);
		}

		public override void WriteDebugSymbol(MonoSymbolFile file)
		{
			if (method_data != null)
			{
				method_data.WriteDebugSymbol(file);
			}
		}
	}
}
