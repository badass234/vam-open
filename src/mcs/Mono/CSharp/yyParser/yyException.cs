using System;

namespace Mono.CSharp.yyParser
{
	public class yyException : Exception
	{
		public yyException(string message)
			: base(message)
		{
		}
	}
}
