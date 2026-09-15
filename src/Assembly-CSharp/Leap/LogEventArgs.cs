namespace Leap
{
	public class LogEventArgs : LeapEventArgs
	{
		public MessageSeverity severity { get; set; }

		public long timestamp { get; set; }

		public string message { get; set; }

		public LogEventArgs(MessageSeverity severity, long timestamp, string message)
			: base(LeapEvent.EVENT_LOG_EVENT)
		{
			this.severity = severity;
			this.message = message;
			this.timestamp = timestamp;
		}
	}
}
