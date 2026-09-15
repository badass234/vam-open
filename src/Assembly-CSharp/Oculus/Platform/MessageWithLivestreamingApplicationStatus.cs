using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithLivestreamingApplicationStatus : Message<LivestreamingApplicationStatus>
	{
		public MessageWithLivestreamingApplicationStatus(IntPtr c_message)
			: base(c_message)
		{
		}

		public override LivestreamingApplicationStatus GetLivestreamingApplicationStatus()
		{
			return base.Data;
		}

		protected override LivestreamingApplicationStatus GetDataFromMessage(IntPtr c_message)
		{
			IntPtr obj = CAPI.ovr_Message_GetNativeMessage(c_message);
			IntPtr o = CAPI.ovr_Message_GetLivestreamingApplicationStatus(obj);
			return new LivestreamingApplicationStatus(o);
		}
	}
}
