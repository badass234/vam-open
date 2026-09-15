using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithRoomInviteNotification : Message<RoomInviteNotification>
	{
		public MessageWithRoomInviteNotification(IntPtr c_message)
			: base(c_message)
		{
		}

		public override RoomInviteNotification GetRoomInviteNotification()
		{
			return base.Data;
		}

		protected override RoomInviteNotification GetDataFromMessage(IntPtr c_message)
		{
			IntPtr obj = CAPI.ovr_Message_GetNativeMessage(c_message);
			IntPtr o = CAPI.ovr_Message_GetRoomInviteNotification(obj);
			return new RoomInviteNotification(o);
		}
	}
}
