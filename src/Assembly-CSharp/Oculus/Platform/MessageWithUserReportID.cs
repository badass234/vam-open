using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithUserReportID : Message<UserReportID>
	{
		public MessageWithUserReportID(IntPtr c_message)
			: base(c_message)
		{
		}

		public override UserReportID GetUserReportID()
		{
			return base.Data;
		}

		protected override UserReportID GetDataFromMessage(IntPtr c_message)
		{
			IntPtr obj = CAPI.ovr_Message_GetNativeMessage(c_message);
			IntPtr o = CAPI.ovr_Message_GetUserReportID(obj);
			return new UserReportID(o);
		}
	}
}
