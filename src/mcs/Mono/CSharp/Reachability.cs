namespace Mono.CSharp
{
	public struct Reachability
	{
		private readonly bool unreachable;

		public bool IsUnreachable => unreachable;

		private Reachability(bool unreachable)
		{
			this.unreachable = unreachable;
		}

		public static Reachability CreateUnreachable()
		{
			return new Reachability(true);
		}

		public static Reachability operator &(Reachability a, Reachability b)
		{
			return new Reachability(a.unreachable && b.unreachable);
		}

		public static Reachability operator |(Reachability a, Reachability b)
		{
			return new Reachability(a.unreachable | b.unreachable);
		}
	}
}
