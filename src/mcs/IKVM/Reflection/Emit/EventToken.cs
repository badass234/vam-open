namespace IKVM.Reflection.Emit
{
	public struct EventToken
	{
		public static readonly EventToken Empty;

		private readonly int token;

		public int Token => token;

		internal EventToken(int token)
		{
			this.token = token;
		}

		public override bool Equals(object obj)
		{
			EventToken? eventToken = obj as EventToken?;
			EventToken eventToken2 = this;
			if (!eventToken.HasValue)
			{
				return false;
			}
			if (!eventToken.HasValue)
			{
				return true;
			}
			return eventToken.GetValueOrDefault() == eventToken2;
		}

		public override int GetHashCode()
		{
			return token;
		}

		public bool Equals(EventToken other)
		{
			return this == other;
		}

		public static bool operator ==(EventToken et1, EventToken et2)
		{
			return et1.token == et2.token;
		}

		public static bool operator !=(EventToken et1, EventToken et2)
		{
			return et1.token != et2.token;
		}
	}
}
