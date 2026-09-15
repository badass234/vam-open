using System.Collections.Generic;

namespace Leap
{
	public class FailedDeviceList : List<FailedDevice>
	{
		public bool IsEmpty => base.Count == 0;

		public FailedDeviceList Append(FailedDeviceList other)
		{
			AddRange(other);
			return this;
		}
	}
}
