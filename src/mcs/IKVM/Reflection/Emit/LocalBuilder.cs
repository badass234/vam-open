namespace IKVM.Reflection.Emit
{
	public sealed class LocalBuilder : LocalVariableInfo
	{
		internal string name;

		internal int startOffset;

		internal int endOffset;

		internal LocalBuilder(Type localType, int index, bool pinned)
			: base(index, localType, pinned)
		{
		}

		internal LocalBuilder(Type localType, int index, bool pinned, CustomModifiers customModifiers)
			: base(index, localType, pinned, customModifiers)
		{
		}

		public void SetLocalSymInfo(string name)
		{
			this.name = name;
		}

		public void SetLocalSymInfo(string name, int startOffset, int endOffset)
		{
			this.name = name;
			this.startOffset = startOffset;
			this.endOffset = endOffset;
		}
	}
}
