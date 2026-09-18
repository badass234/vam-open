using System.Collections.Generic;

namespace Mono.CSharp
{
	public abstract class AbstractMessage
	{
		private readonly string[] extra_info;

		protected readonly int code;

		protected readonly Location location;

		private readonly string message;

		public int Code => code;

		public abstract bool IsWarning { get; }

		public Location Location => location;

		public abstract string MessageType { get; }

		public string[] RelatedSymbols => extra_info;

		public string Text => message;

		protected AbstractMessage(int code, Location loc, string msg, List<string> extraInfo)
		{
			this.code = code;
			if (code < 0)
			{
				this.code = 8000 - code;
			}
			location = loc;
			message = msg;
			if (extraInfo.Count != 0)
			{
				extra_info = extraInfo.ToArray();
			}
		}

		protected AbstractMessage(AbstractMessage aMsg)
		{
			code = aMsg.code;
			location = aMsg.location;
			message = aMsg.message;
			extra_info = aMsg.extra_info;
		}

		public override bool Equals(object obj)
		{
			AbstractMessage abstractMessage = obj as AbstractMessage;
			if (abstractMessage == null)
			{
				return false;
			}
			if (code == abstractMessage.code && location.Equals(abstractMessage.location))
			{
				return message == abstractMessage.message;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return code.GetHashCode();
		}
	}
}
