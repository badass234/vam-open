using System;
using System.Runtime.Serialization;

namespace IKVM.Reflection
{
	[Serializable]
	public sealed class FileFormatLimitationExceededException : InvalidOperationException
	{
		public const int META_E_STRINGSPACE_FULL = -2146233960;

		public int ErrorCode => base.HResult;

		public FileFormatLimitationExceededException(string message, int hresult)
			: base(message)
		{
			base.HResult = hresult;
		}

		private FileFormatLimitationExceededException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
