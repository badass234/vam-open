namespace Mono.CompilerServices.SymbolWriter
{
	public struct CapturedVariable
	{
		public enum CapturedKind : byte
		{
			Local = 0,
			Parameter = 1,
			This = 2
		}

		public readonly string Name;

		public readonly string CapturedName;

		public readonly CapturedKind Kind;

		public CapturedVariable(string name, string captured_name, CapturedKind kind)
		{
			Name = name;
			CapturedName = captured_name;
			Kind = kind;
		}

		internal CapturedVariable(MyBinaryReader reader)
		{
			Name = reader.ReadString();
			CapturedName = reader.ReadString();
			Kind = (CapturedKind)reader.ReadByte();
		}

		internal void Write(MyBinaryWriter bw)
		{
			bw.Write(Name);
			bw.Write(CapturedName);
			bw.Write((byte)Kind);
		}

		public override string ToString()
		{
			return $"[CapturedVariable {Name}:{CapturedName}:{Kind}]";
		}
	}
}
