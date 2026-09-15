using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithLivestreamingStartResult : Message<LivestreamingStartResult>
	{
		public MessageWithLivestreamingStartResult(IntPtr c_message)
			: base(c_message)
		{
		}

		public override LivestreamingStartResult GetLivestreamingStartResult()
		{
			return base.Data;
		}

		protected override LivestreamingStartResult GetDataFromMessage(IntPtr c_message)
		{
			IntPtr obj = CAPI.ovr_Message_GetNativeMessage(c_message);
			IntPtr o = CAPI.ovr_Message_GetLivestreamingStartResult(obj);
			return new LivestreamingStartResult(o);
		}
	}
}
