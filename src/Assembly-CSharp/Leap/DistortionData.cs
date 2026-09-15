using LeapInternal;

namespace Leap
{
	public class DistortionData
	{
		public ulong Version { get; set; }

		public float Width { get; set; }

		public float Height { get; set; }

		public float[] Data { get; set; }

		public bool IsValid
		{
			get
			{
				if (Data != null && Width == (float)LeapC.DistortionSize && Height == (float)LeapC.DistortionSize && (float)Data.Length == Width * Height * 2f)
				{
					return true;
				}
				return false;
			}
		}

		public DistortionData()
		{
		}

		public DistortionData(ulong version, float width, float height, float[] data)
		{
			Version = version;
			Width = width;
			Height = height;
			Data = data;
		}
	}
}
