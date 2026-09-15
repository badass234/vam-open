using System;
using UnityEngine;

namespace Leap.Unity
{
	[Serializable]
	public class SmoothedQuaternion
	{
		public Quaternion value = Quaternion.identity;

		public float delay;

		public bool reset = true;

		public void SetBlend(float blend, float deltaTime = 1f)
		{
			delay = deltaTime * blend / (1f - blend);
		}

		public Quaternion Update(Quaternion input, float deltaTime = 1f)
		{
			if (deltaTime > 0f && !reset)
			{
				float num = delay / deltaTime;
				float num2 = num / (1f + num);
				value = Quaternion.Slerp(value, input, 1f - num2);
			}
			else
			{
				value = input;
				reset = false;
			}
			return value;
		}
	}
}
