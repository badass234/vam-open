namespace IKVM.Reflection.Emit
{
	public struct MethodToken
	{
		public static readonly MethodToken Empty;

		private readonly int token;

		public int Token => token;

		internal MethodToken(int token)
		{
			this.token = token;
		}

		public override bool Equals(object obj)
		{
			MethodToken? methodToken = obj as MethodToken?;
			MethodToken methodToken2 = this;
			if (!methodToken.HasValue)
			{
				return false;
			}
			if (!methodToken.HasValue)
			{
				return true;
			}
			return methodToken.GetValueOrDefault() == methodToken2;
		}

		public override int GetHashCode()
		{
			return token;
		}

		public bool Equals(MethodToken other)
		{
			return this == other;
		}

		public static bool operator ==(MethodToken mt1, MethodToken mt2)
		{
			return mt1.token == mt2.token;
		}

		public static bool operator !=(MethodToken mt1, MethodToken mt2)
		{
			return mt1.token != mt2.token;
		}
	}
}
