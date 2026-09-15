using UnityEngine;

namespace Leap.Unity
{
	public static class DefaultCurve
	{
		public static AnimationCurve Zero
		{
			get
			{
				AnimationCurve animationCurve = new AnimationCurve();
				animationCurve.AddKey(0f, 0f);
				animationCurve.AddKey(1f, 0f);
				return animationCurve;
			}
		}

		public static AnimationCurve One
		{
			get
			{
				AnimationCurve animationCurve = new AnimationCurve();
				animationCurve.AddKey(0f, 1f);
				animationCurve.AddKey(1f, 1f);
				return animationCurve;
			}
		}

		public static AnimationCurve LinearUp
		{
			get
			{
				AnimationCurve animationCurve = new AnimationCurve();
				animationCurve.AddKey(new Keyframe(0f, 0f, 1f, 1f));
				animationCurve.AddKey(new Keyframe(1f, 1f, 1f, 1f));
				return animationCurve;
			}
		}

		public static AnimationCurve LinearDown
		{
			get
			{
				AnimationCurve animationCurve = new AnimationCurve();
				animationCurve.AddKey(new Keyframe(0f, 1f, -1f, -1f));
				animationCurve.AddKey(new Keyframe(1f, 0f, -1f, -1f));
				return animationCurve;
			}
		}

		public static AnimationCurve SigmoidUp
		{
			get
			{
				AnimationCurve animationCurve = new AnimationCurve();
				animationCurve.AddKey(new Keyframe(0f, 0f, 0f, 0f));
				animationCurve.AddKey(new Keyframe(1f, 1f, 0f, 0f));
				return animationCurve;
			}
		}

		public static AnimationCurve SigmoidDown
		{
			get
			{
				AnimationCurve animationCurve = new AnimationCurve();
				animationCurve.AddKey(new Keyframe(0f, 1f, 0f, 0f));
				animationCurve.AddKey(new Keyframe(1f, 0f, 0f, 0f));
				return animationCurve;
			}
		}

		public static AnimationCurve SigmoidUpDown
		{
			get
			{
				AnimationCurve animationCurve = new AnimationCurve();
				animationCurve.AddKey(new Keyframe(0f, 0f, 0f, 0f));
				animationCurve.AddKey(new Keyframe(0.5f, 1f, 0f, 0f));
				animationCurve.AddKey(new Keyframe(1f, 0f, 0f, 0f));
				return animationCurve;
			}
		}
	}
}
