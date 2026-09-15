namespace DynamicCSharp.Demo
{
	internal struct TankEvent
	{
		public TankEventType eventType;

		public float eventValue;

		public TankEvent(TankEventType type, float value = 0f)
		{
			eventType = type;
			eventValue = value;
		}
	}
}
