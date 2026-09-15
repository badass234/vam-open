namespace DynamicCSharp.Compiler
{
	internal struct ScriptCompilerError
	{
		public string errorCode;

		public string errorText;

		public string fileName;

		public int line;

		public int column;

		public bool isWarning;
	}
}
