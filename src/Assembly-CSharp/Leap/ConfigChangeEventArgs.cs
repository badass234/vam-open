namespace Leap
{
	public class ConfigChangeEventArgs : LeapEventArgs
	{
		public string ConfigKey { get; set; }

		public bool Succeeded { get; set; }

		public uint RequestId { get; set; }

		public ConfigChangeEventArgs(string config_key, bool succeeded, uint requestId)
			: base(LeapEvent.EVENT_CONFIG_CHANGE)
		{
			ConfigKey = config_key;
			Succeeded = succeeded;
			RequestId = requestId;
		}
	}
}
