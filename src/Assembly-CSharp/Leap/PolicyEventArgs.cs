namespace Leap
{
	public class PolicyEventArgs : LeapEventArgs
	{
		public ulong currentPolicies { get; set; }

		public ulong oldPolicies { get; set; }

		public PolicyEventArgs(ulong currentPolicies, ulong oldPolicies)
			: base(LeapEvent.EVENT_POLICY_CHANGE)
		{
			this.currentPolicies = currentPolicies;
			this.oldPolicies = oldPolicies;
		}
	}
}
