namespace IKVM.Reflection.Emit
{
	public struct TypeToken
	{
		public static readonly TypeToken Empty;

		private readonly int token;

		public int Token => token;

		internal TypeToken(int token)
		{
			this.token = token;
		}

		public override bool Equals(object obj)
		{
			TypeToken? typeToken = obj as TypeToken?;
			TypeToken typeToken2 = this;
			if (!typeToken.HasValue)
			{
				return false;
			}
			if (!typeToken.HasValue)
			{
				return true;
			}
			return typeToken.GetValueOrDefault() == typeToken2;
		}

		public override int GetHashCode()
		{
			return token;
		}

		public bool Equals(TypeToken other)
		{
			return this == other;
		}

		public static bool operator ==(TypeToken tt1, TypeToken tt2)
		{
			return tt1.token == tt2.token;
		}

		public static bool operator !=(TypeToken tt1, TypeToken tt2)
		{
			return tt1.token != tt2.token;
		}
	}
}
