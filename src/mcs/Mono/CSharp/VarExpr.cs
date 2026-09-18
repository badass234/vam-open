namespace Mono.CSharp
{
	internal class VarExpr : SimpleName
	{
		public VarExpr(Location loc)
			: base("var", loc)
		{
		}

		public bool InferType(ResolveContext ec, Expression right_side)
		{
			if (type != null)
			{
				throw new InternalErrorException("An implicitly typed local variable could not be redefined");
			}
			type = right_side.Type;
			if (type == InternalType.NullLiteral || type.Kind == MemberKind.Void || type == InternalType.AnonymousMethod || type == InternalType.MethodGroup)
			{
				ec.Report.Error(815, loc, "An implicitly typed local variable declaration cannot be initialized with `{0}'", type.GetSignatureForError());
				type = InternalType.ErrorType;
				return false;
			}
			eclass = ExprClass.Variable;
			return true;
		}

		protected override void Error_TypeOrNamespaceNotFound(IMemberContext ec)
		{
			if (ec.Module.Compiler.Settings.Version < LanguageVersion.V_3)
			{
				base.Error_TypeOrNamespaceNotFound(ec);
			}
			else
			{
				ec.Module.Compiler.Report.Error(825, loc, "The contextual keyword `var' may only appear within a local variable declaration");
			}
		}
	}
}
