using System.Reflection;

namespace Mono.CSharp
{
	public abstract class MethodCore : InterfaceMemberBase, IParametersMember, IInterfaceMemberSpec
	{
		protected ParametersCompiled parameters;

		protected ToplevelBlock block;

		protected MethodSpec spec;

		public override Variance ExpectedMemberTypeVariance => Variance.Covariant;

		public TypeSpec[] ParameterTypes => parameters.Types;

		public ParametersCompiled ParameterInfo => parameters;

		AParametersCollection IParametersMember.Parameters => parameters;

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

		public CallingConventions CallingConventions
		{
			get
			{
				CallingConventions callingConventions = parameters.CallingConvention;
				if (!IsInterface && (base.ModFlags & Modifiers.STATIC) == 0)
				{
					callingConventions |= CallingConventions.HasThis;
				}
				return callingConventions;
			}
		}

		public override string DocCommentHeader => "M:";

		public MethodSpec Spec => spec;

		protected MethodCore(TypeDefinition parent, FullNamedExpression type, Modifiers mod, Modifiers allowed_mod, MemberName name, Attributes attrs, ParametersCompiled parameters)
			: base(parent, type, mod, allowed_mod, name, attrs)
		{
			this.parameters = parameters;
		}

		protected override bool CheckOverrideAgainstBase(MemberSpec base_member)
		{
			bool result = base.CheckOverrideAgainstBase(base_member);
			if (!InterfaceMemberBase.CheckAccessModifiers(this, base_member))
			{
				Error_CannotChangeAccessModifiers(this, base_member);
				result = false;
			}
			return result;
		}

		protected override bool CheckBase()
		{
			if (!DefineParameters(parameters))
			{
				return false;
			}
			return base.CheckBase();
		}

		public override void Emit()
		{
			if ((base.ModFlags & Modifiers.COMPILER_GENERATED) == 0)
			{
				parameters.CheckConstraints(this);
			}
			base.Emit();
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
			return base.EnableOverloadChecks(overload);
		}

		public override string GetSignatureForDocumentation()
		{
			string text = base.GetSignatureForDocumentation();
			if (base.MemberName.Arity > 0)
			{
				text = text + "``" + base.MemberName.Arity;
			}
			return text + parameters.GetSignatureForDocumentation();
		}

		public virtual void PrepareEmit()
		{
			parameters.ResolveDefaultValues(this);
		}

		protected override bool VerifyClsCompliance()
		{
			if (!base.VerifyClsCompliance())
			{
				return false;
			}
			if (parameters.HasArglist)
			{
				base.Report.Warning(3000, 1, base.Location, "Methods with variable arguments are not CLS-compliant");
			}
			if (member_type != null && !member_type.IsCLSCompliant())
			{
				base.Report.Warning(3002, 1, base.Location, "Return type of `{0}' is not CLS-compliant", GetSignatureForError());
			}
			parameters.VerifyClsCompliance(this);
			return true;
		}
	}
}
