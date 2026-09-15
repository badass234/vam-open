namespace Leap.Unity
{
	public class StationaryTestLeapProvider : LeapProvider
	{
		private Frame _curFrame;

		private Hand _leftHand;

		private Hand _rightHand;

		public override Frame CurrentFrame => _curFrame;

		public override Frame CurrentFixedFrame => _curFrame;

		private void Awake()
		{
			refreshFrame();
		}

		private void refreshFrame()
		{
			_curFrame = new Frame();
			_leftHand = this.MakeTestHand(true);
			_rightHand = this.MakeTestHand(false);
			_curFrame.Hands.Add(_leftHand);
			_curFrame.Hands.Add(_rightHand);
		}

		private void Update()
		{
			refreshFrame();
			DispatchUpdateFrameEvent(_curFrame);
		}

		private void FixedUpdate()
		{
			DispatchFixedFrameEvent(_curFrame);
		}
	}
}
