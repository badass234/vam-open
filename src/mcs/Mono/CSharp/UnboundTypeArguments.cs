namespace Mono.CSharp
{
	public class UnboundTypeArguments : TypeArguments
	{
		private Location loc;

		public override bool IsEmpty => true;

		public UnboundTypeArguments(int arity, Location loc)
			: base(new FullNamedExpression[arity])
		{
			this.loc = loc;
		}

		public override bool Resolve(IMemberContext mc, bool allowUnbound)
		{
			if (!allowUnbound)
			{
				mc.Module.Compiler.Report.Error(7003, loc, "Unbound generic name is not valid in this context");
			}
			return true;
		}
	}
}
