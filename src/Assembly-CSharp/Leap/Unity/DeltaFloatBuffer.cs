namespace Leap.Unity
{
	public class DeltaFloatBuffer : DeltaBuffer<float, float>
	{
		public DeltaFloatBuffer(int bufferSize)
			: base(bufferSize)
		{
		}

		public override float Delta()
		{
			if (base.Count <= 1)
			{
				return 0f;
			}
			float num = 0f;
			for (int i = 0; i < base.Count - 1; i++)
			{
				num += (Get(i + 1) - Get(i)) / (GetTime(i + 1) - GetTime(i));
			}
			return num / (float)(base.Count - 1);
		}
	}
}
