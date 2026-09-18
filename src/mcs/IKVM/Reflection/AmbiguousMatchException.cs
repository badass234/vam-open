using System;
using System.Runtime.Serialization;

namespace IKVM.Reflection
{
	[Serializable]
	public sealed class AmbiguousMatchException : Exception
	{
		public AmbiguousMatchException()
		{
		}

		public AmbiguousMatchException(string message)
			: base(message)
		{
		}

		public AmbiguousMatchException(string message, Exception inner)
			: base(message, inner)
		{
		}

		private AmbiguousMatchException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
