namespace Leap.Unity.Attributes
{
	public class MinMax : CombinablePropertyAttribute, IFullPropertyDrawer
	{
		public const float PERCENT_NUM = 0.2f;

		public const float SPACING = 3f;

		public readonly float min;

		public readonly float max;

		public readonly bool isInt;

		public MinMax(float min, float max)
		{
			this.min = min;
			this.max = max;
			isInt = false;
		}

		public MinMax(int min, int max)
		{
			this.min = min;
			this.max = max;
			isInt = true;
		}
	}
}
