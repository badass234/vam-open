using System;
using UnityEngine;

namespace Leap.Unity
{
	public abstract class LeapProvider : MonoBehaviour
	{
		public TestHandFactory.TestHandPose editTimePose;

		public abstract Frame CurrentFrame { get; }

		public abstract Frame CurrentFixedFrame { get; }

		public event Action<Frame> OnUpdateFrame;

		public event Action<Frame> OnFixedFrame;

		protected void DispatchUpdateFrameEvent(Frame frame)
		{
			if (this.OnUpdateFrame != null)
			{
				this.OnUpdateFrame(frame);
			}
		}

		protected void DispatchFixedFrameEvent(Frame frame)
		{
			if (this.OnFixedFrame != null)
			{
				this.OnFixedFrame(frame);
			}
		}
	}
}
