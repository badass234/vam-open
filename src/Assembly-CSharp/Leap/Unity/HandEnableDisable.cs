namespace Leap.Unity
{
	public class HandEnableDisable : HandTransitionBehavior
	{
		protected override void Awake()
		{
			base.Awake();
			base.gameObject.SetActive(false);
		}

		protected override void HandReset()
		{
			base.gameObject.SetActive(true);
		}

		protected override void HandFinish()
		{
			base.gameObject.SetActive(false);
		}
	}
}
