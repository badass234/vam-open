namespace Assets.OVR.Scripts
{
	public class RangedRecord : Record
	{
		public float value;

		public float min;

		public float max;

		public RangedRecord(string cat, string msg, float val, float minVal, float maxVal)
			: base(cat, msg)
		{
			value = val;
			min = minVal;
			max = maxVal;
		}
	}
}
