using System;
using IKVM.Reflection.Emit;

namespace IKVM.Reflection.Impl
{
	internal static class SymbolSupport
	{
		internal static ISymbolWriterImpl CreateSymbolWriterFor(ModuleBuilder moduleBuilder)
		{
			throw new NotSupportedException("IKVM.Reflection compiled with NO_SYMBOL_WRITER does not support writing debugging symbols.");
		}

		internal static byte[] GetDebugInfo(ISymbolWriterImpl writer, ref IMAGE_DEBUG_DIRECTORY idd)
		{
			return writer.GetDebugInfo(ref idd);
		}

		internal static void RemapToken(ISymbolWriterImpl writer, int oldToken, int newToken)
		{
			writer.RemapToken(oldToken, newToken);
		}
	}
}
