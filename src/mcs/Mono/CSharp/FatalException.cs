using System;

namespace Mono.CSharp
{
	internal class FatalException : Exception
	{
		public FatalException(string message)
			: base(message)
		{
		}
	}
}
