using System;

namespace Leap
{
	public class FailedDevice : IEquatable<FailedDevice>
	{
		public enum FailureType
		{
			FAIL_UNKNOWN,
			FAIL_CALIBRATION,
			FAIL_FIRMWARE,
			FAIL_TRANSPORT,
			FAIL_CONTROl
		}

		public string PnpId { get; private set; }

		public FailureType Failure { get; private set; }

		public FailedDevice()
		{
			Failure = FailureType.FAIL_UNKNOWN;
			PnpId = "0";
		}

		public bool Equals(FailedDevice other)
		{
			return PnpId == other.PnpId;
		}
	}
}
