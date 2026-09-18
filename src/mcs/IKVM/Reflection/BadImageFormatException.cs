using System;
using System.Runtime.Serialization;

namespace IKVM.Reflection
{
	[Serializable]
	public sealed class BadImageFormatException : Exception
	{
		public BadImageFormatException()
		{
		}

		public BadImageFormatException(string message)
			: base(message)
		{
		}

		public BadImageFormatException(string message, Exception inner)
			: base(message, inner)
		{
		}

		private BadImageFormatException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
