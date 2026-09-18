namespace IKVM.Reflection.Emit
{
	public struct SignatureToken
	{
		public static readonly SignatureToken Empty;

		private readonly int token;

		public int Token => token;

		internal SignatureToken(int token)
		{
			this.token = token;
		}

		public override bool Equals(object obj)
		{
			SignatureToken? signatureToken = obj as SignatureToken?;
			SignatureToken signatureToken2 = this;
			if (!signatureToken.HasValue)
			{
				return false;
			}
			if (!signatureToken.HasValue)
			{
				return true;
			}
			return signatureToken.GetValueOrDefault() == signatureToken2;
		}

		public override int GetHashCode()
		{
			return token;
		}

		public bool Equals(SignatureToken other)
		{
			return this == other;
		}

		public static bool operator ==(SignatureToken st1, SignatureToken st2)
		{
			return st1.token == st2.token;
		}

		public static bool operator !=(SignatureToken st1, SignatureToken st2)
		{
			return st1.token != st2.token;
		}
	}
}
