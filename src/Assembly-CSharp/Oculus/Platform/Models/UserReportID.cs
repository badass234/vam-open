using System;

namespace Oculus.Platform.Models
{
	public class UserReportID
	{
		public readonly ulong ID;

		public UserReportID(IntPtr o)
		{
			ID = CAPI.ovr_UserReportID_GetID(o);
		}
	}
}
