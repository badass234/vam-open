using System;

namespace Mono.CSharp
{
	public class CompletionResult : Exception
	{
		private string[] result;

		private string base_text;

		public string[] Result => result;

		public string BaseText => base_text;

		public CompletionResult(string base_text, string[] res)
		{
			if (base_text == null)
			{
				throw new ArgumentNullException("base_text");
			}
			this.base_text = base_text;
			result = res;
			Array.Sort(result);
		}
	}
}
