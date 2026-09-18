namespace IKVM.Reflection.Emit
{
	public struct StringToken
	{
		private readonly int token;

		public int Token => token;

		internal StringToken(int token)
		{
			this.token = token;
		}

		public override bool Equals(object obj)
		{
			StringToken? stringToken = obj as StringToken?;
			StringToken stringToken2 = this;
			if (!stringToken.HasValue)
			{
				return false;
			}
			if (!stringToken.HasValue)
			{
				return true;
			}
			return stringToken.GetValueOrDefault() == stringToken2;
		}

		public override int GetHashCode()
		{
			return token;
		}

		public bool Equals(StringToken other)
		{
			return this == other;
		}

		public static bool operator ==(StringToken st1, StringToken st2)
		{
			return st1.token == st2.token;
		}

		public static bool operator !=(StringToken st1, StringToken st2)
		{
			return st1.token != st2.token;
		}
	}
}
