namespace Mono.CSharp
{
	public abstract class MemberBase : MemberCore
	{
		protected FullNamedExpression type_expr;

		protected TypeSpec member_type;

		public new TypeDefinition Parent;

		public TypeSpec MemberType => member_type;

		public FullNamedExpression TypeExpression => type_expr;

		protected MemberBase(TypeDefinition parent, FullNamedExpression type, Modifiers mod, Modifiers allowed_mod, Modifiers def_mod, MemberName name, Attributes attrs)
			: base(parent, name, attrs)
		{
			Parent = parent;
			type_expr = type;
			if (name != MemberName.Null)
			{
				base.ModFlags = ModifiersExtensions.Check(allowed_mod, mod, def_mod, base.Location, base.Report);
			}
		}

		public override bool Define()
		{
			DoMemberTypeIndependentChecks();
			if (!ResolveMemberType())
			{
				return false;
			}
			DoMemberTypeDependentChecks();
			return true;
		}

		protected virtual void DoMemberTypeIndependentChecks()
		{
			if ((Parent.ModFlags & Modifiers.SEALED) != 0 && (base.ModFlags & (Modifiers.ABSTRACT | Modifiers.VIRTUAL)) != 0)
			{
				base.Report.Error(549, base.Location, "New virtual member `{0}' is declared in a sealed class `{1}'", GetSignatureForError(), Parent.GetSignatureForError());
			}
		}

		protected virtual void DoMemberTypeDependentChecks()
		{
			if (IsAccessibleAs(MemberType))
			{
				return;
			}
			base.Report.SymbolRelatedToPreviousError(MemberType);
			if (this is Property)
			{
				base.Report.Error(53, base.Location, "Inconsistent accessibility: property type `" + MemberType.GetSignatureForError() + "' is less accessible than property `" + GetSignatureForError() + "'");
			}
			else if (this is Indexer)
			{
				base.Report.Error(54, base.Location, "Inconsistent accessibility: indexer return type `" + MemberType.GetSignatureForError() + "' is less accessible than indexer `" + GetSignatureForError() + "'");
			}
			else if (this is MethodCore)
			{
				if (this is Operator)
				{
					base.Report.Error(56, base.Location, "Inconsistent accessibility: return type `" + MemberType.GetSignatureForError() + "' is less accessible than operator `" + GetSignatureForError() + "'");
				}
				else
				{
					base.Report.Error(50, base.Location, "Inconsistent accessibility: return type `" + MemberType.GetSignatureForError() + "' is less accessible than method `" + GetSignatureForError() + "'");
				}
			}
			else if (this is Event)
			{
				base.Report.Error(7025, base.Location, "Inconsistent accessibility: event type `{0}' is less accessible than event `{1}'", MemberType.GetSignatureForError(), GetSignatureForError());
			}
			else
			{
				base.Report.Error(52, base.Location, "Inconsistent accessibility: field type `" + MemberType.GetSignatureForError() + "' is less accessible than field `" + GetSignatureForError() + "'");
			}
		}

		protected void IsTypePermitted()
		{
			if (MemberType.IsSpecialRuntimeType)
			{
				if (Parent is StateMachine)
				{
					base.Report.Error(4012, base.Location, "Parameters or local variables of type `{0}' cannot be declared in async methods or iterators", MemberType.GetSignatureForError());
				}
				else if (Parent is HoistedStoreyClass)
				{
					base.Report.Error(4013, base.Location, "Local variables of type `{0}' cannot be used inside anonymous methods, lambda expressions or query expressions", MemberType.GetSignatureForError());
				}
				else
				{
					base.Report.Error(610, base.Location, "Field or property cannot be of type `{0}'", MemberType.GetSignatureForError());
				}
			}
		}

		protected virtual bool CheckBase()
		{
			CheckProtectedModifier();
			return true;
		}

		public override string GetSignatureForDocumentation()
		{
			return Parent.GetSignatureForDocumentation() + "." + base.MemberName.Basename;
		}

		protected virtual bool ResolveMemberType()
		{
			if (member_type != null)
			{
				throw new InternalErrorException("Multi-resolve");
			}
			member_type = type_expr.ResolveAsType(this);
			return member_type != null;
		}
	}
}
