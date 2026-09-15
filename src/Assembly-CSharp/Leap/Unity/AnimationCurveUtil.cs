using Leap.Unity.Query;
using UnityEngine;

namespace Leap.Unity
{
	public static class AnimationCurveUtil
	{
		public static bool IsConstant(this AnimationCurve curve)
		{
			Keyframe[] keys = curve.keys;
			Keyframe keyframe = keys[0];
			for (int i = 0; i < keys.Length; i++)
			{
				Keyframe keyframe2 = keys[i];
				if (!Mathf.Approximately(keyframe.value, keyframe2.value))
				{
					return false;
				}
				if (!Mathf.Approximately(keyframe2.inTangent, 0f) && !float.IsInfinity(keyframe2.inTangent))
				{
					return false;
				}
				if (!Mathf.Approximately(keyframe2.outTangent, 0f) && !float.IsInfinity(keyframe2.outTangent))
				{
					return false;
				}
			}
			return true;
		}

		public static bool ContainsKeyAtTime(this AnimationCurve curve, float time, float tolerance = 1E-07f)
		{
			return curve.keys.Query().Any((Keyframe k) => Mathf.Abs(k.time - time) < tolerance);
		}

		public static AnimationCurve GetCropped(this AnimationCurve curve, float start, float end, bool slideToStart = true)
		{
			AnimationCurve animationCurve = new AnimationCurve();
			Keyframe? keyframe = null;
			Keyframe[] keys = curve.keys;
			for (int i = 0; i < keys.Length; i++)
			{
				Keyframe value = keys[i];
				if (value.time >= start)
				{
					break;
				}
				keyframe = value;
			}
			int num = keys.Length;
			while (num-- != 0)
			{
				if (keys[num].time < start || keys[num].time > end)
				{
					curve.RemoveKey(num);
				}
			}
			bool flag = false;
			for (int j = 0; j < keys.Length; j++)
			{
				Keyframe key = keys[j];
				if (key.time >= start && key.time <= end)
				{
					if (slideToStart)
					{
						key.time -= start;
					}
					if (Mathf.Approximately(key.time, 0f))
					{
						flag = true;
					}
					animationCurve.AddKey(key);
				}
			}
			if (keyframe.HasValue && !flag)
			{
				Keyframe value2 = keyframe.Value;
				value2.time = 0f;
				animationCurve.AddKey(value2);
			}
			return animationCurve;
		}

		public static void AddBooleanKey(this AnimationCurve curve, float time, bool value)
		{
			Keyframe keyframe = default(Keyframe);
			keyframe.time = time;
			keyframe.value = (value ? 1 : 0);
			Keyframe key = keyframe;
			curve.AddKey(key);
		}
	}
}
