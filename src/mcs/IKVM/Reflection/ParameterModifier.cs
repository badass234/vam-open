namespace IKVM.Reflection
{
	public struct ParameterModifier
	{
		private readonly bool[] values;

		public bool this[int index]
		{
			get
			{
				return values[index];
			}
			set
			{
				values[index] = value;
			}
		}

		public ParameterModifier(int parameterCount)
		{
			values = new bool[parameterCount];
		}
	}
}
