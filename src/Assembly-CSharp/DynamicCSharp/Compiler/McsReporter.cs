using System.CodeDom.Compiler;
using Mono.CSharp;

namespace DynamicCSharp.Compiler
{
	internal sealed class McsReporter : ReportPrinter
	{
		private readonly CompilerResults results;

		private int warningCount;

		private int errorCount;

		public int WarningCount => warningCount;

		public int ErrorCount => errorCount;

		public McsReporter(CompilerResults results)
		{
			this.results = results;
		}

		public override void Print(AbstractMessage msg, bool showFullPath)
		{
			if (msg.IsWarning)
			{
				warningCount++;
			}
			else
			{
				errorCount++;
			}
			string fileName = "<Unknown>";
			if (msg.Location.SourceFile != null)
			{
				if (showFullPath)
				{
					if (!string.IsNullOrEmpty(msg.Location.SourceFile.FullPathName))
					{
						fileName = msg.Location.SourceFile.FullPathName;
					}
				}
				else if (!string.IsNullOrEmpty(msg.Location.SourceFile.Name))
				{
					fileName = msg.Location.SourceFile.Name;
				}
			}
			results.Errors.Add(new CompilerError
			{
				IsWarning = msg.IsWarning,
				Column = ((!msg.Location.IsNull) ? msg.Location.Column : (-1)),
				Line = ((!msg.Location.IsNull) ? msg.Location.Row : (-1)),
				ErrorNumber = msg.Code.ToString(),
				ErrorText = msg.Text,
				FileName = fileName
			});
		}
	}
}
