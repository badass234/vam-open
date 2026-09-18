using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mono.CSharp
{
	public abstract class ReportPrinter
	{
		protected HashSet<ITypeDefinition> reported_missing_definitions;

		public int ErrorsCount { get; protected set; }

		public int WarningsCount { get; private set; }

		public virtual bool HasRelatedSymbolSupport => true;

		protected virtual string FormatText(string txt)
		{
			return txt;
		}

		public virtual void Print(AbstractMessage msg, bool showFullPath)
		{
			if (msg.IsWarning)
			{
				int warningsCount = WarningsCount + 1;
				WarningsCount = warningsCount;
			}
			else
			{
				int warningsCount = ErrorsCount + 1;
				ErrorsCount = warningsCount;
			}
		}

		protected void Print(AbstractMessage msg, TextWriter output, bool showFullPath)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (!msg.Location.IsNull)
			{
				if (showFullPath)
				{
					stringBuilder.Append(msg.Location.ToStringFullName());
				}
				else
				{
					stringBuilder.Append(msg.Location.ToString());
				}
				stringBuilder.Append(" ");
			}
			stringBuilder.AppendFormat("{0} CS{1:0000}: {2}", msg.MessageType, msg.Code, msg.Text);
			if (!msg.IsWarning)
			{
				output.WriteLine(FormatText(stringBuilder.ToString()));
			}
			else
			{
				output.WriteLine(stringBuilder.ToString());
			}
			if (msg.RelatedSymbols != null)
			{
				string[] relatedSymbols = msg.RelatedSymbols;
				foreach (string text in relatedSymbols)
				{
					output.WriteLine(text + msg.MessageType + ")");
				}
			}
		}

		public bool MissingTypeReported(ITypeDefinition typeDefinition)
		{
			if (reported_missing_definitions == null)
			{
				reported_missing_definitions = new HashSet<ITypeDefinition>();
			}
			if (reported_missing_definitions.Contains(typeDefinition))
			{
				return true;
			}
			reported_missing_definitions.Add(typeDefinition);
			return false;
		}

		public void Reset()
		{
			int errorsCount = (WarningsCount = 0);
			ErrorsCount = errorsCount;
		}
	}
}
