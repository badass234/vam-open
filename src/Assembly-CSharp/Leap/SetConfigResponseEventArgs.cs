namespace Leap
{
	public class SetConfigResponseEventArgs : LeapEventArgs
	{
		public string ConfigKey { get; set; }

		public Config.ValueType DataType { get; set; }

		public object Value { get; set; }

		public uint RequestId { get; set; }

		public SetConfigResponseEventArgs(string config_key, Config.ValueType dataType, object value, uint requestId)
			: base(LeapEvent.EVENT_CONFIG_RESPONSE)
		{
			ConfigKey = config_key;
			DataType = dataType;
			Value = value;
			RequestId = requestId;
		}
	}
}
