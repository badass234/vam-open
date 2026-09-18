namespace Mono.CSharp
{
	public class InterpolatedStringInsert : CompositeExpression
	{
		public Expression Alignment { get; set; }

		public string Format { get; set; }

		public InterpolatedStringInsert(Expression expr)
			: base(expr)
		{
		}

		protected override void CloneTo(CloneContext clonectx, Expression t)
		{
			InterpolatedStringInsert interpolatedStringInsert = (InterpolatedStringInsert)t;
			if (Alignment != null)
			{
				interpolatedStringInsert.Alignment = Alignment.Clone(clonectx);
			}
		}

		protected override Expression DoResolve(ResolveContext rc)
		{
			Expression expression = base.DoResolve(rc);
			if (expression == null)
			{
				return null;
			}
			return Convert.ImplicitConversionRequired(rc, expression, rc.BuiltinTypes.Object, expression.Location);
		}

		public int? ResolveAligment(ResolveContext rc)
		{
			Constant constant = Alignment.ResolveLabelConstant(rc);
			if (constant == null)
			{
				return null;
			}
			constant = constant.ImplicitConversionRequired(rc, rc.BuiltinTypes.Int);
			if (constant == null)
			{
				return null;
			}
			int num = (int)constant.GetValueAsLong();
			if (num > 32767 || num < -32767)
			{
				rc.Report.Warning(8094, 1, Alignment.Location, "Alignment value has a magnitude greater than 32767 and may result in a large formatted string");
			}
			return num;
		}
	}
}
