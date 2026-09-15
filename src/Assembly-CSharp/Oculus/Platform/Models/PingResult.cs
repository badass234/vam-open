namespace Oculus.Platform.Models
{
	public class PingResult
	{
		private ulong? pingTimeUsec;

		public ulong ID { get; private set; }

		public ulong PingTimeUsec => (!pingTimeUsec.HasValue) ? 0 : pingTimeUsec.Value;

		public bool IsTimeout => !pingTimeUsec.HasValue;

		public PingResult(ulong id, ulong? pingTimeUsec)
		{
			ID = id;
			this.pingTimeUsec = pingTimeUsec;
		}
	}
}
