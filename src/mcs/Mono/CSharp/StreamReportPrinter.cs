using System.IO;

namespace Mono.CSharp
{
	public class StreamReportPrinter : ReportPrinter
	{
		private readonly TextWriter writer;

		public StreamReportPrinter(TextWriter writer)
		{
			this.writer = writer;
		}

		public override void Print(AbstractMessage msg, bool showFullPath)
		{
			Print(msg, writer, showFullPath);
			base.Print(msg, showFullPath);
		}
	}
}
