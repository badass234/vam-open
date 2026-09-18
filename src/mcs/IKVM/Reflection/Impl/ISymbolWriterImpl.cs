namespace IKVM.Reflection.Impl
{
	internal interface ISymbolWriterImpl
	{
		bool IsDeterministic { get; }

		byte[] GetDebugInfo(ref IMAGE_DEBUG_DIRECTORY idd);

		void RemapToken(int oldToken, int newToken);

		void DefineLocalVariable2(string name, FieldAttributes attributes, int signature, int addrKind, int addr1, int addr2, int addr3, int startOffset, int endOffset);

		void OpenMethod(SymbolToken symbolToken, MethodBase mb);
	}
}
