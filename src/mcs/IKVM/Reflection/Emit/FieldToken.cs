namespace IKVM.Reflection.Emit
{
	public struct FieldToken
	{
		public static readonly FieldToken Empty;

		private readonly int token;

		public int Token => token;

		internal FieldToken(int token)
		{
			this.token = token;
		}

		public override bool Equals(object obj)
		{
			FieldToken? fieldToken = obj as FieldToken?;
			FieldToken fieldToken2 = this;
			if (!fieldToken.HasValue)
			{
				return false;
			}
			if (!fieldToken.HasValue)
			{
				return true;
			}
			return fieldToken.GetValueOrDefault() == fieldToken2;
		}

		public override int GetHashCode()
		{
			return token;
		}

		public bool Equals(FieldToken other)
		{
			return this == other;
		}

		public static bool operator ==(FieldToken ft1, FieldToken ft2)
		{
			return ft1.token == ft2.token;
		}

		public static bool operator !=(FieldToken ft1, FieldToken ft2)
		{
			return ft1.token != ft2.token;
		}
	}
}
