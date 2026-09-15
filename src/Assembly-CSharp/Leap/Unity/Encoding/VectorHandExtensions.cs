using UnityEngine;

namespace Leap.Unity.Encoding
{
	public static class VectorHandExtensions
	{
		public static Bone GetBone(this Hand hand, int boneIdx)
		{
			return hand.Fingers[boneIdx / 4].bones[boneIdx % 4];
		}

		public static byte FloatToByte(float inFloat, float movementRange = 0.3f)
		{
			float num = Mathf.Clamp(inFloat, (0f - movementRange) / 2f, movementRange / 2f);
			num += movementRange / 2f;
			num /= movementRange;
			num *= 255f;
			num = Mathf.Floor(num);
			return (byte)num;
		}

		public static float ByteToFloat(byte inByte, float movementRange = 0.3f)
		{
			float num = (int)inByte;
			num /= 255f;
			num *= movementRange;
			return num - movementRange / 2f;
		}
	}
}
