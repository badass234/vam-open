namespace Leap.Unity
{
	public static class LeapProviderExtensions
	{
		public static Hand MakeTestHand(this LeapProvider provider, bool isLeft)
		{
			return TestHandFactory.MakeTestHand(isLeft, Hands.Provider.editTimePose).Transform(Hands.Provider.transform.GetLeapMatrix());
		}
	}
}
