namespace IKVM.Reflection.Emit
{
	public struct Label
	{
		private readonly int index1;

		internal int Index => index1 - 1;

		internal Label(int index)
		{
			index1 = index + 1;
		}

		public bool Equals(Label other)
		{
			return other.index1 == index1;
		}

		public override bool Equals(object obj)
		{
			Label value = this;
			Label? label = obj as Label?;
			return value == label;
		}

		public override int GetHashCode()
		{
			return index1;
		}

		public static bool operator ==(Label arg1, Label arg2)
		{
			return arg1.index1 == arg2.index1;
		}

		public static bool operator !=(Label arg1, Label arg2)
		{
			return !(arg1 == arg2);
		}
	}
}
