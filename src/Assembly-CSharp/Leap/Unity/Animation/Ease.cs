namespace Leap.Unity.Animation
{
	public static class Ease
	{
		public static class Quadratic
		{
			public static float InOut(float t)
			{
				t *= 2f;
				if (t < 1f)
				{
					return 0.5f * t * t;
				}
				t -= 1f;
				return -0.5f * (t * (t - 2f) - 1f);
			}
		}

		public static class Cubic
		{
			public static float InOut(float t)
			{
				t *= 2f;
				if (t < 1f)
				{
					return 0.5f * t * t * t;
				}
				t -= 2f;
				return 0.5f * (t * t * t + 2f);
			}
		}

		public static class Quartic
		{
			public static float InOut(float t)
			{
				t *= 2f;
				if (t < 1f)
				{
					return 0.5f * t * t * t * t;
				}
				t -= 2f;
				return -0.5f * (t * t * t * t - 2f);
			}
		}
	}
}
