namespace Mono.CSharp.yyParser
{
	public class yyUnexpectedEof : yyException
	{
		public yyUnexpectedEof(string message)
			: base(message)
		{
		}

		public yyUnexpectedEof()
			: base("")
		{
		}
	}
}
