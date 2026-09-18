namespace Mono.CSharp
{
	public class Yield : YieldStatement<Iterator>
	{
		public Yield(Expression expr, Location loc)
			: base(expr, loc)
		{
		}

		public static bool CheckContext(BlockContext bc, Location loc)
		{
			if (!bc.CurrentAnonymousMethod.IsIterator)
			{
				bc.Report.Error(1621, loc, "The yield statement cannot be used inside anonymous method blocks");
				return false;
			}
			if (bc.HasSet(ResolveContext.Options.FinallyScope))
			{
				bc.Report.Error(1625, loc, "Cannot yield in the body of a finally clause");
				return false;
			}
			return true;
		}

		public override bool Resolve(BlockContext bc)
		{
			if (!CheckContext(bc, loc))
			{
				return false;
			}
			if (bc.HasAny(ResolveContext.Options.TryWithCatchScope))
			{
				bc.Report.Error(1626, loc, "Cannot yield a value in the body of a try block with a catch clause");
			}
			if (bc.HasSet(ResolveContext.Options.CatchScope))
			{
				bc.Report.Error(1631, loc, "Cannot yield a value in the body of a catch clause");
			}
			if (!base.Resolve(bc))
			{
				return false;
			}
			TypeSpec originalIteratorType = bc.CurrentIterator.OriginalIteratorType;
			if (expr.Type != originalIteratorType)
			{
				expr = Convert.ImplicitConversionRequired(bc, expr, originalIteratorType, loc);
				if (expr == null)
				{
					return false;
				}
			}
			return true;
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
