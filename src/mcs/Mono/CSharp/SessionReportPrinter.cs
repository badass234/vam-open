using System.Collections.Generic;

namespace Mono.CSharp
{
	internal class SessionReportPrinter : ReportPrinter
	{
		private List<AbstractMessage> session_messages;

		private List<AbstractMessage> common_messages;

		private List<AbstractMessage> merged_messages;

		private bool showFullPaths;

		public bool IsEmpty
		{
			get
			{
				if (merged_messages == null)
				{
					return common_messages == null;
				}
				return false;
			}
		}

		public void ClearSession()
		{
			session_messages = null;
		}

		public override void Print(AbstractMessage msg, bool showFullPath)
		{
			if (session_messages == null)
			{
				session_messages = new List<AbstractMessage>();
			}
			session_messages.Add(msg);
			showFullPaths = showFullPath;
			base.Print(msg, showFullPath);
		}

		public void EndSession()
		{
			if (session_messages == null)
			{
				return;
			}
			if (common_messages == null)
			{
				common_messages = new List<AbstractMessage>(session_messages);
				merged_messages = session_messages;
				session_messages = null;
				return;
			}
			for (int i = 0; i < common_messages.Count; i++)
			{
				AbstractMessage abstractMessage = common_messages[i];
				bool flag = false;
				foreach (AbstractMessage session_message in session_messages)
				{
					if (abstractMessage.Equals(session_message))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					common_messages.RemoveAt(i);
				}
			}
			for (int j = 0; j < session_messages.Count; j++)
			{
				AbstractMessage abstractMessage2 = session_messages[j];
				bool flag2 = false;
				for (int k = 0; k < merged_messages.Count; k++)
				{
					if (abstractMessage2.Equals(merged_messages[k]))
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					merged_messages.Add(abstractMessage2);
				}
			}
		}

		public bool Merge(ReportPrinter dest)
		{
			List<AbstractMessage> list = merged_messages;
			if (common_messages != null && common_messages.Count > 0)
			{
				list = common_messages;
			}
			if (list == null)
			{
				return false;
			}
			bool flag = false;
			foreach (AbstractMessage item in list)
			{
				dest.Print(item, showFullPaths);
				flag |= !item.IsWarning;
			}
			if (reported_missing_definitions != null)
			{
				foreach (ITypeDefinition reported_missing_definition in reported_missing_definitions)
				{
					dest.MissingTypeReported(reported_missing_definition);
				}
			}
			return flag;
		}
	}
}
