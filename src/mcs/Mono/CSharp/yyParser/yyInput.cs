namespace Mono.CSharp.yyParser
{
	public interface yyInput
	{
		bool advance();

		int token();

		object value();
	}
}
