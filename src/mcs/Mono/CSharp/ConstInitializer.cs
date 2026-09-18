namespace Mono.CSharp
{
	public class ConstInitializer : ShimExpression
	{
		private bool in_transit;

		private readonly FieldBase field;

		public string Name { get; set; }

		public ConstInitializer(FieldBase field, Expression value, Location loc)
			: base(value)
		{
			base.loc = loc;
			this.field = field;
		}

		protected override Expression DoResolve(ResolveContext unused)
		{
			if (type != null)
			{
				return expr;
			}
			ResolveContext.Options options = ResolveContext.Options.ConstantScope;
			if (field is EnumMember)
			{
				options |= ResolveContext.Options.EnumScope;
			}
			ResolveContext rc = new ResolveContext(field, options);
			expr = DoResolveInitializer(rc);
			type = expr.Type;
			return expr;
		}

		protected virtual Expression DoResolveInitializer(ResolveContext rc)
		{
			if (in_transit)
			{
				field.Compiler.Report.Error(110, expr.Location, "The evaluation of the constant value for `{0}' involves a circular definition", GetSignatureForError());
				expr = null;
			}
			else
			{
				in_transit = true;
				expr = expr.Resolve(rc);
			}
			in_transit = false;
			if (expr != null)
			{
				Constant constant = expr as Constant;
				if (constant != null)
				{
					constant = field.ConvertInitializer(rc, constant);
				}
				if (constant == null)
				{
					if (TypeSpec.IsReferenceType(field.MemberType))
					{
						Error_ConstantCanBeInitializedWithNullOnly(rc, field.MemberType, expr.Location, GetSignatureForError());
					}
					else if (!(expr is Constant))
					{
						Error_ExpressionMustBeConstant(rc, expr.Location, GetSignatureForError());
					}
					else
					{
						expr.Error_ValueCannotBeConverted(rc, field.MemberType, false);
					}
				}
				expr = constant;
			}
			if (expr == null)
			{
				expr = New.Constantify(field.MemberType, base.Location);
				if (expr == null)
				{
					expr = Constant.CreateConstantFromValue(field.MemberType, null, base.Location);
				}
				expr = expr.Resolve(rc);
			}
			return expr;
		}

		public override string GetSignatureForError()
		{
			if (Name == null)
			{
				return field.GetSignatureForError();
			}
			return field.Parent.GetSignatureForError() + "." + Name;
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
