using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp
{
	public abstract class MethodOrOperator : MethodCore, IMethodData, IMemberContext, IModuleContext, IMethodDefinition, IMemberDefinition
	{
		private ReturnParameter return_attributes;

		private Dictionary<SecurityAction, PermissionSet> declarative_security;

		protected MethodData MethodData;

		private static readonly string[] attribute_targets = new string[2] { "method", "return" };

		public override AttributeTargets AttributeTargets => AttributeTargets.Method;

		MethodBase IMethodDefinition.Metadata => MethodData.MethodBuilder;

		public MethodBuilder MethodBuilder => MethodData.MethodBuilder;

		public bool IsPartialDefinition
		{
			get
			{
				if ((base.ModFlags & Modifiers.PARTIAL) != 0)
				{
					return base.Block == null;
				}
				return false;
			}
		}

		public bool IsPartialImplementation
		{
			get
			{
				if ((base.ModFlags & Modifiers.PARTIAL) != 0)
				{
					return base.Block != null;
				}
				return false;
			}
		}

		public override string[] ValidAttributeTargets => attribute_targets;

		bool IMethodData.IsAccessor => false;

		public TypeSpec ReturnType => base.MemberType;

		public MemberName MethodName => base.MemberName;

		protected MethodOrOperator(TypeDefinition parent, FullNamedExpression type, Modifiers mod, Modifiers allowed_mod, MemberName name, Attributes attrs, ParametersCompiled parameters)
			: base(parent, type, mod, allowed_mod, name, attrs, parameters)
		{
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Target == AttributeTargets.ReturnValue)
			{
				if (return_attributes == null)
				{
					return_attributes = new ReturnParameter(this, MethodBuilder, base.Location);
				}
				return_attributes.ApplyAttributeBuilder(a, ctor, cdata, pa);
				return;
			}
			if (a.Type == pa.MethodImpl)
			{
				if ((base.ModFlags & Modifiers.ASYNC) != 0 && (a.GetMethodImplOptions() & MethodImplOptions.Synchronized) != 0)
				{
					base.Report.Error(4015, a.Location, "`{0}': Async methods cannot use `MethodImplOptions.Synchronized'", GetSignatureForError());
				}
				is_external_implementation = a.IsInternalCall();
			}
			else if (a.Type == pa.DllImport)
			{
				if ((base.ModFlags & (Modifiers.STATIC | Modifiers.EXTERN)) != (Modifiers.STATIC | Modifiers.EXTERN))
				{
					base.Report.Error(601, a.Location, "The DllImport attribute must be specified on a method marked `static' and `extern'");
				}
				if (base.MemberName.IsGeneric || Parent.IsGenericOrParentIsGeneric)
				{
					base.Report.Error(7042, a.Location, "The DllImport attribute cannot be applied to a method that is generic or contained in a generic type");
				}
				is_external_implementation = true;
			}
			if (a.IsValidSecurityAttribute())
			{
				a.ExtractSecurityPermissionSet(ctor, ref declarative_security);
			}
			else if (MethodBuilder != null)
			{
				MethodBuilder.SetCustomAttribute((ConstructorInfo)ctor.GetMetaInfo(), cdata);
			}
		}

		protected override bool CheckForDuplications()
		{
			return Parent.MemberCache.CheckExistingMembersOverloads(this, parameters);
		}

		public virtual EmitContext CreateEmitContext(ILGenerator ig, SourceMethodBuilder sourceMethod)
		{
			return new EmitContext(this, ig, base.MemberType, sourceMethod);
		}

		public override bool Define()
		{
			if (!base.Define())
			{
				return false;
			}
			if (!CheckBase())
			{
				return false;
			}
			MemberKind kind = ((this is Operator) ? MemberKind.Operator : ((!(this is Destructor)) ? MemberKind.Method : MemberKind.Destructor));
			string exlicitName;
			if (IsPartialDefinition)
			{
				caching_flags &= ~Flags.Excluded_Undetected;
				caching_flags |= Flags.Excluded;
				if ((caching_flags & Flags.PartialDefinitionExists) != 0)
				{
					return true;
				}
				if (IsExplicitImpl)
				{
					return true;
				}
				exlicitName = null;
			}
			else
			{
				MethodData = new MethodData(this, base.ModFlags, flags, this, base_method);
				if (!MethodData.Define(Parent.PartialContainer, GetFullName(base.MemberName)))
				{
					return false;
				}
				exlicitName = MethodData.MetadataName;
			}
			spec = new MethodSpec(kind, Parent.Definition, this, ReturnType, parameters, base.ModFlags);
			if (base.MemberName.Arity > 0)
			{
				spec.IsGeneric = true;
			}
			Parent.MemberCache.AddMember(this, exlicitName, spec);
			return true;
		}

		protected override void DoMemberTypeIndependentChecks()
		{
			base.DoMemberTypeIndependentChecks();
			CheckAbstractAndExtern(block != null);
			if ((base.ModFlags & Modifiers.PARTIAL) == 0)
			{
				return;
			}
			for (int i = 0; i < parameters.Count; i++)
			{
				IParameterData parameterData = parameters.FixedParameters[i];
				if ((parameterData.ModFlags & Parameter.Modifier.OUT) != Parameter.Modifier.NONE)
				{
					base.Report.Error(752, base.Location, "`{0}': A partial method parameters cannot use `out' modifier", GetSignatureForError());
				}
				if (parameterData.HasDefaultValue && IsPartialImplementation)
				{
					((Parameter)parameterData).Warning_UselessOptionalParameter(base.Report);
				}
			}
		}

		protected override void DoMemberTypeDependentChecks()
		{
			base.DoMemberTypeDependentChecks();
			if (base.MemberType.IsStatic)
			{
				Error_StaticReturnType();
			}
		}

		public override void Emit()
		{
			if ((base.ModFlags & Modifiers.COMPILER_GENERATED) != 0 && !Parent.IsCompilerGenerated)
			{
				Module.PredefinedAttributes.CompilerGenerated.EmitAttribute(MethodBuilder);
			}
			if ((base.ModFlags & Modifiers.DEBUGGER_HIDDEN) != 0)
			{
				Module.PredefinedAttributes.DebuggerHidden.EmitAttribute(MethodBuilder);
			}
			if ((base.ModFlags & Modifiers.DEBUGGER_STEP_THROUGH) != 0)
			{
				Module.PredefinedAttributes.DebuggerStepThrough.EmitAttribute(MethodBuilder);
			}
			if (ReturnType.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
			{
				return_attributes = new ReturnParameter(this, MethodBuilder, base.Location);
				Module.PredefinedAttributes.Dynamic.EmitAttribute(return_attributes.Builder);
			}
			else if (ReturnType.HasDynamicElement)
			{
				return_attributes = new ReturnParameter(this, MethodBuilder, base.Location);
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
					MethodBuilder.AddDeclarativeSecurity(item.Key, item.Value);
				}
			}
			if (type_expr != null)
			{
				ConstraintChecker.Check(this, member_type, type_expr.Location);
			}
			base.Emit();
			if (MethodData != null)
			{
				MethodData.Emit(Parent);
			}
			if (block != null && block.StateMachine is AsyncTaskStorey)
			{
				Module.PredefinedAttributes.AsyncStateMachine.EmitAttribute(MethodBuilder, block.StateMachine);
			}
			if ((base.ModFlags & Modifiers.PARTIAL) == 0)
			{
				base.Block = null;
			}
		}

		protected void Error_ConditionalAttributeIsNotValid()
		{
			base.Report.Error(577, base.Location, "Conditional not valid on `{0}' because it is a constructor, destructor, operator or explicit interface implementation", GetSignatureForError());
		}

		public override string[] ConditionalConditions()
		{
			if ((caching_flags & (Flags.Excluded_Undetected | Flags.Excluded)) == 0)
			{
				return null;
			}
			if ((base.ModFlags & Modifiers.PARTIAL) != 0 && (caching_flags & Flags.Excluded) != 0)
			{
				return new string[0];
			}
			caching_flags &= ~Flags.Excluded_Undetected;
			string[] array2;
			if (base_method == null)
			{
				if (base.OptAttributes == null)
				{
					return null;
				}
				Attribute[] array = base.OptAttributes.SearchMulti(Module.PredefinedAttributes.Conditional);
				if (array == null)
				{
					return null;
				}
				array2 = new string[array.Length];
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i] = array[i].GetConditionalAttributeValue();
				}
			}
			else
			{
				array2 = base_method.MemberDefinition.ConditionalConditions();
			}
			if (array2 != null)
			{
				caching_flags |= Flags.Excluded;
			}
			return array2;
		}

		public override void PrepareEmit()
		{
			base.PrepareEmit();
			MethodBuilder methodBuilder = MethodData.DefineMethodBuilder(Parent);
			if (CurrentTypeParameters != null)
			{
				string[] array = new string[CurrentTypeParameters.Count];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = CurrentTypeParameters[i].Name;
				}
				GenericTypeParameterBuilder[] array2 = MethodBuilder.DefineGenericParameters(array);
				for (int j = 0; j < CurrentTypeParameters.Count; j++)
				{
					CurrentTypeParameters[j].Define(array2[j]);
				}
			}
			methodBuilder.SetParameters(parameters.GetMetaInfo());
			methodBuilder.SetReturnType(ReturnType.GetMetaInfo());
		}

		public override void WriteDebugSymbol(MonoSymbolFile file)
		{
			if (MethodData != null && !IsPartialDefinition)
			{
				MethodData.WriteDebugSymbol(file);
			}
		}
	}
}
